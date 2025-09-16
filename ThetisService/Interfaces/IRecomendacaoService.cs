using ThetisModel.DTOs;
using ThetisModel.ViewModels;

namespace ThetisService.Interfaces
{
    public interface IRecomendacaoService
    {
        Task<CarteiraRecomendadaViewModel> GerarRecomendacaoAsync(SolicitacaoRecomendacaoDto solicitacao);
        Task<IEnumerable<CarteiraRecomendadaViewModel>> GetRecomendacoesByClienteAsync(int clienteId);
        Task<CarteiraRecomendadaViewModel> GetRecomendacaoByIdAsync(int carteiraId);
        Task<CarteiraRecomendadaViewModel> AprovarCarteiraAsync(int carteiraId, bool aprovada);
        Task<RelatorioDiversificacaoViewModel> AnalisarDiversificacaoAsync(int carteiraId);
        Task<SimulacaoRendimentoViewModel> SimularRendimentoAsync(int carteiraId, int meses);
    }
}
