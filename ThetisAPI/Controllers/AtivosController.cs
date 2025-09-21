using Microsoft.AspNetCore.Mvc;
using ThetisModel.DTOs;
using ThetisModel.Enums;
using ThetisModel.ViewModels;
using ThetisService.Interfaces;

namespace ThetisApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Produces("application/json")]
    public class AtivosController : ControllerBase
    {
        private readonly IAtivoService _ativoService;

        public AtivosController(IAtivoService ativoService)
        {
            _ativoService = ativoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AtivoViewModel>>> GetAll()
        {
            var ativos = await _ativoService.GetAllAsync();
            return Ok(ativos);
        }

        [HttpGet("{tipo}")]
        public async Task<ActionResult<IEnumerable<AtivoViewModel>>> GetByTipo(TipoAtivo tipo)
        {
            var ativos = await _ativoService.GetByTipoAsync(tipo);
            return Ok(ativos);
        }

        [HttpGet("{codigo}")]
        public async Task<ActionResult<AtivoViewModel>> GetByCodigo(string codigo)
        {
            var ativos = await _ativoService.GetByCodigoAsync(codigo);
            return Ok(ativos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AtivoViewModel>> GetById(int id)
        {
            try
            {
                var ativo = await _ativoService.GetByIdAsync(id);
                return Ok(ativo);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Ativo com ID {id} não encontrado");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AtivoViewModel>>> GetRecomendados(
            [FromQuery] PerfilRisco perfil, [FromQuery] decimal valorDisponivel)
        {
            var ativos = await _ativoService.GetRecomendadosParaPerfilAsync(perfil, valorDisponivel);
            return Ok(ativos);
        }

        [HttpPost]
        public async Task<ActionResult<AtivoViewModel>> Create([FromBody] AtivoDto ativoDto)
        {
            try
            {
                var ativo = await _ativoService.CreateAsync(ativoDto);
                return CreatedAtAction(nameof(GetById), new { id = ativo.Id }, ativo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AtivoViewModel>> Update(int id, [FromBody] AtivoDto dto)
        {
            try
            {
                var ativo = await _ativoService.UpdateAsync(id, dto);
                return Ok(ativo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _ativoService.DeleteAsync(id);
            return NoContent();
        }
    }
}
