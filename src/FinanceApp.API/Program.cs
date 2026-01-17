using FinanceApp.Shared.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


// Load .env file (Search in current dir and up to 3 parent dirs)
var currentSearchPath = AppDomain.CurrentDomain.BaseDirectory;
string? envPath = null;

for (int i = 0; i < 4; i++)
{
    var candidate = Path.Combine(currentSearchPath, ".env");
    if (File.Exists(candidate))
    {
        envPath = candidate;
        break;
    }
    
    var parent = Directory.GetParent(currentSearchPath);
    if (parent == null) break;
    currentSearchPath = parent.FullName;
}

if (envPath != null)
{
    DotNetEnv.Env.Load(envPath);
    Console.WriteLine($"[INFO] Loaded .env from {envPath}");
}
else
{
    Console.WriteLine($"[WARN] .env not found at {envPath}");
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FinanceApp.API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Database
var connectionString = $"Server={Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost"};" +
                       $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "3306"};" +
                       $"Database={Environment.GetEnvironmentVariable("DB_DATABASE") ?? "finance_app"};" +
                       $"Uid={Environment.GetEnvironmentVariable("DB_USER") ?? "root"};" +
                       $"Pwd='{Environment.GetEnvironmentVariable("DB_PASSWORD") ?? ""}';" +
                       "AllowUserVariables=True;UseCompression=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// JWT Authentication
// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Services
builder.Services.AddScoped<FinanceApp.API.Services.IEmailService, FinanceApp.API.Services.EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Support for WAF/Proxies (load balancer terminating SSL)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        FinanceApp.API.Data.DbInitializer.Seed(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] An error occurred while seeding the database: {ex.Message}");
    }
}

app.Run();
