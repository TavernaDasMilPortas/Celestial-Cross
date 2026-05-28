# Plano de Refatoração — Sistema de IA com Behavior Trees Visuais

> **Status:** Aguardando aprovação  
> **Escopo:** Substituição completa do sistema de IA (AIBehaviorProfile → Behavior Tree visual)  
> **Fases:** 5 fases de implementação  

---

## 1. Decisões de Design (consolidadas da entrevista)

| Decisão | Escolha |
|---|---|
| Direção arquitetural | Behavior Tree visual dedicada, independente do AbilityGraph |
| Coexistência com BehaviorProfile | **Substituição total** — sem migração (não há inimigos criados) |
| Serialização | ScriptableObject próprio (`BehaviorTreeSO`) seguindo padrão do AbilityGraphSO |
| Compatibilidade com autofarm | Genérico — funciona para qualquer `Unit` (EnemyUnit, Pet, PlayerUnit futuro) |
| Referência a habilidades | **Por categoria genérica** (AIAbilityHint) — a BT pede "melhor Dano", o Utility scoring escolhe |
| Blackboard | **Persistente entre turnos** — mantém histórico (quem atacou, dano recebido) |
| Fases de boss | Integradas como sub-árvores com condições de HP na própria BT (elimina AIPatternData) |
| Nós Decorators | Incluir na v1 (Inverter, Repeater, Random %, Cooldown) |
| Templates pré-construídos | Sim — Agressivo Melee, Ranged Defensivo, Suporte/Healer, Boss Básico |
| Debug visual | Highlight de nós em Play Mode (verde=sucesso, vermelho=falha) + log de Blackboard |
| Wizard | Integrar step "IA / Behavior Tree" no UnitCreationWizard existente |
| Auto-gen AIAbilityHint | Sim, na v1 — ao salvar AbilityGraph, inferir categoria e valores dos nós. Campo `isLocked` para tuning manual |
| Idioma do editor | Somente Português |
| Execução runtime | Assíncrona via coroutine (avaliação rápida, execução visual com delays) |
| Infraestrutura do editor | Seguir padrão do AbilityGraph (GraphView + SO + SaveUtility) |

### Decisões extras capturadas
- **Campos de deslocamento e range na unidade** — variáveis globais para modificar habilidades de deslocamento e ataques básicos
- **"Save As" no AbilityGraph** — permitir salvar variação de habilidade como novo SO
- **Inimigos sem limitação de slots de habilidades** — diferente de units do jogador

---

## 2. Arquitetura Proposta

### 2.1. Modelo de Dados

```
BehaviorTreeSO (ScriptableObject) ← [NEW]
  ├── List<BTNodeData> Nodes       (GUID, tipo, posição, JsonData)
  ├── List<BTLinkData> Links       (source GUID/port → target GUID/port)
  └── string description           (auto-gerada)

BTNodeData (Serializable) ← [NEW]
  ├── string Guid
  ├── string NodeType              ("SelectorNode", "AttackActionNode", etc.)
  ├── string NodeTitle
  ├── Vector2 Position
  └── string JsonData              (campos custom em JSON)

BTLinkData (Serializable) ← [NEW]
  ├── string ParentGuid
  ├── string ParentPort
  ├── string ChildGuid
  └── string ChildPort
```

### 2.2. Hierarquia de Nós Runtime

