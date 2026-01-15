ESPECIFICAÇÃO TÉCNICA

FINANCE APP

# 1\. INTRODUÇÃO

## 1.1 Propósito do Documento

Este documento descreve a arquitetura técnica, regras de negócio, modelo de dados e funcionalidades do sistema Finance App, servindo como referência para desenvolvedores, analistas, auditores técnicos e stakeholders.

# 2\. VISÃO GERAL DO SISTEMA

## 2.1 Identificação do Projeto

| Nome do Sistema | Finance App |
| --- | --- |
| Versão | 1.0 |
| Plataforma | Desktop (WPF - Windows Presentation Foundation) |
| Framework | .NET 10 |
| Linguagem | C# 14.0 |
| Banco de Dados | MySQL |

**ORM:** Entity Framework Core 8.0

## 2.2 Objetivo

Aplicação desktop para controle financeiro pessoal, permitindo aos usuários:

- Registrar entradas e saídas financeiras
- Visualizar extratos com filtros
- Controlar parcelamentos
- Analisar saldo mensal

## 2.3 Público-Alvo

Usuários individuais que desejam organizar suas finanças pessoais de forma simples, clara e eficiente.

# 3\. ARQUITETURA DO SISTEMA

## 3.1 Padrão Arquitetural

- MVVM (Model-View-ViewModel) - padrão principal
- Code-Behind para lógicas específicas de interface
- ViewModels para regras de negócio e lógica reutilizável

## 3.2 Estrutura de Pastas

FinanceApp/  
├── Data/ # Contexto do banco de dados  
├── Models/ # Entidades de domínio  
├── Views/ # Telas XAML e Code-Behind  
├── ViewModels/ # Lógica de apresentação  
├── Helpers/ # Classes utilitárias  
├── Services/ # Serviços (autenticação, regras, etc.)  
└── Migrations/ # Migrações do EF Core

## 3.3 Tecnologias Utilizadas

| WPF | Interface gráfica |
| --- | --- |
| Entity Framework Core | ORM |
| MySQL | Banco de dados relacional |
| SHA256 | Criptografia de senhas |
| DotNetEnv | Gerenciamento de variáveis de ambiente |
| INotifyPropertyChanged | Data Binding reativo |

# 4\. MODELO DE DADOS

## 4.1 Entidades

### 4.1.1 User (Usuário)

- id_usuario (int, PK, Auto-increment)
- nm_nomeUsuario (string)
- nm_email (string)
- hs_senha (string - hash SHA256)

### 4.1.2 Lancamento (Lançamento Financeiro)

- id_lancamento (int, PK, Auto-increment)
- id_usuario (int, FK → User)
- nm_tipo (string - "Entrada" | "Saída")
- nm_descricao (string)
- nr_valor (decimal)
- dt_dataLancamento (DateTime)
- nm_formaPagamento (string - "Débito" | "Crédito")
- nr_parcelas (int, padrão = 1)
- nr_parcelaInicial (int)

## 4.2 Relacionamentos

User 1:N Lancamento - Um usuário pode possuir vários lançamentos

## 4.3 Diagrama Entidade-Relacionamento

