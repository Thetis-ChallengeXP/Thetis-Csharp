using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ThetisModel.Entities;

namespace ThetisData.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Ativo> Ativos { get; set; }
        public DbSet<CarteiraRecomendada> CarteirasRecomendadas { get; set; }
        public DbSet<ItemCarteira> ItensCarteira { get; set; }
        public DbSet<HistoricoInvestimento> HistoricoInvestimentos { get; set; }
        public DbSet<VariavelMacroeconomica> VariaveisMacroeconomicas { get; set; }
        public DbSet<LogRecomendacao> LogsRecomendacao { get; set; }
        public DbSet<AvaliacaoCarteira> AvaliacoesCarteira { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Converters Y/N
            var toYN = new BoolToStringConverter("N", "Y");
            var toYNnullable = new ValueConverter<bool?, string?>(
                v => v == null ? null : (v.Value ? "Y" : "N"),
                v => v == null ? (bool?)null : v == "Y"
            );

            foreach (var et in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var p in et.GetProperties())
                {
                    if (p.ClrType == typeof(bool))
                    {
                        p.SetValueConverter(toYN);
                        p.SetColumnType("CHAR(1)");
                        p.SetMaxLength(1);
                    }
                    else if (p.ClrType == typeof(bool?))
                    {
                        p.SetValueConverter(toYNnullable);
                        p.SetColumnType("CHAR(1)");
                        p.SetMaxLength(1);
                    }
                }
            }

            // Configurações de índices únicos
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Cpf)
                .IsUnique()
                .HasDatabaseName("IX_CLIENTE_CPF");

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("IX_CLIENTE_EMAIL");

            modelBuilder.Entity<Ativo>()
                .HasIndex(a => a.Codigo)
                .IsUnique()
                .HasDatabaseName("IX_ATIVO_CODIGO");

            modelBuilder.Entity<VariavelMacroeconomica>()
                .HasIndex(v => v.Codigo)
                .IsUnique()
                .HasDatabaseName("IX_VARIAVEL_CODIGO");

            // Configurações de relacionamentos
            modelBuilder.Entity<CarteiraRecomendada>()
                .HasOne(c => c.Cliente)
                .WithMany(cl => cl.CarteirasRecomendadas)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CARTEIRA_CLIENTE");

            modelBuilder.Entity<ItemCarteira>()
                .HasOne(i => i.CarteiraRecomendada)
                .WithMany(c => c.Itens)
                .HasForeignKey(i => i.CarteiraRecomendadaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ITEM_CARTEIRA");

            modelBuilder.Entity<ItemCarteira>()
                .HasOne(i => i.Ativo)
                .WithMany(a => a.ItensCarteira)
                .HasForeignKey(i => i.AtivoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ITEM_ATIVO");

            modelBuilder.Entity<HistoricoInvestimento>()
                .HasOne(h => h.Cliente)
                .WithMany(c => c.HistoricoInvestimentos)
                .HasForeignKey(h => h.ClienteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_HISTORICO_CLIENTE");

            modelBuilder.Entity<HistoricoInvestimento>()
                .HasOne(h => h.Ativo)
                .WithMany()
                .HasForeignKey(h => h.AtivoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_HISTORICO_ATIVO");

            modelBuilder.Entity<HistoricoInvestimento>()
                .HasOne(h => h.CarteiraRecomendada)
                .WithMany()
                .HasForeignKey(h => h.CarteiraRecomendadaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_HISTORICO_CARTEIRA");

            modelBuilder.Entity<LogRecomendacao>()
                .HasOne(l => l.Cliente)
                .WithMany()
                .HasForeignKey(l => l.ClienteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_LOG_CLIENTE");

            modelBuilder.Entity<LogRecomendacao>()
                .HasOne(l => l.CarteiraRecomendada)
                .WithMany()
                .HasForeignKey(l => l.CarteiraRecomendadaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_LOG_CARTEIRA");

            modelBuilder.Entity<AvaliacaoCarteira>()
                .HasOne(a => a.CarteiraRecomendada)
                .WithMany()
                .HasForeignKey(a => a.CarteiraRecomendadaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AVALIACAO_CARTEIRA");

            modelBuilder.Entity<AvaliacaoCarteira>()
                .HasOne(a => a.Cliente)
                .WithMany()
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AVALIACAO_CLIENTE");

            modelBuilder.Entity<VariavelMacroeconomica>()
                .HasMany(v => v.AtivosAfetados)
                .WithMany(a => a.VariaveisMacroeconomicas)
                .UsingEntity<Dictionary<string, object>>(
                    "THETIS_ATIVO_VARIAVEL_MACRO",
                    j => j.HasOne<Ativo>().WithMany().HasForeignKey("ID_ATIVO").HasConstraintName("FK_ATIVO_VAR_ATIVO"),
                    j => j.HasOne<VariavelMacroeconomica>().WithMany().HasForeignKey("ID_VARIAVEL").HasConstraintName("FK_ATIVO_VAR_VARIAVEL"),
                    j =>
                    {
                        j.HasKey("ID_ATIVO", "ID_VARIAVEL");
                        j.ToTable("THETIS_ATIVO_VARIAVEL_MACRO");
                    }
                );

            // Configurações de validação
            modelBuilder.Entity<Cliente>()
               .Property(c => c.Cpf)
               .HasMaxLength(11)
               .IsFixedLength();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.PerfilRisco)
                .HasConversion<int>()
                .HasComment("1=Conservador, 2=Moderado, 3=Agressivo");

            modelBuilder.Entity<Cliente>()
                .Property(c => c.ObjetivoPrincipal)
                .HasConversion<int>()
                .HasComment("1=CurtoPrazo, 2=MedioPrazo, 3=LongoPrazo, 4=Aposentadoria, 5=ReservaEmergencia");

            modelBuilder.Entity<Ativo>()
                .Property(a => a.TipoAtivo)
                .HasConversion<int>()
                .HasComment("1=RendaFixa, 2=RendaVariavel, 3=Fundos, 4=CriptoMoedas, 5=Commodities");

            modelBuilder.Entity<Ativo>()
                .Property(a => a.NivelRisco)
                .HasConversion<int>()
                .HasComment("1=Conservador, 2=Moderado, 3=Agressivo");

            modelBuilder.Entity<CarteiraRecomendada>()
                .Property(c => c.NivelRisco)
                .HasConversion<int>()
                .HasComment("1=Conservador, 2=Moderado, 3=Agressivo");

            modelBuilder.Entity<CarteiraRecomendada>()
                .Property(c => c.Objetivo)
                .HasConversion<int>()
                .HasComment("1=CurtoPrazo, 2=MedioPrazo, 3=LongoPrazo, 4=Aposentadoria, 5=ReservaEmergencia");

            // Configurações de performance
            modelBuilder.Entity<CarteiraRecomendada>()
                .HasIndex(c => new { c.ClienteId, c.DataGeracao })
                .HasDatabaseName("IX_CARTEIRA_CLIENTE_DATA");

            modelBuilder.Entity<CarteiraRecomendada>()
                .HasIndex(c => c.Ativa)
                .HasDatabaseName("IX_CARTEIRA_ATIVA");

            modelBuilder.Entity<HistoricoInvestimento>()
                .HasIndex(h => new { h.ClienteId, h.DataOperacao })
                .HasDatabaseName("IX_HISTORICO_CLIENTE_DATA");

            modelBuilder.Entity<HistoricoInvestimento>()
                .HasIndex(h => h.TipoOperacao)
                .HasDatabaseName("IX_HISTORICO_TIPO");

            modelBuilder.Entity<VariavelMacroeconomica>()
                .HasIndex(v => v.DataAtualizacao)
                .HasDatabaseName("IX_VARIAVEL_DATA");

            modelBuilder.Entity<LogRecomendacao>()
                .HasIndex(l => l.DataProcessamento)
                .HasDatabaseName("IX_LOG_DATA");
        }
    }
}
