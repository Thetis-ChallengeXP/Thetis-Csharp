using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ThetisModel.DTOs;
using ThetisService.Interfaces;

namespace ThetisService.Implementations
{
    public class BancoCentralService : IBancoCentralService
    {
        private readonly HttpClient _httpClient;
        private const string BCB_BASE_URL = "https://api.bcb.gov.br/dados/serie/bcdata.sgs";

        public BancoCentralService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<decimal?> BuscarSerieAsync(int codigoSerie, string nomeSerie)
        {
            try
            {
                var url = $"{BCB_BASE_URL}.{codigoSerie}/dados/ultimos/1?formato=json";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var dados = JsonSerializer.Deserialize<List<BancoCentralSerieDto>>(json);

                if (dados == null || !dados.Any())
                {
                    return null;
                }

                var ultimoDado = dados.First();
                var valor = decimal.Parse(
                    ultimoDado.Valor.Replace(",", "."),
                    CultureInfo.InvariantCulture
                );

                return valor;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DadosMacroeconomicosDto> GetDadosMacroeconomicosAsync()
        {
            var resultado = new DadosMacroeconomicosDto
            {
                DataAtualizacao = DateTime.Now,
                Sucesso = true
            };

            try
            {
                // Buscar dados em paralelo para maior performance
                var tarefas = new[]
                {
                    BuscarSerieAsync(SeriesBancoCentral.SELIC, "SELIC"),
                    BuscarSerieAsync(SeriesBancoCentral.IPCA, "IPCA"),
                    BuscarSerieAsync(SeriesBancoCentral.CDI, "CDI"),
                    BuscarSerieAsync(SeriesBancoCentral.DOLAR_PTAX, "Dólar PTAX")
                };

                var resultados = await Task.WhenAll(tarefas);

                resultado.Selic = resultados[0] ?? 0;
                resultado.Ipca = resultados[1] ?? 0;
                resultado.Cdi = resultados[2] ?? 0;
                resultado.Dolar = resultados[3] ?? 0;

                // Validar se conseguimos obter pelo menos alguns dados
                var dadosObtidos = resultados.Count(r => r.HasValue);

                if (dadosObtidos == 0)
                {
                    resultado.Sucesso = false;
                    resultado.MensagemErro = "Não foi possível obter nenhum dado das APIs externas";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                resultado.Sucesso = false;
                resultado.MensagemErro = $"Erro: {ex.Message}";
                return resultado;
            }
        }

        public async Task<decimal?> GetSelicAsync()
        {
            return await BuscarSerieAsync(SeriesBancoCentral.SELIC, "SELIC");
        }

        public async Task<decimal?> GetIpcaAsync()
        {
            return await BuscarSerieAsync(SeriesBancoCentral.IPCA, "IPCA");
        }

        public async Task<decimal?> GetCdiAsync()
        {
            return await BuscarSerieAsync(SeriesBancoCentral.CDI, "CDI");
        }

        public async Task<decimal?> GetDolarAsync()
        {
            return await BuscarSerieAsync(SeriesBancoCentral.DOLAR_PTAX, "Dólar PTAX");
        }

        public async Task<bool> TestarConexaoAsync()
        {
            try
            {
                var selic = await GetSelicAsync();
                var resultado = selic.HasValue;

                return resultado;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
