using Microsoft.EntityFrameworkCore;
using ThetisData.Context;
using ThetisService.Interfaces;
using ThetisModel.Entities;
using ThetisModel.DTOs;
using ThetisModel.ViewModels;
using ThetisModel.Enums;
using AutoMapper;
using Newtonsoft.Json;

namespace ThetisService.Implementations
{
    public class RecomendacaoService : IRecomendacaoService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IClienteService _clienteService;
        private readonly IAtivoService _ativoService;

        public RecomendacaoService(AppDbContext context, IMapper mapper,
            IClienteService clienteService, IAtivoService ativoService)
        {
            _context = context;
            _mapper = mapper;
            _clienteService = clienteService;
            _ativoService = ativoService;
        }

        public async Task<CarteiraRecomendadaViewModel> GerarRecomendacaoAsync(SolicitacaoRecomendacaoDto solicitacao)
        {
            // 1. Buscar perfil do cliente
            var perfilCliente = await _clienteService.GetPerfilInvestidorAsync(solicitacao.ClienteId);

            // 2. Buscar ativos disponíveis compatíveis com o perfil
            var ativosDisponiveis = await _ativoService.GetRecomendadosParaPerfilAsync(
                perfilCliente.PerfilRisco, solicitacao.ValorInvestimento);

            // 3. Filtrar por ativos específicos se fornecidos
            if (solicitacao.AtivosDisponiveis?.Any() == true)
            {
                ativosDisponiveis = ativosDisponiveis
                    .Where(a => solicitacao.AtivosDisponiveis.Contains(a.Id));
            }

            // 4. Aplicar algoritmo de recomendação
            var carteira = await ExecutarAlgoritmoRecomendacao(
                perfilCliente, ativosDisponiveis.ToList(), solicitacao);

            // 5. Salvar carteira recomendada
            var carteiraEntity = _mapper.Map<CarteiraRecomendada>(carteira);
            carteiraEntity.ClienteId = solicitacao.ClienteId;
            carteiraEntity.DataGeracao = DateTime.Now;
            carteiraEntity.Ativa = true;

            await _context.CarteirasRecomendadas.AddAsync(carteiraEntity);
            await _context.SaveChangesAsync();

            // 6. Salvar itens da carteira
            foreach (var item in carteira.Itens)
            {
                var itemEntity = new ItemCarteira
                {
                    CarteiraRecomendadaId = carteiraEntity.Id,
                    AtivoId = item.AtivoId,
                    PercentualCarteira = item.Percentual,
                    ValorInvestimento = item.Valor,
                    RentabilidadeEsperada = item.RentabilidadeEsperada,
                    Justificativa = item.Justificativa,
                    Prioridade = item.Prioridade
                };
                await _context.ItensCarteira.AddAsync(itemEntity);
            }
            await _context.SaveChangesAsync();

            // 7. Registrar log da recomendação
            await RegistrarLogRecomendacao(solicitacao, carteiraEntity.Id, true);

            return await GetRecomendacaoByIdAsync(carteiraEntity.Id);
        }

        private async Task<CarteiraRecomendadaViewModel> ExecutarAlgoritmoRecomendacao(
            ClientePerfilViewModel perfil, List<AtivoViewModel> ativos, SolicitacaoRecomendacaoDto solicitacao)
        {
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
                    carteira = await CriarCarteiraConservadora(carteira, ativos, solicitacao);
                    break;
                case PerfilRisco.Moderado:
                    carteira = await CriarCarteiraModerada(carteira, ativos, solicitacao);
                    break;
                case PerfilRisco.Agressivo:
                    carteira = await CriarCarteiraAgressiva(carteira, ativos, solicitacao);
                    break;
            }

            // Calcular rentabilidade esperada e score de adequação
            carteira.RendimentoEsperado = CalcularRentabilidadeEsperada(carteira.Itens);
            carteira.ScoreAdequacao = CalcularScoreAdequacao(perfil, carteira);
            carteira.Explicacao = GerarExplicacaoCarteira(perfil, carteira);

