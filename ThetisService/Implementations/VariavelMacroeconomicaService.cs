using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ThetisData.Context;
using ThetisModel.DTOs;
using ThetisModel.ViewModels;
using ThetisService.Interfaces;

namespace ThetisService.Implementations
{
    public class VariavelMacroeconomicaService : IVariavelMacroeconomicaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IBancoCentralService _bancoCentralService;

        public VariavelMacroeconomicaService(AppDbContext context, IMapper mapper, IBancoCentralService bancoCentralService)
        {
            _context = context;
            _mapper = mapper;
            _bancoCentralService = bancoCentralService;
        }

        public async Task<IEnumerable<VariavelMacroeconomicaViewModel>> GetAllAsync()
        {
            var variaveis = await _context.VariaveisMacroeconomicas
                .Where(v => v.Ativa)
                .OrderBy(v => v.Codigo)
                .ToListAsync();

            return _mapper.Map<IEnumerable<VariavelMacroeconomicaViewModel>>(variaveis);
        }

        public async Task<VariavelMacroeconomicaViewModel> GetByCodigoAsync(string codigo)
        {
            var code = codigo.Trim().ToUpperInvariant();

            var variavel = await _context.VariaveisMacroeconomicas
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Ativa && v.Codigo.ToUpper() == code);

            if (variavel == null && code.Length >= 2)
            {
                variavel = await _context.VariaveisMacroeconomicas
                    .AsNoTracking()
                    .Where(v => v.Ativa && v.Codigo.ToUpper().Contains(code))
                    .FirstOrDefaultAsync();
            }

            if (variavel == null)
                throw new KeyNotFoundException($"Variável {codigo} não encontrada");