USER ||--o{ LANCAMENTO : possui  
<br/>USER {  
int id_usuario PK  
string nm_nomeUsuario  
string nm_email  
string hs_senha  
}  
<br/>LANCAMENTO {  
int id_lancamento PK  
int id_usuario FK  
string nm_tipo  
string nm_descricao  
decimal nr_valor  
DateTime dt_dataLancamento  
string nm_formaPagamento  
int nr_parcelas  
int nr_parcelaInicial  
}

# 5\. FUNCIONALIDADES DO SISTEMA

## 5.1 Módulo de Autenticação

### 5.1.1 Tela de Login (LoginWindow)

Funcionalidades

- Login com e-mail e senha
- Exibição/ocultação de senha
- Validação de credenciais
- Auto-login configurável
- Navegação para cadastro

Regras de Negócio

- Senhas armazenadas com SHA256
- Validação por e-mail
- Sessão mantida em UsuarioLogado

### 5.1.2 Tela de Registro (RegisterWindow)

Funcionalidades

- Cadastro de novo usuário
- Campos obrigatórios: Nome, E-mail, Senha, Confirmação
- Validações de e-mail e senha

Regras de Negócio

- E-mail único
- Senhas devem coincidir
- Senha criptografada antes do salvamento

## 5.2 Módulo Principal (Dashboard)

### 5.2.1 Tela Principal (MainWindow)

Funcionalidades

- Exibição do usuário logado
- Resumo financeiro mensal
- Navegação entre módulos
- Logout

Cálculos

- Apenas lançamentos do mês/ano atual
- Parcelas expandidas virtualmente

Formatação

- Moeda: R\$ (pt-BR)
- Formato: C2

## 5.3 Módulo de Lançamentos

### 5.3.1 Tela de Novo Lançamento (LancamentoWindow)

Funcionalidades

- Cadastro de entrada ou saída
- Controle de parcelamento
- Validações de campos

Regras de Negócio

- Parcelamento apenas para crédito
- Parcela inicial padrão = 1
- Data padrão = data atual

Lógica de Parcelamento

Se nr_parcelaInicial == 1:  
valorParcela = nr_valor / nr_parcelas  
Senão:  
valorParcela = nr_valor

## 5.4 Módulo de Extratos

### 5.4.1 Tela de Extratos (ExtratosWindow)

Funcionalidades

- Listagem de lançamentos
- Filtros por tipo, mês e ano
- Resumo financeiro do período
- Exclusão de lançamentos

Sistema de Parcelas Virtuais

- Parcelas expandidas em memória
- ID virtual: id_original \* 1000 + numero_parcela
- Exclusão sempre afeta o lançamento original

## 5.5 Módulo de Perfil

### 5.5.1 Tela de Perfil (PerfilWindow)

Funcionalidades

- Visualização e edição de dados
- Alteração de senha

Regras de Negócio

- Senha mínima de 6 caracteres
- Validação de senha atual
- Atualização da sessão

# 6\. REGRAS DE NEGÓCIO E PROCESSOS

## 6.1 Sistema de Parcelamento

Cenário 1 - Parcela Inicial = 1

- Valor: R\$ 1.200,00
- Parcelas: 12
- Resultado: 12× R\$ 100,00

Cenário 2 - Parcela Inicial ≠ 1

- Valor: R\$ 300,00
- Parcelas: 12
- Parcela inicial: 5
- Resultado: 8 parcelas restantes

## 6.2 Cálculo de Saldo

**SaldoMes = TotalEntradas - TotalSaidas**

## 6.3 Segurança de Senha

- Armazenamento: SHA256(senha)
- Validação: hash digitado == hash armazenado

# 7\. INTERFACE DE USUÁRIO (UI/UX) E PADRÕES

## 7.1 Padrões de Design

- Verde #4CAF50 - Entradas
- Vermelho #E53935 - Saídas
- Datas: dd/MM/yyyy
- Ícones: Emojis Unicode

## 7.2 Validações

- Campos numéricos restritos
- Confirmação para ações destrutivas
- Feedback visual em tempo real

# 8\. CONFIGURAÇÃO, AMBIENTE E DEPLOY

## 8.1 Variáveis de Ambiente (.env)

DB_SERVER=localhost  
DB_PORT=3306  
DB_DATABASE=finance_app  
DB_USER=root  
DB_PASSWORD=senha

## 8.2 appsettings.json

{  
"AutoLogin": {  
"Ativo": "false",  
"Email": "<usuario@exemplo.com>",  
"Senha": "senha123"  
}  
}

# 9\. COMPONENTES DE SUPORTE E UTILITÁRIOS

- Session: gerenciamento do usuário logado
- PasswordHelper: hash e validação de senha
- AppConfig: leitura de configurações
- AppDbContextFactory: criação do DbContext

# 10\. LIMITAÇÕES, RISCOS E EVOLUÇÕES

## 10.1 Limitações

- SHA256 sem salt
- Sem recuperação de senha
- Sem relatórios ou gráficos

## 10.2 Melhorias Futuras

- bcrypt / Argon2
- 2FA
- Gráficos e metas
- Tema claro/escuro
- Testes unitários e logs

# 11\. GLOSSÁRIO TÉCNICO

| Termo | Definição |
| --- | --- |
| Lançamento | Registro financeiro |
| Parcela Virtual | Parcela simulada em memória |
| Saldo | Entradas - Saídas |
| MVVM | Model-View-ViewModel |

# 12\. ANEXOS TÉCNICOS

## 12.1 Estrutura do Banco de Dados

CREATE TABLE users (  
id_usuario INT PRIMARY KEY AUTO_INCREMENT,  
nm_nomeUsuario VARCHAR(255) NOT NULL,  
nm_email VARCHAR(255) NOT NULL UNIQUE,  
hs_senha VARCHAR(255) NOT NULL  
);  
<br/>CREATE TABLE lancamentos (  
id_lancamento INT PRIMARY KEY AUTO_INCREMENT,  
id_usuario INT NOT NULL,  
nm_tipo VARCHAR(50) NOT NULL,  
nm_descricao TEXT NOT NULL,  
nr_valor DECIMAL(65,30) NOT NULL,  
dt_dataLancamento DATETIME NOT NULL,  
nm_formaPagamento VARCHAR(50) NOT NULL,  
nr_parcelas INT DEFAULT 1,  
nr_parcelaInicial INT,  
FOREIGN KEY (id_usuario) REFERENCES users(id_usuario)  
);