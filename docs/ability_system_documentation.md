# Documentação do Sistema de Habilidades e Ações — Celestial-Cross

> Versão pós-migração. Cobre: `AbilityGraphSO`, `AbilityGraphInterpreter`, `CombatContext`, `PassiveManager`, `PreparedActionManager`, Nodes, Variáveis e Condições.
> O sistema legado `AbilityBlueprint` foi **completamente removido** do código-fonte. Este documento descreve o sistema unificado baseado exclusivamente em grafos.

---

## Índice

1. [Visão Geral da Arquitetura](#1-visão-geral-da-arquitetura)
2. [AbilityGraphSO — O ScriptableObject de Dados](#2-abilitygraphso--o-scriptableobject-de-dados)
3. [CombatContext — O Estado da Execução](#3-combatcontext--o-estado-da-execução)
4. [AbilityGraphInterpreter — O Motor de Execução](#4-abilitygraphinterpreter--o-motor-de-execução)
5. [Catálogo de Nodes](#5-catálogo-de-nodes)
6. [Sistema de Variáveis](#6-sistema-de-variáveis)
7. [Sistema de Condições e Status](#7-sistema-de-condições-e-status)
8. [PassiveManager — Ciclo de Vida das Passivas e Condições](#8-passivemanager--ciclo-de-vida-das-passivas-e-condições)
9. [Habilidades que Modificam Outras Habilidades](#9-habilidades-que-modificam-outras-habilidades)
10. [PreparedActionManager — Ações com Delay](#10-preparedactionmanager--ações-com-delay)
11. [Targeting — Seleção de Alvos](#11-targeting--seleção-de-alvos)
12. [Scaling de Dano e Cura](#12-scaling-de-dano-e-cura)
13. [Fluxo Completo de Execução](#13-fluxo-completo-de-execução)
14. [Erros Comuns e Armadilhas](#14-erros-comuns-e-armadilhas)

---

## 1. Visão Geral da Arquitetura

O sistema de habilidades do Celestial-Cross é baseado em um **grafo de nós** serializado em `ScriptableObject`. Cada habilidade é um arquivo `.asset` do tipo `AbilityGraphSO`, composto por nós (`AbilityNodeData`) conectados por links (`NodeLinkData`).

A pilha funcional tem 4 camadas:

```
[AbilityGraphSO]          ← Dados/Configuração (Designer)
      |
[AbilityGraphInterpreter] ← Motor de Execução (Runtime, Singleton)
      |
[CombatContext]           ← Estado vivo da execução (passado entre nós)
      |
[PassiveManager]          ← Gerencia condições ativas e dispara hooks
```

**Tipos de habilidade** (definidos no `StartNode`):
- **Active**: Ação manual do jogador (invocada por `OnManualCast`).
- **Passive**: Habilidade inata, dispara via hooks do ciclo de turnos.
- **Condition**: Buff/Debuff aplicado sobre uma unidade com duração.

---

## 2. AbilityGraphSO — O ScriptableObject de Dados

**Arquivo:** [`AbilityGraphSO.cs`](file:///c:/Users/Rubens/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Abilities/Graph/AbilityGraphSO.cs)

Todo grafo de habilidade é um `ScriptableObject` com os seguintes grupos de dados:

### Estrutura Principal

| Campo | Tipo | Descrição |
|---|---|---|
| `abilityName` | `string` | Nome exibido na UI. |
| `abilityIcon` | `Sprite` | Ícone da habilidade na UI. |
| `abilityDescription` | `string` | Texto descritivo para o jogador. |
| `displayRange` | `int` | Alcance visual para preview no grid. |
| `NodeData` | `List<AbilityNodeData>` | Todos os nós deste grafo. |
| `NodeLinks` | `List<NodeLinkData>` | Conexões entre os nós (GUIDs). |
| `Dependencies` | `List<Dependency>` | Assets externos referenciados por ID (ex: `AreaPatternData`, grafos de condição). |
| `Variables` | `List<GraphVariable>` | Variáveis declaradas no Blackboard do grafo. |
| `MaxLevel` | `int` | Nível máximo da habilidade (para `LevelBranchNode`). |
| `aiHint` | `AIAbilityHint` | Dicas para o Behavior Tree da IA. |

### Serialização dos Nós

Cada `AbilityNodeData` contém:
- `Guid`: ID único do nó (usado em links).
- `NodeType`: String que identifica o tipo (ex: `"DamageEffectNode"`).
- `JsonData`: Dados dinâmicos do nó serializados via `JsonUtility.ToJson`. Cada tipo de nó tem sua própria classe de dados (ex: `DamageNodeData`, `TargetNodeData`).
- `areaPattern`: Referência legacy a `AreaPatternData` diretamente no nó. **Deprecated** — use `Dependencies`.

> **⚠️ Importante:** `JsonUtility` não serializa referências a `UnityEngine.Object`. Por isso, assets como `AreaPatternData` e grafos de condição são referenciados via `Dependencies` (usando um `id` string) e não embutidos no `JsonData`.

### Helpers de Metadados

O SO expõe helpers que lêem os dados de nós específicos:
- `GetAbilityType()` → lê o `StartNode`.
- `GetDuration()` → lê o `DurationNode`.
- `GetCanStack()` / `GetMaxStacks()` → lê `StatModifierEffectNode` ou `ApplyModifierNode`.
- `GetIsPersistent()` → verifica se `DurationNode` é do tipo `Infinite`.
- `GetIsBuff()` → lê o campo `isBuff` do `StartNode`.
- `GetAsset<T>(id)` → busca um asset nas `Dependencies` por ID.
- `GetRamificationTiers()` → retorna opções de ramificação para a UI da Skill Tree.

---

## 3. CombatContext — O Estado da Execução

**Arquivo:** [`CombatHook.cs`](file:///c:/Users/Rubens/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Combat/CombatHook.cs)

O `CombatContext` é o **objeto de estado compartilhado** que trafega entre todos os nós durante uma execução de grafo. Em vez de passar dezenas de parâmetros, o interpretador cria um único contexto e o passa por referência para cada `ProcessNode`.

### Campos do CombatContext

| Campo | Tipo | Descrição |
|---|---|---|
| `source` | `Unit` | Quem está conjurando/disparando a habilidade. |
| `target` | `Unit` | Alvo principal (pode ser `null`). |
| `targets` | `List<Unit>` | Lista completa de alvos afetados. Alimentada pelo `TargetNode`. |
| `amount` | `int` | Valor calculado (dano, cura). Geralmente preenchido pelo nó de efeito. |
| `isCritical` | `bool` | Flag de acerto crítico — definida pelo `DamageProcessor`. |
| `action` | `IUnitAction` | Referência à ação do sistema de turno (pode ser `null`). |
| `abilityLevel` | `int` | Nível da habilidade — usado no `LevelBranchNode`. |
| `slotId` | `string` | Slot em que a habilidade está equipada (ex: `"Skill_1"`). Permite isolar variáveis por slot. |
| `targetPos` | `Vector2Int?` | Posição no grid do alvo selecionado. Nulo se não houver seleção posicional. |
| `hasTriggeredPetAnimation` | `bool` | Flag one-shot para disparar animação do pet. |
| `Variables` | `Dictionary<string, float>` | Blackboard local da execução (temporário). |
| `loopCounters` | `Dictionary<string, int>` | Contadores de loop por GUID de `LoopNode`. |
| `conditionPool` | `List<AbilityConditionData>` | Pool de condições (atualmente subutilizado). |

### Ciclo de Vida

1. O contexto é **criado** no início de `ExecuteGraphCoroutine` com `source` e, se disponível, `target`.
2. As variáveis do Blackboard são **inicializadas** a partir de `graph.Variables`.
3. O contexto é **passado e modificado** por cada nó durante o processamento.
4. Ao final, o contexto é **descartado** (não persiste entre execuções).

### Clone

`CombatContext.Clone()` cria uma cópia profunda (deep copy) do contexto. Usado pelo `ScheduleExecutionNode` para capturar o estado atual antes de suspender a coroutine.

---

## 4. AbilityGraphInterpreter — O Motor de Execução

**Arquivo:** [`AbilityGraphInterpreter.cs`](file:///c:/Users/Rubens/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Abilities/Graph/Runtime/AbilityGraphInterpreter.cs)

Singleton `MonoBehaviour` (criado automaticamente via `DontDestroyOnLoad`) responsável por percorrer o grafo de nós e executar cada um.

### 4.1. Pontos de Entrada

| Método | Modo | Uso |
|---|---|---|
| `ExecuteGraphCoroutine(...)` | **Assíncrono** | Ações ativas do jogador ou IA. Suporta `yield return` para delays e animações. |
| `ExecuteGraphSync(...)` | **Síncrono** | Passivas disparadas por hooks de combate. Roda tudo no mesmo frame. |
| `ExecuteFromNode(...)` | **Assíncrono** | Retoma uma execução suspensa por `ScheduleExecutionNode`. Recebe o nó de partida e o snapshot do contexto. |

### 4.2. Lógica de Entrada no Grafo

Ao iniciar uma execução, o interpretador determina **qual nó é o ponto de entrada** com base no `CombatHook` recebido:

```
OnManualCast  → procura nó do tipo "StartNode"
Outro hook    → procura nó do tipo "TriggerNode" com trigger == hook
```

Se nenhum nó de entrada for encontrado:
- Para `OnManualCast`: log de erro — grafo mal configurado.
- Para outros hooks: saída silenciosa — grafo simplesmente não reage a esse hook.

### 4.3. O Loop de Execução

```
while (nó atual != null):
    1. Chama ProcessNode(nó atual) → retorna "nextPort" (string)
    2. Se nextPort == "Scheduled": break (coroutine suspensa)
    3. Busca em NodeLinks: link onde BaseNodeGuid == guid atual E PortName == nextPort
    4. Se encontrou link → nó atual = link.TargetNodeGuid
    5. Se não encontrou → nó atual = null (fim do caminho)
```

A **porta de saída** padrão é sempre `"Out"`. Nós de decisão podem retornar portas alternativas como `"True"`, `"False"`, `"Loop"`, `"Exit"`, `"Level 2"`, ou nomes de ramificação personalizados.

### 4.4. Diferenças entre Assíncrono e Síncrono

| Aspecto | Assíncrono (`ExecuteGraphCoroutine`) | Síncrono (`ExecuteGraphSync`) |
|---|---|---|
| Targeting Manual | Aguarda input do jogador (`WaitUntil`). | Usa `AutoTargetResolver` diretamente. |
| Dano/Cura | `ExecuteDamageRoutine` — delay de 0.2s entre hits múltiplos. | `ExecuteDamage` — sem delay. |
| Movimento | Animação via DOTween com duração calculada por velocidade. | Não suportado. |
| `ScheduleExecution` | Suspende a coroutine e transfere para `PreparedActionManager`. | Não suportado. |

---

## 5. Catálogo de Nodes

**Arquivo dos dados:** [`AbilityNodeRuntimeData.cs`](file:///c:/Users/Rubens/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Abilities/Graph/Runtime/AbilityNodeRuntimeData.cs)

### 5.1. Controle de Entrada

#### `StartNode` (dados: `StartNodeData`)
Ponto de entrada de habilidades ativas.
- `type`: `AbilityType` — `Active`, `Passive`, ou `Condition`.
- `subtype`: `AbilitySubtype` — subtipo da habilidade.
- `isBuff`: Define se a habilidade (quando usada como Condition) é um buff ou debuff.

#### `TriggerNode` (dados: `TriggerNodeData`)
Ponto de entrada de habilidades passivas/condições reativas.
- `trigger`: `CombatHook` — o gatilho que ativa este grafo (ex: `OnAfterDealDamage`).

Múltiplos `TriggerNode`s podem existir no mesmo grafo para reações a múltiplos hooks.

---

### 5.2. Targeting

#### `TargetNode` (dados: `TargetNodeData`)
Resolve e popula `context.targets` com as unidades alvo.

| Campo | Tipo | Descrição |
|---|---|---|
| `reusePrevious` | `bool` | Se `true` e `context.targets` já tem alvos, pula o processo de targeting. |
| `sourceType` | `GraphTargetSourceType` | `Manual` (input do jogador) ou `AutoStrategy` (IA/passiva). |
| `mode` | `GraphTargetMode` | `Single` ou `Area`. |
| `range` | `int` | Alcance base em tiles. |
| `rangeVariable` | `string` | Nome de variável de contexto para usar como range (sobrescreve `range`). |
| `useExtraRangeVariable` | `bool` | Se `true`, lê `ExtraRange` da `VariableStore` da unidade e soma ao range. |
| `factionType` | `GraphFactionType` | Filtra por `Ally`, `Enemy` ou `Any`. |
| `strategy` | `GraphAutoStrategyType` | Para `AutoStrategy`: `ClosestUnit`, `FarthestUnit`, `LowestAttribute`, `HighestAttribute`, `Self`, `RandomTarget`. |
| `multipleTargets` | `bool` | Permite selecionar múltiplos alvos manualmente. |
| `allowSameTargetMultipleTimes` | `bool` | Permite que o mesmo alvo seja adicionado múltiplas vezes. |
| `maxTargets` | `int` | Limite de alvos. |
| `patternReferenceId` | `string` | ID de `AreaPatternData` nas Dependencies para modo `Area`. |
| `autoRotate` | `bool` | Se o padrão de área rotaciona automaticamente. |
| `preferredDirection` | `Direction` | Direção padrão do padrão de área. |

> **Nota sobre IA:** Se o `CombatContext` já tiver `targets` preenchidos pelo Behavior Tree da IA ao chegar no `TargetNode`, esses alvos têm prioridade sobre a lógica de auto-strategy. O nó apenas expande a área se necessário.

---

### 5.3. Efeitos de Combate

#### `DamageEffectNode` (dados: `DamageNodeData`)
Causa dano nos alvos de `context.targets`.

| Campo | Tipo | Descrição |
|---|---|---|
| `scalings` | `List<StatScalingData>` | Scalings de atributos (veja seção 12). Se vazio, usa `source.Stats.attack` como fallback. |
| `variableReference` | `string` | Nome de variável de contexto para usar como **multiplicador** do valor base. |
| `scaleWithDistance` | `bool` | Ativa scaling por distância. |
| `distanceScaleFactor` | `float` | Redução por tile de distância. |

O cálculo final: `floor(baseValue × multiplier)`. Cada alvo recebe um `CombatContext` separado e o dano é processado via `DamageProcessor.ProcessAndApplyDamage()`.

#### `HealEffectNode` (dados: `HealNodeData`)
Cura os alvos em `context.targets`.

| Campo | Tipo | Descrição |
|---|---|---|
| `scalings` | `List<StatScalingData>` | Scalings de atributos. Se vazio, usa `target.Health.MaxHealth` como fallback. |
| `variableReference` | `string` | Multiplicador via variável de contexto. |
| `canCrit` | `bool` | Se a cura pode ser um acerto crítico. |
| `allowOverheal` | `bool` | Se pode ultrapassar o HP máximo. |

Processado via `DamageProcessor.ProcessAndApplyHeal()`.

#### `StatModifierEffectNode` (dados: `StatModifierNodeData`)
Aplica um buff/debuff de atributo **gerado dinamicamente** nos alvos. Chama `PassiveManager.ApplyStatModifierCondition()` diretamente, sem intermediários.

| Campo | Tipo | Descrição |
|---|---|---|
| `stats` | `List<StatEntry>` | Lista de modificadores. Cada entrada tem `statTypeName` (string do enum `StatType`), `value`, `bonusType` (`Flat`/`Percent`) e `valueMode` (`Value`/`Variable`). |
| `isBuff` | `bool` | Classifica o modificador como buff ou debuff (afeta remoção por `CleanseStatusNode`). |
| `canStack` | `bool` | Se pode acumular stacks. |
| `maxStacks` | `int` | Limite de stacks. |

Busca um `DurationNode` conectado à porta `"Duration"` para definir a duração. Se não encontrado, usa duração padrão de 1 turno.

> **Implementação:** O interpretador chama `PassiveManager.ApplyStatModifierCondition()` passando os dados do nó diretamente. O `PassiveManager` armazena internamente como um `RuntimeStatCondition` (classe interna). O nome estável (`GraphBuff_{graph.name}_{node.Guid[:4]}`) é a chave de identificação para evitar aplicações duplicadas do mesmo buff.

#### `ApplyModifierNode` (dados: `ApplyModifierNodeData`)
Aplica uma **condição baseada em outro grafo** (um `AbilityGraphSO` do tipo `Condition`) nos alvos.

| Campo | Tipo | Descrição |
|---|---|---|
| `modifierId` | `string` | ID do grafo-condição nas `Dependencies`. |
| `stacks` | `int` | Quantos stacks aplicar de uma vez. |
| `canStack` | `bool` | Relevante para o lookup (mas o controle real é no grafo da condição). |
| `maxStacks` | `int` | Idem. |

Fluxo:
1. Busca o `AbilityGraphSO` nas `Dependencies` pelo `modifierId`.
2. Verifica `isBuff` do grafo de condição.
3. Se for **debuff**, passa por `EffectResistanceCheck.ShouldApplyEffect()`.
4. Chama `PassiveManager.ApplyGraphCondition()` no alvo.

> **⚠️ Inconsistência observada:** Os campos `canStack` e `maxStacks` em `ApplyModifierNodeData` não são usados na implementação atual — o controle de stack é lido diretamente do `AbilityGraphSO` de condição via `conditionGraph.GetCanStack()`. Esses campos no nó são redundantes.

#### `ModifyAPNode` (dados: `ModifyAPNodeData`)
Modifica os Pontos de Ação (AP) do **conjurador** (`context.source`).

| Campo | Tipo | Descrição |
|---|---|---|
| `amount` | `int` | Quantidade a adicionar (negativo = reduzir). |
| `modifyMax` | `bool` | Se `true`, modifica `MaxAP`. Se `false`, modifica `CurrentAP`. |

#### `CostNode` (dados: `CostNodeData`)
Desconta recursos da habilidade. Atualmente loga o custo, mas **a dedução real de Mana/Stamina ainda está pendente de implementação** (`// TODO` no código).

| Campo | Tipo | Descrição |
|---|---|---|
| `manaCost` | `int` | Custo em Mana. |
| `manaVariable` | `string` | Variável de contexto como override de `manaCost`. |
| `staminaCost` | `int` | Custo em Stamina. |
| `staminaVariable` | `string` | Variável de contexto como override de `staminaCost`. |

#### `SacrificeHealthNode` (dados: `SacrificeHealthNodeData`)
O **conjurador** sofre dano direto na própria barra de HP. Não passa pelo sistema de resistência/defesa.

| Campo | Tipo | Descrição |
|---|---|---|
| `usePercentage` | `bool` | Se `true`, o valor é `% do MaxHealth`. Se `false`, valor fixo. |
| `amount` | `float` | Valor do sacrifício. |
| `outputVariable` | `string` | Variável de contexto onde o HP sacrificado é armazenado (para uso posterior). |

#### `CleanseStatusNode` (dados: `CleanseStatusNodeData`)
Remove condições ativas dos alvos via `PassiveManager`.

| Campo | Tipo | Descrição |
|---|---|---|
| `allPositive` | `bool` | Remove todos os buffs. |
| `allNegative` | `bool` | Remove todos os debuffs. |

#### `MoveEffectNode` (dados: `MoveEffectNodeData`)
Move o conjurador ou os alvos no grid.

| Campo | Tipo | Descrição |
|---|---|---|
| `moveMode` | `MoveMode` | `MoveCaster` (move `source`) ou `MoveTarget` (move todos em `targets`). |
| `range` | `int` | Alcance do movimento. |
| `rangeVariable` | `string` | Override do range via variável de contexto. |
| `manualDestination` | `bool` | Se `true`, abre seleção de tile para o jogador. |
| `allowOccupiedTiles` | `bool` | Permite mover para tiles ocupados. |
| `moveType` | `MoveType` | `Push`, `Pull`, `DashToTarget`, `TeleportToTarget` — controla a easing da animação. |

Processo:
1. Se `manualDestination` e houver `context.targetPos` já definido, usa esse ponto.
2. Caso contrário, se manual, abre o `TargetSelector` para o jogador escolher um tile (com whitelist de tiles válidos).
3. Calcula caminho via `GridMap.FindPath()` e anima via DOTween.
4. Teleporte usa `DOScale` (desaparece/reaparece), Push usa `OutExpo`, Pull usa `InBack`.

#### `VfxNode` (dados: `VfxNodeData`)
Dispara um efeito visual/sonoro. **Implementação incompleta** — atualmente apenas loga o `vfxId`.

---

### 5.4. Controle de Fluxo

#### `ConditionalFlowNode`
Agrega múltiplas condições com lógica **AND**.
- Busca todos os links de entrada com `TargetPortName.StartsWith("Cond")`.
- Avalia cada nó-fonte como subprocesso.
- Retorna `"True"` apenas se **todos** retornarem `"True"` ou `"Bool Out"`.
- 0 condições conectadas = resultado `"True"` (vacuamente verdadeiro).

#### `LoopNode` (dados: `LoopNodeData`)
Cria um laço de repetição controlado por `context.loopCounters`.

| Campo | Tipo | Descrição |
|---|---|---|
| `iterations` | `int` | Número de iterações. |
| `iterationsVariable` | `string` | Override via variável de contexto. |

- Porta `"Loop"`: continua iterando.
- Porta `"Exit"`: saiu do laço.

O contador é resetado ao sair (`context.loopCounters[guid] = 0`), permitindo loops reutilizáveis em estruturas de grafo cíclicas.

#### `LimitPerTurnNode` (dados: `LimitPerTurnNodeData`)
Limita execuções de um trecho do grafo por turno. Armazena o contador em `VariableStore` usando a chave `Limit_{guid}_{RoundCounter}`.

| Campo | Tipo | Descrição |
|---|---|---|
| `maxExecutionsPerTurn` | `int` | Limite por turno. |

- Porta `"True"`: dentro do limite, execução permitida e contador incrementado.
- Porta `"False"`: limite atingido.

#### `LevelBranchNode` (dados: `LevelBranchNodeData`)
Redireciona o fluxo baseado em `context.abilityLevel`.
- Retorna a porta `"Level {N}"` (ex: `"Level 1"`, `"Level 2"`).

#### `RamificationNode` (dados: `RamificationNodeData`)
Redireciona o fluxo baseado na escolha de ramificação feita na Skill Tree do personagem.

| Campo | Tipo | Descrição |
|---|---|---|
| `tierIndex` | `int` | Qual tier de ramificação verificar. |
| `flows` | `List<RamificationFlowData>` | Lista de opções (`flowId`, `flowName`). |

Consulta `context.source.Loadout.branchSelections` para encontrar a escolha do jogador para este grafo/tier. Se nenhuma escolha for encontrada, retorna `"Base"`.

#### `RamificationSpecNode` (dados: `RamificationSpecNodeData`)
Nó descritivo conectado a cada porta de `RamificationNode`. Passthrough puro na execução — existe apenas para armazenar metadados de exibição (nome, descrição, ícone da opção).

#### `ScheduleExecutionNode` (dados: `ScheduleExecutionNodeData`)
Suspende a execução atual e agenda o restante do grafo para um turno futuro.

| Campo | Tipo | Descrição |
|---|---|---|
| `delayTurns` | `int` | Após quantos turnos retomar. |

Processo:
1. Encontra o nó seguinte (via link `"Out"`).
2. Cria um `Clone()` do `CombatContext` atual.
3. Registra a ação futura no `PreparedActionManager`.
4. Retorna a porta especial `"Scheduled"`, fazendo o interpretador **parar** a coroutine.

---

### 5.5. Nós Condicionais (Retornam Booleano)

Todos retornam `"True"` ou `"False"`. São alimentados por `ProcessNode` recursivamente ao serem conectados ao `ConditionalFlowNode`.

#### `AttributeConditionNode` (dados: `AttributeConditionNodeData`)
Testa um atributo de uma unidade contra um threshold.

| Campo | Tipo | Descrição |
|---|---|---|
| `targetToCheck` | `TargetType` | `Caster` ou `Target`. |
| `attribute` | `AttributeType` | `HP`, `Attack`, `Defense`. |
| `mode` | `ValueMode` | `Absolute` ou `Percentage`. |
| `comparison` | `Comparison` | `GreaterThan`, `LessThan`, `Equal`, `GreaterOrEqual`, `LessOrEqual`. |
| `threshold` | `float` | Valor de referência. |

#### `DistanceConditionNode` (dados: `DistanceConditionNodeData`)
Verifica a distância em Chebyshev (`max(|Δx|, |Δy|)`) entre `source` e o primeiro alvo em `targets`.

| Campo | Tipo | Descrição |
|---|---|---|
| `checkType` | `DistanceType` | `Min`, `Max`, ou `Exact`. |
| `distanceValue` | `int` | Valor de distância. |
| `checkFaction` | `bool` | Se verdadeiro, filtra por facção antes de checar distância. |
| `faction` | `FactionTarget` | `Ally` ou `Enemy`. |

> **Nota:** Usa Chebyshev (distância do rei no xadrez), não Manhattan.

#### `RangeConditionNode` (dados: `RangeConditionNodeData`)
Conta quantas unidades existem dentro de um raio de origem e compara com um valor.

| Campo | Tipo | Descrição |
|---|---|---|
| `origin` | `RangeOrigin` | `Caster` ou `Target`. |
| `range` | `int` | Raio de busca (em Chebyshev). |
| `filter` | `UnitFilter` | `Allies`, `Enemies`, ou `Both`. |
| `targetCount` | `int` | Valor de referência para comparação. |
| `comparison` | `Comparison` | `GreaterOrEqual`, `LessOrEqual`, `Exact`. |

> **⚠️ Performance:** Usa `FindObjectsByType<Unit>()` a cada avaliação. Em combates grandes, pode ser custoso.

#### `FactionConditionNode` (dados: `FactionConditionNodeData`)
Verifica se uma unidade é aliada ou inimiga do `source`.

| Campo | Tipo | Descrição |
|---|---|---|
| `target` | `TargetType` | `Caster` ou `Target`. |
| `faction` | `FactionTarget` | `Ally` ou `Enemy`. |

#### `SpeedAdvantageConditionNode` (dados: `SpeedAdvantageConditionNodeData`)
Compara a velocidade do `source` com a do primeiro alvo.

| Campo | Tipo | Descrição |
|---|---|---|
| `requiredDifference` | `int` | Diferença mínima de velocidade. |
| `greaterOrEqual` | `bool` | Se `true`: `source.speed >= target.speed + diff`. Se `false`: `abs(diff) >= required`. |

#### `TurnOrderConditionNode` (dados: `TurnOrderConditionNodeData`)
Verifica a posição do `source` na ordem de turnos.

| Campo | Tipo | Descrição |
|---|---|---|
| `type` | `OrderType` | `FirstInRound` — verifica se `source == TurnManager.RoundStartUnit`. |
| `specificIndex` | `int` | Reservado para lógica futura (não implementado). |

---

### 5.6. Nó de Duração

#### `DurationNode` (dados: `DurationNodeData`)
Não é um nó de fluxo — é conectado como **entrada** em nós de efeito como `StatModifierEffectNode` (via porta `"Duration"`).

| Campo | Tipo | Descrição |
|---|---|---|
| `type` | `DurationType` | `Turns` (duração limitada) ou `Infinite` (persistente). |
| `value` | `int` | Número de turnos (ignorado se `Infinite`). |

---

## 6. Sistema de Variáveis

O sistema tem **três camadas** de variáveis, com escopos e persistência distintos.

### 6.1. Blackboard do Contexto (`context.Variables`)

Dicionário `Dictionary<string, float>` local à execução.

- **Inicialização:** Populado com `initialValue` das `GraphVariable`s declaradas no `AbilityGraphSO` antes de começar o loop de nós.
- **Leitura:** `GetVariable(context, varName, defaultValue)` — retorna o valor ou o default se não existir.
- **Escrita:** `ModifyVariable(context, varName, operation, value)` via `VariableModifierNode`.
- **Escopo:** Existe apenas enquanto o grafo está em execução. **Não persiste.**
- **Variável especial:** `"stacks"` é automaticamente inserida pelo `PassiveManager` ao disparar hooks, permitindo que grafos de condições leiam quantos stacks estão ativos.

#### `VariableModifierNode` (dados: `VariableModifierNodeData`)
Modifica variáveis no blackboard.

| Campo | Tipo | Descrição |
|---|---|---|
| `variableName` | `string` | Nome da variável no contexto. |
| `operation` | `Operation` | `Set`, `Add`, `Multiply`, `Divide`. |
| `value` | `float` | Valor base. |
| `valueVariableReference` | `string` | Override do valor via outra variável de contexto. |
| `useCasterAttribute` | `bool` | Se `true`, usa `casterAttribute` como percentual do atributo do conjurador. |
| `casterAttribute` | `StatType` | Atributo do conjurador a usar como base (valor = `stat * (value / 100)`). |

### 6.2. VariableStore da Unidade (Persistente)

Armazenamento persistente acoplado à unidade durante todo o combate.

- **Global:** `GetGlobalVar(key)` / `SetGlobalVar(key, value)` — compartilhado entre todas as habilidades.
- **Por Slot:** `GetSlotVar(slotId, key)` / `SetSlotVar(slotId, key, value)` — isolado por slot, evita que habilidades de slots diferentes interfiram entre si.

#### `UnitVariableNode` (dados: `UnitVariableNodeData`)
Interface para ler/escrever na `VariableStore`.

| Campo | Tipo | Descrição |
|---|---|---|
| `variable` | `UnitVariable` | Enum com as variáveis disponíveis. |
| `operation` | `UnitVariableOperation` | `Get`, `Set`, `Add`, `Subtract`, `Multiply`, `Divide`. |
| `scope` | `UnitVariableScope` | `Global` ou `Slot`. |
| `value` | `float` | Valor para operações de escrita. |
| `contextVariableReference` | `string` | Override do `value` via variável de contexto. |
| `outputVariable` | `string` | Para operação `Get`: salva o valor lido no contexto com esse nome. |

#### Enum `UnitVariable` — Variáveis disponíveis

| Variável | Leitura | Escrita | Descrição |
|---|---|---|---|
| `Health` | ✅ | ❌ | HP atual da unidade. Read-only. |
| `Attack` | ✅ | ❌ | Ataque base. Read-only. |
| `Defense` | ✅ | ❌ | Defesa base. Read-only. |
| `Speed` | ✅ | ❌ | Velocidade base. Read-only. |
| `CriticalChance` | ✅ | ❌ | Chance de crítico. Read-only. |
| `CriticalDamage` | ✅ | ❌ | Dano de crítico. Read-only. |
| `EffectAccuracy` | ✅ | ❌ | Acurácia de efeito. Read-only. |
| `EffectResistance` | ✅ | ❌ | Resistência a efeitos. Read-only. |
| `BonusDamagePercent` | ✅ | ✅ | Bônus % de dano. |
| `DamageReductionPercent` | ✅ | ✅ | Redução % de dano recebido. |
| `HealingBonusPercent` | ✅ | ✅ | Bônus % de cura. |
| `ExtraRange` | ✅ | ✅ | Alcance adicional (lido pelo `TargetNode`). |
| `ExtraMoveRange` | ✅ | ✅ | Alcance de movimento adicional. |
| `Counter1/2/3` | ✅ | ✅ | Contadores livres para uso genérico. |

> **Detalhe importante:** `ExtraRange` é lido automaticamente pelo `TargetNode` quando `useExtraRangeVariable = true`, sem precisar de um `UnitVariableNode` explícito.

---

## 7. Sistema de Condições e Status

Uma **condição** é um `AbilityGraphSO` do tipo `Condition` que é aplicado sobre uma unidade com duração e stacks.

### 7.1. Tipos de Condição

| Tipo | Identificação | Duração padrão | Stacks |
|---|---|---|---|
| Passiva Inata | `IsPassive == true` | Persistente | Geralmente 1 |
| Buff | `GetIsBuff() == true` | Definida pelo `DurationNode` | Configurável |
| Debuff | `GetIsBuff() == false` | Definida pelo `DurationNode` | Configurável |

### 7.2. Resistência a Efeitos

Antes de aplicar um **debuff** (via `ApplyModifierNode`), o sistema consulta `EffectResistanceCheck.ShouldApplyEffect(source, target, hitRateOverride)`. Se resistido, o efeito não é aplicado e uma mensagem `[Resistido]` é logada.

Buffs **não** passam por verificação de resistência.

### 7.3. Duração e Tick

A duração é decrementada em **`TickConditionsOnTurnEnd()`** no `PassiveManager`, chamado no evento `OnTurnEnd` **apenas da unidade que possui a condição** (`TurnManager.CurrentUnit == unit`).

- Condições com `isPersistent = true` não fazem tick.
- Ao chegar em 0, a condição é removida da lista.

### 7.4. Stacks

- Ao re-aplicar uma condição já existente, o contador de stacks é incrementado (se `canStack = true`).
- O número de stacks é passado para o contexto como variável `"stacks"` ao disparar hooks.
- Stacks respeitam `maxStacks`.

---

## 8. PassiveManager — Ciclo de Vida das Passivas e Condições

**Arquivo:** [`PassiveManager.cs`](file:///c:/Users/Rubens/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Combat/PassiveManager.cs)

Componente acoplado a cada `Unit`. Gerencia duas listas internas:
- `activeGraphConditions` (`List<RuntimeGraphCondition>`): condições baseadas em `AbilityGraphSO` — aplicadas via `ApplyModifierNode`.
- `activeStatConditions` (`List<RuntimeStatCondition>`): modificadores de status gerados em runtime por `StatModifierEffectNode` — armazenam diretamente os dados de stat sem depender de um grafo externo.

### 8.1. Fontes de Passivas Estáticas

O `PassiveManager` reconhece passivas de múltiplas fontes (retornadas em `GetStaticPassives()`):

| Fonte | Descrição |
|---|---|
| Skill Tree (Slot 1 / Slot 2) | Habilidades equipadas como passivas nos slots. |
| Ataque Básico | Se o grafo de ataque básico for do tipo `Passive`. |
| Movimentação | Se o grafo de movimento for do tipo `Passive`. |
| Constelação | Passivas desbloqueadas via `ConstellationService`. |
| Pet (Grafo) | Passivas do pet equipado (via `PetSpeciesSO.AbilityGraphs`). |
| Set de Artefatos (Grafo) | Passivas de conjunto de artefatos (via `ArtifactSet.SetBonus.passiveGraph`). |

### 8.2. Evento de Hook

```
TurnManager.OnTurnStarted → HandleTurnStarted → TriggerHook(OnTurnStart)
TurnManager.OnTurnEnded  → HandleTurnEnded   → TriggerHook(OnTurnEnd) + TickConditions
TurnManager.OnRoundStarted → HandleRoundStarted → TriggerHook(OnRoundStart)
```

**`TriggerHook(hook, context)`** percorre `activeGraphConditions` em ordem. Para cada condição ativa cujo grafo não está em execução (guard anti-recursão), chama `ExecuteGraphForHook()` que dispara `ExecuteGraphSync()` no interpretador.

> **Nota:** `activeStatConditions` (de `StatModifierEffectNode`) **não** são executadas pelo hook — elas são dados puros de bônus de stat, consumidos por `GetTotalStatBonuses()` e `GetActiveStatModifiers()`.

**Guard anti-recursão:** `executingAbilities` é um `HashSet<object>` que previne que uma passiva em execução seja re-disparada por um hook gerado pela própria execução.

### 8.3. Hooks Disparados no Apply/Remove

Ao aplicar uma condição (via `ApplyGraphCondition` ou `ApplyStatModifierCondition`):
- `OnBeforeApplyCondition` é disparado na **unidade alvo** E na **unidade fonte**.
- `OnAfterApplyCondition` é disparado nas mesmas unidades após a aplicação.

Isso permite, por exemplo, uma passiva da fonte que reage toda vez que ela aplica um debuff em alguém.

### 8.4. Cálculo de Bônus de Stats

`GetTotalStatBonuses(baseStats)` agrega os bônus flat e percent de **todas as `activeStatConditions`** e retorna o total. Percentuais são calculados sobre o `baseStats` passado como parâmetro.

A lista `activeGraphConditions` **não** contribui para `GetTotalStatBonuses` — condições baseadas em grafos completos que precisam modificar stats devem o fazer via hooks (`TriggerNode → UnitVariableNode`) em vez de agregação passiva.

### 8.5. Exibição de Condições Ativas

`GetActiveConditionsInfo()` retorna uma lista unificada de `PassiveInfo` que combina:
- Condições de grafo (`activeGraphConditions`) — nome, ícone e descrição do `AbilityGraphSO`.
- Condições de stat (`activeStatConditions`) — nome, ícone e descrição "Modificador de Status".

`GetActiveStatModifiers()` retorna detalhes por stat de cada `RuntimeStatCondition` ativa, para exibição em UI de status.

---

## 9. Habilidades que Modificam Outras Habilidades

Este é um dos padrões mais poderosos do sistema. A interação entre habilidades acontece através da `VariableStore` da unidade.

### 9.1. Padrão: Passiva que Amplifica Skill Específica

**Exemplo:** Passiva que aumenta o dano de `Skill_1` se o HP estiver abaixo de 50%.

```
[Grafo da Passiva - Hook: OnTurnStart]
  TriggerNode (OnTurnStart)
    → AttributeConditionNode (HP < 50%, mode=Percentage)
      → ConditionalFlowNode
        → [True] UnitVariableNode (BonusDamagePercent, Slot=Slot_1, Set, value=50)
        → [False] UnitVariableNode (BonusDamagePercent, Slot=Slot_1, Set, value=0)
```

```
[Grafo da Skill_1 - Active]
  StartNode
    → TargetNode
      → VariableModifierNode (local_mult, Set, value=1)
        → UnitVariableNode (BonusDamagePercent, scope=Slot, Get → output: "bonus")
          → VariableModifierNode (local_mult, Multiply, valueRef="bonus") 
                                  // multiplica pela bonus
            → DamageEffectNode (variableReference="local_mult")
```

### 9.2. Padrão: Habilidade que Modifica Outra via Contador Global

**Exemplo:** Skill 2 acumula `Counter1` e a Skill 1 consome esses stacks.

```
[Grafo Skill_2]
  ...
    → UnitVariableNode (Counter1, Global, Add, value=1)

[Grafo Skill_1]
  ...
    → UnitVariableNode (Counter1, Global, Get → "stacks_counter")
      → VariableModifierNode (damage_mult, Set, valueRef="stacks_counter")
        → DamageEffectNode (variableReference="damage_mult")
          → UnitVariableNode (Counter1, Global, Set, value=0)  ← Consome os stacks
```

### 9.3. Padrão: Condição que Modifica o Alcance de Outra Skill

**Exemplo:** Condição "Afiado" que aumenta o range de todas as habilidades do Slot 1.

```
[Grafo "Afiado" - Condition]
  TriggerNode (OnTurnStart)
    → UnitVariableNode (ExtraRange, scope=Slot_1, Set, value=2)

[Grafo de qualquer Skill no Slot 1]
  TargetNode (useExtraRangeVariable = true) ← lê automaticamente ExtraRange do Slot_1
```

### 9.4. Padrão: Passiva que Aplica uma Condição em Reação a Evento

**Exemplo:** Toda vez que `source` causa dano, aplica "Marcado" no alvo.

```
[Grafo da Passiva - Hook: OnAfterDealDamage]
  TriggerNode (OnAfterDealDamage)
    → ApplyModifierNode (modifierId="condicao_marcado", stacks=1)
      ← Condição "Marcado" está nas Dependencies do grafo da passiva
```

---

## 10. PreparedActionManager — Ações com Delay

**Arquivo:** [`PreparedActionManager.cs`](file:///c:/Users/Rubens/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Combat/PreparedActionManager.cs)

Gerencia ações que foram agendadas via `ScheduleExecutionNode` para execução futura.

### 10.1. Ciclo de Vida

```
ScheduleExecutionNode
  → PreparedActionManager.ScheduleAction(caster, graph, context.Clone(), nextNode, delayTurns)
  
TurnManager.OnTurnStarted (para o caster):
  → decrementa TurnsRemaining de cada PreparedAction do caster
  → quando TurnsRemaining <= 0: Remove da lista + StartCoroutine(ExecutePreparedAction)
  
ExecutePreparedAction:
  → AbilityGraphInterpreter.ExecuteFromNode(caster, graph, nextNode, contextSnapshot)
```

### 10.2. Visuals de Telegraph

Cada tile dos alvos no `ContextSnapshot` recebe um `ApplyTelegraph(turnsRemaining)`, mostrando ao jogador quantos turnos faltam para o ataque chegar. Isso é atualizado a cada turno e limpo ao fim de cada fase (`OnPhaseEnded`).

### 10.3. Isolamento do Contexto

O `context.Clone()` garante que o snapshot do estado (alvos, variáveis) no momento do agendamento seja preservado, mesmo que o contexto original seja modificado por outras habilidades nos turnos intermediários.

---

## 11. Targeting — Seleção de Alvos

### 11.1. Targeting Manual (Jogador)

Usado quando `sourceType == Manual` e `source.Team == Player`.
1. Cria e configura um `TargetingRuleData` baseado nos dados do `TargetNode`.
2. Adiciona um componente `TargetSelector` na unidade.
3. Aguarda `WaitUntil(() => selectionConfirmed)`.
4. Remove o componente e popula `context.targets`.

### 11.2. Auto Strategy (IA/Passivas)

Usado em modo síncrono ou quando `sourceType == AutoStrategy`. Delegado ao `AutoTargetResolver.Resolve(source, data)`.

| Strategy | Comportamento |
|---|---|
| `Self` | Adiciona o próprio `source`. |
| `ClosestUnit` | Unidade mais próxima (Euclidiana). |
| `FarthestUnit` | Unidade mais distante. |
| `LowestAttribute` | Menor HP (fallback fixo — campo `attributeType` não é usado completamente). |
| `HighestAttribute` | Maior HP (idem). |
| `RandomTarget` | `targetCount` alvos aleatórios da lista filtrada (com repetição possível). |
| `MainTarget` | Definido mas sem implementação em `AutoTargetResolver` — usa o alvo pré-definido. |

### 11.3. Alvos da IA (Behavior Tree)

Se a IA passar alvos via `context.targets` ou `context.targetPos` antes do `TargetNode`, eles têm **prioridade**. O nó expande a área de hits se necessário (`mode == Area`), mas não sobrescreve os alvos já definidos.

---

## 12. Scaling de Dano e Cura

O cálculo base para dano e cura segue o mesmo padrão:

```
baseValue = Σ (unit.VariableStore.GetStat(scaling.statType) × scaling.percentage / 100)
multiplier = context.Variables[variableReference] (default: 1.0)
finalAmount = Floor(baseValue × multiplier)
```

- **`scalings` vazio (Dano):** Fallback para `source.Stats.attack`.
- **`scalings` vazio (Cura):** Fallback para `target.Health.MaxHealth`.
- `scaling.useTargetStat`: Se `true`, usa o atributo do **alvo** em vez do conjurador.

O `variableReference` age como **multiplicador global** — ideal para habilidades que escalam com stacks ou com valores calculados dinamicamente.

---

## 13. Fluxo Completo de Execução

### Habilidade Ativa (Jogador)

```
1. Jogador seleciona a habilidade no Slot_1
2. AbilityExecutor dispara ExecuteGraphCoroutine(caster, graph, OnManualCast, ...)
3. Interpeter cria CombatContext, inicializa Blackboard (graph.Variables)
4. Busca StartNode → inicia loop
5. TargetNode → abre TargetSelector → jogador confirma alvo → context.targets preenchido
6. ConditionalFlowNode → avalia condições → segue True/False
7. DamageEffectNode → calcula dano com scalings → DamageProcessor.ProcessAndApplyDamage()
   → PassiveManager do source: TriggerHook(OnBeforeDealDamage) e OnAfterDealDamage
   → PassiveManager do target: TriggerHook(OnBeforeTakeDamage) e OnAfterTakeDamage
8. Fim do caminho → onComplete?.Invoke()
```

### Passiva Disparada por Turno

```
1. TurnManager.OnTurnStarted → PassiveManager.HandleTurnStarted(currentUnit)
2. PassiveManager.TriggerHook(OnTurnStart, context)
3. Para cada AbilityGraphSO em condições ativas:
   → ExecuteGraphForHook → ExecuteGraphSync(unit, graph, OnTurnStart)
4. Interpreter busca TriggerNode com trigger == OnTurnStart
5. Percorre nós síncronos (AttributeCondition, VariableModifier, etc.)
6. Fim da execução
```

### Aplicação de Condição

```
1. ApplyModifierNode (em execução de grafo de habilidade)
2. Busca AbilityGraphSO de condição nas Dependencies
3. Se debuff: EffectResistanceCheck
4. PassiveManager.ApplyGraphCondition(conditionGraph, source)
   → TriggerHook(OnBeforeApplyCondition) em ambas as unidades
   → Verifica se já existe (update stacks/duração)
   → Se novo: adiciona em activeGraphConditions
   → TriggerHook(OnAfterApplyCondition) em ambas as unidades
5. Em cada OnTurnEnd da unidade: TickConditionsOnTurnEnd
   → Decrementa remainingTurns
   → Se 0: remove da lista
```

---

## 14. Erros Comuns e Armadilhas

| Situação | Causa | Solução |
|---|---|---|
| Passiva não dispara | `TriggerNode` com hook errado, ou grafo não está nas fontes reconhecidas pelo `PassiveManager`. | Verificar o `trigger` do `TriggerNode` e as fontes (Slot, Pet, Constelação, etc.). |
| Link não encontrado no grafo | `PortName` no `NodeLink` não bate com a string retornada por `ProcessNode`. | O `ProcessNode` loga os links disponíveis ao falhar — verificar o log para ver o que está sendo retornado. |
| Buff de stat não re-aplica | A chave `GraphBuff_{graph.name}_{guid[:4]}` identifica condições de stat duplicadas. Se o grafo for renomeado, a referência pode ser perdida. | Não renomear o grafo enquanto o buff estiver ativo em combate. |
| Condição Graph não modifica stats via `GetTotalStatBonuses` | `GetTotalStatBonuses` lê apenas `activeStatConditions` (de `StatModifierEffectNode`), não `activeGraphConditions`. | Para condições baseadas em grafos completos: usar `TriggerNode (OnTurnStart)` + `UnitVariableNode` para escrever o bônus na `VariableStore` e consumi-lo no grafo de skill. |
| Loop infinito no grafo | `LoopNode` sem conexão de saída ou `ConditionalFlowNode` que sempre retorna True. | Sempre verificar se a porta `Exit` do loop está conectada. O `ExecuteGraphSync` tem safety counter de 1000 iterações. |
| `VfxNode` sem efeito visual | A implementação do `VfxNode` está incompleta — apenas loga o `vfxId`. | O sistema de VFX precisa ser implementado separadamente. |
| `CostNode` sem efeito real | A dedução de Mana/Stamina ainda é um TODO. | Monitorar quando o campo for implementado em `CombatStats`/`Unit`. |
