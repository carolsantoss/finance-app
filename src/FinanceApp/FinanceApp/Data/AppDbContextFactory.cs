using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace FinanceApp.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args = null)
        {
            var projectDirectory = Directory.GetCurrentDirectory();
            var envPath = Path.Combine(projectDirectory, ".env");
            
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
            }
            else
            {
                var parentEnvPath = Path.Combine(Directory.GetParent(projectDirectory)?.Parent?.Parent?.FullName ?? "", ".env");
                if (File.Exists(parentEnvPath))
                {
                    Env.Load(parentEnvPath);
                }
            }

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