            return _mapper.Map<VariavelMacroeconomicaViewModel>(variavel);
        }

        public async Task<VariavelMacroeconomicaViewModel> UpdateAsync(int id, VariavelMacroeconomicaDto dto)
        {
            var variavel = await _context.VariaveisMacroeconomicas.FindAsync(id);
            if (variavel == null)
                throw new KeyNotFoundException($"Variável {id} não encontrada");

            variavel.ValorAnterior = variavel.ValorAtual;
            variavel.ValorAtual = dto.ValorAtual;
            variavel.DataAtualizacao = DateTime.Now;
            variavel.DataReferencia = dto.DataReferencia;
            variavel.Tendencia = dto.Tendencia;

            await _context.SaveChangesAsync();
            return _mapper.Map<VariavelMacroeconomicaViewModel>(variavel);
        }

        public async Task<DadosMacroeconomicosDto> AtualizarVariaveisAutomaticamenteAsync()
        {
            try
            {
                // 1) Buscar dados reais das APIs externas
                var dadosReais = await _bancoCentralService.GetDadosMacroeconomicosAsync();

                if (!dadosReais.Sucesso)
                {
                    return dadosReais;
                }

                // 2) Atualizar cada variável no banco
                await AtualizarVariavelAsync("SELIC", dadosReais.Selic, "Taxa básica de juros");
                await AtualizarVariavelAsync("IPCA", dadosReais.Ipca, "Índice de inflação");
                await AtualizarVariavelAsync("CDI", dadosReais.Cdi, "Taxa de referência");
                await AtualizarVariavelAsync("USD", dadosReais.Dolar, "Cotação do dólar");

                await _context.SaveChangesAsync();

                return dadosReais;
            }
            catch (Exception ex)
            {
                return new DadosMacroeconomicosDto
                {
                    Sucesso = false,
                    MensagemErro = ex.Message,
                    DataAtualizacao = DateTime.Now
                };
            }
        }

        private async Task AtualizarVariavelAsync(string codigo, decimal novoValor, string contexto)
        {
            var variavel = await _context.VariaveisMacroeconomicas
                .FirstOrDefaultAsync(v => v.Codigo == codigo && v.Ativa);

            if (variavel == null)
            {
                return;
            }

            // Guardar valor anterior
            variavel.ValorAnterior = variavel.ValorAtual;

            // Atualizar com novo valor
            variavel.ValorAtual = novoValor;
            variavel.DataAtualizacao = DateTime.Now;

            // Determinar tendência
            if (variavel.ValorAnterior.HasValue)
            {
                var diferenca = novoValor - variavel.ValorAnterior.Value;
                var variacao = Math.Abs(diferenca);

                if (variacao < 0.01m) // variação menor que 0.01
                    variavel.Tendencia = "ESTAVEL";
                else if (diferenca > 0)
                    variavel.Tendencia = "ALTA";
                else
                    variavel.Tendencia = "BAIXA";
            }
            else
            {
                variavel.Tendencia = "ESTAVEL";
            }
        }

        public async Task<RelatorioMacroeconomicoViewModel> GetRelatorioMacroeconomicoAsync()
        {
            var variaveis = await GetAllAsync();

            return new RelatorioMacroeconomicoViewModel
            {
                DataRelatorio = DateTime.Now,
                Selic = variaveis.FirstOrDefault(v => v.Codigo == "SELIC")?.ValorAtual ?? 0,
                Ipca = variaveis.FirstOrDefault(v => v.Codigo == "IPCA")?.ValorAtual ?? 0,
                Cdi = variaveis.FirstOrDefault(v => v.Codigo == "CDI")?.ValorAtual ?? 0,
                Dolar = variaveis.FirstOrDefault(v => v.Codigo == "USD")?.ValorAtual ?? 0,
                Ibovespa = variaveis.FirstOrDefault(v => v.Codigo == "IBOV")?.ValorAtual ?? 0,
                CenarioGeral = DeterminarCenarioGeral(variaveis),
                Recomendacoes = GerarRecomendacoesMacroeconomicas(variaveis)
            };
        }

        private string DeterminarCenarioGeral(IEnumerable<VariavelMacroeconomicaViewModel> variaveis)
        {
            var tendenciasAlta = variaveis.Count(v => v.Tendencia == "ALTA");
            var tendenciasBaixa = variaveis.Count(v => v.Tendencia == "BAIXA");

            if (tendenciasAlta > tendenciasBaixa)
                return "Otimista";
            else if (tendenciasBaixa > tendenciasAlta)
                return "Cauteloso";
            else
                return "Neutro";
        }

        private List<string> GerarRecomendacoesMacroeconomicas(IEnumerable<VariavelMacroeconomicaViewModel> variaveis)
        {
            var recomendacoes = new List<string>();

            var selic = variaveis.FirstOrDefault(v => v.Codigo == "SELIC");
            if (selic != null && selic.Tendencia == "ALTA")
                recomendacoes.Add("Com a Selic em alta, considere aumentar posição em renda fixa");

            var dolar = variaveis.FirstOrDefault(v => v.Codigo == "USD");
            if (dolar != null && dolar.Tendencia == "ALTA")
                recomendacoes.Add("Dólar em alta favorece exportadoras e fundos cambiais");

            var ipca = variaveis.FirstOrDefault(v => v.Codigo == "IPCA");
            if (ipca != null && ipca.ValorAtual > 5.0m)
                recomendacoes.Add("IPCA acima da meta - considere proteção inflacionária (Tesouro IPCA+)");

            return recomendacoes;
        }

        //public async Task AtualizarVariaveisAutomaticamenteAsync()
        //{
        //    // Simular atualizações automáticas das variáveis
        //    // Na implementação real, integraria com APIs do Banco Central, IBGE, etc.

        //    var selic = await _context.VariaveisMacroeconomicas
        //        .FirstOrDefaultAsync(v => v.Codigo == "SELIC");
        //    if (selic != null)
        //    {
        //        selic.ValorAnterior = selic.ValorAtual;
        //        selic.DataAtualizacao = DateTime.Now;
        //        // Pequena variação aleatória para simulação
        //        var random = new Random();
        //        var variacao = (decimal)(random.NextDouble() * 0.5 - 0.25); // -0.25 a +0.25
        //        selic.ValorAtual += variacao;
        //    }

        //    await _context.SaveChangesAsync();
        //}
    }
}
