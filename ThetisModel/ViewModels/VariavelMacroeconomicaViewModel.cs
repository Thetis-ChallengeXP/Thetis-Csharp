using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThetisModel.ViewModels
{
    public class VariavelMacroeconomicaViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public decimal ValorAtual { get; set; }
        public decimal? ValorAnterior { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public DateTime DataReferencia { get; set; }
        public string UnidadeMedida { get; set; }
        public string FonteDados { get; set; }
        public string Tendencia { get; set; }
        public string ImpactoInvestimentos { get; set; }
        public decimal? VariacaoPercentual => ValorAnterior.HasValue && ValorAnterior > 0
            ? ((ValorAtual - ValorAnterior.Value) / ValorAnterior.Value) * 100
            : null;
    }

    public class RelatorioMacroeconomicoViewModel
    {
        public DateTime DataRelatorio { get; set; }
        public decimal Selic { get; set; }
        public decimal Ipca { get; set; }
        public decimal Cdi { get; set; }
        public decimal Dolar { get; set; }
        public decimal Ibovespa { get; set; }
        public string CenarioGeral { get; set; }
        public List<string> Recomendacoes { get; set; } = new();
    }
}
