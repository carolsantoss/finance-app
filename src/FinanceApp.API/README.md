# FinanceApp - Backend API

Built with **ASP.NET Core Web API (.NET 10)**.

## Architecture
- **Controllers**: `AuthController`, `LancamentoController`.
- **Data Access**: Entity Framework Core (MySQL).
- **Shared Library**: Uses `FinanceApp.Shared` for Models and DTOs.

## Configuration
Update `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=finance_app;..."
}
```

## API Documentation
Swagger UI is enabled in Development mode at `/swagger/index.html`.
