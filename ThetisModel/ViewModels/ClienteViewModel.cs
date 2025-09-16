using ThetisModel.Enums;

namespace ThetisModel.ViewModels
{
    public class ClienteViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Cpf { get; set; }
        public DateTime DataNascimento { get; set; }
        public decimal RendaMensal { get; set; }
        public decimal ValorDisponivel { get; set; }
        public PerfilRisco PerfilRisco { get; set; }
        public string PerfilRiscoDescricao => PerfilRisco.ToString();
        public ObjetivoInvestimento ObjetivoPrincipal { get; set; }
        public string ObjetivoPrincipalDescricao => ObjetivoPrincipal.ToString();
        public int PrazoInvestimentoMeses { get; set; }
        public DateTime DataCadastro { get; set; }
        public int QuantidadeCarteiras { get; set; }
        public decimal TotalInvestido { get; set; }
    }

    public class ClientePerfilViewModel
    {
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public PerfilRisco PerfilRisco { get; set; }
        public ObjetivoInvestimento ObjetivoPrincipal { get; set; }
        public int PrazoInvestimentoMeses { get; set; }
        public decimal ValorDisponivel { get; set; }
        public decimal RendaMensal { get; set; }
        public int Idade { get; set; }
        public decimal CapacidadeInvestimento { get; set; }
    }
}
