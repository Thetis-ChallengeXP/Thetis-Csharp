using Microsoft.AspNetCore.Mvc;
using ThetisModel.DTOs;
using ThetisModel.Enums;
using ThetisModel.ViewModels;
using ThetisService.Interfaces;

namespace ThetisApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AtivosController : ControllerBase
    {
        private readonly IAtivoService _ativoService;

        public AtivosController(IAtivoService ativoService)
        {
            _ativoService = ativoService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AtivoViewModel>), 200)]
        public async Task<ActionResult<IEnumerable<AtivoViewModel>>> GetAll()
        {
            var ativos = await _ativoService.GetAllAsync();
            return Ok(ativos);
        }

        [HttpGet("tipo/{tipo}")]
        [ProducesResponseType(typeof(IEnumerable<AtivoViewModel>), 200)]
        public async Task<ActionResult<IEnumerable<AtivoViewModel>>> GetByTipo(TipoAtivo tipo)
        {
            var ativos = await _ativoService.GetByTipoAsync(tipo);
            return Ok(ativos);
        }

        [HttpGet("recomendados")]
        [ProducesResponseType(typeof(IEnumerable<AtivoViewModel>), 200)]
        public async Task<ActionResult<IEnumerable<AtivoViewModel>>> GetRecomendados(
            [FromQuery] PerfilRisco perfil, [FromQuery] decimal valorDisponivel)
        {
            var ativos = await _ativoService.GetRecomendadosParaPerfilAsync(perfil, valorDisponivel);
            return Ok(ativos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AtivoViewModel), 200)]
        [ProducesResponseType(404)]
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

        [HttpPost]
        [ProducesResponseType(typeof(AtivoViewModel), 201)]
        [ProducesResponseType(400)]
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
    }
}