```
BTNode (abstract)                         ← [NEW]
  ├── Evaluate(AIBlackboard) → BTResult {Success, Failure, Running}
  │
  ├── BTComposite (abstract)              ← [NEW] — tem lista de filhos
  │   ├── BTSelector                      ← testa filhos até Success
  │   └── BTSequence                      ← testa filhos até Failure
  │
  ├── BTDecorator (abstract)              ← [NEW] — modifica 1 filho
  │   ├── BTInverter                      ← inverte resultado
  │   ├── BTRepeater                      ← repete N vezes
  │   ├── BTRandomChance                  ← executa com X% de chance
  │   └── BTCooldownDecorator             ← bloqueia por N turnos
  │
  ├── BTCondition (abstract)              ← [NEW] — retorna Success/Failure
  │   ├── ConditionHPPercent              ← HP% do próprio (acima/abaixo de X)
  │   ├── ConditionAllyHPPercent          ← HP% de aliados
  │   ├── ConditionAllyCount              ← número de aliados vivos
  │   ├── ConditionIsAlone                ← está sozinho?
  │   ├── ConditionDistanceToTarget       ← distância ao alvo (maior/menor que X)
  │   ├── ConditionTargetInRange          ← tem alvo no alcance de ataque?
  │   ├── ConditionAllyNeedsHeal          ← aliado com HP% abaixo de X?
  │   ├── ConditionTargetHasBuff          ← alvo tem buff/debuff ativo?
  │   ├── ConditionAoEHitCount            ← habilidade atingiria N+ alvos?
  │   ├── ConditionIsFirstTurn            ← é o primeiro turno?
  │   ├── ConditionTurnNumber             ← turno atual == X?
  │   └── ConditionAbilityReady           ← cooldown da habilidade disponível?
  │
  └── BTAction (abstract)                 ← [NEW] — executa ação + retorna
      ├── ActionAttack                    ← ataque básico (Utility scoring para alvo)
      ├── ActionMove                      ← movimento (Utility scoring para tile)
      ├── ActionUseAbility                ← usa habilidade por categoria (Dano/Cura/Buff/Debuff)
      ├── ActionWait                      ← pular turno
      ├── ActionRetreat                   ← recuar para tile seguro (longe de ameaças)
      ├── ActionProtectAlly               ← mover para ficar adjacente ao aliado mais fraco
      ├── ActionFocusTarget               ← atacar alvo específico por tag/role/classe
      └── ActionPatrol                    ← movimentação predefinida por área
```

### 2.3. AIBlackboard (Memória Persistente)

```
AIBlackboard ← [NEW]
  ├── Dados de Turno (recalculados a cada turno):
  │   ├── List<Unit> allies, enemies
  │   ├── HashSet<Vector2Int> reachableTiles
  │   ├── float myHpPercent
  │   ├── int aliveAllyCount
  │   ├── Unit closestEnemy, weakestEnemy, strongestEnemy
  │   ├── bool isAlone
  │   └── int currentTurnNumber
  │
  └── Dados Persistentes (mantidos entre turnos):
      ├── Dictionary<Unit, float> damageReceivedFrom   ← quem me atacou e quanto
      ├── Dictionary<Unit, float> damageDoneToward     ← quanto causei a cada alvo
      ├── Unit lastAttacker                            ← quem me atacou por último
      ├── int turnsAlive                               ← quantos turnos vivo
      └── Dictionary<string, int> abilityCooldowns     ← cooldowns restantes
```

### 2.4. Fluxo de Execução

```
TurnManager.ProcessUnitTurn(EnemyUnit)
  └── AIBrain.ExecuteTurn()
       ├── 1. blackboard.UpdateTurnData()        ← atualiza dados do turno
       ├── 2. behaviorTree.Evaluate(blackboard)  ← percorre a árvore
       │       └── Selector
       │           ├── Sequence [Se HP < 30%]
       │           │   ├── ConditionHPPercent(< 0.3)
       │           │   └── ActionRetreat
       │           ├── Sequence [Se tem alvo no range]
       │           │   ├── ConditionTargetInRange
       │           │   └── ActionAttack
       │           └── ActionMove (agressivo)    ← fallback
       │
       └── 3. StartCoroutine(ExecutePlan())      ← executa com delays visuais
```

---

## 3. Fases de Implementação

### FASE 1 — Core Runtime (~15 arquivos novos)

**Objetivo:** Classes base da Behavior Tree, Blackboard, e ScriptableObject de serialização.

#### Arquivos a criar:

