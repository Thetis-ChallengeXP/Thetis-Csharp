using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThetisModel.Enums;

namespace ThetisModel.Entities
{
    [Table("THETIS_CLIENTES")]
    public class Cliente
    {
        [Key]
        [Column("ID_CLIENTE")]
        public int Id { get; set; }

        [Column("NOME")]
        [StringLength(200)]
        [Required]
        public required string Nome { get; set; }

        [Column("EMAIL")]
        [StringLength(150)]
        [Required]
        public required string Email { get; set; }

        [Column("CPF")]
        [StringLength(11)]
        [Required]
        public required string Cpf { get; set; }

        [Column("DATA_NASCIMENTO")]
        public DateTime DataNascimento { get; set; }

        [Column("RENDA_MENSAL", TypeName = "NUMBER(15,2)")]
        public decimal RendaMensal { get; set; }

        [Column("VALOR_DISPONIVEL", TypeName = "NUMBER(15,2)")]
        public decimal ValorDisponivel { get; set; }

        [Column("PERFIL_RISCO")]
        public PerfilRisco PerfilRisco { get; set; }

        [Column("OBJETIVO_PRINCIPAL")]
        public ObjetivoInvestimento ObjetivoPrincipal { get; set; }

        [Column("PRAZO_INVESTIMENTO")]
        public int PrazoInvestimentoMeses { get; set; }

        [Column("DATA_CADASTRO")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Column("ATIVO", TypeName = "NUMBER(1)")]
        public bool Ativo { get; set; } = true;

        // Relacionamentos
        public virtual ICollection<CarteiraRecomendada> CarteirasRecomendadas { get; set; }
        public virtual ICollection<HistoricoInvestimento> HistoricoInvestimentos { get; set; }
    }
}
