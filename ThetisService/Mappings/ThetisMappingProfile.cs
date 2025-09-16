using AutoMapper;
using ThetisModel.DTOs;
using ThetisModel.Entities;
using ThetisModel.Enums;
using ThetisModel.ViewModels;

namespace ThetisService.Mappings
{
    public class ThetisMappingProfile : Profile
    {
        public ThetisMappingProfile()
        {
            // Cliente Mappings
            CreateMap<ClienteDto, Cliente>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DataCadastro, opt => opt.Ignore())
                .ForMember(dest => dest.Ativo, opt => opt.Ignore())
                .ForMember(dest => dest.CarteirasRecomendadas, opt => opt.Ignore())
                .ForMember(dest => dest.HistoricoInvestimentos, opt => opt.Ignore());

            CreateMap<Cliente, ClienteViewModel>()
                .ForMember(dest => dest.QuantidadeCarteiras, opt => opt.MapFrom(src =>
                    src.CarteirasRecomendadas != null ? src.CarteirasRecomendadas.Count : 0))
                .ForMember(dest => dest.TotalInvestido, opt => opt.MapFrom(src =>
                    src.CarteirasRecomendadas != null
                        ? src.CarteirasRecomendadas.Where(c => c.AprovadaCliente == true).Sum(c => c.ValorTotal)
                        : 0));

            // Ativo Mappings
            CreateMap<AtivoDto, Ativo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
                .ForMember(dest => dest.isAtivo, opt => opt.Ignore())
                .ForMember(dest => dest.ItensCarteira, opt => opt.Ignore())
                .ForMember(dest => dest.VariaveisMacroeconomicas, opt => opt.Ignore());

            CreateMap<Ativo, AtivoViewModel>()
                .ForMember(dest => dest.VariaveisInfluencia, opt => opt.MapFrom(src =>
                    src.VariaveisMacroeconomicas != null
                        ? src.VariaveisMacroeconomicas.Select(v => v.Nome).ToList()
                        : new List<string>()));

            // Carteira Mappings
            CreateMap<CarteiraRecomendada, CarteiraRecomendadaViewModel>()
                .ForMember(dest => dest.NomeCliente, opt => opt.MapFrom(src =>
                    src.Cliente != null ? src.Cliente.Nome : string.Empty))
                .ForMember(dest => dest.NivelRisco, opt => opt.MapFrom(src => src.NivelRisco.ToString()))
                .ForMember(dest => dest.Objetivo, opt => opt.MapFrom(src => src.Objetivo.ToString()))
                .ForMember(dest => dest.Itens, opt => opt.MapFrom(src => src.Itens));

            CreateMap<CarteiraRecomendadaViewModel, CarteiraRecomendada>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Cliente, opt => opt.Ignore())
                .ForMember(dest => dest.Itens, opt => opt.Ignore())
                .ForMember(dest => dest.NivelRisco, opt => opt.MapFrom(src =>
                    Enum.Parse<PerfilRisco>(src.NivelRisco)))
                .ForMember(dest => dest.Objetivo, opt => opt.MapFrom(src =>
                    Enum.Parse<ObjetivoInvestimento>(src.Objetivo)));

            // ItemCarteira Mappings
            CreateMap<ItemCarteira, ItemCarteiraViewModel>()
                .ForMember(dest => dest.AtivoId, opt => opt.MapFrom(src => src.AtivoId))
                .ForMember(dest => dest.NomeAtivo, opt => opt.MapFrom(src =>
                    src.Ativo != null ? src.Ativo.Nome : string.Empty))
                .ForMember(dest => dest.TipoAtivo, opt => opt.MapFrom(src =>
                    src.Ativo != null ? src.Ativo.TipoAtivo.ToString() : string.Empty));

            CreateMap<ItemCarteiraViewModel, ItemCarteira>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CarteiraRecomendada, opt => opt.Ignore())
                .ForMember(dest => dest.Ativo, opt => opt.Ignore())
                .ForMember(dest => dest.CarteiraRecomendadaId, opt => opt.Ignore());

            // VariavelMacroeconomica Mappings
            CreateMap<VariavelMacroeconomica, VariavelMacroeconomicaViewModel>();

            CreateMap<VariavelMacroeconomicaDto, VariavelMacroeconomica>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Nome, opt => opt.Ignore())
                .ForMember(dest => dest.Codigo, opt => opt.Ignore())
                .ForMember(dest => dest.Descricao, opt => opt.Ignore())
                .ForMember(dest => dest.UnidadeMedida, opt => opt.Ignore())
                .ForMember(dest => dest.FonteDados, opt => opt.Ignore())
                .ForMember(dest => dest.ImpactoInvestimentos, opt => opt.Ignore())
                .ForMember(dest => dest.Ativa, opt => opt.Ignore())
                .ForMember(dest => dest.AtivosAfetados, opt => opt.Ignore());
        }
    }
}