| Arquivo | Caminho | Descrição |
|---|---|---|
| `BehaviorTreeSO.cs` | `Unit/Enemy/AI/BehaviorTree/` | ScriptableObject com NodeData + LinkData |
| `BTNodeData.cs` | `Unit/Enemy/AI/BehaviorTree/` | Dados de serialização de um nó |
| `BTLinkData.cs` | `Unit/Enemy/AI/BehaviorTree/` | Dados de serialização de um link |
| `BTNode.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Classe abstrata base de todos os nós |
| `BTResult.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Enum {Success, Failure, Running} |
| `BTComposite.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Base abstrata para Selector/Sequence |
| `BTSelector.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Selector node |
| `BTSequence.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Sequence node |
| `BTDecorator.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Base abstrata para decorators |
| `BTInverter.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Inverte resultado do filho |
| `BTRepeater.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Repete filho N vezes |
| `BTRandomChance.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Executa filho com X% chance |
| `BTCooldownDecorator.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Bloqueia por N turnos |
| `AIBlackboard.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Memória compartilhada persistente |
| `BehaviorTreeRunner.cs` | `Unit/Enemy/AI/BehaviorTree/Runtime/` | Constrói árvore runtime a partir do SO e avalia |

---

### FASE 2 — Editor Visual (~8 arquivos novos)

**Objetivo:** Editor de nós visual seguindo o padrão do AbilityGraph.

#### Arquivos a criar:

| Arquivo | Caminho | Descrição |
|---|---|---|
| `BTEditorWindow.cs` | `Unit/Enemy/AI/BehaviorTree/Editor/` | EditorWindow com toolbar (ObjectField + Save/Load) |
| `BTGraphView.cs` | `Unit/Enemy/AI/BehaviorTree/Editor/` | GraphView com zoom, drag, grid, search |
| `BTEditorNode.cs` | `Unit/Enemy/AI/BehaviorTree/Editor/` | Classe base de nó visual (extends `Node`) |
| `BTNodeSearchWindow.cs` | `Unit/Enemy/AI/BehaviorTree/Editor/` | ISearchWindowProvider com categorias hierárquicas |
| `BTSaveUtility.cs` | `Unit/Enemy/AI/BehaviorTree/Editor/` | Serializa/deserializa GraphView ↔ SO |
| `BTAssetHandler.cs` | `Unit/Enemy/AI/BehaviorTree/Editor/` | Double-click para abrir editor |
| `BTDebugOverlay.cs` | `Unit/Enemy/AI/BehaviorTree/Editor/` | Highlight de nós durante Play Mode |
| `BTNodeStylesheet.uss` | `Unit/Enemy/AI/BehaviorTree/Editor/` | Estilos visuais dos nós (cores por tipo) |

#### Categorias no Search Window:

```
Controle/
  ├── Seletor
  └── Sequência
Decoradores/
  ├── Inverter
  ├── Repetidor
  ├── Chance Aleatória (%)
  └── Cooldown (Turnos)
Condições/
  ├── HP% Próprio
  ├── HP% de Aliados
  ├── Aliados Vivos
  ├── Está Sozinho?
  ├── Distância ao Alvo
  ├── Alvo no Alcance
  ├── Aliado Precisa de Cura
  ├── Alvo com Buff/Debuff
  ├── AoE Atingiria N+ Alvos
  ├── Primeiro Turno?
  ├── Turno Atual
  └── Habilidade Pronta?
Ações/
  ├── Atacar
  ├── Mover
  ├── Usar Habilidade (por Categoria)
  ├── Esperar
  ├── Recuar para Segurança
  ├── Proteger Aliado
  ├── Focar Alvo Específico
  └── Patrulhar Área
