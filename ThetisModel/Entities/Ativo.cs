using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThetisModel.Enums;

namespace ThetisModel.Entities
{
    [Table("THETIS_ATIVOS")]
    public class Ativo
    {
        [Key]
        [Column("ID_ATIVO")]
        public int Id { get; set; }

        [Column("CODIGO")]
        [StringLength(20)]
        public required string Codigo { get; set; }

        [Column("NOME")]
        [StringLength(200)]
        public required string Nome { get; set; }

        [Column("DESCRICAO")]
        [StringLength(500)]
        public string Descricao { get; set; }

        [Column("TIPO_ATIVO")]
        public TipoAtivo TipoAtivo { get; set; }

        [Column("RENTABILIDADE_ESPERADA", TypeName = "NUMBER(5,2)")]
        public decimal RentabilidadeEsperada { get; set; }

        [Column("NIVEL_RISCO")]
        public PerfilRisco NivelRisco { get; set; }

        [Column("LIQUIDEZ_DIAS")]
        public int LiquidezDias { get; set; }

        [Column("VALOR_MINIMO", TypeName = "NUMBER(15,2)")]
        public decimal ValorMinimo { get; set; }

        [Column("TAXA_ADMINISTRACAO", TypeName = "NUMBER(5,2)")]
        public decimal TaxaAdministracao { get; set; }

        [Column("ATIVO_SISTEMA")]
        public bool AtivoSistema { get; set; } = true;

        [Column("DATA_CRIACAO")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // Relacionamentos
        public virtual ICollection<ItemCarteira> ItensCarteira { get; set; }
        public virtual ICollection<VariavelMacroeconomica> VariaveisMacroeconomicas { get; set; }
    }
}
