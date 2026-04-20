# Plano 3 & 4: Migração de Contas, Cloud Save e Gacha Server-Side

Este plano foca na transição do salvamento local para uma arquitetura híbrida (Local/Server) usando Unity Gaming Services (UGS).

## 1. Arquitetura de Sincronização
- **IStorageProvider:** Interface para abstrair o salvamento.
    - `LocalStorageProvider`: Salva em `Application.persistentDataPath` (JSON).
    - `CloudStorageProvider`: Usa `Unity Cloud Save`.
- **Data Conflict Policy:** Se o save local tiver data de modificação diferente do servidor, o usuário deve escolher qual manter (conflito de nuvem).

## 2. Divisão de Dados (Server vs Local)
- **Server-Side (Autoritativo):**
    - Moedas (Money, StarMaps, Stardust).
    - Inventário de Unidades e Artifacts (para evitar dupes/hacks).
    - Pity do Gacha.
- **Local (Cachê/Não sensível):**
    - Configurações de som/gráficos.
    - Times pré-selecionados (Loadouts) - para agilizar o login, mas sincronizados depois.

## 3. Gacha Autorritativo (Cloud Code)
- **O Problema:** Sorteio local permite o "Save Scumming" (fechar o jogo se o drop for ruim).
- **A Solução:** 
    - O cliente envia um request `POST /pull-gacha`.
    - O **Unity Cloud Code** roda o script de sorteio no servidor.
    - O servidor atualiza o banco de dados do jogador e retorna apenas o resultado para o cliente exibir a animação.

## 4. Modo de Teste Local
- Se `USE_SERVER` não estiver definido em `Project Settings`, o `AccountManager` usará o `LocalStorageProvider` e uma classe `LocalGachaSim` que emula a lógica do servidor localmente.
