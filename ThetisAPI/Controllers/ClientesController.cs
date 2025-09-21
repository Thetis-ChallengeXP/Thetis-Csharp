using Microsoft.AspNetCore.Mvc;
using ThetisModel.DTOs;
using ThetisModel.ViewModels;
using ThetisService.Interfaces;

namespace ThetisApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Produces("application/json")]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteViewModel>>> GetAll()
        {
            var clientes = await _clienteService.GetAllAsync();
            return Ok(clientes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteViewModel>> GetById(int id)
        {
            try
            {
                var cliente = await _clienteService.GetByIdAsync(id);
                return Ok(cliente);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Cliente com ID {id} não encontrado");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientePerfilViewModel>> GetPerfil(int id)
        {
            try
            {
                var perfil = await _clienteService.GetPerfilInvestidorAsync(id);
                return Ok(perfil);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Cliente com ID {id} não encontrado");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ClienteViewModel>> Create([FromBody] ClienteDto clienteDto)
        {
            try
            {
                var cliente = await _clienteService.CreateAsync(clienteDto);
                return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ClienteViewModel>> Update(int id, [FromBody] ClienteDto clienteDto)
        {
            try
            {
                var cliente = await _clienteService.UpdateAsync(id, clienteDto);
                return Ok(cliente);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Cliente com ID {id} não encontrado");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _clienteService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Cliente com ID {id} não encontrado");
            }
        }
    }
}
