using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThetisModel.Entities
{
    [Table("THETIS_HISTORICO_INVESTIMENTOS")]
    public class HistoricoInvestimento
    {
        [Key]
        [Column("ID_HISTORICO")]
        public int Id { get; set; }

        [Column("ID_CLIENTE")]
        public int ClienteId { get; set; }

        [Column("ID_ATIVO")]
        public int? AtivoId { get; set; }

        [Column("TIPO_OPERACAO")]
        [StringLength(20)]
        public required string TipoOperacao { get; set; } // COMPRA, VENDA, RESGATE

        [Column("VALOR_OPERACAO", TypeName = "NUMBER(15,2)")]
        public decimal ValorOperacao { get; set; }

        [Column("QUANTIDADE", TypeName = "NUMBER(15,6)")]
        public decimal? Quantidade { get; set; }

        [Column("PRECO_UNITARIO", TypeName = "NUMBER(15,6)")]
        public decimal? PrecoUnitario { get; set; }

        [Column("DATA_OPERACAO")]
        public DateTime DataOperacao { get; set; }

        [Column("RENTABILIDADE_OBTIDA", TypeName = "NUMBER(5,2)")]
        public decimal? RentabilidadeObtida { get; set; }

        [Column("OBSERVACOES")]
        [StringLength(500)]
        public string Observacoes { get; set; }

        [Column("ORIGEM_RECOMENDACAO")]
        public int? CarteiraRecomendadaId { get; set; }

        [Column("TAXA_CORRETAGEM", TypeName = "NUMBER(15,2)")]
        public decimal? TaxaCorretagem { get; set; }

        [Column("IMPOSTO_RENDA", TypeName = "NUMBER(15,2)")]
        public decimal? ImpostoRenda { get; set; }

        [Column("DATA_VENCIMENTO")]
        public DateTime? DataVencimento { get; set; }

        // Relacionamentos
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("AtivoId")]
        public virtual Ativo Ativo { get; set; }

        [ForeignKey("CarteiraRecomendadaId")]
        public virtual CarteiraRecomendada CarteiraRecomendada { get; set; }
    }
}