```

#### Cores dos nós no editor:

| Tipo | Cor |
|---|---|
| Controle (Selector/Sequence) | 🔵 Azul |
| Decorator | 🟣 Roxo |
| Condição | 🟡 Amarelo |
| Ação | 🟢 Verde |
| Debug (sucesso em Play Mode) | ✅ Verde brilhante |
| Debug (falha em Play Mode) | ❌ Vermelho |

---

### FASE 3 — Integração e Remoção do Sistema Antigo (~20 arquivos novos, ~15 arquivos removidos/modificados)

**Objetivo:** Criar todos os nós de condição e ação, refatorar AIBrain, remover código legado.

#### 3.1. Nós de Condição a criar:

| Arquivo | Descrição |
|---|---|
| `ConditionHPPercent.cs` | Verifica HP% do próprio (operador: <, >, ==) |
| `ConditionAllyHPPercent.cs` | Verifica HP% dos aliados |
| `ConditionAllyCount.cs` | Número de aliados vivos (>, <, ==) |
| `ConditionIsAlone.cs` | Retorna Success se não tem aliados |
| `ConditionDistanceToTarget.cs` | Distância Chebyshev ao alvo mais próximo |
| `ConditionTargetInRange.cs` | Tem alvo no alcance de ataque? |
| `ConditionAllyNeedsHeal.cs` | Algum aliado com HP% abaixo de threshold? |
| `ConditionTargetHasBuff.cs` | Alvo tem buff/debuff ativo específico? |
| `ConditionAoEHitCount.cs` | Habilidade AoE atingiria N+ alvos? |
| `ConditionIsFirstTurn.cs` | turnsAlive == 0? |
| `ConditionTurnNumber.cs` | Turno atual do combate (via TurnManager) |
| `ConditionAbilityReady.cs` | Habilidade de categoria X está fora de cooldown? |

#### 3.2. Nós de Ação a criar:

| Arquivo | Descrição |
|---|---|
| `ActionAttack.cs` | Utility scoring para escolher melhor alvo, executa ataque |
| `ActionMove.cs` | Utility scoring para escolher melhor tile (agressivo/defensivo/suporte) |
| `ActionUseAbility.cs` | Filtra por categoria AIAbilityHint, pontua, executa melhor |
| `ActionWait.cs` | Pula turno |
| `ActionRetreat.cs` | BFS para encontrar tile mais seguro (longe de ameaças) |
| `ActionProtectAlly.cs` | Move para adjacente ao aliado mais frágil |
| `ActionFocusTarget.cs` | Ataca alvo específico por role/classe/tag |
| `ActionPatrol.cs` | Movimento em padrão predefinido |

#### 3.3. Refatoração do AIBrain:

```diff
- AIBrain.cs (803 linhas monolíticas)
+ AIBrain.cs (~100 linhas)
    - Mantém: Awake/Start, ExecuteTurn() como entry point
    - Novo: Cria AIBlackboard, chama BehaviorTreeRunner.Evaluate()
    - Move: Toda lógica de scoring para dentro dos nós de Ação
    - Move: FindAlivePlayerUnits/Allies para AIBlackboard
    - Move: GetReachableTiles, ChebyshevDistance para utility class
    - Remove: PlanTurn, EvaluateAllActions, EvaluateAttack, EvaluateMove, EvaluateAbility
    - Remove: CheckPatternTriggers (substituído por ConditionHPPercent na BT)
    - Remove: SelectPreferredTarget (absorvido pelos nós de ação)
