using AutoMapper;
using ThetisData.Context;
using ThetisData.Repositories;
using ThetisModel.Entities;
using ThetisModel.DTOs;
using ThetisModel.ViewModels;
using ThetisService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ThetisService.Implementations
{
    public class ClienteService : IClienteService
    {
        private readonly IRepository<Cliente> _clienteRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public ClienteService(IRepository<Cliente> clienteRepository, IMapper mapper, AppDbContext context)
        {
            _clienteRepository = clienteRepository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<ClienteViewModel>> GetAllAsync()
        {
            var clientes = await _context.Clientes
                .Where(c => c.Ativo)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClienteViewModel>>(clientes);
        }

        public async Task<ClienteViewModel> GetByIdAsync(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.CarteirasRecomendadas)
                .Include(c => c.HistoricoInvestimentos)
                .FirstOrDefaultAsync(c => c.Id == id && c.Ativo);

            if (cliente == null)
                throw new KeyNotFoundException($"Cliente com ID {id} não encontrado");

            return _mapper.Map<ClienteViewModel>(cliente);
        }

        public async Task<ClienteViewModel> CreateAsync(ClienteDto clienteDto)
        {
            // Validações de negócio
            if (await ExistsByCpfAsync(clienteDto.Cpf))
                throw new InvalidOperationException("CPF já cadastrado no sistema");

            if (await ExistsByEmailAsync(clienteDto.Email))
                throw new InvalidOperationException("Email já cadastrado no sistema");

            var cliente = _mapper.Map<Cliente>(clienteDto);
            cliente.DataCadastro = DateTime.Now;
            cliente.Ativo = true;

            var clienteCriado = await _clienteRepository.AddAsync(cliente);
            return _mapper.Map<ClienteViewModel>(clienteCriado);
        }

        public async Task<ClienteViewModel> UpdateAsync(int id, ClienteDto clienteDto)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null || !cliente.Ativo)
                throw new KeyNotFoundException($"Cliente com ID {id} não encontrado");

            // Verificar CPF duplicado (exceto o próprio cliente)
            var existeCpf = await _context.Clientes
                .AnyAsync(c => c.Cpf == clienteDto.Cpf && c.Id != id && c.Ativo);
            if (existeCpf)
                throw new InvalidOperationException("CPF já cadastrado por outro cliente");

            // Verificar Email duplicado (exceto o próprio cliente)
            var existeEmail = await _context.Clientes
                .AnyAsync(c => c.Email == clienteDto.Email && c.Id != id && c.Ativo);
            if (existeEmail)
                throw new InvalidOperationException("Email já cadastrado por outro cliente");

            _mapper.Map(clienteDto, cliente);
            var clienteAtualizado = await _clienteRepository.UpdateAsync(cliente);
            return _mapper.Map<ClienteViewModel>(clienteAtualizado);
        }

        public async Task DeleteAsync(int id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
                throw new KeyNotFoundException($"Cliente com ID {id} não encontrado");

            // Soft delete
            cliente.Ativo = false;
            await _clienteRepository.UpdateAsync(cliente);
        }

        public async Task<bool> ExistsByCpfAsync(string cpf)
        {
            return await _context.Clientes
                .AnyAsync(c => c.Cpf == cpf && c.Ativo);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Clientes
                .AnyAsync(c => c.Email == email && c.Ativo);
        }

        public async Task<ClientePerfilViewModel> GetPerfilInvestidorAsync(int clienteId)
        {
            var cliente = await GetByIdAsync(clienteId);

            return new ClientePerfilViewModel
            {
                ClienteId = cliente.Id,
                Nome = cliente.Nome,
                PerfilRisco = cliente.PerfilRisco,
                ObjetivoPrincipal = cliente.ObjetivoPrincipal,
                PrazoInvestimentoMeses = cliente.PrazoInvestimentoMeses,
                ValorDisponivel = cliente.ValorDisponivel,
                RendaMensal = cliente.RendaMensal,
                Idade = CalcularIdade(cliente.DataNascimento),
                CapacidadeInvestimento = CalcularCapacidadeInvestimento(cliente.RendaMensal, cliente.ValorDisponivel)
            };
        }

        private int CalcularIdade(DateTime dataNascimento)
        {
            var idade = DateTime.Now.Year - dataNascimento.Year;
            if (DateTime.Now.DayOfYear < dataNascimento.DayOfYear)
                idade--;
            return idade;
        }

        private decimal CalcularCapacidadeInvestimento(decimal rendaMensal, decimal valorDisponivel)
        {
            // Lógica: 10% da renda mensal para investimento regular + valor disponível
            return (rendaMensal * 0.1m) + valorDisponivel;
        }
    }
}
