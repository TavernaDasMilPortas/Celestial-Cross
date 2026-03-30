# Plano de Implementação: Fluxo de Jogo e Gerenciamento de Dados

Este documento detalha a arquitetura e as tarefas para implementar o fluxo de jogo principal, desde o menu até a conclusão de uma fase e o retorno ao hub.

## Arquitetura Geral

Usaremos uma abordagem de cenas dedicadas com gerenciadores de dados persistentes que sobrevivem às trocas de cena.

```mermaid
graph TD
    subgraph Jogo
        A[Cena de Inicialização/Menu] --> B{Cena do Hub/Seleção de Fase};
        B --> C{Cena de Preparação};
        C --> D[Cena de Batalha];
        D --> B;
    end

    subgraph Sistemas Persistentes (DontDestroyOnLoad)
        E[AccountManager]
        F[GameFlowManager]
    end

    A -- Carrega --> E;
    A -- Carrega --> F;
    B -- Lê de --> E;
    B -- Escreve em --> F;
    C -- Lê de --> E;
    C -- Escreve em --> F;
    D -- Lê de --> F;
    D -- Escreve em --> E;
```

- **AccountManager**: Persiste os dados do jogador (dinheiro, unidades possuídas) entre sessões de jogo.
- **GameFlowManager**: Persiste os dados da sessão atual (fase escolhida, unidades selecionadas para a batalha) entre as cenas.

Componentes adicionados para suportar o fluxo:

- **UnitCatalog**: Mapeia `UnitID -> Prefab` (e opcionalmente `UnitData`) para permitir spawn e UI.
- **LevelCatalog**: Lista ordenada de `LevelData` disponíveis no Hub.
- **AccountBootstrapConfig**: Configuração de conta inicial (units/pets/dinheiro/energia) para facilitar testes.
- **AccountProfile (debug)**: Perfil alternativo para simular contas diferentes no Inspector.

---

## Divisão de Tarefas

### Parte 1: Sistemas de Gerenciamento Core

O foco é criar a estrutura de dados que irá transitar entre as cenas.

- **Tarefa 1.1: Criar `GameFlowManager.cs`**:
  - Deve ser um Singleton persistente (`DontDestroyOnLoad`).
  - Conterá campos para os dados da sessão: `selectedLevel`, `selectedUnitIDs`, `unitInitialPositions`.

- **Tarefa 1.2: Criar `LevelData.cs`**:
  - Um `ScriptableObject` para definir os dados de uma fase: nome da cena, inimigos, posições dos inimigos, recompensas.

- **Tarefa 1.2b: Criar `UnitCatalog.cs` e `LevelCatalog.cs`**:
  - `UnitCatalog`: resolve o prefab a instanciar a partir de um `UnitID`.
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
  - Permite ao jogador selecionar unidades e posicioná-las em um grid 3x3.
  - Ao confirmar, salva a seleção no `GameFlowManager` e carrega a cena de batalha correspondente.

  Implementação de script:
  - `PreparationSceneController` + `FormationSlotUI`: seleção por clique e posicionamento em 3x3.

- **Tarefa 2.3: Modificar a Cena de Batalha**:
  - Criar um `LevelBuilder` (ou usar o `PhaseManager`) que, no `Start`, lê os dados do `GameFlowManager`.
  - Instancia as unidades do jogador e dos inimigos nas posições corretas.

  Implementação de script:
  - `BattleLevelBuilder`: instancia units e sincroniza ocupação no `GridMap`.

### Parte 3: Testes e Refinamento

Garantir que o ciclo completo funcione sem problemas.

- **Tarefa 3.1: Testar o Fluxo Completo**:
  - Jogar desde o Hub, selecionar uma fase, preparar o time, lutar, vencer e verificar se as recompensas foram salvas e se o jogo retorna ao Hub.

- **Tarefa 3.2: Adicionar Unidades Iniciais à Conta**:
  - Criar um mecanismo (pode ser temporário) para adicionar algumas unidades à conta do jogador para que a cena de preparação seja funcional.

  Implementação:
  - `AccountBootstrapConfig` aplicado no primeiro load (sem `account.json`).
