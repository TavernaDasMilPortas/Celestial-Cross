# Plano de Implementação: Fluxo de Jogo e Gerenciamento de Dados

Este documento detalha a arquitetura e as tarefas para implementar o fluxo de jogo principal, desde o menu até a conclusão de uma fase e o retorno ao hub.

## Arquitetura Geral

Usaremos uma abordagem de cenas dedicadas com gerenciadores de dados persistentes que sobrevivem às trocas de cena.

```mermaid
graph TD
    subgraph Jogo
  A[Cena de Inicialização/Menu] --> C[Cena de Preparação];
  C --> D[Cena de Batalha];
  D --> A;
    end

    subgraph Sistemas Persistentes (DontDestroyOnLoad)
        E[AccountManager]
        F[GameFlowManager]
    end

    A -- Carrega --> E;
    A -- Carrega --> F;
    C -- Lê de --> E;
    C -- Escreve em --> F;
    D -- Lê de --> F;
    D -- Escreve em --> E;
```

- **AccountManager**: Persiste os dados do jogador (dinheiro, unidades possuídas) entre sessões de jogo.
- **GameFlowManager**: Persiste os dados da sessão atual (fase escolhida, unidades selecionadas para a batalha) entre as cenas.

Componentes adicionados para suportar o fluxo:

- **UnitCatalog**: Mapeia `UnitID -> UnitData` para permitir UI e seleção (não há mais prefab por UnitID).
- **LevelCatalog**: Lista ordenada de `LevelData` disponíveis no Hub.
- **AccountBootstrapConfig**: Configuração de conta inicial (units/pets/dinheiro/energia) para facilitar testes.
- **AccountProfile (debug)**: Perfil alternativo para simular contas diferentes no Inspector.

Nota (protótipo): o fluxo acima ignora o Hub e usa um `LevelData` padrão definido no Menu.

---

## Divisão de Tarefas

### Parte 0: Protótipo Menu → Preparação → Combate (passo a passo)

Objetivo: ter um “Start” rápido que já leva para a preparação e depois para a batalha.

1) Criar a cena `MenuScene`
  - Adicione um GameObject `_Managers` com:
    - `AccountManager` (opcional: `bootstrapConfig`)
    - `GameFlowManager`
  - Crie um `Canvas` com um `Button` “Start”.
  - Adicione um GameObject `_Menu` com `StartMenuController` e configure:
    - `defaultLevel` (um `LevelData`)
    - `preparationSceneName = PreparationScene`
    - `startButton` apontando para o botão.

2) Criar/configurar um `LevelData` (para o protótipo)
  - `SceneName`: nome exato da cena de batalha.
  - `PhaseMap`: PhaseMap do grid.
  - `Waves`: pode ter 1 ou mais waves.
    - por enquanto o jogo usa apenas a Wave 0 para spawn inicial.
  - (Legacy) `Enemies`: só use se não quiser preencher `Waves`.

3) Criar a cena `PreparationScene`
  - UI para listar unidades possuídas e permitir selecionar até `maxUnitsToBring`.
  - `PreparationSceneController` deve estar configurado com `UnitCatalog`.
  - Ao clicar “Start Battle”, ele salva `SelectedUnitIDs` no `GameFlowManager` e carrega `SelectedLevel.SceneName`.

4) Criar a cena de batalha (a que está em `LevelData.SceneName`)
  - Coloque um `GridMap` (sem precisar setar PhaseMap manualmente; o `BattleLevelBuilder` aplica o do `LevelData`).
  - Coloque `BattleLevelBuilder` e configure os prefabs “molde”:
    - `playerUnitMold`
    - `enemyUnitMold`
    - `placementManager`
  - Coloque `PlacementManager` e configure:
    - `playerUnitMoldPrefab`
    - `unitCatalog`
    - `tileLayer`
    - `placementActionBar`
  - Crie um botão de UI “Confirmar posicionamento” que chama `PlacementManager.EndPlacementPhase()`.
  - Marque os tiles iniciais do player via `TileDefinition.isPlayerSpawnZone = true`.
  - Coloque `CombatInitializer` (opcional) e `TurnManager`.
  - Coloque `PhaseManager` para vitória/derrota e recompensas.

