using System.Text.Json.Serialization;

namespace ThetisModel.DTOs
{
    public class BancoCentralSerieDto
    {
        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;

        [JsonPropertyName("valor")]
        public string Valor { get; set; } = string.Empty;
    }

    public static class SeriesBancoCentral
    {
        public const int SELIC = 432;           // Taxa Selic (% a.a.)
        public const int IPCA = 433;            // IPCA (% mensal)
        public const int CDI = 4392;            // CDI (% a.a.)
        public const int DOLAR_PTAX = 1;        // Dólar PTAX (R$)
        public const int IGPM = 189;            // IGP-M (% mensal)
        public const int INPC = 188;            // INPC (% mensal)
        public const int POUPANCA = 195;        // Poupança (% a.a.)
    }

    public class DadosMacroeconomicosDto
    {
        public decimal Selic { get; set; }
        public decimal Ipca { get; set; }
        public decimal Cdi { get; set; }
        public decimal Dolar { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public bool Sucesso { get; set; }
        public string? MensagemErro { get; set; }
    }
}
