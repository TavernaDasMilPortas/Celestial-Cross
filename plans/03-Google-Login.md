# Plano 5: Autenticação Google e Identidade Mobile

Integração do fluxo de login para Android e persistência de identidade entre dispositivos.

## 1. Fluxo de Autenticação (UGS Authentication)
1. **Login Anônimo:** Criado no primeiro boot para salvar o progresso imediatamente.
2. **Vínculo (Link):** Botão "Conectar Google Play Games" nas configurações.
3. **Migração:** Se o player trocar de celular, ao logar no Google, o UGS recupera o `PlayerID` associado e baixa o progresso.

## 2. Requisitos Play Store
- Configurar o **SHA-1 Fingerprint** no Google Cloud Console.
- Configurar o **OAuth 2.0 Client ID**.
- Instalar o pacote `Google Play Games plugin for Unity`.

## 3. Estrutura de Código
- `AuthManager.cs`: Gerencia o ciclo de vida (SignIn, SignOut, CheckStatus).
- Eventos de `OnAuthSuccess` para carregar o `AccountManager`.
