
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

            // Config
            var conn = builder.Configuration.GetConnectionString("Oracle");

            // Serviços
            builder.Services.AddControllers();
            builder.Services.AddDbContextPool<AppDbContext>(opt => opt.UseOracle(conn));
            builder.Services.AddAutoMapper(cfg => { }, typeof(ThetisMappingProfile).Assembly);

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IClienteService, ClienteService>();
            builder.Services.AddScoped<IAtivoService, AtivoService>();
            builder.Services.AddScoped<IRecomendacaoService, RecomendacaoService>();
            builder.Services.AddScoped<IVariavelMacroeconomicaService, VariavelMacroeconomicaService>();
            builder.Services.AddSingleton<IFileStorage, FileStorage>();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new()
                {
                    Title = "Thetis API",
                    Version = "v1",
                    Description = "API para recomendação de investimentos"
                });
            });

            var app = builder.Build();

            // HTTPS
            app.UseHttpsRedirection();

            // Swagger na raiz sempre
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Assessor Virtual API v1");
                c.RoutePrefix = string.Empty;
            });

            app.MapControllers();
            app.Run();
        }
    }
}

