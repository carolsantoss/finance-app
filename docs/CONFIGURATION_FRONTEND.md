# Configuração do Frontend (Variáveis de Ambiente)

Diferente do Backend, o Frontend não lê variáveis de ambiente em tempo de execução. As variáveis são **embutidas no código** durante o comando `npm run build`.

## Comportamento Padrão (Recomendado)

O frontend foi configurado para usar o caminho relativo `/api` por padrão. Isso significa que ele vai tentar acessar a API **no mesmo domínio/IP** onde o site está aberto.

Exemplo:
- Se você acessar `http://192.168.1.50/`, o frontend chamará `http://192.168.1.50/api/...`.
- Graças ao Nginx (que redireciona `/api` para o backend na porta 5000), isso funciona automaticamente sem configuração extra.

## Forçando uma URL específica (Opcional)

Se você precisar apontar o frontend para um backend diferente (outro servidor), você precisa criar um arquivo `.env` **na pasta do projeto Web antes do build**.

Local: `/app/finance-app/src/FinanceApp.Web/.env`

```ini
VITE_API_URL=http://meu-outro-servidor.com/api
```

Após criar/editar este arquivo, **o build precisa rodar novamente**. No Jenkins, isso acontece a cada deploy.
