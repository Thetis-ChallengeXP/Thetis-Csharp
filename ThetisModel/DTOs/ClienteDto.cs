using ThetisModel.Enums;

namespace ThetisModel.DTOs
{
    public class ClienteDto
    {
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public string Cpf { get; set; }
        public DateTime DataNascimento { get; set; }
        public decimal RendaMensal { get; set; }
        public decimal ValorDisponivel { get; set; }
        public PerfilRisco PerfilRisco { get; set; }
        public ObjetivoInvestimento ObjetivoPrincipal { get; set; }
        public int PrazoInvestimentoMeses { get; set; }
    }

    public class SolicitacaoRecomendacaoDto
    {
        public int ClienteId { get; set; }
        public List<int> AtivosDisponiveis { get; set; } = new();
        public decimal ValorInvestimento { get; set; }
        public ObjetivoInvestimento Objetivo { get; set; }
        public int PrazoMeses { get; set; }
        public bool ConsiderarVariaveisMacroeconomicas { get; set; } = true;
    }

    public class AtivoDto
    {
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public TipoAtivo TipoAtivo { get; set; }
        public decimal RentabilidadeEsperada { get; set; }
        public PerfilRisco NivelRisco { get; set; }
        public int LiquidezDias { get; set; }
        public decimal ValorMinimo { get; set; }
        public decimal TaxaAdministracao { get; set; }
    }

    public class VariavelMacroeconomicaDto
    {
        public decimal ValorAtual { get; set; }
        public DateTime DataReferencia { get; set; }
        public string Tendencia { get; set; }
    }
}
