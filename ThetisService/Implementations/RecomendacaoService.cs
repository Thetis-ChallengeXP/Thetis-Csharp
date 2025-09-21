using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ThetisData.Context;
using ThetisModel.DTOs;
using ThetisModel.Entities;
using ThetisModel.Enums;
using ThetisModel.ViewModels;
using ThetisService.Interfaces;

namespace ThetisService.Implementations
{
    public class RecomendacaoService : IRecomendacaoService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IClienteService _clienteService;
        private readonly IAtivoService _ativoService;
        private readonly IVariavelMacroeconomicaService _variavelService;

        public RecomendacaoService(AppDbContext context, IMapper mapper,
            IClienteService clienteService, IAtivoService ativoService, IVariavelMacroeconomicaService variavelService)
        {
            _context = context;
            _mapper = mapper;
            _clienteService = clienteService;
            _ativoService = ativoService;
            _variavelService = variavelService;
        }

        public async Task<CarteiraRecomendadaViewModel> GerarRecomendacaoAsync(SolicitacaoRecomendacaoDto solicitacao)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string resultadoAlgoritmo = "";
            string erro = "";

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) Perfil do cliente
                var perfil = await _clienteService.GetPerfilInvestidorAsync(solicitacao.ClienteId);

                // 2) Ativos candidatos
                var ativosVM = await _ativoService.GetRecomendadosParaPerfilAsync(perfil.PerfilRisco, solicitacao.ValorInvestimento);

                if (solicitacao.AtivosDisponiveis?.Any() == true)
                    ativosVM = ativosVM.Where(a => solicitacao.AtivosDisponiveis.Contains(a.Id));

                // 3) Executar algoritmo
                var carteiraVM = await ExecutarAlgoritmoRecomendacao(perfil, ativosVM.ToList(), solicitacao);

                // 4) Montar entity
                var entity = new CarteiraRecomendada
                {
                    ClienteId = solicitacao.ClienteId,
                    NomeCarteira = string.IsNullOrWhiteSpace(carteiraVM.NomeCarteira)
                        ? $"Carteira {perfil.PerfilRisco} - {DateTime.Now:yyyyMMddHHmm}"
                        : carteiraVM.NomeCarteira,
                    ValorTotal = carteiraVM.ValorTotal,
                    RentabilidadeEsperada = carteiraVM.RendimentoEsperado,
                    NivelRisco = perfil.PerfilRisco,
                    Objetivo = solicitacao.Objetivo,
                    PrazoMeses = solicitacao.PrazoMeses,
                    Explicacao = carteiraVM.Explicacao,
                    ScoreAdequacao = carteiraVM.ScoreAdequacao,
                    DataGeracao = DateTime.Now,
                    Ativa = true,
                    AprovadaCliente = null
                };

                await _context.CarteirasRecomendadas.AddAsync(entity);
                await _context.SaveChangesAsync();

                // 5) Itens (entity)
                foreach (var itemVM in carteiraVM.Itens)
                {
                    var item = new ItemCarteira
                    {
                        CarteiraRecomendadaId = entity.Id,
                        AtivoId = itemVM.AtivoId,
                        PercentualCarteira = itemVM.Percentual,
                        ValorInvestimento = itemVM.Valor,
                        Quantidade = itemVM.Quantidade,
                        RentabilidadeEsperada = itemVM.RentabilidadeEsperada,
                        Justificativa = itemVM.Justificativa,
                        Prioridade = itemVM.Prioridade,
                        DataIncluido = DateTime.Now
                    };
                    await _context.ItensCarteira.AddAsync(item);
                }
                await _context.SaveChangesAsync();

                // 6) Log (sucesso)
                sw.Stop();
                resultadoAlgoritmo = JsonSerializer.Serialize(new
                {
                    carteiraId = entity.Id,
                    itens = carteiraVM.Itens.Select(i => new { i.AtivoId, i.Percentual, i.Valor })
                });

                await RegistrarLogRecomendacao(solicitacao, entity.Id, true, sw.ElapsedMilliseconds, resultadoAlgoritmo, erro);

                await tx.CommitAsync();

                // 7) Retornar VM atualizada do banco
                return await GetRecomendacaoByIdAsync(entity.Id);
            }
            catch (Exception ex)
            {
                sw.Stop();
                erro = ex.Message;
                await RegistrarLogRecomendacao(solicitacao, 0, false, sw.ElapsedMilliseconds, resultadoAlgoritmo, erro);
                await tx.RollbackAsync();
                throw;
            }
        }

        private async Task<CarteiraRecomendadaViewModel> ExecutarAlgoritmoRecomendacao(
            ClientePerfilViewModel perfil, List<AtivoViewModel> ativos, SolicitacaoRecomendacaoDto solicitacao)
        {
            // Alvos base por perfil
            decimal rf = perfil.PerfilRisco switch
            {
                PerfilRisco.Conservador => 80m,   // renda fixa
                PerfilRisco.Moderado => 50m,
                PerfilRisco.Agressivo => 20m,
                _ => 50m
            };

            decimal rv = perfil.PerfilRisco switch
            {
                PerfilRisco.Conservador => 0m,    // renda variável
                PerfilRisco.Moderado => 30m,
                PerfilRisco.Agressivo => 60m,
                _ => 30m
            };

            decimal fundos = 100m - (rf + rv);    // completa 100

            // Ajuste leve por macro
            if (solicitacao.ConsiderarVariaveisMacroeconomicas)
            {
                var macro = await _variavelService.GetRelatorioMacroeconomicoAsync();

                // Regra A: SELIC alta + cenário "Cauteloso" -> +10% em renda fixa (cap 80%)
                if (macro.Selic >= 10m && string.Equals(macro.CenarioGeral, "Cauteloso", StringComparison.OrdinalIgnoreCase))
                {
                    var add = Math.Min(10m, 80m - rf);
                    if (add > 0)
                    {
                        rf += add;
                        var totalOutros = rv + fundos;
                        if (totalOutros > 0)
                        {
                            // retira proporcionalmente de RV e Fundos
                            rv -= add * (rv / totalOutros);
                            fundos -= add * (fundos / totalOutros);
                        }
                    }
                }

                // Regra B: IBOV negativo e perfil Conservador -> -5% de RV, +5% em RF
                if (perfil.PerfilRisco == PerfilRisco.Conservador && macro.Ibovespa < 0)
                {
                    var sub = Math.Min(5m, rv);
                    rv -= sub;
                    rf = Math.Min(80m, rf + sub);
                }
            }

            // Monta a carteira passando os alvos
            var carteira = new CarteiraRecomendadaViewModel
            {
                ClienteId = perfil.ClienteId,
                NomeCliente = perfil.Nome,
                ValorTotal = solicitacao.ValorInvestimento,
                NivelRisco = perfil.PerfilRisco.ToString(),
                Objetivo = solicitacao.Objetivo.ToString(),
                PrazoMeses = solicitacao.PrazoMeses,
                DataGeracao = DateTime.Now,
                Itens = new List<ItemCarteiraViewModel>()
            };

            // Algoritmo baseado no perfil de risco
            switch (perfil.PerfilRisco)
            {
                case PerfilRisco.Conservador:
                    carteira = CriarCarteiraConservadora(carteira, ativos, solicitacao, rf, rv, fundos);
                    break;
                case PerfilRisco.Moderado:
                    carteira = CriarCarteiraModerada(carteira, ativos, solicitacao, rf, rv, fundos);
                    break;
                case PerfilRisco.Agressivo:
                    carteira = CriarCarteiraAgressiva(carteira, ativos, solicitacao, rf, rv, fundos);
                    break;
            }

            // Calcular rentabilidade esperada e score de adequação
            carteira.RendimentoEsperado = CalcularRentabilidadeEsperada(carteira.Itens);
            carteira.ScoreAdequacao = CalcularScoreAdequacao(perfil, carteira);
            carteira.Explicacao = GerarExplicacaoCarteira(perfil, carteira);

            return carteira;
        }

        private CarteiraRecomendadaViewModel CriarCarteiraConservadora(
            CarteiraRecomendadaViewModel carteira, List<AtivoViewModel> ativos, SolicitacaoRecomendacaoDto solicitacao,
            decimal alvoRF, decimal alvoRV, decimal alvoFundos)
        {
            var valorTotal = solicitacao.ValorInvestimento;

            var ativosRendaFixa = ativos.Where(a => a.TipoAtivo == TipoAtivo.RendaFixa).ToList();
            var fundosConservadores = ativos.Where(a => a.TipoAtivo == TipoAtivo.Fundos &&
                                                      a.NivelRisco == PerfilRisco.Conservador).ToList();

            if (ativosRendaFixa.Any())
            {
                var melhor = ativosRendaFixa
                    .Where(a => a.LiquidezDias <= 30)
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .First();

                var pct1 = Math.Round(alvoRF * 0.75m, 2);
                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = melhor.Id,
                    NomeAtivo = melhor.Nome,
                    TipoAtivo = melhor.TipoAtivoDescricao,
                    Percentual = pct1,
                    Valor = valorTotal * (pct1 / 100m),
                    RentabilidadeEsperada = melhor.RentabilidadeEsperada,
                    Justificativa = "Investimento principal com baixo risco e boa liquidez",
                    Prioridade = 1
                });

                var segundo = ativosRendaFixa
                    .Where(a => a.Id != melhor.Id)
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .FirstOrDefault();

                if (segundo != null)
                {
                    var pct2 = Math.Round(alvoRF - pct1, 2);
                    if (pct2 > 0)
                    {
                        carteira.Itens.Add(new ItemCarteiraViewModel
                        {
                            AtivoId = segundo.Id,
                            NomeAtivo = segundo.Nome,
                            TipoAtivo = segundo.TipoAtivoDescricao,
                            Percentual = pct2,
                            Valor = valorTotal * (pct2 / 100m),
                            RentabilidadeEsperada = segundo.RentabilidadeEsperada,
                            Justificativa = "Diversificação em renda fixa para reduzir riscos",
                            Prioridade = 2
                        });
                    }
                }
            }

            if (fundosConservadores.Any() && alvoFundos > 0)
            {
                var melhorFundo = fundosConservadores
                    .OrderBy(a => a.TaxaAdministracao)
                    .ThenByDescending(a => a.RentabilidadeEsperada)
                    .First();

                var pct = Math.Round(alvoFundos, 2);
                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = melhorFundo.Id,
                    NomeAtivo = melhorFundo.Nome,
                    TipoAtivo = melhorFundo.TipoAtivoDescricao,
                    Percentual = pct,
                    Valor = valorTotal * (pct / 100m),
                    RentabilidadeEsperada = melhorFundo.RentabilidadeEsperada,
                    Justificativa = "Fundo profissionalmente gerido para estabilidade",
                    Prioridade = 3
                });
            }

            return carteira;
        }

        private CarteiraRecomendadaViewModel CriarCarteiraModerada(
            CarteiraRecomendadaViewModel carteira, List<AtivoViewModel> ativos, SolicitacaoRecomendacaoDto solicitacao,
            decimal alvoRF, decimal alvoRV, decimal alvoFundos)
        {
            var valorTotal = solicitacao.ValorInvestimento;

            // Alocação moderada: 50% Renda Fixa, 30% Ações, 20% Fundos
            var rendaFixa = ativos.Where(a => a.TipoAtivo == TipoAtivo.RendaFixa).ToList();
            var acoes = ativos.Where(a => a.TipoAtivo == TipoAtivo.RendaVariavel).ToList();
            var fundos = ativos.Where(a => a.TipoAtivo == TipoAtivo.Fundos).ToList();

            // 50% Renda Fixa
            if (alvoRF > 0 && rendaFixa.Any())
            {
                // Regra simples: 60% do bloco RF no melhor papel, 40% no segundo
                var melhorRF = rendaFixa.OrderByDescending(a => a.RentabilidadeEsperada).First();

                var pctRF1 = Math.Round(alvoRF * 0.60m, 2);
                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = melhorRF.Id,
                    NomeAtivo = melhorRF.Nome,
                    TipoAtivo = melhorRF.TipoAtivoDescricao,
                    Percentual = pctRF1,
                    Valor = valorTotal * (pctRF1 / 100m),
                    RentabilidadeEsperada = melhorRF.RentabilidadeEsperada,
                    Justificativa = "Base conservadora da carteira para estabilidade",
                    Prioridade = 1
                });

                var segundoRF = rendaFixa.Where(a => a.Id != melhorRF.Id)
                                         .OrderByDescending(a => a.RentabilidadeEsperada)
                                         .FirstOrDefault();
                var pctRF2 = Math.Round(alvoRF - pctRF1, 2);
                if (segundoRF != null && pctRF2 > 0)
                {
                    carteira.Itens.Add(new ItemCarteiraViewModel
                    {
                        AtivoId = segundoRF.Id,
                        NomeAtivo = segundoRF.Nome,
                        TipoAtivo = segundoRF.TipoAtivoDescricao,
                        Percentual = pctRF2,
                        Valor = valorTotal * (pctRF2 / 100m),
                        RentabilidadeEsperada = segundoRF.RentabilidadeEsperada,
                        Justificativa = "Diversificação na renda fixa para reduzir riscos",
                        Prioridade = 2
                    });
                }
            }


            // 30% Ações
            if (alvoRV > 0 && acoes.Any())
            {
                // Critério: nivel de risco <= Moderado, e maior rentabilidade
                var candidatasRv = acoes
                    .Where(a => a.NivelRisco <= PerfilRisco.Moderado)
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .Take(2) // 2 papéis
                    .ToList();

                // Distribuição: 2/3 e 1/3 do bloco RV
                var pctRv1 = Math.Round(alvoRV * (2m / 3m), 2);
                var pctRv2 = Math.Round(alvoRV - pctRv1, 2);

                if (candidatasRv.Count >= 1)
                {
                    var acao1 = candidatasRv[0];
                    carteira.Itens.Add(new ItemCarteiraViewModel
                    {
                        AtivoId = acao1.Id,
                        NomeAtivo = acao1.Nome,
                        TipoAtivo = acao1.TipoAtivoDescricao,
                        Percentual = pctRv1,
                        Valor = valorTotal * (pctRv1 / 100m),
                        RentabilidadeEsperada = acao1.RentabilidadeEsperada,
                        Justificativa = "Exposição a renda variável para crescimento",
                        Prioridade = 3
                    });
                }
                if (candidatasRv.Count >= 2 && pctRv2 > 0)
                {
                    var acao2 = candidatasRv[1];
                    carteira.Itens.Add(new ItemCarteiraViewModel
                    {
                        AtivoId = acao2.Id,
                        NomeAtivo = acao2.Nome,
                        TipoAtivo = acao2.TipoAtivoDescricao,
                        Percentual = pctRv2,
                        Valor = valorTotal * (pctRv2 / 100m),
                        RentabilidadeEsperada = acao2.RentabilidadeEsperada,
                        Justificativa = "Diversificação dentro da renda variável",
                        Prioridade = 4
                    });
                }
            }

            // 20% Fundos
            if (alvoFundos > 0 && fundos.Any())
            {
                var melhorFundo = fundos
                    .Where(f => f.NivelRisco <= PerfilRisco.Moderado)
                    .OrderBy(f => f.TaxaAdministracao)
                    .ThenByDescending(f => f.RentabilidadeEsperada)
                    .First();

                var pctFundos = Math.Round(alvoFundos, 2);
                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = melhorFundo.Id,
                    NomeAtivo = melhorFundo.Nome,
                    TipoAtivo = melhorFundo.TipoAtivoDescricao,
                    Percentual = pctFundos,
                    Valor = valorTotal * (pctFundos / 100m),
                    RentabilidadeEsperada = melhorFundo.RentabilidadeEsperada,
                    Justificativa = "Diversificação profissional e gestão ativa",
                    Prioridade = 5
                });
            }

            return carteira;
        }

        private CarteiraRecomendadaViewModel CriarCarteiraAgressiva(
            CarteiraRecomendadaViewModel carteira, List<AtivoViewModel> ativos, SolicitacaoRecomendacaoDto solicitacao,
            decimal alvoRF, decimal alvoRV, decimal alvoFundos)
        {
            var valorTotal = solicitacao.ValorInvestimento;

            // Alocação agressiva: 20% Renda Fixa, 60% Ações, 20% Fundos/Alternativo
            var rendaFixa = ativos.Where(a => a.TipoAtivo == TipoAtivo.RendaFixa).ToList();
            var acoes = ativos.Where(a => a.TipoAtivo == TipoAtivo.RendaVariavel).ToList();
            var fundos = ativos.Where(a => a.TipoAtivo == TipoAtivo.Fundos).ToList();

            // 20% Renda Fixa (reserva de liquidez)
            if (alvoRF > 0 && rendaFixa.Any())
            {
                // Para agressivo: priorize liquidez curta e boa rentabilidade
                var liquida = rendaFixa
                    .Where(a => a.LiquidezDias <= 1)
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .FirstOrDefault();

                var escolhidoRF = liquida ?? rendaFixa.OrderByDescending(a => a.RentabilidadeEsperada).First();

                var pctRF = Math.Round(alvoRF, 2);
                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = escolhidoRF.Id,
                    NomeAtivo = escolhidoRF.Nome,
                    TipoAtivo = escolhidoRF.TipoAtivoDescricao,
                    Percentual = pctRF,
                    Valor = valorTotal * (pctRF / 100m),
                    RentabilidadeEsperada = escolhidoRF.RentabilidadeEsperada,
                    Justificativa = "Reserva de liquidez e ancoragem da carteira",
                    Prioridade = 1
                });
            }

            // 60% Ações (diversificado em 3-4 papéis)
            if (alvoRV > 0 && acoes.Any())
            {
                // Top 3 por rentabilidade esperada
                var topAcoes = acoes
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .Take(3)
                    .ToList();

                // Distribuição interna do bloco RV: 40% / 35% / 25%
                var pesos = new[] { 0.40m, 0.35m, 0.25m };

                for (int i = 0; i < topAcoes.Count; i++)
                {
                    var acao = topAcoes[i];
                    var pct = Math.Round(alvoRV * pesos[i], 2);

                    carteira.Itens.Add(new ItemCarteiraViewModel
                    {
                        AtivoId = acao.Id,
                        NomeAtivo = acao.Nome,
                        TipoAtivo = acao.TipoAtivoDescricao,
                        Percentual = pct,
                        Valor = valorTotal * (pct / 100m),
                        RentabilidadeEsperada = acao.RentabilidadeEsperada,
                        Justificativa = "Potencial de alto crescimento",
                        Prioridade = i + 2
                    });
                }
            }

            // 20% Fundos agressivos
            if (alvoFundos > 0 && fundos.Any())
            {
                // Para agressivo: priorize maior retorno esperado
                var fundoAgressivo = fundos
                    .OrderByDescending(f => f.RentabilidadeEsperada)
                    .First();

                var pctFundos = Math.Round(alvoFundos, 2);
                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = fundoAgressivo.Id,
                    NomeAtivo = fundoAgressivo.Nome,
                    TipoAtivo = fundoAgressivo.TipoAtivoDescricao,
                    Percentual = pctFundos,
                    Valor = valorTotal * (pctFundos / 100m),
                    RentabilidadeEsperada = fundoAgressivo.RentabilidadeEsperada,
                    Justificativa = "Gestão profissional para maximizar retornos",
                    Prioridade = 5
                });
            }

            return carteira;
        }

        private decimal CalcularRentabilidadeEsperada(List<ItemCarteiraViewModel> itens)
        {
            return itens.Sum(i => (i.Percentual / 100m) * i.RentabilidadeEsperada);
        }

        private decimal CalcularScoreAdequacao(ClientePerfilViewModel perfil, CarteiraRecomendadaViewModel carteira)
        {
            decimal score = 0.7m; // Base score

            // Ajustar baseado na adequação ao perfil
            var rendaFixaPerc = carteira.Itens.Where(i => i.TipoAtivo.Contains("Renda Fixa")).Sum(i => i.Percentual);

            switch (perfil.PerfilRisco)
            {
                case PerfilRisco.Conservador:
                    score += (rendaFixaPerc >= 70) ? 0.3m : 0.1m;
                    break;
                case PerfilRisco.Moderado:
                    score += (rendaFixaPerc >= 40 && rendaFixaPerc <= 70) ? 0.3m : 0.1m;
                    break;
                case PerfilRisco.Agressivo:
                    score += (rendaFixaPerc <= 40) ? 0.3m : 0.1m;
                    break;
            }

            return Math.Min(score, 1.0m);
        }

        private string GerarExplicacaoCarteira(ClientePerfilViewModel perfil, CarteiraRecomendadaViewModel carteira)
        {
            var explicacao = $"Carteira recomendada para perfil {perfil.PerfilRisco} com objetivo {perfil.ObjetivoPrincipal}. ";
            explicacao += $"A alocação foi estruturada considerando seu horizonte de {perfil.PrazoInvestimentoMeses} meses e ";
            explicacao += $"valor disponível de R$ {carteira.ValorTotal:N2}. ";
            explicacao += $"Rentabilidade esperada: {carteira.RendimentoEsperado:F2}% ao ano. ";
            explicacao += "Os ativos foram selecionados considerando liquidez, risco e potencial de retorno compatíveis com seu perfil.";

            return explicacao;
        }

        private async Task RegistrarLogRecomendacao(
            SolicitacaoRecomendacaoDto solicitacao,
            int carteiraId,
            bool sucesso,
            long tempoMs,
            string resultadoAlgoritmo,
            string erroMensagem)
        {
            var log = new LogRecomendacao
            {
                ClienteId = solicitacao.ClienteId,
                CarteiraRecomendadaId = carteiraId > 0 ? carteiraId : (int?)null,
                ParametrosEntrada = JsonSerializer.Serialize(solicitacao),
                ResultadoAlgoritmo = resultadoAlgoritmo ?? "",
                TempoProcessamentoMs = (int)tempoMs,
                VersaoAlgoritmo = "1.0",
                DataProcessamento = DateTime.Now,
                Sucesso = sucesso,
                ErroMensagem = erroMensagem
            };

            await _context.LogsRecomendacao.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CarteiraRecomendadaViewModel>> GetRecomendacoesByClienteAsync(int clienteId)
        {
            var carteiras = await _context.CarteirasRecomendadas
                .Include(c => c.Cliente)
                .Include(c => c.Itens)
                    .ThenInclude(i => i.Ativo)
                .Where(c => c.ClienteId == clienteId && c.Ativa)
                .OrderByDescending(c => c.DataGeracao)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CarteiraRecomendadaViewModel>>(carteiras);
        }

        public async Task<CarteiraRecomendadaViewModel> GetRecomendacaoByIdAsync(int carteiraId)
        {
            var carteira = await _context.CarteirasRecomendadas
                .Include(c => c.Cliente)
                .Include(c => c.Itens)
                    .ThenInclude(i => i.Ativo)
                .FirstOrDefaultAsync(c => c.Id == carteiraId);

            if (carteira == null)
                throw new KeyNotFoundException($"Carteira {carteiraId} não encontrada");

            return _mapper.Map<CarteiraRecomendadaViewModel>(carteira);
        }

        public async Task<CarteiraRecomendadaViewModel> AprovarCarteiraAsync(int carteiraId, bool aprovada)
        {
            var carteira = await _context.CarteirasRecomendadas.FindAsync(carteiraId);
            if (carteira == null)
                throw new KeyNotFoundException($"Carteira {carteiraId} não encontrada");

            carteira.AprovadaCliente = aprovada;
            carteira.DataAprovacao = DateTime.Now;

            await _context.SaveChangesAsync();
            return await GetRecomendacaoByIdAsync(carteiraId);
        }

        public async Task<RelatorioDiversificacaoViewModel> AnalisarDiversificacaoAsync(int carteiraId)
        {
            var carteira = await GetRecomendacaoByIdAsync(carteiraId);

            return new RelatorioDiversificacaoViewModel
            {
                CarteiraId = carteiraId,
                PercentualRendaFixa = carteira.Itens.Where(i => i.TipoAtivo.Contains("Renda Fixa")).Sum(i => i.Percentual),
                PercentualRendaVariavel = carteira.Itens.Where(i => i.TipoAtivo.Contains("Variavel")).Sum(i => i.Percentual),
                PercentualFundos = carteira.Itens.Where(i => i.TipoAtivo.Contains("Fundos")).Sum(i => i.Percentual),
                NumeroAtivos = carteira.Itens.Count,
                MaiorConcentracao = carteira.Itens.Max(i => i.Percentual),
                RiscoConcentracao = carteira.Itens.Max(i => i.Percentual) > 40 ? "Alto" : "Adequado"
            };
        }

        public async Task<SimulacaoRendimentoViewModel> SimularRendimentoAsync(int carteiraId, int meses)
        {
            var carteira = await GetRecomendacaoByIdAsync(carteiraId);
            var rentabilidadeMensal = carteira.RendimentoEsperado / 12m;

            var projecoes = new List<ProjecaoMensalViewModel>();
            var valorAtual = carteira.ValorTotal;

            for (int mes = 1; mes <= meses; mes++)
            {
                var rendimento = valorAtual * (rentabilidadeMensal / 100m);
                valorAtual += rendimento;

                projecoes.Add(new ProjecaoMensalViewModel
                {
                    Mes = mes,
                    ValorInvestido = carteira.ValorTotal,
                    RendimentoAcumulado = valorAtual - carteira.ValorTotal,
                    ValorTotal = valorAtual
                });
            }

            return new SimulacaoRendimentoViewModel
            {
                CarteiraId = carteiraId,
                ValorInicial = carteira.ValorTotal,
                ValorFinal = valorAtual,
                RendimentoTotal = valorAtual - carteira.ValorTotal,
                PercentualGanho = ((valorAtual - carteira.ValorTotal) / carteira.ValorTotal) * 100,
                Projecoes = projecoes
            };
        }
    }
}
