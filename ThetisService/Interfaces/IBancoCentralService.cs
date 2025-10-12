using ThetisModel.DTOs;

namespace ThetisService.Interfaces
{
    public interface IBancoCentralService
    {
        Task<DadosMacroeconomicosDto> GetDadosMacroeconomicosAsync();
        Task<decimal?> GetSelicAsync();
        Task<decimal?> GetIpcaAsync();
        Task<decimal?> GetCdiAsync();
        Task<decimal?> GetDolarAsync();
        Task<bool> TestarConexaoAsync();
    }
}
