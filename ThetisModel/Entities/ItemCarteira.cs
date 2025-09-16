using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThetisModel.Entities
{
    [Table("THETIS_ITENS_CARTEIRA")]
    public class ItemCarteira
    {
        [Key]
        [Column("ID_ITEM")]
        public int Id { get; set; }

        [Column("ID_CARTEIRA")]
        public int CarteiraRecomendadaId { get; set; }

        [Column("ID_ATIVO")]
        public int AtivoId { get; set; }

        [Column("PERCENTUAL_CARTEIRA", TypeName = "NUMBER(5,2)")]
        public decimal PercentualCarteira { get; set; }

        [Column("VALOR_INVESTIMENTO", TypeName = "NUMBER(15,2)")]
        public decimal ValorInvestimento { get; set; }

        [Column("QUANTIDADE", TypeName = "NUMBER(15,6)")]
        public decimal? Quantidade { get; set; }

        [Column("RENTABILIDADE_ESPERADA", TypeName = "NUMBER(5,2)")]
        public decimal RentabilidadeEsperada { get; set; }

        [Column("JUSTIFICATIVA")]
        [StringLength(500)]
        public string Justificativa { get; set; }

        [Column("PRIORIDADE")]
        public int Prioridade { get; set; }

        [Column("DATA_INCLUIDO")]
        public DateTime DataIncluido { get; set; } = DateTime.Now;

        // Relacionamentos
        [ForeignKey("CarteiraRecomendadaId")]
        public virtual CarteiraRecomendada CarteiraRecomendada { get; set; }

        [ForeignKey("AtivoId")]
        public virtual Ativo Ativo { get; set; }
    }
}
