using ThetisModel.DTOs;
using ThetisModel.ViewModels;

namespace ThetisService.Interfaces
{
    public interface IClienteService
    {
        Task<IEnumerable<ClienteViewModel>> GetAllAsync();
        Task<ClienteViewModel> GetByIdAsync(int id);
        Task<ClienteViewModel> CreateAsync(ClienteDto clienteDto);
        Task<ClienteViewModel> UpdateAsync(int id, ClienteDto clienteDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsByCpfAsync(string cpf);
        Task<bool> ExistsByEmailAsync(string email);
        Task<ClientePerfilViewModel> GetPerfilInvestidorAsync(int clienteId);
    }
}
