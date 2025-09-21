using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThetisModel.Entities
{
    [Table("THETIS_VARIAVEIS_MACROECONOMICAS")]
    public class VariavelMacroeconomica
    {
        [Key]
        [Column("ID_VARIAVEL")]
        public int Id { get; set; }

        [Column("NOME")]
        [StringLength(100)]
        public required string Nome { get; set; }

        [Column("CODIGO")]
        [StringLength(20)]
        public required string Codigo { get; set; } // SELIC, IPCA, CDI, DOLAR, etc.

        [Column("DESCRICAO")]
        [StringLength(300)]
        public string Descricao { get; set; }

        [Column("VALOR_ATUAL", TypeName = "NUMBER(10,4)")]
        public decimal ValorAtual { get; set; }

        [Column("VALOR_ANTERIOR", TypeName = "NUMBER(10,4)")]
        public decimal? ValorAnterior { get; set; }

        [Column("DATA_ATUALIZACAO")]
        public DateTime DataAtualizacao { get; set; }

        [Column("DATA_REFERENCIA")]
        public DateTime DataReferencia { get; set; }

        [Column("UNIDADE_MEDIDA")]
        [StringLength(20)]
        public string UnidadeMedida { get; set; } // %, R$, pontos, etc.

        [Column("FONTE_DADOS")]
        [StringLength(100)]
        public string FonteDados { get; set; } // BACEN, IBGE, etc.

        [Column("TENDENCIA")]
        [StringLength(20)]
        public string Tendencia { get; set; } // ALTA, BAIXA, ESTAVEL

        [Column("IMPACTO_INVESTIMENTOS")]
        [StringLength(500)]
        public string ImpactoInvestimentos { get; set; }

        [Column("ATIVA")]
        public bool Ativa { get; set; } = true;

        // Relacionamentos
        public virtual ICollection<Ativo> AtivosAfetados { get; set; }
    }
}
