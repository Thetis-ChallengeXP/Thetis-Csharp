
using Microsoft.EntityFrameworkCore;
using ThetisData.Context;
using ThetisData.Repositories;
using ThetisService.Implementations;
using ThetisService.Interfaces;
using ThetisService.Mappings;

namespace InvestmentAdvisorAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== SERVIÇOS BÁSICOS =====

            // Controllers
            builder.Services.AddControllers();

            // Entity Framework Oracle
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseOracle("User Id=RM98680;Password=271204;Data Source=oracle.fiap.com.br:1521/ORCL;"));

            // AutoMapper - Configuração simples
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ThetisMappingProfile>();
            });

            // Repositories
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Services
            builder.Services.AddScoped<IClienteService, ClienteService>();
            builder.Services.AddScoped<IAtivoService, AtivoService>();
            builder.Services.AddScoped<IRecomendacaoService, RecomendacaoService>();
            builder.Services.AddScoped<IVariavelMacroeconomicaService, VariavelMacroeconomicaService>();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new()
                {
                    Title = "Assessor Virtual API",
                    Version = "v1",
                    Description = "API para recomendação de investimentos"
                });
            });

            var app = builder.Build();

            // ===== PIPELINE =====

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Assessor Virtual API");
                    c.RoutePrefix = string.Empty; // Swagger na raiz
                });
            }

            app.UseHttpsRedirection();
            app.MapControllers();

            // Endpoint de teste
            app.MapGet("/api", () => new
            {
                Message = "Assessor Virtual de Investimentos API",
                Version = "1.0.0",
                Status = "Running"
            });

            app.Run();
        }
    }
}
