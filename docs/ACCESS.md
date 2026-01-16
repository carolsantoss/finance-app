# Acesso ao Sistema e Portas

Após o deploy automatizado pelo Jenkins, o sistema estará acessível nas seguintes portas:

## 🌐 Endereço Principal (Frontend + API)
**URL:** `http://IP_DO_SERVIDOR` (Porta 80)
- Exemplo: `http://192.168.1.50` ou `http://finance.uspery.local`
- **Nginx** serve o Frontend Vue.js e redireciona chamadas `/api/*` para o Backend.

## ⚙️ Backend API (Interno)
**Porta:** `5000` (HTTP)
- Endereço interno: `http://localhost:5000`
- O Nginx faz o proxy reverso para esta porta. Você não precisa (e geralmente não deve) acessar essa porta diretamente de for a, a menos que abra no firewall para debug.

## 🛠️ Logs e Troubleshooting
Se não conseguir acessar:
1. **Verifique se o Nginx está rodando:**
   ```bash
   sudo systemctl status nginx
   ```
2. **Verifique se a API está rodando:**
   ```bash
   sudo systemctl status finance-api
   ```
3. **Logs de Erro:**
   - Nginx: `/var/log/nginx/error.log`
   - API: `journalctl -u finance-api -f`
