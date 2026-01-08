using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceApp.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args = null)
        {
            Env.Load();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            var server = Env.GetString("DB_SERVER", "localhost");
            var port = Env.GetString("DB_PORT", "3306");
            var database = Env.GetString("DB_DATABASE", "finance_app");
            var user = Env.GetString("DB_USER", "root");
            var password = Env.GetString("DB_PASSWORD", "");

            var connectionStringBuilder = new MySqlConnector.MySqlConnectionStringBuilder
            {
                Server = server,
                Port = uint.Parse(port),
                Database = database,
                UserID = user,
                Password = password,
                AllowUserVariables = true,
                UseCompression = true
            };

            var connectionString = connectionStringBuilder.ConnectionString;

            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
