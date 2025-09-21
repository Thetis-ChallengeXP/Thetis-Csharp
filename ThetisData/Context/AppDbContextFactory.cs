using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ThetisData.Context
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseOracle("User Id=RM98680;Password=271204;Data Source=oracle.fiap.com.br:1521/ORCL;").EnableSensitiveDataLogging(true);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
