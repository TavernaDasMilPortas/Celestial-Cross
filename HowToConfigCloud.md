# Guia de Configuração: Unity Cloud Save (UGS) - Padrão 2026

Para que o sistema de nuvem funcione no seu projeto Unity seguindo os métodos mais recentes (UGS 2024/2026), siga estes passos:

## 1. Vincular o Projeto no Unity Editor
Atualmente, o Unity não usa mais a aba "Services" antiga para a configuração inicial.
1. No Unity Editor, vá em **Edit > Project Settings > Project Cloud Connector** (ou **Services** em algumas versões, mas a interface mudou).
2. Clique no ícone de "nuvem" ou no botão **"Link to Unity Cloud Project"**.
3. Uma janela do seu navegador se abrirá. Faça login na sua conta Unity e selecione (ou crie) o projeto.
4. Volte para o Unity. Você verá o **Project ID** preenchido.

## 2. Habilitar os Serviços no Unity Cloud Dashboard
A configuração agora é feita majoritariamente via web:
1. Vá para o [Unity Cloud Dashboard](https://cloud.unity.com).
2. Selecione seu projeto na lista.
3. No Dashboard, você precisa encontrar a seção de **Authentication**. Como a interface da Unity muda frequentemente, tente um destes caminhos:
   - Procure por um ícone de "cadeado" ou "perfil" no menu lateral chamado **"Authentication"** ou **"Identity & Access"**.
   - Ou vá na **barra de busca** no topo da tela e digite **"Identity Providers"**.
4. Dentro de Authentication/Identity Providers:
   - Clique na aba ou botão **"Identity Providers"**.
   - Localize o provedor **"Anonymous"** na lista.
   - Clique em **"Enable"** ou no botão de configuração para ativá-lo.
5. No menu lateral, vá em **Solutions > Cloud Save**:
   - Clique em **Enable** para ativar o serviço de armazenamento.

## 3. Instalar os pacotes corretos
Certifique-se de que está usando as versões mais recentes no **Window > Package Manager**:
- **Services Core** (Instalado automaticamente com os outros).
- **Authentication**: Procure por "Authentication" no Unity Registry.
- **Cloud Save**: Procure por "Cloud Save".

## 4. Configuração no Componente AccountManager
1. Localize o GameObject `AccountManager` na cena inicial.
2. No Inspector:
   - Marque **Use Cloud Save**.
   - Defina uma **Account Key** (ex: `player_account`).
   - O sistema usará o **Anonymous Sign-in** automaticamente para identificar o dispositivo.

## 5. Como o sistema funciona agora (Resolução de Conflitos)
- **Salvamento Automático por Data**: O código compara o campo `LastSaveTime` dentro do JSON. 
- **Lógica**: Se você jogar offline e depois conectar, o jogo verá que o save local é mais novo que o da nuvem e fará o upload. Se você instalar o jogo em um celular novo, o save da nuvem será mais novo e o jogo fará o download.
