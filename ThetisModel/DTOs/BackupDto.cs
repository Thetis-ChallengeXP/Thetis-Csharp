namespace ThetisModel.DTOs
{
    public class BackupPayload
    {
        public DateTime? Data { get; set; }
        public List<AtivoImportDto>? Ativos { get; set; } = new();
        public List<ClienteImportDto>? Clientes { get; set; } = new();
        public List<VariavelImportDto>? Variaveis { get; set; } = new();
    }

    public class AtivoImportDto
    {
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int TipoAtivo { get; set; }
        public decimal RentabilidadeEsperada { get; set; }
        public int NivelRisco { get; set; }    
        public int LiquidezDias { get; set; }
        public decimal ValorMinimo { get; set; }
        public decimal TaxaAdministracao { get; set; }
    }

    public class ClienteImportDto
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Cpf { get; set; }
        public DateTime DataNascimento { get; set; }
        public decimal RendaMensal { get; set; }
        public decimal ValorDisponivel { get; set; }
        public int PerfilRisco { get; set; }      
        public int ObjetivoPrincipal { get; set; }      
        public int PrazoInvestimentoMeses { get; set; }
    }

    public class VariavelImportDto
    {
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
    }
}
