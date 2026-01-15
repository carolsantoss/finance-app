# FinanceApp (Migration: WPF -> Web)

This repository contains the source code for **FinanceApp**, a personal finance management system.
The project has been migrated from a WPF Desktop application to a modern Web Application.

## Architecture

- **Frontend**: Vue.js 3 + Vite + TailwindCSS
- **Backend**: ASP.NET Core Web API 10.0
- **Database**: MySQL

## Project Structure

- `src/FinanceApp.API`: Backend API.
- `src/FinanceApp.Web`: Frontend Application.
- `src/FinanceApp.Shared`: Shared resources (Models, DTOs).
- `src/FinanceApp`: (Legacy) Original WPF Application.

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- Node.js & npm
- MySQL Server

### Running the Backend
1. Navigate to `src/FinanceApp.API`.
2. Configure `appsettings.json` with your database credentials.
3. Run:
   ```bash
   dotnet run
   ```
   API will listen on `http://localhost:5000`.

### Running the Frontend
1. Navigate to `src/FinanceApp.Web`.
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start development server:
   ```bash
   npm run dev
   ```
4. Access `http://localhost:5173`.

## Authentication
- Default Login endpoint: `/api/auth/login`.
- Register new users via the Web UI.

## Comandos de Migração (Banco de Dados)

Para criar uma nova migração (gerar o código SQL das tabelas):
```bash
dotnet ef migrations add NomeDaMigracao --project src/FinanceApp.Shared --startup-project src/FinanceApp.API
```

Para aplicar as migrações no banco de dados (criar/atualizar tabelas):
```bash
dotnet ef database update --project src/FinanceApp.Shared --startup-project src/FinanceApp.API
```
