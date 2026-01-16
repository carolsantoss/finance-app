# Guia de Deploy - Finance App (Build & Deploy no Ubuntu)

Este documento descreve o processo de deploy da aplicação Finance App (Backend .NET + Frontend Vue.js) realizando **todo o processo de build e execução diretamente no servidor Linux Ubuntu**.

## 1. Pré-requisitos do Servidor

Conecte-se ao servidor e instale as ferramentas necessárias para compilar e rodar a aplicação.

### 1.1. Atualizar Sistema e Instalar Ferramentas Básicas
```bash
sudo apt update && sudo apt upgrade -y
sudo apt install -y nginx curl git libssl-dev
```

### 1.2. Instalar .NET SDK (Para Build e Runtime)
Como vamos compilar o código, precisamos do **SDK** (Software Development Kit), não apenas do Runtime.

*(Exemplo para versão .NET 10 ou 8, ajuste conforme a versão do projeto)*
```bash
# Adicionar repositório da Microsoft (Exemplo genérico)
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Instalar SDK
sudo apt update
sudo apt install -y dotnet-sdk-8.0
# OU, se estiver usando .NET 10 Preview via script:
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0 --install-dir /usr/share/dotnet
sudo ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet
```

### 1.3. Instalar Node.js e NPM (Para Build do Frontend)
Necessário para compilar o Vue.js.

```bash
# Instala o Node.js 20.x (LTS recomendado)
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs

# Verificar instalações
dotnet --version
node -v
npm -v
```

### 1.4. Banco de Dados (MySQL)
```bash
sudo apt install -y mysql-server
sudo mysql_secure_installation
```
*(Configure o banco e usuário conforme documentação anterior)*

---

## 2. Preparação do Ambiente

Vamos criar uma estrutura de diretórios para o código fonte (`source`) e para a aplicação compilada (`release`).

```bash
# Pasta para o "site" rodando
sudo mkdir -p /var/www/finance-app/api
sudo mkdir -p /var/www/finance-app/web

# Pasta para baixar o código fonte (pode ser na home do usuário ou /opt)
mkdir -p ~/git-projects
cd ~/git-projects
```

---

## 3. Clone e Build

### 3.1. Obter o Código
```bash
# Clone o repositório (Use HTTPS ou configure chaves SSH se for privado)
git clone https://seu-repositorio.git finance-app
cd finance-app
```

### 3.2. Build do Backend (.NET)
```bash
cd src/FinanceApp.API

# Restaura pacotes e compila versão de produção (Release)
# Saída direcionada para a pasta final do servidor
sudo dotnet publish -c Release -o /var/www/finance-app/api
```

### 3.3. Build do Frontend (Vue.js)
```bash
cd ../FinanceApp.Web

# Instala dependências
npm install

# Compila para produção
npm run build

# Copia os arquivos gerados (pasta dist) para o servidor Web
sudo cp -r dist/* /var/www/finance-app/web/
```

### 3.4. Permissões
Garanta que o usuário do Nginx (`www-data`) tenha acesso aos arquivos finais.
```bash
sudo chown -R www-data:www-data /var/www/finance-app
sudo chmod -R 755 /var/www/finance-app
```

---

## 4. Configuração dos Serviços (Systemd e Nginx)

### 4.1. Serviço Systemd (Backend)
Arquivo: `/etc/systemd/system/finance-api.service`

```ini
[Unit]
Description=Finance App API
After=network.target

[Service]
WorkingDirectory=/var/www/finance-app/api
ExecStart=/usr/bin/dotnet /var/www/finance-app/api/FinanceApp.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=finance-api
User=www-data
# Variáveis de Ambiente (Idealmente use um arquivo .env ou o appsettings.Production.json na pasta)
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ConnectionStrings__DefaultConnection="Server=localhost;Database=finance_app;User=finance_user;Password=SUA_SENHA;"

[Install]
WantedBy=multi-user.target
```

Ativar serviço:
```bash
sudo systemctl daemon-reload
sudo systemctl enable finance-api
sudo systemctl restart finance-api
```

### 4.2. Configuração Nginx (Frontend + Proxy)
Arquivo: `/etc/nginx/sites-available/finance-app`

```nginx
server {
    listen 80;
    server_name SEU_IP_OU_DOMINIO;

    # Frontend
    location / {
        root /var/www/finance-app/web;
        try_files $uri $uri/ /index.html;
        index index.html;
    }

    # Backend
    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Ativar site:
```bash
sudo ln -sF /etc/nginx/sites-available/finance-app /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl reload nginx
```

---

## 5. Script de Atualização Rápida (Re-deploy)

Para facilitar futuras atualizações, você pode criar um script simples `deploy.sh` na raiz do projeto no servidor:

```bash
#!/bin/bash
echo "Iniciando Deploy..."

# 1. Atualizar Código
git pull origin main

# 2. Build Backend
echo "Build Backend..."
cd src/FinanceApp.API
sudo dotnet publish -c Release -o /var/www/finance-app/api

# 3. Build Frontend
echo "Build Frontend..."
cd ../FinanceApp.Web
npm install
npm run build
sudo cp -r dist/* /var/www/finance-app/web/

# 4. Reiniciar Serviços
echo "Reiniciando serviços..."
sudo systemctl restart finance-api
sudo systemctl reload nginx

echo "Deploy concluído com sucesso!"
```
Dê permissão de execução: `chmod +x deploy.sh`
