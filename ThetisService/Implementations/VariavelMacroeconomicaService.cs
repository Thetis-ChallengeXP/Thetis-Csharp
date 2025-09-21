using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ThetisData.Context;
using ThetisModel.DTOs;
using ThetisModel.Entities;
using ThetisModel.ViewModels;
using ThetisService.Interfaces;

namespace ThetisService.Implementations
{
    public class VariavelMacroeconomicaService : IVariavelMacroeconomicaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public VariavelMacroeconomicaService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

            return recomendacoes;
        }

        public async Task AtualizarVariaveisAutomaticamenteAsync()
        {
            // Simular atualizações automáticas das variáveis
            // Na implementação real, integraria com APIs do Banco Central, IBGE, etc.

            var selic = await _context.VariaveisMacroeconomicas
                .FirstOrDefaultAsync(v => v.Codigo == "SELIC");
            if (selic != null)
            {
                selic.ValorAnterior = selic.ValorAtual;
                selic.DataAtualizacao = DateTime.Now;
                // Pequena variação aleatória para simulação
                var random = new Random();
                var variacao = (decimal)(random.NextDouble() * 0.5 - 0.25); // -0.25 a +0.25
                selic.ValorAtual += variacao;
            }

            await _context.SaveChangesAsync();
        }
    }
}