            return carteira;
        }

        private async Task<CarteiraRecomendadaViewModel> CriarCarteiraConservadora(
            CarteiraRecomendadaViewModel carteira, List<AtivoViewModel> ativos, SolicitacaoRecomendacaoDto solicitacao)
        {
            var valorTotal = solicitacao.ValorInvestimento;

            // Alocação conservadora: 80% Renda Fixa, 20% Fundos conservadores
            var ativosRendaFixa = ativos.Where(a => a.TipoAtivo == TipoAtivo.RendaFixa).ToList();
            var fundosConservadores = ativos.Where(a => a.TipoAtivo == TipoAtivo.Fundos &&
                                                      a.NivelRisco == PerfilRisco.Conservador).ToList();

            // 60% em Tesouro Direto/CDB
            if (ativosRendaFixa.Any())
            {
                var melhorRendaFixa = ativosRendaFixa
                    .Where(a => a.LiquidezDias <= 30) // Boa liquidez para conservador
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .First();

                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = melhorRendaFixa.Id,
                    NomeAtivo = melhorRendaFixa.Nome,
                    TipoAtivo = melhorRendaFixa.TipoAtivoDescricao,
                    Percentual = 60m,
                    Valor = valorTotal * 0.6m,
                    RentabilidadeEsperada = melhorRendaFixa.RentabilidadeEsperada,
                    Justificativa = "Investimento principal com baixo risco e boa liquidez",
                    Prioridade = 1
                });
            }

            // 20% em segundo ativo de renda fixa
            var segundoRendaFixa = ativosRendaFixa
                .Where(a => a.Id != carteira.Itens.FirstOrDefault()?.AtivoId)
                .OrderByDescending(a => a.RentabilidadeEsperada)
                .FirstOrDefault();

            if (segundoRendaFixa != null)
            {
                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = segundoRendaFixa.Id,
                    NomeAtivo = segundoRendaFixa.Nome,
                    TipoAtivo = segundoRendaFixa.TipoAtivoDescricao,
                    Percentual = 20m,
                    Valor = valorTotal * 0.2m,
                    RentabilidadeEsperada = segundoRendaFixa.RentabilidadeEsperada,
                    Justificativa = "Diversificação em renda fixa para reduzir riscos",
                    Prioridade = 2
                });
            }

            // 20% em fundo conservador
            if (fundosConservadores.Any())
            {
                var melhorFundo = fundosConservadores
                    .OrderBy(a => a.TaxaAdministracao)
                    .ThenByDescending(a => a.RentabilidadeEsperada)
                    .First();

                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = melhorFundo.Id,
                    NomeAtivo = melhorFundo.Nome,
                    TipoAtivo = melhorFundo.TipoAtivoDescricao,
                    Percentual = 20m,
                    Valor = valorTotal * 0.2m,
                    RentabilidadeEsperada = melhorFundo.RentabilidadeEsperada,
                    Justificativa = "Fundo profissionalmente gerido para estabilidade",
                    Prioridade = 3
                });
            }

            return carteira;
        }

        private async Task<CarteiraRecomendadaViewModel> CriarCarteiraModerada(
            CarteiraRecomendadaViewModel carteira, List<AtivoViewModel> ativos, SolicitacaoRecomendacaoDto solicitacao)
        {
            var valorTotal = solicitacao.ValorInvestimento;

            // Alocação moderada: 50% Renda Fixa, 30% Ações, 20% Fundos
            var ativosRendaFixa = ativos.Where(a => a.TipoAtivo == TipoAtivo.RendaFixa).ToList();
            var acoes = ativos.Where(a => a.TipoAtivo == TipoAtivo.RendaVariavel).ToList();
            var fundos = ativos.Where(a => a.TipoAtivo == TipoAtivo.Fundos).ToList();

            // 50% Renda Fixa
            if (ativosRendaFixa.Any())
            {
                var melhorRendaFixa = ativosRendaFixa
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .First();

                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = melhorRendaFixa.Id,
                    NomeAtivo = melhorRendaFixa.Nome,
                    TipoAtivo = melhorRendaFixa.TipoAtivoDescricao,
                    Percentual = 50m,
                    Valor = valorTotal * 0.5m,
                    RentabilidadeEsperada = melhorRendaFixa.RentabilidadeEsperada,
                    Justificativa = "Base conservadora da carteira para estabilidade",
                    Prioridade = 1
                });
            }

            // 30% Ações
            if (acoes.Any())
            {
                var melhoresAcoes = acoes
                    .Where(a => a.NivelRisco <= PerfilRisco.Moderado)
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .Take(2)
                    .ToList();

                foreach (var (acao, index) in melhoresAcoes.Select((a, i) => (a, i)))
                {
                    var percentual = index == 0 ? 20m : 10m;
                    carteira.Itens.Add(new ItemCarteiraViewModel
                    {
                        AtivoId = acao.Id,
                        NomeAtivo = acao.Nome,
                        TipoAtivo = acao.TipoAtivoDescricao,
                        Percentual = percentual,
                        Valor = valorTotal * (percentual / 100m),
                        RentabilidadeEsperada = acao.RentabilidadeEsperada,
                        Justificativa = $"Exposição a renda variável para potencial de crescimento",
                        Prioridade = index + 2
                    });
                }
            }

            // 20% Fundos
            if (fundos.Any())
            {
                var melhorFundo = fundos
                    .Where(a => a.NivelRisco <= PerfilRisco.Moderado)
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .First();

                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = melhorFundo.Id,
                    NomeAtivo = melhorFundo.Nome,
                    TipoAtivo = melhorFundo.TipoAtivoDescricao,
                    Percentual = 20m,
                    Valor = valorTotal * 0.2m,
                    RentabilidadeEsperada = melhorFundo.RentabilidadeEsperada,
                    Justificativa = "Diversificação profissional e gestão ativa",
                    Prioridade = 4
                });
            }

            return carteira;
        }

        private async Task<CarteiraRecomendadaViewModel> CriarCarteiraAgressiva(
            CarteiraRecomendadaViewModel carteira, List<AtivoViewModel> ativos, SolicitacaoRecomendacaoDto solicitacao)
        {
            var valorTotal = solicitacao.ValorInvestimento;

            // Alocação agressiva: 20% Renda Fixa, 60% Ações, 20% Fundos/Alternativo
            var ativosRendaFixa = ativos.Where(a => a.TipoAtivo == TipoAtivo.RendaFixa).ToList();
            var acoes = ativos.Where(a => a.TipoAtivo == TipoAtivo.RendaVariavel).ToList();
            var fundos = ativos.Where(a => a.TipoAtivo == TipoAtivo.Fundos).ToList();

            // 20% Renda Fixa (reserva de liquidez)
            if (ativosRendaFixa.Any())
            {
                var melhorLiquidez = ativosRendaFixa
                    .Where(a => a.LiquidezDias <= 1)
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .FirstOrDefault() ?? ativosRendaFixa.First();

                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = melhorLiquidez.Id,
                    NomeAtivo = melhorLiquidez.Nome,
                    TipoAtivo = melhorLiquidez.TipoAtivoDescricao,
                    Percentual = 20m,
                    Valor = valorTotal * 0.2m,
                    RentabilidadeEsperada = melhorLiquidez.RentabilidadeEsperada,
                    Justificativa = "Reserva de liquidez e ancoragem da carteira",
                    Prioridade = 1
                });
            }

            // 60% Ações (diversificado em 3-4 papéis)
            if (acoes.Any())
            {
                var melhoresAcoes = acoes
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .Take(3)
                    .ToList();

                var percentuais = new[] { 25m, 20m, 15m };

                foreach (var (acao, index) in melhoresAcoes.Select((a, i) => (a, i)))
                {
                    carteira.Itens.Add(new ItemCarteiraViewModel
                    {
                        AtivoId = acao.Id,
                        NomeAtivo = acao.Nome,
                        TipoAtivo = acao.TipoAtivoDescricao,
                        Percentual = percentuais[index],
                        Valor = valorTotal * (percentuais[index] / 100m),
                        RentabilidadeEsperada = acao.RentabilidadeEsperada,
                        Justificativa = $"Potencial de alto crescimento - {acao.Descricao}",
                        Prioridade = index + 2
                    });
                }
            }

            // 20% Fundos agressivos
            if (fundos.Any())
            {
                var fundoAgressivo = fundos
                    .OrderByDescending(a => a.RentabilidadeEsperada)
                    .First();

                carteira.Itens.Add(new ItemCarteiraViewModel
                {
                    AtivoId = fundoAgressivo.Id,
                    NomeAtivo = fundoAgressivo.Nome,
                    TipoAtivo = fundoAgressivo.TipoAtivoDescricao,
                    Percentual = 20m,
                    Valor = valorTotal * 0.2m,
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

        private async Task RegistrarLogRecomendacao(SolicitacaoRecomendacaoDto solicitacao, int carteiraId, bool sucesso)
        {
            var log = new LogRecomendacao
            {
                ClienteId = solicitacao.ClienteId,
                CarteiraRecomendadaId = carteiraId,
                ParametrosEntrada = JsonConvert.SerializeObject(solicitacao),
                TempoProcessamentoMs = 1000, // Placeholder
                VersaoAlgoritmo = "1.0",
                DataProcessamento = DateTime.Now,
                Sucesso = sucesso
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
