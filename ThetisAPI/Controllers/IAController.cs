using Microsoft.AspNetCore.Mvc;
using ThetisModel.DTOs;
using ThetisService.Interfaces;

namespace ThetisApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Produces("application/json")]
    public class IAController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly IRecomendacaoService _recomendacaoService;

        public IAController(IGeminiService geminiService, IRecomendacaoService recomendacaoService)
        {
            _geminiService = geminiService;
            _recomendacaoService = recomendacaoService;
        }

        [HttpPost]
        public async Task<ActionResult<AnaliseIADto>> Perguntar([FromBody] string pergunta)
        {
            if (string.IsNullOrWhiteSpace(pergunta))
                return BadRequest("Pergunta não pode ser vazia");

            var resposta = await _geminiService.AskAsync(pergunta);

            if (!resposta.Sucesso)
                return StatusCode(500, resposta);

            return Ok(resposta);
        }

        [HttpGet("{carteiraId}")]
        public async Task<ActionResult<AnaliseCarteiraIADto>> AnalisarCarteira(int carteiraId)
        {
            try
            {
                var carteira = await _recomendacaoService.GetRecomendacaoByIdAsync(carteiraId);
                var analise = await _geminiService.AnalisarCarteiraAsync(carteira);

                return Ok(analise);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Carteira {carteiraId} não encontrada");
            }
        }

        [HttpGet]
        public async Task<ActionResult<ExplicacaoPersonalizadaDto>> ExplicarConceito(
            [FromQuery] string conceito,
            [FromQuery] string nivel = "basico")
        {
            if (string.IsNullOrWhiteSpace(conceito))
                return BadRequest("Informe o conceito a ser explicado");

            var niveisValidos = new[] { "basico", "intermediario", "avancado" };
            if (!niveisValidos.Contains(nivel.ToLower()))
                return BadRequest($"Nível deve ser: {string.Join(", ", niveisValidos)}");

            var explicacao = await _geminiService.ExplicarConceitoAsync(conceito, nivel);
            return Ok(explicacao);
        }

        [HttpGet("{carteiraId}")]
        public async Task<ActionResult<AnaliseIADto>> CompararPerfilCarteira(int carteiraId)
        {
            try
            {
                var carteira = await _recomendacaoService.GetRecomendacaoByIdAsync(carteiraId);

                var prompt = $@"
Compare o perfil do investidor com a carteira recomendada:

PERFIL DO INVESTIDOR:
- Perfil de Risco: {carteira.NivelRisco}
- Objetivo: {carteira.Objetivo}
- Prazo: {carteira.PrazoMeses} meses

CARTEIRA RECOMENDADA:
- Valor: R$ {carteira.ValorTotal:N2}
- Rentabilidade Esperada: {carteira.RendimentoEsperado:F2}%
- Composição: {carteira.Itens.Count} ativos

A carteira está adequada ao perfil? Por quê?
Existe algum desalinhamento? O que pode ser melhorado?";

                var resposta = await _geminiService.AskAsync(prompt);
                return Ok(resposta);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Carteira {carteiraId} não encontrada");
            }
        }

        [HttpGet("{carteiraId}")]
        public async Task<ActionResult<AnaliseIADto>> SugerirMelhorias(int carteiraId)
        {
            try
            {
                var carteira = await _recomendacaoService.GetRecomendacaoByIdAsync(carteiraId);

                var prompt = $@"
Analise esta carteira e sugira 5 melhorias concretas:

CARTEIRA ATUAL:
{string.Join("\n", carteira.Itens.Select(i =>
    $"- {i.NomeAtivo}: {i.Percentual:F1}% (R$ {i.Valor:N2})"))}

Valor Total: R$ {carteira.ValorTotal:N2}
Perfil: {carteira.NivelRisco}
Objetivo: {carteira.Objetivo}

Forneça:
1. Ajustes de alocação
2. Ativos alternativos
3. Estratégias de diversificação
4. Considerações de prazo
5. Rebalanceamento sugerido";

                var resposta = await _geminiService.AskAsync(prompt);
                return Ok(resposta);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Carteira {carteiraId} não encontrada");
            }
        }

        [HttpGet]
        public ActionResult ListarConceitos()
        {
            var conceitos = new
            {
                basicos = new[]
                {
                    "Renda Fixa",
                    "Renda Variável",
                    "Diversificação",
                    "Liquidez",
                    "Rentabilidade",
                    "Taxa Selic",
                    "IPCA",
                    "CDI"
                },
                intermediarios = new[]
                {
                    "Tesouro Direto",
                    "CDB",
                    "LCI/LCA",
                    "Ações",
                    "Fundos de Investimento",
                    "ETF",
                    "Perfil de Investidor"
                },
                avancados = new[]
                {
                    "Derivativos",
                    "Hedge",
                    "Alocação de Ativos",
                    "Rebalanceamento",
                    "Análise Fundamentalista",
                    "Análise Técnica"
                }
            };

            return Ok(new
            {
                conceitos,
                uso = "Use o endpoint /IA/ExplicarConceito?conceito=XXX&nivel=basico"
            });
        }

        [HttpGet]
        public async Task<ActionResult> TestarConexao()
        {
            var conectado = await _geminiService.TestarConexaoAsync();

            return Ok(new
            {
                conectado,
                mensagem = conectado
                    ? "✅ Google Gemini está funcionando!"
                    : "❌ Falha ao conectar com Google Gemini. Verifique a API Key no appsettings.json",
                apiUrl = "https://ai.google.dev",
                planoGratuito = "60 requisições/minuto - 100% GRÁTIS",
                comoObterChave = "https://makersuite.google.com/app/apikey",
                dataTesteConexao = DateTime.Now
            });
        }
    }
}
