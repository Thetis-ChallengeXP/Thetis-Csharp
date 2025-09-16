using ThetisModel.Enums;

namespace ThetisModel.ViewModels
{
    public class AtivoViewModel
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public TipoAtivo TipoAtivo { get; set; }
        public string TipoAtivoDescricao => TipoAtivo.ToString();
        public decimal RentabilidadeEsperada { get; set; }
        public PerfilRisco NivelRisco { get; set; }
        public string NivelRiscoDescricao => NivelRisco.ToString();
        public int LiquidezDias { get; set; }
        public decimal ValorMinimo { get; set; }
        public decimal TaxaAdministracao { get; set; }
        public DateTime DataCriacao { get; set; }
        public List<string> VariaveisInfluencia { get; set; } = new();
    }
}
