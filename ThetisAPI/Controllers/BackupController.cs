using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;                   
using System.Text;
using System.Text.Json;
using ThetisData.Context;
using ThetisModel.DTOs;
using ThetisModel.Entities;
using ThetisModel.Enums;
using ThetisService.Interfaces;

namespace ThetisApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class BackupController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        private readonly IFileStorage _store;

        public BackupController(AppDbContext ctx, IFileStorage store)
        {
            _ctx = ctx;
            _store = store;
        }

        [HttpGet]
        public async Task<IActionResult> ExportJson()
        {
            var payload = new
            {
                Data = DateTime.UtcNow,
                Clientes = await _ctx.Clientes.AsNoTracking().ToListAsync(),
                Ativos = await _ctx.Ativos.AsNoTracking().ToListAsync(),
                Variaveis = await _ctx.VariaveisMacroeconomicas.AsNoTracking().ToListAsync()
            };

            var json = JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions { WriteIndented = true });

            var fileName = $"export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
            await _store.SaveAsync(fileName, Encoding.UTF8.GetBytes(json));

            return File(Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }

        [HttpPost()]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> ImportJsonBody([FromBody] BackupPayload payload)
        {
            if (payload is null)
                return BadRequest("Body JSON ausente ou inválido.");

            // helpers de normalização
            static string OnlyDigits(string s) => new string((s ?? "").Where(char.IsDigit).ToArray());
            static string NormCodigo(string s) => (s ?? "").Trim().ToUpperInvariant();
            static string NormEmail(string s) => (s ?? "").Trim().ToLowerInvariant();

            var result = new
            {
                Created = new { Ativos = 0, Clientes = 0, Variaveis = 0 },
                Updated = new { Ativos = 0, Clientes = 0, Variaveis = 0 }
            };

            // Usaremos variáveis mutáveis pra contar
            int createdAtivos = 0, updatedAtivos = 0;
            int createdClientes = 0, updatedClientes = 0;
            int createdVars = 0, updatedVars = 0;

            using var tx = await _ctx.Database.BeginTransactionAsync();
            try
            {
                // 1) ATIVOS (upsert por Código)
                if (payload.Ativos?.Any() == true)
                {
                    foreach (var dto in payload.Ativos)
                    {
                        var codigo = NormCodigo(dto.Codigo);
                        if (string.IsNullOrWhiteSpace(codigo)) continue;

                        var entity = await _ctx.Ativos.FirstOrDefaultAsync(a => a.Codigo == codigo);
                        if (entity == null)
                        {
                            entity = new Ativo
                            {
                                Codigo = codigo,
                                Nome = dto.Nome.Trim(),
                                Descricao = dto.Descricao,
                                TipoAtivo = (TipoAtivo)dto.TipoAtivo,
                                RentabilidadeEsperada = dto.RentabilidadeEsperada,
                                NivelRisco = (PerfilRisco)dto.NivelRisco,
                                LiquidezDias = dto.LiquidezDias,
                                ValorMinimo = dto.ValorMinimo,
                                TaxaAdministracao = dto.TaxaAdministracao,
                                DataCriacao = DateTime.Now,
                                AtivoSistema = true
                            };
                            _ctx.Ativos.Add(entity);
                            createdAtivos++;
                        }
                        else
                        {
                            entity.Nome = dto.Nome.Trim();
                            entity.Descricao = dto.Descricao;
                            entity.TipoAtivo = (TipoAtivo)dto.TipoAtivo;
                            entity.RentabilidadeEsperada = dto.RentabilidadeEsperada;
                            entity.NivelRisco = (PerfilRisco)dto.NivelRisco;
                            entity.LiquidezDias = dto.LiquidezDias;
                            entity.ValorMinimo = dto.ValorMinimo;
                            entity.TaxaAdministracao = dto.TaxaAdministracao;
                            entity.AtivoSistema = true;
                            updatedAtivos++;
                        }
                    }
                    await _ctx.SaveChangesAsync();
                }

                // 2) CLIENTES (upsert por CPF OU Email)
                if (payload.Clientes?.Any() == true)
                {
                    foreach (var dto in payload.Clientes)
                    {
                        var cpf = OnlyDigits(dto.Cpf);
                        var email = NormEmail(dto.Email);

                        if (string.IsNullOrWhiteSpace(cpf) && string.IsNullOrWhiteSpace(email))
                            continue; // precisa de identificador

                        // tenta achar por CPF primeiro, senão por email
                        var entity = await _ctx.Clientes
                            .FirstOrDefaultAsync(c => (!string.IsNullOrEmpty(cpf) && c.Cpf == cpf)
                                                   || (!string.IsNullOrEmpty(email) && c.Email == email));

                        if (entity == null)
                        {
                            entity = new Cliente
                            {
                                Nome = dto.Nome.Trim(),
                                Email = email,
                                Cpf = cpf,
                                DataNascimento = dto.DataNascimento,
                                RendaMensal = dto.RendaMensal,
                                ValorDisponivel = dto.ValorDisponivel,
                                PerfilRisco = (PerfilRisco)dto.PerfilRisco,
                                ObjetivoPrincipal = (ObjetivoInvestimento)dto.ObjetivoPrincipal,
                                PrazoInvestimentoMeses = dto.PrazoInvestimentoMeses,
                                DataCadastro = DateTime.Now,
                                Ativo = true
                            };
                            _ctx.Clientes.Add(entity);
                            createdClientes++;
                        }
                        else
                        {
                            entity.Nome = dto.Nome.Trim();
                            if (!string.IsNullOrWhiteSpace(email)) entity.Email = email;
                            if (!string.IsNullOrWhiteSpace(cpf)) entity.Cpf = cpf;
                            entity.DataNascimento = dto.DataNascimento;
                            entity.RendaMensal = dto.RendaMensal;
                            entity.ValorDisponivel = dto.ValorDisponivel;
                            entity.PerfilRisco = (PerfilRisco)dto.PerfilRisco;
                            entity.ObjetivoPrincipal = (ObjetivoInvestimento)dto.ObjetivoPrincipal;
                            entity.PrazoInvestimentoMeses = dto.PrazoInvestimentoMeses;
                            entity.Ativo = true;
                            updatedClientes++;
                        }
                    }
                    await _ctx.SaveChangesAsync();
                }

                // 3) VARIÁVEIS MACRO (upsert por Código)
                if (payload.Variaveis?.Any() == true)
                {
                    foreach (var dto in payload.Variaveis)
                    {
                        var codigo = NormCodigo(dto.Codigo);
                        if (string.IsNullOrWhiteSpace(codigo)) continue;

                        var entity = await _ctx.VariaveisMacroeconomicas
                            .FirstOrDefaultAsync(v => v.Codigo == codigo);

                        if (entity == null)
                        {
                            entity = new VariavelMacroeconomica
                            {
                                Nome = dto.Nome.Trim(),
                                Codigo = codigo,
                                Descricao = dto.Descricao,
                                ValorAtual = dto.ValorAtual,
                                ValorAnterior = dto.ValorAnterior,
                                DataAtualizacao = DateTime.Now,
                                DataReferencia = dto.DataReferencia,
                                UnidadeMedida = dto.UnidadeMedida,
                                FonteDados = dto.FonteDados,
                                Tendencia = dto.Tendencia,
                                ImpactoInvestimentos = dto.ImpactoInvestimentos,
                                Ativa = true
                            };
                            _ctx.VariaveisMacroeconomicas.Add(entity);
                            createdVars++;
                        }
                        else
                        {
                            entity.Nome = dto.Nome.Trim();
                            entity.Descricao = dto.Descricao;
                            entity.ValorAnterior = dto.ValorAnterior ?? entity.ValorAtual;
                            entity.ValorAtual = dto.ValorAtual;
                            entity.DataAtualizacao = DateTime.Now;
                            entity.DataReferencia = dto.DataReferencia;
                            entity.UnidadeMedida = dto.UnidadeMedida;
                            entity.FonteDados = dto.FonteDados;
                            entity.Tendencia = dto.Tendencia;
                            entity.ImpactoInvestimentos = dto.ImpactoInvestimentos;
                            entity.Ativa = true;
                            updatedVars++;
                        }
                    }
                    await _ctx.SaveChangesAsync();
                }

                await tx.CommitAsync();

                return Ok(new
                {
                    Message = "Importação concluída.",
                    Criados = new { Ativos = createdAtivos, Clientes = createdClientes, Variaveis = createdVars },
                    Atualizados = new { Ativos = updatedAtivos, Clientes = updatedClientes, Variaveis = updatedVars }
                });
            }
            catch (DbUpdateException dbex)
            {
                await tx.RollbackAsync();
                return UnprocessableEntity($"Falha ao salvar no banco: {dbex.InnerException?.Message ?? dbex.Message}");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return BadRequest($"Erro na importação: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> RelatorioTxt()
        {
            var clientesAtivos = await _ctx.Clientes.CountAsync(c => c.Ativo);
            var ativosAtivos = await _ctx.Ativos.CountAsync(a => a.AtivoSistema);
            var carteiras = await _ctx.CarteirasRecomendadas.CountAsync();

            var txt = new StringBuilder()
                .AppendLine("Relatório Geral - Assessor Virtual")
                .AppendLine($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .AppendLine($"Clientes ativos: {clientesAtivos}")
                .AppendLine($"Ativos disponíveis: {ativosAtivos}")
                .AppendLine($"Carteiras recomendadas: {carteiras}")
                .ToString();

            var fileName = $"relatorio-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt";
            await _store.SaveAsync(fileName, Encoding.UTF8.GetBytes(txt));
            return File(Encoding.UTF8.GetBytes(txt), "text/plain", fileName);
        }
    }
}