5) Build Settings
  - Adicione: `MenuScene`, `PreparationScene`, e a(s) cena(s) de batalha.

### Parte 1: Sistemas de Gerenciamento Core

O foco é criar a estrutura de dados que irá transitar entre as cenas.

- **Tarefa 1.1: Criar `GameFlowManager.cs`**:
  - Deve ser um Singleton persistente (`DontDestroyOnLoad`).
  - Conterá campos para os dados da sessão: `SelectedLevel`, `SelectedUnitIDs`.
  - Durante a batalha, a formação colocada pode ser armazenada em `PlayerFormation` (runtime).

- **Tarefa 1.2: Criar `LevelData.cs`**:
  - Um `ScriptableObject` para definir os dados de uma fase: nome da cena, inimigos, posições dos inimigos, recompensas.
  - Suporte a waves/ondas: `Waves` (lista), usando Wave 0 como spawn inicial.

- **Tarefa 1.2b: Criar `UnitCatalog.cs` e `LevelCatalog.cs`**:
  - `UnitCatalog`: resolve `UnitData` a partir de um `UnitID` (para UI/seleção).
  - `LevelCatalog`: define a lista de fases exibida no Hub.

- **Tarefa 1.3: Integrar `PhaseManager` e `AccountManager`**:
  - Modificar `PhaseManager` para que, ao final da fase, ele use o `AccountManager.Instance` para adicionar as recompensas (dinheiro, energia) à conta do jogador e salvar.

### Parte 2: Implementação das Cenas

Com os sistemas prontos, criamos as cenas e a UI para interagir com eles.

- **Tarefa 2.1: Criar Cena de Hub (`HubScene`)**:
  - UI básica para mostrar as fases disponíveis (lendo os `LevelData` de uma pasta).
  - Ao clicar em uma fase, armazena o `LevelData` no `GameFlowManager` e carrega a `PreparationScene`.

  Implementação de script:
  - `HubSceneController`: instancia botões de fase e chama `SceneManager.LoadScene`.

- **Tarefa 2.2: Criar Cena de Preparação (`PreparationScene`)**:
  - UI que lê as unidades do `AccountManager`.
  - Permite ao jogador selecionar unidades (até `maxUnitsToBring`).
  - Ao confirmar, salva `SelectedUnitIDs` no `GameFlowManager` e carrega a cena de batalha correspondente.

  Implementação de script:
  - `PreparationSceneController`: seleção por clique (sem grid 3×3).

- **Tarefa 2.3: Modificar a Cena de Batalha**:
  - Criar um `LevelBuilder` (ou usar o `PhaseManager`) que, no `Start`, lê os dados do `GameFlowManager`.
  - Instancia os inimigos do `LevelData`.
  - Inicia uma fase de posicionamento do player (antes de chamar `StartCombat`).

  Implementação de script:
  - `BattleLevelBuilder`: spawna inimigos e aguarda `PlacementManager` finalizar para iniciar o combate.
  - `PlacementManager`: cria preview, permite colocar units em tiles `isPlayerSpawnZone`, e ao confirmar dispara o fim do posicionamento.

### Parte 3: Testes e Refinamento

Garantir que o ciclo completo funcione sem problemas.

- **Tarefa 3.1: Testar o Fluxo Completo**:
  - Jogar desde o Hub, selecionar uma fase, preparar o time, lutar, vencer e verificar se as recompensas foram salvas e se o jogo retorna ao Hub.

- **Tarefa 3.2: Adicionar Unidades Iniciais à Conta**:
  - Criar um mecanismo (pode ser temporário) para adicionar algumas unidades à conta do jogador para que a cena de preparação seja funcional.

  Implementação:
  - `AccountBootstrapConfig` aplicado no primeiro load (sem `account.json`).
