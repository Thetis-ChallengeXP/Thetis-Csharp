using System.Text.Json.Serialization;

namespace ThetisModel.DTOs
{
    public class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = new();

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    public class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = new();

        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";
    }

    public class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class GeminiGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public decimal Temperature { get; set; } = 0.2m;

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; } = 5000;
        
    }

    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }

        [JsonPropertyName("error")]
        public GeminiError? Error { get; set; }
    }

    public class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string FinishReason { get; set; } = string.Empty;
    }

    public class GeminiError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class AnaliseIADto
    {
        public string Pergunta { get; set; } = string.Empty;
        public string Resposta { get; set; } = string.Empty;
        public DateTime DataAnalise { get; set; }
        public bool Sucesso { get; set; }
        public string? MensagemErro { get; set; }
        public int TokensUsados { get; set; }
    }

    public class AnaliseCarteiraIADto
    {
        public int CarteiraId { get; set; }
        public string NomeCarteira { get; set; } = string.Empty;
        public string AnaliseDetalhada { get; set; } = string.Empty;
        public List<string> PontosFortes { get; set; } = new();
        public List<string> PontosDeAtencao { get; set; } = new();
        public List<string> Recomendacoes { get; set; } = new();
        public string RiscoGeral { get; set; } = string.Empty;
        public decimal ScoreIA { get; set; }
        public DateTime DataAnalise { get; set; }
    }

    public class ExplicacaoPersonalizadaDto
    {
        public string Contexto { get; set; } = string.Empty;
        public string Explicacao { get; set; } = string.Empty;
    }

    public class LlmAnaliseResponse
    {
        public string? analiseDetalhada { get; set; }
        public List<string>? pontosFortes { get; set; }
        public List<string>? pontosDeAtencao { get; set; }
        public List<string>? recomendacoes { get; set; }
        public string? riscoGeral { get; set; }
        public decimal? scoreIA { get; set; }
    }
}
