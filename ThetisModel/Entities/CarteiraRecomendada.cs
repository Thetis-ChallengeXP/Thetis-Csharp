using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThetisModel.Enums;

namespace ThetisModel.Entities
{
    [Table("THETIS_CARTEIRAS_RECOMENDADAS")]
    public class CarteiraRecomendada
    {
        [Key]
        [Column("ID_CARTEIRA")]
        public int Id { get; set; }

        [Column("ID_CLIENTE")]
        public int ClienteId { get; set; }

        [Column("NOME_CARTEIRA")]
        [StringLength(200)]
        public required string NomeCarteira { get; set; }

        [Column("VALOR_TOTAL", TypeName = "NUMBER(15,2)")]
        public decimal ValorTotal { get; set; }

        [Column("RENTABILIDADE_ESPERADA", TypeName = "NUMBER(5,2)")]
        public decimal RentabilidadeEsperada { get; set; }

        [Column("NIVEL_RISCO")]
        public PerfilRisco NivelRisco { get; set; }

        [Column("OBJETIVO")]
        public ObjetivoInvestimento Objetivo { get; set; }

        [Column("PRAZO_MESES")]
        public int PrazoMeses { get; set; }

        [Column("EXPLICACAO")]
        [StringLength(2000)]
        public string Explicacao { get; set; }

        [Column("SCORE_ADEQUACAO", TypeName = "NUMBER(3,2)")]
        public decimal ScoreAdequacao { get; set; }

        [Column("DATA_GERACAO")]
        public DateTime DataGeracao { get; set; } = DateTime.Now;

        [Column("ATIVA", TypeName = "NUMBER(1)")]
        public bool Ativa { get; set; } = true;

        [Column("APROVADA_CLIENTE", TypeName = "NUMBER(1)")]
        public bool? AprovadaCliente { get; set; }

        [Column("DATA_APROVACAO")]
        public DateTime? DataAprovacao { get; set; }

        // Relacionamentos
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        public virtual ICollection<ItemCarteira> Itens { get; set; }
    }
}
