using Microsoft.AspNetCore.Mvc;
using ThetisModel.DTOs;
using ThetisModel.ViewModels;
using ThetisService.Interfaces;

namespace ThetisApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RecomendacoesController : ControllerBase
    {
        private readonly IRecomendacaoService _recomendacaoService;

        public RecomendacoesController(IRecomendacaoService recomendacaoService)
        {
            _recomendacaoService = recomendacaoService;
        }

        [HttpPost("gerar")]
        [ProducesResponseType(typeof(CarteiraRecomendadaViewModel), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<CarteiraRecomendadaViewModel>> GerarRecomendacao(
            [FromBody] SolicitacaoRecomendacaoDto solicitacao)
        {
            try
            {
                var carteira = await _recomendacaoService.GerarRecomendacaoAsync(solicitacao);
                return Ok(carteira);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao gerar recomendação: {ex.Message}");
            }
        }

        [HttpGet("cliente/{clienteId}")]
        [ProducesResponseType(typeof(IEnumerable<CarteiraRecomendadaViewModel>), 200)]
        public async Task<ActionResult<IEnumerable<CarteiraRecomendadaViewModel>>> GetByCliente(int clienteId)
        {
            var carteiras = await _recomendacaoService.GetRecomendacoesByClienteAsync(clienteId);
            return Ok(carteiras);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CarteiraRecomendadaViewModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CarteiraRecomendadaViewModel>> GetById(int id)
        {
            try
            {
                var carteira = await _recomendacaoService.GetRecomendacaoByIdAsync(id);
                return Ok(carteira);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Carteira com ID {id} não encontrada");
            }
        }

        [HttpPatch("{id}/aprovar")]
        [ProducesResponseType(typeof(CarteiraRecomendadaViewModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CarteiraRecomendadaViewModel>> AprovarCarteira(int id, [FromBody] bool aprovada)
        {
            try
            {
                var carteira = await _recomendacaoService.AprovarCarteiraAsync(id, aprovada);
                return Ok(carteira);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Carteira com ID {id} não encontrada");
            }
        }

        [HttpGet("{id}/diversificacao")]
        [ProducesResponseType(typeof(RelatorioDiversificacaoViewModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RelatorioDiversificacaoViewModel>> AnalisarDiversificacao(int id)
        {
            try
            {
                var relatorio = await _recomendacaoService.AnalisarDiversificacaoAsync(id);
                return Ok(relatorio);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Carteira com ID {id} não encontrada");
            }
        }

        [HttpGet("{id}/simulacao")]
        [ProducesResponseType(typeof(SimulacaoRendimentoViewModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SimulacaoRendimentoViewModel>> SimularRendimento(int id, [FromQuery] int meses = 12)
        {
            try
            {
                var simulacao = await _recomendacaoService.SimularRendimentoAsync(id, meses);
                return Ok(simulacao);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Carteira com ID {id} não encontrada");
            }
        }
    }
}
