using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ThetisData.Context;
using ThetisData.Repositories;
using ThetisModel.DTOs;
using ThetisModel.Entities;
using ThetisModel.Enums;
using ThetisModel.ViewModels;
using ThetisService.Interfaces;

namespace ThetisService.Implementations
{
    public class AtivoService : IAtivoService
    {
        private readonly IRepository<Ativo> _ativoRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public AtivoService(IRepository<Ativo> ativoRepository, IMapper mapper, AppDbContext context)
        {
            _ativoRepository = ativoRepository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<AtivoViewModel>> GetAllAsync()
        {
            var ativos = await _context.Ativos
                .Where(a => a.AtivoSistema)
                .OrderBy(a => a.TipoAtivo)
                .ThenBy(a => a.Nome)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AtivoViewModel>>(ativos);
        }

        public async Task<IEnumerable<AtivoViewModel>> GetByTipoAsync(TipoAtivo tipo)
        {
            var ativos = await _context.Ativos
                .Where(a => a.TipoAtivo == tipo && a.AtivoSistema)
                .OrderBy(a => a.Nome)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AtivoViewModel>>(ativos);
        }

        public async Task<IEnumerable<AtivoViewModel>> GetByPerfilRiscoAsync(PerfilRisco perfil)
        {
            var ativos = await _context.Ativos
                .Where(a => a.NivelRisco == perfil && a.AtivoSistema)
                .OrderBy(a => a.TipoAtivo)
                .ThenBy(a => a.Nome)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AtivoViewModel>>(ativos);
        }

        public async Task<AtivoViewModel> GetByIdAsync(int id)
        {
            var ativo = await _context.Ativos
                .Include(a => a.VariaveisMacroeconomicas)
                .FirstOrDefaultAsync(a => a.Id == id && a.AtivoSistema);

            if (ativo == null)
                throw new KeyNotFoundException($"Ativo com ID {id} não encontrado");

            return _mapper.Map<AtivoViewModel>(ativo);
        }

        public async Task<AtivoViewModel> GetByCodigoAsync(string codigo)
        {
            var code = codigo.Trim().ToUpperInvariant();

            var ativo = await _context.Ativos
                .AsNoTracking()
                .Include(a => a.VariaveisMacroeconomicas)
                .FirstOrDefaultAsync(a => a.AtivoSistema && a.Codigo.ToUpper() == code);

            if (ativo == null && code.Length >= 2) 
            {
                ativo = await _context.Ativos
                    .AsNoTracking()
                    .Include(a => a.VariaveisMacroeconomicas)
                    .Where(a => a.AtivoSistema && a.Codigo.ToUpper().Contains(code))
                    .OrderBy(a => a.Codigo)
                    .FirstOrDefaultAsync();
            }

            if (ativo == null)
                throw new KeyNotFoundException($"Ativo com código {codigo} não encontrado");

            return _mapper.Map<AtivoViewModel>(ativo);
        }

        public async Task<AtivoViewModel> CreateAsync(AtivoDto ativoDto)
        {
            // Validar código único
            var idExistente = await _context.Ativos
               .Where(a => a.Codigo == ativoDto.Codigo && a.AtivoSistema)
               .Select(a => (int?)a.Id)
               .FirstOrDefaultAsync();

            if (idExistente.HasValue)
                throw new InvalidOperationException($"Código {ativoDto.Codigo} já existe");

            var ativo = _mapper.Map<Ativo>(ativoDto);
            ativo.DataCriacao = DateTime.Now;
            ativo.AtivoSistema = true;

            var ativoCriado = await _ativoRepository.AddAsync(ativo);
            return _mapper.Map<AtivoViewModel>(ativoCriado);
        }

        public async Task<AtivoViewModel> UpdateAsync(int id, AtivoDto ativoDto)
        {
            var ativo = await _ativoRepository.GetByIdAsync(id);
            if (ativo == null || !ativo.AtivoSistema)
                throw new KeyNotFoundException($"Ativo com ID {id} não encontrado");

            // Verificar código duplicado (exceto o próprio ativo)
            var existeCodigo = await _context.Ativos
               .Where(a => a.Codigo == ativoDto.Codigo && a.AtivoSistema == true)
               .Select(a => (int?)a.Id)
               .FirstOrDefaultAsync();

            if (existeCodigo.HasValue)
                throw new InvalidOperationException($"Código {ativoDto.Codigo} já existe");

            _mapper.Map(ativoDto, ativo);
            var ativoAtualizado = await _ativoRepository.UpdateAsync(ativo);
            return _mapper.Map<AtivoViewModel>(ativoAtualizado);
        }

        public async Task DeleteAsync(int id)
        {
            var ativo = await _ativoRepository.GetByIdAsync(id);
            if (ativo == null)
                throw new KeyNotFoundException($"Ativo com ID {id} não encontrado");

            // Soft delete
            ativo.AtivoSistema = false;
            await _ativoRepository.UpdateAsync(ativo);
        }

        public async Task<IEnumerable<AtivoViewModel>> GetRecomendadosParaPerfilAsync(PerfilRisco perfil, decimal valorDisponivel)
        {
            var query = _context.Ativos.Where(a => a.AtivoSistema);

            // Filtrar por nível de risco compatível
            switch (perfil)
            {
                case PerfilRisco.Conservador:
                    query = query.Where(a => a.NivelRisco == PerfilRisco.Conservador);
                    break;
                case PerfilRisco.Moderado:
                    query = query.Where(a => a.NivelRisco == PerfilRisco.Conservador ||
                                           a.NivelRisco == PerfilRisco.Moderado);
                    break;
                case PerfilRisco.Agressivo:
                    // Agressivo pode investir em qualquer nível
                    break;
            }

            // Filtrar por valor mínimo
            query = query.Where(a => a.ValorMinimo <= valorDisponivel);

            var ativos = await query
                .OrderBy(a => a.NivelRisco)
                .ThenByDescending(a => a.RentabilidadeEsperada)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AtivoViewModel>>(ativos);
        }
    }
}
