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
        public VariaveisController(IVariavelMacroeconomicaService variaveisService) 
        {
            _variaveisService = variaveisService;
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
        public async Task<IActionResult> AtualizarAutomaticamente()
        {
            await _variaveisService.AtualizarVariaveisAutomaticamenteAsync();
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<RelatorioMacroeconomicoViewModel>> Relatorio()
        {
            var variaveis = await _variaveisService.GetRelatorioMacroeconomicoAsync();
            return Ok(variaveis);
        }
    }
}
