# Configuração de Permissões do Jenkins (Sudoers)

Para que o Jenkins consiga executar comandos como `sudo mkdir`, `sudo chown` e `sudo systemctl` sem pedir senha, é necessário configurar o arquivo `sudoers` no servidor Linux (`CTUSYSRVPRD01`).

## 1. Identificar o Usuário do Jenkins
Geralmente o usuário é `jenkins`. Você pode confirmar rodando este comando no terminal do servidor:
```bash
grep 'jenkins' /etc/passwd
```

## 2. Editar o Arquivo Sudoers
Use o comando `visudo` para editar o arquivo de segurança. **Nunca edite `/etc/sudoers` diretamente com editores de texto comuns.**

```bash
sudo visudo
```

## 3. Adicionar Permissões

Você tem duas opções: permissão total (mais fácil) ou restrita (mais segura).

### Opção A: Permissão Total (Recomendada para Agentes Dedicados)
Adicione a seguinte linha ao final do arquivo:

```ini
jenkins ALL=(ALL) NOPASSWD: ALL
```

### Opção B: Permissão Restrita (Apenas Comandos Necessários)
Se preferir restringir, adicione apenas os comandos usados no pipeline:

```ini
jenkins ALL=(ALL) NOPASSWD: /usr/bin/mkdir, /usr/bin/chown, /usr/bin/systemctl, /usr/bin/rsync
```
*(Certifique-se que os caminhos dos binários estão corretos usando `which mkdir`, `which systemctl`, etc)*

## 4. Salvar e Sair
- Se estiver usando o **nano** (padrão no Ubuntu): Pressione `Ctrl+O`, `Enter` para salvar, e `Ctrl+X` para sair.
- Se estiver usando o **vim**: Pressione `Esc`, digite `:wq` e `Enter`.

## 5. Testar
Mude para o usuário jenkins e tente rodar um comando sudo:

```bash
sudo su - jenkins
sudo systemctl status nginx
```
Se não pedir senha, a configuração está correta.
