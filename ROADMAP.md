# Roadmap - Finance App Professional

Este documento descreve o plano de evolução do sistema para transformá-lo em uma solução financeira robusta e profissional (SaaS-ready).

## 🚀 Fase 1: Estrutura Fundamental (Core)
O foco desta fase é dar inteligência aos dados, saindo de "lançamentos soltos" para dados estruturados.

- [ ] **Gestão de Categorias e Tags**
    - Criar entidade `Category` (Nome, Ícone, Cor, Tipo: Receita/Despesa).
    - Vincular lançamentos a categorias.
    - Seed de categorias padrão (Alimentação, Transporte, Lazer, etc.).
- [ ] **Múltiplas Contas / Carteiras**
    - Criar entidade `Wallet` (Nome, Tipo: Conta Corrente, Carteira, Poupança).
    - Controle de saldo por carteira.
    - Funcionalidade de Transferência entre contas (Saída de A -> Entrada em B).
- [ ] **Gestão de Cartão de Crédito**
    - Entidade `CreditCard` vinculada a `Wallet`.
    - Controle de Dia de Fechamento e Vencimento.
    - Diferenciação visual de compras no crédito.

## 💰 Fase 2: Controle e Planejamento (Budgeting)
Ferramentas para o usuário economizar e planejar o futuro.

- [ ] **Orçamentos e Metas**
    - Definir limites de gastos por Categoria/Mês.
    - Dashboard de acompanhamento (Previsto vs Realizado).
    - Alertas visuais quando próximo do limite.
- [ ] **Lançamentos Recorrentes e Agendados**
    - Engine de recorrência (Diário, Semanal, Mensal, Anual).
    - Gerar lançamentos futuros automaticamente ou sob aprovação.
    - Visualização de "Contas a Pagar" (Futuro).

## 📊 Fase 3: Analytics e Relatórios
Visualização avançada dos dados.

- [ ] **Relatórios Avançados**
    - Gráfico de Pizza (Gastos por Categoria).
    - Gráfico de Linha (Evolução Patrimonial).
    - Exportação de dados (PDF, Excel/CSV).
- [ ] **Filtros Avançados**
    - Filtrar extratos por múltiplos critérios (Data, Categoria, Conta, Tags).

## 🎨 Fase 4: UX e Acabamento
Melhorias na experiência do usuário e interface.

- [ ] **Interface Otimizada**
    - Modo Claro/Escuro (Theme Switching).
    - Dashboard personalizável (Drag & Drop de widgets - *Futuro*).
    - Onboarding para novos usuários (Tour guiado).

## 🛠️ Fase 5: Excelência Técnica (Nível Enterprise)
Melhorias de arquitetura, segurança e performance.

- [ ] **Segurança e Acesso**
    - Recuperação de senha (Email com token).
    - Confirmação de email.
    - 2FA (Autenticação de Dois Fatores).
- [ ] **Performance e Escalabilidade**
    - Paginação no Backend e Frontend (evitar carregar tudo de uma vez).
    - Caching (Redis ou In-Memory) para dashboards pesados.
    - Validações robustas (FluentValidation).
