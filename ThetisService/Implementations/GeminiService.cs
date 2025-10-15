using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ThetisModel.DTOs;
using ThetisModel.ViewModels;
using ThetisService.Interfaces;

namespace ThetisService.Implementations
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiApiKey"] ?? "";
        }

        public async Task<AnaliseIADto> AskAsync(string pergunta)
        {
            try
            {
                var request = new GeminiRequest
                {
                    Contents = new List<GeminiContent>
                    {
                        new GeminiContent
                        {
                            Parts = new List<GeminiPart>
                            {
                                new GeminiPart { Text = pergunta }
                            }
                        }
                    },
                    GenerationConfig = new GeminiGenerationConfig
                    {
                        Temperature = 0.2m,
                        MaxOutputTokens = 5000,
                    }
                };

                var resposta = await SendRequestAsync(request);

                if (!resposta.Sucesso)
                {
                    return resposta;
                }

                return resposta;
            }
            catch (Exception ex)
            {
                return new AnaliseIADto
                {
                    Pergunta = pergunta,
                    Sucesso = false,
                    MensagemErro = ex.Message,
                    DataAnalise = DateTime.Now
                };
            }
        }

        public async Task<AnaliseCarteiraIADto> AnalisarCarteiraAsync(CarteiraRecomendadaViewModel carteira)
        {
            try
            {
                var contexto = $@"
Analise a seguinte carteira de investimentos e forneça insights detalhados:

DADOS DA CARTEIRA:
- Nome: {carteira.NomeCarteira}
- Valor Total: R$ {carteira.ValorTotal:N2}
- Perfil de Risco: {carteira.NivelRisco}
- Objetivo: {carteira.Objetivo}
- Prazo: {carteira.PrazoMeses} meses
- Rentabilidade Esperada: {carteira.RendimentoEsperado:F2}% ao ano

COMPOSIÇÃO:
{string.Join("\n", carteira.Itens.Select(i =>
    $"- {i.NomeAtivo} ({i.TipoAtivo}): {i.Percentual:F1}% (R$ {i.Valor:N2}) - Rent. Esperada: {i.RentabilidadeEsperada:F2}%"))}

Por favor, forneça:
1. Uma análise detalhada da diversificação
2. 3 pontos fortes desta carteira
3. 3 pontos de atenção ou riscos
4. 3 recomendações práticas
5. Uma classificação de risco (Baixo/Médio/Alto)
6. Um score de 0 a 10 para a qualidade da carteira

Formato da resposta: responda SOMENTE com um JSON válido, sem markdown e sem ```.
Use exatamente estas chaves: analiseDetalhada, pontosFortes (array), pontosDeAtencao (array), recomendacoes (array), riscoGeral (string), scoreIA (número).";

                var resposta = await AskAsync(contexto);

                if (!resposta.Sucesso)
                {
                    return new AnaliseCarteiraIADto
                    {
                        CarteiraId = carteira.Id,
                        NomeCarteira = carteira.NomeCarteira,
                        AnaliseDetalhada = "Não foi possível analisar a carteira no momento.",
                        DataAnalise = DateTime.Now
                    };
                }

                var analise = TentarParsearAnalise(resposta.Resposta, carteira);

                return analise;
            }
            catch (Exception ex)
            {
                return new AnaliseCarteiraIADto
                {
                    CarteiraId = carteira.Id,
                    NomeCarteira = carteira.NomeCarteira,
                    AnaliseDetalhada = $"Erro na análise: {ex.Message}",
                    DataAnalise = DateTime.Now
                };
            }
        }

        public async Task<ExplicacaoPersonalizadaDto> ExplicarConceitoAsync(string conceito, string nivelConhecimento = "basico", string nivelExplicacao = "simples")
        {
            var prompt = $@"
Explique o conceito de '{conceito}' em investimentos de forma {nivelConhecimento}.

Forneça:
1. Uma explicação {nivelExplicacao}
2. 2-3 exemplos práticos
3. Próximos passos para aplicar esse conhecimento

Seja didático e objetivo.";

            var resposta = await AskAsync(prompt);

            return new ExplicacaoPersonalizadaDto
            {
                Contexto = conceito,
                Explicacao = resposta.Resposta,
            };
        }

        private async Task<AnaliseIADto> SendRequestAsync(GeminiRequest request)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new AnaliseIADto
                {
                    Sucesso = false,
                    MensagemErro = "API Key do Gemini não configurada. Configure GeminiApiKey no appsettings.json",
                    DataAnalise = DateTime.Now
                };
            }

            var url = $"{BASE_URL}?key={_apiKey}";
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new AnaliseIADto
                {
                    Sucesso = false,
                    MensagemErro = $"Erro HTTP: {response.StatusCode}",
                    DataAnalise = DateTime.Now
                };
            }

            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseJson);

            if (geminiResponse?.Candidates == null || !geminiResponse.Candidates.Any())
            {
                return new AnaliseIADto
                {
                    Sucesso = false,
                    MensagemErro = "Nenhuma resposta retornada pela IA",
                    DataAnalise = DateTime.Now
                };
            }

            var respostaTexto = geminiResponse.Candidates.First().Content?.Parts?.FirstOrDefault()?.Text ?? "";

            return new AnaliseIADto
            {
                Pergunta = request.Contents.FirstOrDefault()?.Parts?.FirstOrDefault()?.Text ?? "",
                Resposta = respostaTexto,
                Sucesso = true,
                DataAnalise = DateTime.Now,
                TokensUsados = respostaTexto.Length / 4
            };
        }

        private static string? ExtractFirstJsonObject(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            // remove cercas ``` (com ou sem "json")
            text = Regex.Replace(text, @"^```(?:json)?\s*|\s*```$", "", RegexOptions.Multiline).Trim();

            // remove prefixo "json" avulso no começo
            if (text.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                text = text.Substring(4).Trim();

            // encontra o primeiro objeto JSON balanceando chaves
            int start = text.IndexOf('{');
            if (start < 0) return null;

            int depth = 0;
            bool inString = false;
            bool escape = false;

            for (int i = start; i < text.Length; i++)
            {
                char c = text[i];

                if (inString)
                {
                    if (escape) { escape = false; }
                    else if (c == '\\') { escape = true; }
                    else if (c == '"') { inString = false; }
                }
                else
                {
                    if (c == '"') inString = true;
                    else if (c == '{') depth++;
                    else if (c == '}')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            return text.Substring(start, i - start + 1);
                        }
                    }
                }
            }

            return null;
        }

        private AnaliseCarteiraIADto TentarParsearAnalise(string respostaIA, CarteiraRecomendadaViewModel carteira)
        {
            var dto = new AnaliseCarteiraIADto
            {
                CarteiraId = carteira.Id,
                NomeCarteira = carteira.NomeCarteira,
                DataAnalise = DateTime.Now
            };

            try
            {
                var json = ExtractFirstJsonObject(respostaIA);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var model = JsonSerializer.Deserialize<LlmAnaliseResponse>(json!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (model != null)
                    {
                        dto.AnaliseDetalhada = model.analiseDetalhada ?? "";
                        dto.PontosFortes = model.pontosFortes ?? new List<string>();
                        dto.PontosDeAtencao = model.pontosDeAtencao ?? new List<string>();
                        dto.Recomendacoes = model.recomendacoes ?? new List<string>();
                        dto.RiscoGeral = string.IsNullOrWhiteSpace(model.riscoGeral)
                            ? DeterminarRisco(carteira.NivelRisco)
                            : model.riscoGeral;
                        dto.ScoreIA = model.scoreIA ?? CalcularScore(carteira);
                        return dto;
                    }
                }
            }
            catch
            {
                // cai no fallback abaixo
            }

            dto.AnaliseDetalhada = respostaIA ?? "";
            dto.PontosFortes = ExtrairPontos(respostaIA, "pontos fortes", "fortes", "vantagens");
            dto.PontosDeAtencao = ExtrairPontos(respostaIA, "atenção", "riscos", "cuidados");
            dto.Recomendacoes = ExtrairPontos(respostaIA, "recomendações", "sugestões");
            dto.RiscoGeral = DeterminarRisco(carteira.NivelRisco);
            dto.ScoreIA = CalcularScore(carteira);
            return dto;
        }

        private List<string> ExtrairPontos(string texto, params string[] palavrasChave)
        {
            return new List<string> { texto };
        }

        private string DeterminarRisco(string nivelRisco)
        {
            return nivelRisco.ToLower() switch
            {
                "conservador" => "Baixo",
                "moderado" => "Médio",
                "agressivo" => "Alto",
                _ => "Médio"
            };
        }

        private decimal CalcularScore(CarteiraRecomendadaViewModel carteira)
        {
            var score = (decimal)carteira.ScoreAdequacao * 10;
            return Math.Min(Math.Max(score, 0), 10);
        }

        public async Task<bool> TestarConexaoAsync()
        {
            try
            {
                var resposta = await AskAsync("Teste de conexão: responda apenas 'OK'");
                var resultado = resposta.Sucesso;

                return resultado;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
