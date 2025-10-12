using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThetisModel.DTOs;
using ThetisModel.ViewModels;

namespace ThetisService.Interfaces
{
    public interface IGeminiService
    {
        Task<AnaliseIADto> AskAsync(string pergunta);
        Task<AnaliseCarteiraIADto> AnalisarCarteiraAsync(CarteiraRecomendadaViewModel carteira);
        Task<ExplicacaoPersonalizadaDto> ExplicarConceitoAsync(string conceito, string nivelConhecimento = "basico");
        Task<bool> TestarConexaoAsync();
    }
}