```

#### 3.4. Arquivos a REMOVER:

| Arquivo | Razão |
|---|---|
| `AIBehaviorProfile.cs` | Substituído por BehaviorTreeSO |
| `AIBehaviorRule.cs` | Substituído por nós de condição na BT |
| `AIActionScore.cs` | Incorporado nos nós de ação |
| `AITurnPlan.cs` | Substituído pelo fluxo da BT |
| `AITargetPreference.cs` | Absorvido como campo configurável nos nós de ação |
| `BehaviorType.cs` | Absorvido nos nós de ação (move agressivo/defensivo/suporte = campo do nó) |
| `AIPatternData.cs` | Substituído por condições de HP na BT |
| `AIBehaviorProfileEditor.cs` | Sem uso |
| `AIBehaviorRuleDrawer.cs` | Sem uso |
| `AIBossPhaseDrawer.cs` | Sem uso |
| `AIBrainEditor.cs` | Substituído pelo BTEditorWindow |
| `AIPatternDataEditor.cs` | Sem uso |

#### 3.5. Arquivos a MODIFICAR:

| Arquivo | Mudança |
|---|---|
| `EnemyUnit.cs` | Trocar campo `behaviorProfile` por `behaviorTreeSO`. Remover campo `patternData`. |
| `TurnManager.cs` | Sem mudança — já chama `AIBrain.ExecuteTurn()` |
| `LevelData.cs` | Trocar `OverrideBehaviorProfile` por `OverrideBehaviorTree`. Remover `OverridePatternData`. |
| `BattleLevelBuilder.cs` | Adaptar override de BT no spawn |
| `UnitData.cs` | Adicionar campo `BehaviorTreeSO defaultBehaviorTree` + campos `displacement` e `baseRange` |

#### 3.6. Manter sem alteração:

| Arquivo | Razão |
|---|---|
| `AIAbilityHint.cs` | Continua sendo metadados por habilidade — a BT consulta por categoria |
| `AICooldownTracker.cs` | Movido para dentro do AIBlackboard (incorporado, não removido) |

---

### FASE 4 — Wizard, Templates e Debug Visual (~6 arquivos)

**Objetivo:** Integrar no UnitCreationWizard, criar templates prontos, debug em Play Mode.

#### 4.1. Wizard
- **Modificar `UnitCreationWizard.cs`:** Adicionar step "IA / Behavior Tree" após Abilities
  - Dropdown para selecionar template ou BT existente
  - Botão "Abrir Editor de BT" para customização
  - Quando `UnitType == Enemy`, o step aparece automaticamente

#### 4.2. Templates (4 BehaviorTreeSO assets)

| Template | Estrutura |
|---|---|
| **Agressivo Melee** | `Selector → [Se alvo no range → Atacar] [Senão → Mover agressivo]` |
| **Ranged Defensivo** | `Selector → [Se alvo perto → Recuar] [Se alvo no range → Usar habilidade Dano] [Senão → Mover defensivo]` |
| **Suporte/Healer** | `Selector → [Se aliado precisa cura → Usar habilidade Cura] [Se aliado precisa buff → Usar habilidade Buff] [Senão → Mover para aliado]` |
| **Boss Básico (2 fases)** | `Selector → [Se HP < 50% → Sequência fase 2 (buff + ataque forte)] [Senão → Sequência fase 1 (ataque normal)]` |

#### 4.3. Debug Visual
- **BTDebugOverlay.cs:** Durante Play Mode, o BTEditorWindow observa a unidade selecionada
  - Nós avaliados recebem borda verde (Success) ou vermelha (Failure)
  - Nó em execução atual recebe highlight amarelo pulsante
  - Painel lateral mostra dados do AIBlackboard (HP, aliados, cooldowns, histórico)

---

### FASE 5 — Qualidade de Vida (~4 arquivos)

**Objetivo:** Auto-geração de AIAbilityHint, Save As, campos extras.

#### 5.1. Auto-geração de AIAbilityHint
- **Modificar `GraphSaveUtility.cs`:** Ao salvar o graph:
  1. Se `aiHint.isLocked == false`:
     - Detectar nós de efeito no graph (`DamageEffectNode` → category = Damage, `HealEffectNode` → category = Heal, `StatModifierEffectNode` → Buff/Debuff)
     - Somar `estimatedValue` a partir dos valores numéricos dos nós
     - Inferir `targetsFriendlies` (Heal/Buff = true, Damage/Debuff = false)
     - Preencher `basePriority` com heurística (ex: skills com mais dano = prioridade maior)
  2. Se `aiHint.isLocked == true`: não sobrescrever
- **Modificar `AIAbilityHint.cs`:** Adicionar campo `bool isLocked`

#### 5.2. "Save As" no AbilityGraph
- **Modificar `AbilityGraphWindow.cs`:** Adicionar botão "Salvar Como..." no toolbar
  - Abre diálogo de salvamento para criar novo SO a partir do graph atual
  - Gera cópia profunda (novo GUID, novos nós, mesmos valores)

#### 5.3. Campos extras na unidade
- **Modificar `UnitData.cs`:** Adicionar:
  - `int displacement` — deslocamento base da unidade
  - `int baseRange` — range base da unidade
  - Esses campos serão lidos por habilidades de deslocamento e ataques básicos

---

## 4. Estrutura Final de Pastas

```
Unit/Enemy/AI/
  ├── AIAbilityHint.cs                    ← [MANTIDO, modificado: +isLocked]
  ├── AICooldownTracker.cs                ← [INCORPORADO ao Blackboard]
  │
  ├── BehaviorTree/
  │   ├── BehaviorTreeSO.cs               ← [NEW] ScriptableObject
  │   ├── BTNodeData.cs                   ← [NEW] Serialização
  │   ├── BTLinkData.cs                   ← [NEW] Serialização
  │   │
  │   ├── Runtime/
  │   │   ├── BTNode.cs                   ← [NEW] Base abstrata
  │   │   ├── BTResult.cs                 ← [NEW] Enum
  │   │   ├── BTComposite.cs              ← [NEW]
  │   │   ├── BTSelector.cs               ← [NEW]
  │   │   ├── BTSequence.cs               ← [NEW]
  │   │   ├── BTDecorator.cs              ← [NEW]
  │   │   ├── BTInverter.cs               ← [NEW]
  │   │   ├── BTRepeater.cs               ← [NEW]
  │   │   ├── BTRandomChance.cs           ← [NEW]
  │   │   ├── BTCooldownDecorator.cs      ← [NEW]
  │   │   ├── AIBlackboard.cs             ← [NEW]
  │   │   ├── BehaviorTreeRunner.cs       ← [NEW]
  │   │   └── AIGridUtility.cs            ← [NEW] BFS, Chebyshev, etc.
  │   │
  │   ├── Conditions/
  │   │   ├── ConditionHPPercent.cs        ← [NEW]
  │   │   ├── ConditionAllyHPPercent.cs    ← [NEW]
  │   │   ├── ConditionAllyCount.cs        ← [NEW]
  │   │   ├── ConditionIsAlone.cs          ← [NEW]
  │   │   ├── ConditionDistanceToTarget.cs ← [NEW]
  │   │   ├── ConditionTargetInRange.cs    ← [NEW]
  │   │   ├── ConditionAllyNeedsHeal.cs    ← [NEW]
  │   │   ├── ConditionTargetHasBuff.cs    ← [NEW]
  │   │   ├── ConditionAoEHitCount.cs      ← [NEW]
  │   │   ├── ConditionIsFirstTurn.cs      ← [NEW]
  │   │   ├── ConditionTurnNumber.cs       ← [NEW]
  │   │   └── ConditionAbilityReady.cs     ← [NEW]
  │   │
  │   ├── Actions/
  │   │   ├── ActionAttack.cs              ← [NEW]
  │   │   ├── ActionMove.cs                ← [NEW]
  │   │   ├── ActionUseAbility.cs          ← [NEW]
  │   │   ├── ActionWait.cs                ← [NEW]
  │   │   ├── ActionRetreat.cs             ← [NEW]
  │   │   ├── ActionProtectAlly.cs         ← [NEW]
  │   │   ├── ActionFocusTarget.cs         ← [NEW]
  │   │   └── ActionPatrol.cs              ← [NEW]
  │   │
  │   └── Editor/
  │       ├── BTEditorWindow.cs            ← [NEW]
  │       ├── BTGraphView.cs               ← [NEW]
  │       ├── BTEditorNode.cs              ← [NEW]
  │       ├── BTNodeSearchWindow.cs        ← [NEW]
  │       ├── BTSaveUtility.cs             ← [NEW]
  │       ├── BTAssetHandler.cs            ← [NEW]
  │       ├── BTDebugOverlay.cs            ← [NEW]
  │       └── BTNodeStylesheet.uss         ← [NEW]
  │
  ├── [DELETE] AIBehaviorProfile.cs
  ├── [DELETE] AIBehaviorRule.cs
  ├── [DELETE] AIActionScore.cs
  ├── [DELETE] AITurnPlan.cs
  ├── [DELETE] AITargetPreference.cs
  ├── [DELETE] BehaviorType.cs
  ├── [DELETE] Patterns/AIPatternData.cs
  └── [DELETE] Editor/ (5 arquivos antigos)
