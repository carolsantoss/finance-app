# Configuração do Ambiente (.env)

O sistema Finance App utiliza variáveis de ambiente para conectar ao banco de dados e outras configurações sensíveis.

## Onde fica o arquivo?

No servidor, o arquivo `.env` deve ser criado na **pasta onde a API está rodando**:

📂 `/app/finance-app/src/FinanceApp.API/publish/.env`

*(Nota: Como este arquivo contém senhas, ele **não é versionado** e nem criado automaticamente pelo Jenkins. Você deve criá-lo manualmente na primeira vez.)*

## Passo a Passo para criar/editar

1. Acesse o servidor e navegue até a pasta:
   ```bash
   cd /app/finance-app/src/FinanceApp.API/publish
   ```

2. Crie ou edite o arquivo `.env`:
   ```bash
   sudo nano .env
   ```

3. Cole o conteúdo de configuração (ajuste os valores conforme sua senha real):

   ```ini
   DB_SERVER=localhost
   DB_PORT=3306
   DB_DATABASE=finance_app
   DB_USER=root
   DB_PASSWORD=SUA_SENHA_AQUI
   ```

4. Salve (`Ctrl+O`, `Enter`) e Saia (`Ctrl+X`).

5. **Reinicie o serviço da API** para aplicar as mudanças:
   ```bash
   sudo systemctl restart finance-api
   ```

---

## Verificando

Você pode verificar se a API leu o arquivo checando os logs:

```bash
sudo journalctl -u finance-api -f
```
Deve aparecer uma mensagem como: `[INFO] Loaded .env from ...`
