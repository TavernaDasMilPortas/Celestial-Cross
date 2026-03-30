# Configuração do Fluxo de Cenas (Hub → Preparação → Batalha)

Este guia ensina a configurar, no Editor da Unity, o fluxo completo de jogo implementado via scripts:

- Hub (seleção de fase)
- Preparação (seleção de unidades + formação 3×3)
- Batalha (spawn conforme seleção + inimigos do LevelData)
- Finalização da fase (recompensa + retorno opcional ao Hub)

## 1) Pré-requisitos (Assets)

### 1.1 UnitData: campos obrigatórios
Em cada `UnitData`:

- `UnitID`
  - É gerado automaticamente (GUID do asset) e não é editável no Inspector.
  - Usado para: salvar conta, seleção de units, spawn.
- `displayName`
- `icon` (Sprite)

Se `UnitID` estiver vazio, o bootstrap e/ou spawn podem falhar com warnings.

### 1.2 UnitCatalog (mapeamento UnitID → Prefab)
Crie um asset `UnitCatalog`:

- Menu: `Create → RPG → Unit Catalog`
- Para cada unit, adicione uma Entry:
  - `UnitData` (arraste o asset)
  - `Prefab` (prefab que contém um componente `Unit`)
  - `UnitID` é sincronizado automaticamente a partir do `UnitData`.

Este catálogo é usado por:
- `PreparationSceneController` (para pegar `displayName`/`icon`)
- `BattleLevelBuilder` (para instanciar prefabs via UnitID)

### 1.3 RewardPackage (recompensas)
Crie um ou mais assets de recompensa:

- Menu: `Create → RPG → Reward Package`
- Configure `Money` e `Energy`.

### 1.4 LevelData e LevelCatalog (fases)
Crie um `LevelData` por fase:

- Menu: `Create → RPG → Level Data`
- Preencha:
  - `LevelName`
  - `SceneName` (nome exato da cena de batalha no Build Settings)
  - `PhaseMap` (layout do grid/tiles a ser usado na batalha)
  - `Enemies` (lista de `UnitData` + `GridPosition`)
  - `VictoryRewards` (RewardPackage)

Crie também um `LevelCatalog`:

- Menu: `Create → RPG → Level Catalog`
- Adicione seus `LevelData` na lista `Levels`.

### 1.5 Conta inicial (para testar rápido)
#### Opção A — Bootstrap (primeira execução)
Crie um `AccountBootstrapConfig`:

- Menu: `Create → Account → Bootstrap Config`
- Configure:
  - `StartingMoney`, `StartingEnergy`
  - `StartingUnits` (lista de UnitData iniciais)

O bootstrap é aplicado **apenas quando não existe** `account.json` ainda.

#### Opção B — Perfil de debug (simular contas diferentes)
Crie um ou mais `AccountProfile`:

- Menu: `Create → Account → Profile`
- Configure `Money`, `Energy`, `OwnedUnits` e `OwnedPets` (arraste os assets).

No `AccountManager`, habilite:
- `useDebugProfile = true`
- `debugProfile = (seu perfil)`

## 2) Build Settings (obrigatório)
Em `File → Build Settings…`, adicione as cenas:

1. `HubScene`
2. `PreparationScene`
3. Todas as cenas de batalha referenciadas em `LevelData.SceneName`

## 3) Cena: HubScene
Objetivo: mostrar fases disponíveis e ir para a Preparação.

### 3.1 Managers
Crie um GameObject vazio `_Managers` e adicione:

- `AccountManager`
  - `bootstrapConfig` (opcional, recomendado)
  - `useDebugProfile` e `debugProfile` (opcional)
- `GameFlowManager`

### 3.2 UI mínima
Crie um `Canvas` e, dentro dele:

- Dois `Text` (UnityEngine.UI):
  - Money
  - Energy
- Um container para botões de fase (ex.: `VerticalLayoutGroup`)
- Um prefab de `Button` (com um `Text` filho)

### 3.3 Controller
Crie um GameObject `_Hub` e adicione `HubSceneController`.
No Inspector, configure:

- `levelCatalog` (LevelCatalog)
- `levelsContainer` (container)
- `levelButtonPrefab` (prefab do botão)
- `moneyText`, `energyText`
- `preparationSceneName = PreparationScene`

## 4) Cena: PreparationScene
Objetivo: selecionar units e posicionar formação 3×3.

### 4.1 UI — lista de units
Crie um `Canvas` e:

- Container para lista de units possuídas (ex.: `VerticalLayoutGroup`)
- Prefab de `Button` para cada unit (com `Text` filho)

### 4.2 UI — grid 3×3
Crie um grid 3×3 (ex.: `GridLayoutGroup`) com 9 `Button`.
Em cada slot, adicione `FormationSlotUI` e configure:

- `GridPos` (x,y) de (0..2, 0..2)
- `iconImage`: um `Image` associado ao slot (pode ser filho)

### 4.3 Botão de iniciar
Crie um `Button` “Start Battle”.

### 4.4 Controller
Crie um GameObject `_Preparation` e adicione `PreparationSceneController`.
No Inspector, configure:

- `unitCatalog` (UnitCatalog)
- `ownedUnitsContainer`, `ownedUnitButtonPrefab`
- `formationSlots` (arraste os 9 slots)
- `startBattleButton`
- `maxUnitsToBring` (ex.: 3)

## 5) Cena(s): Batalha
Objetivo: spawn do player (seleção) + spawn dos inimigos (LevelData) + iniciar combate.

### 5.1 Grid
Garanta que existe um `GridMap` na cena.

O `BattleLevelBuilder` aplica automaticamente o `LevelData.PhaseMap` no `GridMap` e regenera o grid.

Observação: o `PhaseMap` pode conter `unitSpawns` fixos. Como o spawn agora é dinâmico via `BattleLevelBuilder`, recomenda-se:
- durante testes: deixar `BattleLevelBuilder.clearExistingUnits = true`
- mais tarde: remover/ignorar `unitSpawns` do `PhaseMap` para evitar confusão (ou ativar/desativar no `GridMap.spawnUnitsFromPhaseMap`).

### 5.2 Managers obrigatórios
Na cena de batalha, crie:

- `_PhaseManager` com `PhaseManager`
- `_GraveyardManager` com `GraveyardManager`
- `_BattleLevelBuilder` com `BattleLevelBuilder`
  - `unitCatalog` (UnitCatalog)
  - `autoStartCombatAfterBuild = true`

### 5.3 CombatInitializer
Se houver um `CombatInitializer` na cena:
- Defina `autoStart = false`.

O `BattleLevelBuilder` chama `CombatInitializer.StartCombat()` ao final do build.

### 5.4 Retorno ao Hub (opcional)
No `PhaseManager`:

- `autoReturnToHub = true`
- `hubSceneName = HubScene`
- `returnDelaySeconds` (ex.: 1.0)

## 6) Verificação rápida (checklist)

- HubScene abre e lista fases
- Clicar numa fase abre PreparationScene
- PreparationScene lista units (da conta) e permite posicionar no 3×3 (ícone aparece)
- Start Battle carrega a cena de batalha configurada no LevelData
- Na batalha, player spawna em posições escolhidas; inimigos spawnam conforme `Enemies`
- Ao vencer, recompensa é aplicada em `AccountManager` (Money/Energy) e salva
- Ao voltar ao Hub, os valores atualizados aparecem

## 7) Reset de teste (limpar save)
O save fica em `Application.persistentDataPath/account.json`.

Para simular “primeira execução” novamente:
- apague o arquivo `account.json`, ou
- ative `useDebugProfile` e escolha um `AccountProfile`.