```

---

## 5. Riscos e Mitigações

| Risco | Mitigação |
|---|---|
| Complexidade do GraphView API | Seguir padrão do AbilityGraph já existente — copiar estrutura |
| Performance de avaliação da BT | AIBlackboard cacheia dados; árvore avalia em O(nós) que é rápido |
| Mutação de GridPosition durante simulação (bug herdado) | Corrigir na v1: usar variável temporária em vez de mutar posição real |
| Cooldown timing incorreto (bug herdado) | Mover cooldowns para AIBlackboard com tick explícito |
| FindObjectsByType a cada turno (performance) | AIBlackboard mantém cache de unidades vivas |

---

## 6. Ordem de Execução Detalhada (Checklist)

### Fase 1 — Core Runtime
- [ ] Criar `BTResult.cs` (enum)
- [ ] Criar `BTNodeData.cs` e `BTLinkData.cs` (serialização)
- [ ] Criar `BehaviorTreeSO.cs` (ScriptableObject com CreateAssetMenu)
- [ ] Criar `BTNode.cs` (abstrato: Evaluate, GetJsonData, LoadFromJson)
- [ ] Criar `BTComposite.cs`, `BTSelector.cs`, `BTSequence.cs`
- [ ] Criar `BTDecorator.cs`, `BTInverter.cs`, `BTRepeater.cs`, `BTRandomChance.cs`, `BTCooldownDecorator.cs`
- [ ] Criar `AIBlackboard.cs` (dados de turno + persistentes)
- [ ] Criar `AIGridUtility.cs` (BFS, Chebyshev, queries de tile)
- [ ] Criar `BehaviorTreeRunner.cs` (constrói árvore runtime, avalia)

### Fase 2 — Editor Visual
- [ ] Criar `BTEditorWindow.cs` (EditorWindow + toolbar)
- [ ] Criar `BTGraphView.cs` (GraphView com zoom, drag, grid, minimap)
- [ ] Criar `BTEditorNode.cs` (nó visual base com ports)
- [ ] Criar `BTNodeSearchWindow.cs` (categorias hierárquicas em PT)
- [ ] Criar `BTSaveUtility.cs` (serialização GraphView ↔ SO)
- [ ] Criar `BTAssetHandler.cs` (double-click para abrir)
- [ ] Criar `BTNodeStylesheet.uss` (cores por tipo de nó)

### Fase 3 — Integração
- [ ] Criar todos os 12 nós de Condição
- [ ] Criar todos os 8 nós de Ação (com Utility scoring migrado do AIBrain)
- [ ] Refatorar `AIBrain.cs` para usar BehaviorTreeRunner
- [ ] Modificar `EnemyUnit.cs` (trocar BehaviorProfile → BehaviorTreeSO)
- [ ] Modificar `LevelData.cs` (trocar overrides)
- [ ] Modificar `BattleLevelBuilder.cs` (adaptar spawn)
- [ ] Modificar `UnitData.cs` (adicionar defaultBehaviorTree)
- [ ] Remover 12 arquivos do sistema antigo
- [ ] Compilar e testar no Unity

### Fase 4 — Wizard, Templates, Debug
- [ ] Criar 4 templates de BehaviorTreeSO (assets)
- [ ] Modificar `UnitCreationWizard.cs` — adicionar step de IA
- [ ] Implementar `BTDebugOverlay.cs` para Play Mode
- [ ] Testar debug visual com unidade inimiga em combate

### Fase 5 — Qualidade de Vida
- [ ] Modificar `AIAbilityHint.cs` — adicionar `isLocked`
- [ ] Modificar `GraphSaveUtility.cs` — auto-geração de AIAbilityHint
- [ ] Modificar `AbilityGraphWindow.cs` — botão "Salvar Como..."
- [ ] Modificar `UnitData.cs` — campos `displacement` e `baseRange`
