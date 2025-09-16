using ThetisModel.Enums;

namespace ThetisModel.ViewModels
{
    public class RelatorioDiversificacaoViewModel
    {
        public int CarteiraId { get; set; }
        public decimal PercentualRendaFixa { get; set; }
        public decimal PercentualRendaVariavel { get; set; }
        public decimal PercentualFundos { get; set; }
        public int NumeroAtivos { get; set; }
        public decimal MaiorConcentracao { get; set; }
        public string RiscoConcentracao { get; set; }
    }

    public class SimulacaoRendimentoViewModel
    {
        public int CarteiraId { get; set; }
        public decimal ValorInicial { get; set; }
        public decimal ValorFinal { get; set; }
        public decimal RendimentoTotal { get; set; }
        public decimal PercentualGanho { get; set; }
        public List<ProjecaoMensalViewModel> Projecoes { get; set; } = new();
    }

    public class ProjecaoMensalViewModel
    {
        public int Mes { get; set; }
        public decimal ValorInvestido { get; set; }
        public decimal RendimentoAcumulado { get; set; }
        public decimal ValorTotal { get; set; }
    }
    public class CarteiraRecomendadaViewModel
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string NomeCliente { get; set; }
        public string NomeCarteira { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal RendimentoEsperado { get; set; }
        public string NivelRisco { get; set; }
        public string Objetivo { get; set; }
        public int PrazoMeses { get; set; }
        public string Explicacao { get; set; }
        public decimal ScoreAdequacao { get; set; }
        public DateTime DataGeracao { get; set; }
        public bool Ativa { get; set; }
        public bool? AprovadaCliente { get; set; }
        public DateTime? DataAprovacao { get; set; }
        public List<ItemCarteiraViewModel> Itens { get; set; } = new();
    }

    public class ItemCarteiraViewModel
    {
        public int Id { get; set; }
        public int AtivoId { get; set; }
        public string NomeAtivo { get; set; }
        public string TipoAtivo { get; set; }
        public decimal Percentual { get; set; }
        public decimal Valor { get; set; }
        public decimal? Quantidade { get; set; }
        public decimal RentabilidadeEsperada { get; set; }
        public string Justificativa { get; set; }
        public int Prioridade { get; set; }
        public DateTime DataIncluido { get; set; }
    }
}
