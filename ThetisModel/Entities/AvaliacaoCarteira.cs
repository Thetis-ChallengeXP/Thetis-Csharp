using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThetisModel.Entities
{
    [Table("THETIS_AVALIACOES_CARTEIRA")]
    public class AvaliacaoCarteira
    {
        [Key]
        [Column("ID_AVALIACAO")]
        public int Id { get; set; }

        [Column("ID_CARTEIRA")]
        public int CarteiraRecomendadaId { get; set; }

        [Column("ID_CLIENTE")]
        public int ClienteId { get; set; }

        [Column("NOTA_GERAL")]
        public int NotaGeral { get; set; } // 1 a 5

        [Column("NOTA_ADEQUACAO")]
        public int NotaAdequacao { get; set; } // 1 a 5

        [Column("NOTA_EXPLICACAO")]
        public int NotaExplicacao { get; set; } // 1 a 5

        [Column("COMENTARIOS")]
        [StringLength(1000)]
        public string Comentarios { get; set; }

        [Column("IMPLEMENTOU_RECOMENDACAO", TypeName = "NUMBER(1)")]
        public bool ImplementouRecomendacao { get; set; }

        [Column("PERCENTUAL_IMPLEMENTADO", TypeName = "NUMBER(5,2)")]
        public decimal? PercentualImplementado { get; set; }

        [Column("MOTIVO_NAO_IMPLEMENTACAO")]
        [StringLength(500)]
        public string MotivoNaoImplementacao { get; set; }

        [Column("DATA_AVALIACAO")]
        public DateTime DataAvaliacao { get; set; } = DateTime.Now;

        [Column("RECOMENDARIA", TypeName = "NUMBER(1)")]
        public bool Recomendaria { get; set; }

        // Relacionamentos
        [ForeignKey("CarteiraRecomendadaId")]
        public virtual CarteiraRecomendada CarteiraRecomendada { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }
    }
}
