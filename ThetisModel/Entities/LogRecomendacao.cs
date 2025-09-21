using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThetisModel.Entities
{
    [Table("THETIS_LOGS_RECOMENDACAO")]
    public class LogRecomendacao
    {
        [Key]
        [Column("ID_LOG")]
        public int Id { get; set; }

        [Column("ID_CLIENTE")]
        public int ClienteId { get; set; }

        [Column("ID_CARTEIRA")]
        public int? CarteiraRecomendadaId { get; set; }

        [Column("PARAMETROS_ENTRADA", TypeName = "CLOB")]
        public string ParametrosEntrada { get; set; } // JSON com todos os parâmetros

        [Column("RESULTADO_ALGORITMO", TypeName = "CLOB")]
        public string ResultadoAlgoritmo { get; set; } // JSON com resultado completo

        [Column("TEMPO_PROCESSAMENTO")]
        public int TempoProcessamentoMs { get; set; }

        [Column("VERSAO_ALGORITMO")]
        [StringLength(20)]
        public string VersaoAlgoritmo { get; set; }

        [Column("DATA_PROCESSAMENTO")]
        public DateTime DataProcessamento { get; set; } = DateTime.Now;

        [Column("SUCESSO")]
        public bool Sucesso { get; set; }

        [Column("ERRO_MENSAGEM")]
        [StringLength(1000)]
        public string? ErroMensagem { get; set; }

        // Relacionamentos
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("CarteiraRecomendadaId")]
        public virtual CarteiraRecomendada CarteiraRecomendada { get; set; }
    }
}
