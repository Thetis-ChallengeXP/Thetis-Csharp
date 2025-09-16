using ThetisModel.DTOs;
using ThetisModel.Enums;
using ThetisModel.ViewModels;

namespace ThetisService.Interfaces
{
    public interface IAtivoService
    {
        Task<IEnumerable<AtivoViewModel>> GetAllAsync();
        Task<IEnumerable<AtivoViewModel>> GetByTipoAsync(TipoAtivo tipo);
        Task<IEnumerable<AtivoViewModel>> GetByPerfilRiscoAsync(PerfilRisco perfil);
        Task<AtivoViewModel> GetByIdAsync(int id);
        Task<AtivoViewModel> GetByCodigoAsync(string codigo);
        Task<AtivoViewModel> CreateAsync(AtivoDto ativoDto);
        Task<AtivoViewModel> UpdateAsync(int id, AtivoDto ativoDto);
        Task DeleteAsync(int id);
        Task<IEnumerable<AtivoViewModel>> GetRecomendadosParaPerfilAsync(PerfilRisco perfil, decimal valorDisponivel);
    }
}
