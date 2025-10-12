using ThetisModel.DTOs;
using ThetisModel.ViewModels;

namespace ThetisService.Interfaces
{
    public interface IVariavelMacroeconomicaService
    {
        Task<IEnumerable<VariavelMacroeconomicaViewModel>> GetAllAsync();
        Task<VariavelMacroeconomicaViewModel> GetByCodigoAsync(string codigo);
        Task<VariavelMacroeconomicaViewModel> UpdateAsync(int id, VariavelMacroeconomicaDto dto);
        Task<RelatorioMacroeconomicoViewModel> GetRelatorioMacroeconomicoAsync();
        Task<DadosMacroeconomicosDto> AtualizarVariaveisAutomaticamenteAsync();
    }
}
