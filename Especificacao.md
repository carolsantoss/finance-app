ESPECIFICAÇÃO TÉCNICA

FINANCE APP

# 1. INTRODUÇÃO

## 1.1 Propósito do Documento

Este documento descreve a arquitetura técnica, regras de negócio, modelo de dados e funcionalidades do sistema Finance App, servindo como referência para desenvolvedores, analistas e stakeholders.

# 2. VISÃO GERAL DO SISTEMA

## 2.1 Identificação do Projeto

| Nome do Sistema | Finance App |
| --- | --- |
| Versão | 1.0 |
| Plataforma | Web (SPA - Single Page Application) |
| Backend | .NET 10 (Web API) |
| Frontend | Vue.js 3 + Vite + Tailwind CSS |
| Linguagem | C# (Backend) / TypeScript (Frontend) |
| Banco de Dados | MySQL |

**ORM:** Entity Framework Core 8.0

## 2.2 Objetivo

Aplicação web para controle financeiro pessoal, permitindo aos usuários:

- Registrar entradas e saídas financeiras
- Visualizar dashboard com gráficos e resumos
- Controlar parcelamentos
- Analisar saldo mensal e fluxo de caixa

## 2.3 Público-Alvo

Usuários individuais que desejam organizar suas finanças pessoais de forma simples, clara e eficiente.

# 3. ARQUITETURA DO SISTEMA

## 3.1 Padrão Arquitetural

- **Client-Server**: Separação completa entre Frontend e Backend.
- **REST API**: Backend expõe endpoints HTTP JSON.
- **SPA (Single Page Application)**: Frontend reativo utilizando Vue.js e Pinia para gerenciamento de estado.

## 3.2 Estrutura de Pastas

FinanceApp/
├── src/
│   ├── FinanceApp.API/        # Backend (.NET Web API)
│   │   ├── Controllers/       # Endpoints da API
│   │   ├── Data/             # Contexto do EF Core
│   │   └── Properties/       # Configurações de lançamento
│   │
│   ├── FinanceApp.Web/        # Frontend (Vue.js)
│   │   ├── src/
│   │   │   ├── api/          # Configuração do Axios
│   │   │   ├── components/   # Componentes Reutilizáveis (Modais, Charts)
│   │   │   ├── stores/       # Gerenciamento de Estado (Pinia)
│   │   │   └── views/        # Páginas (Dashboard, Extratos)
│   │   └── public/
│   │
│   └── FinanceApp.Shared/     # Biblioteca de Classes Compartilhadas
│       ├── DTOs/             # Data Transfer Objects
│       └── Models/           # Entidades do Domínio

## 3.3 Tecnologias Utilizadas

| Frontend | Vue.js 3, Vite, Tailwind CSS, Chart.js, Pinia, Lucide Icons |
| --- | --- |
| Backend | ASP.NET Core (.NET 10), Entity Framework Core, Swagger |
| Banco de Dados | MySQL |
| Autenticação | JWT (JSON Web Tokens) |

# 4. MODELO DE DADOS

## 4.1 Entidades

### 4.1.1 User (Usuário)

- id_usuario (int, PK, Auto-increment)
- nm_nomeUsuario (string)
- nm_email (string)
- hs_senha (string - hash)

### 4.1.2 Lancamento (Lançamento Financeiro)

- id_lancamento (int, PK, Auto-increment)
- id_usuario (int, FK -> User)
- nm_tipo (string - "Entrada" | "Saída")
- nm_descricao (string)
- nr_valor (decimal)
- dt_dataLancamento (DateTime)
- nm_formaPagamento (string - "Débito" | "Crédito")
- nr_parcelas (int, padrão = 1)
- nr_parcelaInicial (int, apenas controle interno ou legado)
- nr_parcelasPagas (int, padrão = 0)

## 4.2 Relacionamentos

User 1:N Lancamento - Um usuário pode possuir vários lançamentos.

# 5. FUNCIONALIDADES DO SISTEMA

## 5.1 Módulo Principal (Dashboard)

### 5.1.1 Visão Geral

Funcionalidades:
- Exibição de Cards Informativos (Saldo Total, Entradas, Saídas).
- Gráfico de Fluxo de Caixa (Receitas vs Despesas dos últimos 6 meses).
- Lista rápida das últimas transações.
- Botão de acesso rápido para "Nova Transação".

Implementação:
- Consome `/api/lancamentos/summary` para os cards.
- Consome `/api/lancamentos/chart` para o gráfico.

## 5.2 Módulo de Lançamentos

### 5.2.1 Modal de Nova Transação

Funcionalidades:
- Cadastro de entrada ou saída.
- Seleção de categoria/tipo e forma de pagamento.
- Definição de parcelamento (se aplicável).

## 5.3 Módulo de Extratos

Funcionalidades:
- Listagem completa de lançamentos.
- (Planejado) Filtros avançados e paginação.

# 6. REGRAS DE NEGÓCIO E PROCESSOS

## 6.1 Autenticação e Segurança

- Todos os endpoints (exceto Login/Registro) requerem token JWT válido no cabeçalho `Authorization: Bearer <token>`.
- O token é gerado no login e validado pelo middleware do ASP.NET Core.

## 6.2 Lançamentos

- Um lançamento pertence estritamente a um usuário (`id_usuario`).
- Usuários não podem visualizar ou manipular lançamentos de outros usuários (Filtro global no DBSet ou na Query).

# 7. CONFIGURAÇÃO, AMBIENTE E DEPLOY

## 7.1 Variáveis de Ambiente (Backend - .env)

```
DB_SERVER=localhost
DB_PORT=3306
DB_DATABASE=finance_app
DB_USER=root
DB_PASSWORD=senha
Jwt:Key=SuaChaveSecretaSuperSegura
Jwt:Issuer=FinanceApp
Jwt:Audience=FinanceAppUsers
```

## 7.2 Configuração Frontend

O frontend utiliza variáveis de ambiente do Vite (`.env`) para definir a URL da API, se necessário.

# 8. LIMITAÇÕES E MELHORIAS FUTURAS

## 8.1 Melhorias Planejadas

- Implementação completa da tela de Extratos com filtros.
- Sistema de metas financeiras.
- Modo escuro/claro (já parcialmente suportado via Tailwind).
- Testes unitários automatizados para o Backend.