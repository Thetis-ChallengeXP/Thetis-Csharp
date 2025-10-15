using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThetisModel.DTOs;
using ThetisModel.Entities;
using ThetisModel.ViewModels;
using ThetisService.Implementations;
using ThetisService.Interfaces;

namespace ThetisApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Produces("application/json")]
    public class VariaveisController : ControllerBase
    {
        private readonly IVariavelMacroeconomicaService _variaveisService;
        private readonly IBancoCentralService _bancoCentralService;
        public VariaveisController(IVariavelMacroeconomicaService variaveisService, IBancoCentralService bancoCentralService) 
        {
            _variaveisService = variaveisService;
            _bancoCentralService = bancoCentralService;
        } 

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VariavelMacroeconomicaViewModel>>> GetAll()
        {
            var variaveis = await _variaveisService.GetAllAsync();
            return Ok(variaveis);
        }

        [HttpGet("{codigo}")]
        public async Task<ActionResult<VariavelMacroeconomicaViewModel>> GetByCodigo(string codigo)
        {
            var variaveis = await _variaveisService.GetByCodigoAsync(codigo);
            return Ok(variaveis);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<VariavelMacroeconomicaViewModel>> Update(int id, [FromBody] VariavelMacroeconomicaDto dto)
        {
            try
            {
                var variavel = await _variaveisService.UpdateAsync(id, dto);
                return Ok(variavel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<DadosMacroeconomicosDto>> AtualizarAutomaticamente()
        {
            var resultado = await _variaveisService.AtualizarVariaveisAutomaticamenteAsync();

            if (!resultado.Sucesso)
                return StatusCode(500, resultado);

            return Ok(resultado);
        }

        [HttpGet]
        public async Task<ActionResult<DadosMacroeconomicosDto>> GetDadosTempoReal()
        {
            var dados = await _bancoCentralService.GetDadosMacroeconomicosAsync();
            return Ok(dados);
        }

        [HttpGet]
        public async Task<ActionResult<decimal?>> GetSelicTempoReal()
        {
            var selic = await _bancoCentralService.GetSelicAsync();

            if (!selic.HasValue)
                return NotFound("Não foi possível obter a SELIC no momento");

            return Ok(new
            {
                valor = selic.Value,
                unidade = "% a.a.",
                fonte = "Banco Central do Brasil",
                dataConsulta = DateTime.Now
            });
        }

        [HttpGet]
        public async Task<ActionResult<decimal?>> GetIpcaTempoReal()
        {
            var ipca = await _bancoCentralService.GetIpcaAsync();

            if (!ipca.HasValue)
                return NotFound("Não foi possível obter o IPCA no momento");

            return Ok(new
            {
                valor = ipca.Value,
                unidade = "% mensal",
                fonte = "IBGE via Banco Central",
                dataConsulta = DateTime.Now
            });
        }

        [HttpGet]
        public async Task<ActionResult<decimal?>> GetCdiTempoReal()
        {
            var cdi = await _bancoCentralService.GetCdiAsync();

            if (!cdi.HasValue)
                return NotFound("Não foi possível obter o CDI no momento");

            return Ok(new
            {
                valor = cdi.Value,
                unidade = "% a.a.",
                fonte = "IBGE via Banco Central",
                dataConsulta = DateTime.Now
            });
        }

        [HttpGet]
        public async Task<ActionResult<decimal?>> GetDolarTempoReal()
        {
            var dolar = await _bancoCentralService.GetDolarAsync();

            if (!dolar.HasValue)
                return NotFound("Não foi possível obter o Dólar no momento");

            return Ok(new
            {
                valor = dolar.Value,
                unidade = "$USD",
                fonte = "IBGE via Banco Central",
                dataConsulta = DateTime.Now
            });
        }

        [HttpGet]
        public async Task<ActionResult> TestarConexaoApiBCB()
        {
            var sucesso = await _bancoCentralService.TestarConexaoAsync();

            return Ok(new
            {
                conectado = sucesso,
                mensagem = sucesso
                    ? "API externa está funcionando corretamente"
                    : "Falha ao conectar com API externa",
                dataTesteConexao = DateTime.Now
            });
        }

        [HttpGet]
        public async Task<ActionResult<RelatorioMacroeconomicoViewModel>> Relatorio()
        {
            var variaveis = await _variaveisService.GetRelatorioMacroeconomicoAsync();
            return Ok(variaveis);
        }
    }
}
