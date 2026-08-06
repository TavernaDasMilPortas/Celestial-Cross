# Plano de Migração: Serverless-First + PvP Síncrono Futuro
**Projeto:** Celestial Cross  
**Stack:** Unity → PlayFab + Azure Functions (.NET 10 Isolated) → Azure SignalR (futuro)  
**Princípio Guia:** *"Construa o PvE serverless como se o PvP já existisse."*  
**Status:** ✅ Decisões Arquiteturais Aprovadas — Pronto para Execução

---

## Decisões Arquiteturais Confirmadas

| # | Decisão | Escolha | Justificativa |
|---|---|---|---|
| 1 | Persistência do BattleState | **PlayFab Entity Objects** | 20 unidades ≈ 20~25KB; limite é 1MB. Folga confortável. |
| 2 | Dados de jogo (gamedata.json) | **PlayFab Title Data** (migrar na Fase 2) | Permite atualização sem redeploy da Azure Function. |
| 3 | Granularidade de ações | **Uma ação por requisição** | Servidor retorna toda a cadeia de efeitos em um único `TurnResult.Events[]`. |
| 4 | Evolução do sistema de combate | **Padrão Event-Driven extensível** | Novas mecânicas não exigem alteração no cliente Unity. |
| 5 | Habilidades no servidor | **HeadlessGraphInterpreter** | O servidor interpreta os mesmos grafos `AbilityGraphSO` da Unity, sem alterar como habilidades são montadas. |

---

## Visão Geral da Estratégia

A chave para **minimizar retrabalho** é tomar uma decisão arquitetural central agora e segui-la:

> **O cliente Unity nunca calcula o resultado de um turno. Ele apenas envia intenções e anima resultados.**

O `TurnManager`, `DamageProcessor`, `PassiveManager` e `AIBrain` da Unity **continuam existindo**, mas com papéis completamente diferentes:
- **Antes:** calculavam e aplicavam resultados diretamente no jogo.
- **Depois:** servem apenas para **renderizar visualmente** o que o servidor mandou.

> [!IMPORTANT]
> **A montagem de habilidades NÃO muda.** Habilidades continuam sendo criadas como grafos visuais (`AbilityGraphSO`) no editor da Unity. O servidor recebe esses grafos exportados em JSON e os interpreta com a mesma lógica do `AbilityGraphInterpreter`, mas sem visual (headless).

Quando o PvP Síncrono chegar, não haverá reescrita — apenas adição do canal de comunicação bidirecional (SignalR) por cima da mesma camada serverless.

---

## Mapa Arquitetural Final

```
┌─────────────────────────────────────────────────────────────┐
│                      CLIENTE UNITY                           │
│                                                              │
│  PlayerInput → [NetworkTurnProxy] → HTTP / SignalR WS        │
│  Servidor    → [TurnResultApplier] → Animações / VFX         │
│                                                              │
│  Componentes PERMANENTES (papel visual apenas):              │
│    TurnManager, PassiveManager, DamageProcessor,             │
│    AbilityExecutor, AbilityGraphInterpreter, AIBrain         │
│                                                              │
│  Montagem de Habilidades (SEM MUDANÇA):                      │
│    AbilityGraphSO (ScriptableObject)                         │
│    ├── NodeData[] (Start, Target, Damage, Heal, etc.)        │
│    ├── NodeLinks[] (conexões entre nós)                      │
│    ├── Variables[] (blackboard)                               │
│    └── Dependencies[] (sub-grafos, patterns de AoE)          │
│                                                              │
│  [NOVO] Export Pipeline:                                     │
│    AbilityExporter.cs (Editor) → grafos → gamedata.json      │
└───────────────────┬─────────────────┬───────────────────────┘
                    │ HTTP (PvE)       │ WebSocket (PvP)
                    ▼                 ▼
┌─────────────────────────────────────────────────────────────┐
│                   AZURE FUNCTIONS (Serverless)                │
│                                                              │
│  BattleFunctions.cs                                          │
│  ├── StartPveMatch (valida energia, cria estado)             │
│  ├── SubmitPveTurn (valida, simula, retorna delta)           │
│  └── EndPveMatch   (valida vitória, concede recompensas)     │
│                                                              │
│  [FUTURO PvP]  PvpFunctions.cs                               │
│  ├── Negotiate (token SignalR)                               │
│  ├── StartPvpMatch (matchmaking, conecta SignalR)            │
│  ├── SubmitPvpAction (MESMO Core do PvE!)                    │
│  └── EndPvpMatch                                             │
│                                                              │
│  Núcleo: CombatSimulator.Core.dll (C# puro)                 │
│  ┌─────────────────────────────────────────────────────┐     │
│  │  Interpreter/                                       │     │
│  │    HeadlessGraphInterpreter.cs  ← Percorre grafos   │     │
│  │    NodeProcessors/ (Damage, Heal, Target, etc.)     │     │
│  ├─────────────────────────────────────────────────────┤     │
│  │  Simulation/                                        │     │
│  │    CombatSimulator.cs  ← Orquestra (entry point)    │     │
│  │    TurnOrderResolver, DamageCalculator              │     │
│  │    PassiveProcessor, AIResolver                     │     │
│  ├─────────────────────────────────────────────────────┤     │
│  │  Models/                                            │     │
│  │    BattleState, UnitState, TurnAction, TurnResult   │     │
│  │    BattleEvent, SimCombatContext                     │     │
│  │    AbilityGraphData (versão pura do SO)              │     │
│  │    NodeDataModels (cópia pura dos RuntimeData)       │     │
│  └─────────────────────────────────────────────────────┘     │
└───────────────────┬─────────────────────────────────────────┘
                    │ SDK PlayFab Server
                    ▼
┌─────────────────────────────────────────────────────────────┐
│                   PLAYFAB (Persistência)                      │
│  Entity Objects → "ActiveBattle" (BattleState ativo)         │
│  User Data → inventário, energia, account save               │
│  Title Data → gamedata.json + ability graphs (Fase 2)        │
└─────────────────────────────────────────────────────────────┘
```

---

## Padrão Fundamental: Event-Driven Extensível

> [!IMPORTANT]
> **Este padrão é a resposta direta à preocupação de manter o sistema de combate atualizável.**  
> Ele garante que adicionar novas mecânicas (nova passiva, novo tipo de dano, nova condição de status) exige alteração **apenas no `CombatSimulator.Core`**. O Backend e a Unity se adaptam automaticamente.

### O Problema que ele resolve
Sem este padrão, adicionar uma nova passiva de "refletir dano" exigiria:
1. Alterar o `Core` (cálculo)
2. Alterar o `Backend` (processar o novo efeito)
3. Alterar a Unity (animar o novo efeito)

Com o padrão, você altera só o `Core`. O `Backend` já sabe serializar qualquer `BattleEvent`. A Unity anima o que conhece e usa fallback para o que não conhece ainda.

### Como funciona

**`BattleEvent.cs`** — evento genérico e extensível:
```csharp
public class BattleEvent
{
    // Tipo como string, não enum! Novos tipos não quebram código antigo.
    public string EventType;  // "DamageDealt", "StatusApplied", "UnitDied", etc.

    // Dados do evento — campos opcionais por convenção
    public string SourceUnitId;
    public string TargetUnitId;
    public int? Amount;           // Dano ou cura
    public bool? IsCritical;
    public string StatusId;       // Qual buff/debuff foi aplicado
    public int? Stacks;           // Número de stacks aplicados
    public int? Duration;         // Duração em turnos
    public SimpleVector2Int? Position;  // Para eventos de movimento
    public string AbilityId;      // Qual habilidade gerou o evento
    public Dictionary<string, string> Extra; // Dados extras ad-hoc para tipos novos
}
```

**Tipos de eventos iniciais** (lista viva — adicionar novos não quebra nada):
```
"DamageDealt"         → SourceUnitId, TargetUnitId, Amount, IsCritical
"HealApplied"         → SourceUnitId, TargetUnitId, Amount, IsCritical
"ConditionApplied"    → SourceUnitId, TargetUnitId, StatusId, Duration, Stacks
"ConditionRemoved"    → TargetUnitId, StatusId
"ConditionTick"       → TargetUnitId, StatusId, Amount
"StatModApplied"      → TargetUnitId, StatusId, Extra{stats JSON}
"UnitMoved"           → SourceUnitId, Position (destino)
"UnitDied"            → TargetUnitId, SourceUnitId
"TurnStarted"         → SourceUnitId (quem começa o turno)
"TurnEnded"           → SourceUnitId
"RoundStarted"        → (nenhum extra)
"AbilityUsed"         → SourceUnitId, AbilityId
"APModified"          → TargetUnitId, Amount
"HealthSacrificed"    → SourceUnitId, Amount
"CleanseApplied"      → TargetUnitId, Extra{tipo}
"CombatEnded"         → Extra{WinnerTeam}
```

**Na Unity — `TurnResultApplier.cs`:**
```csharp
// Registrar handlers por tipo de evento
private static Dictionary<string, Action<BattleEvent>> _handlers = new()
{
    ["DamageDealt"]  = e => DamageProcessor.ApplyVisual(e),
    ["HealApplied"]  = e => HealVFX.Play(e),
    ["ConditionApplied"] = e => PassiveManager.ApplyVisual(e),
    ["UnitDied"]     = e => UnitDeathHandler.Play(e),
    ["UnitMoved"]    = e => GridMovementHandler.Play(e),
    // ...
};

// Se não conhecer o evento, usa animação padrão — nunca quebra
foreach (var ev in result.Events)
{
    if (_handlers.TryGetValue(ev.EventType, out var handler))
        await handler(ev);
    else
        await DefaultEventAnimation(ev); // fallback seguro
}
```

> [!TIP]
> **Ao adicionar uma nova passiva de "escudo":** Você cria o grafo no editor Unity, exporta. O servidor interpreta automaticamente. Na Unity, registra o handler visual. Enquanto o handler não existe, o jogo não quebra — usa o fallback.

---

## FASE 1 — Biblioteca `CombatSimulator.Core` + Export Pipeline

> [!IMPORTANT]
> **Fase mais crítica. Todo o resto depende dela.**  
> Estimativa: 3~4 semanas.

### Princípio: Mesmos Grafos, Duas Execuções

O Celestial Cross define **todas** as habilidades (ativas, passivas, condições) como grafos visuais em `AbilityGraphSO`. Cada grafo é uma rede de nós (`AbilityNodeData`) conectados por links (`NodeLinkData`), com dados serializados em JSON dentro de cada nó.

O `CombatSimulator.Core` **não reimplementa** habilidades em código. Ele inclui um **`HeadlessGraphInterpreter`** que percorre os mesmos grafos, executando a lógica de cada nó sem disparar animações ou visuais.

```
                     MESMA HABILIDADE
                    ┌───────────────┐
                    │ AbilityGraphSO│  (grafo visual no editor)
                    │  ├── Nodes[]  │
                    │  ├── Links[]  │
                    │  └── Vars[]   │
                    └──────┬────────┘
                           │
              ┌────────────┼────────────┐
              ▼                         ▼
    Unity (visual)              Servidor (headless)
    AbilityGraphInterpreter     HeadlessGraphInterpreter
    ├── ProcessNode()           ├── ProcessNode()
    ├── Coroutines + VFX        ├── Síncrono, sem visual
    ├── DOTween animations      ├── Atualiza BattleState
    └── DamagePopups            └── Emite BattleEvent[]
```

### Onde criar

```
c:\Users\Rubens\Bichinhos-Magicos\
└── CombatSimulator.Core\              ← Projeto C# puro (.NET Standard 2.1)
    ├── CombatSimulator.Core.csproj
    │
    ├── Models\
    │   ├── BattleState.cs             ← Estado completo da partida
    │   ├── UnitState.cs               ← Unidade sem MonoBehaviour
    │   ├── TurnAction.cs              ← Intenção do jogador
    │   ├── TurnResult.cs              ← Resposta completa do servidor
    │   ├── BattleEvent.cs             ← Evento extensível (padrão central)
    │   ├── SimCombatContext.cs         ← Versão pura do CombatContext
    │   ├── AbilityGraphData.cs        ← Versão serializável do AbilityGraphSO
    │   ├── NodeDataModels.cs          ← Cópia pura dos AbilityNodeRuntimeData
    │   ├── CombatStatsPure.cs         ← CombatStats sem Unity
    │   └── GridState.cs               ← Estado do grid (tiles, ocupação)
    │
    ├── Interpreter\                   ← CORAÇÃO — interpreta grafos headless
    │   ├── HeadlessGraphInterpreter.cs← Percorre grafos (loop while + switch)
    │   └── NodeProcessors\
    │       ├── StartNodeProcessor.cs
    │       ├── TriggerNodeProcessor.cs
    │       ├── TargetNodeProcessor.cs      ← AutoTargetResolver puro
    │       ├── DamageNodeProcessor.cs      ← Chama DamageCalculator
    │       ├── HealNodeProcessor.cs        ← Chama HealCalculator
    │       ├── MoveNodeProcessor.cs        ← Atualiza GridPosition
    │       ├── StatModifierProcessor.cs    ← Aplica buff/debuff no state
    │       ├── ApplyModifierProcessor.cs   ← Aplica sub-grafo condição
    │       ├── ConditionProcessors.cs      ← Attribute, Distance, Range, etc.
    │       ├── ControlFlowProcessors.cs    ← Loop, ConditionalFlow, LevelBranch
    │       ├── VariableProcessors.cs       ← VariableModifier, UnitVariable
    │       └── EffectProcessors.cs         ← ModifyAP, Cleanse, Sacrifice, etc.
    │
    ├── Simulation\
    │   ├── CombatSimulator.cs         ← Orquestra tudo; ponto de entrada único
    │   ├── TurnOrderResolver.cs       ← De TurnManager.cs (fila por velocidade)
    │   ├── DamageCalculator.cs        ← De DamageProcessor.cs (fórmulas)
    │   ├── HealCalculator.cs          ← De DamageProcessor.ProcessAndApplyHeal
    │   ├── PassiveProcessor.cs        ← Triggera hooks → executa sub-grafos
    │   ├── AIResolver.cs              ← De AIBrain.cs (sem MonoBehaviour)
    │   └── SeededRandom.cs            ← RNG determinístico (substitui Random.Range)
    │
    └── Validation\
        ├── TurnValidator.cs           ← Validações de segurança / anti-cheat
        └── GridValidator.cs           ← Alcance, movimento, células livres
```

### 1a. Export Pipeline — `AbilityExporter.cs` (Editor Unity)

> [!IMPORTANT]
> **A ponte entre a Unity e o servidor.** Um script de Editor que serializa todos os `AbilityGraphSO` para JSON, pronto para o servidor consumir.

Os grafos já guardam dados dos nós em `AbilityNodeData.JsonData` (string JSON), então o export é quase direto. O que precisa ser resolvido:

1. **Dependencies de tipo `AbilityGraphSO`** (sub-condições) → resolvidas por nome/ID, não referência Unity
2. **`AreaPatternData`** (patterns de AoE) → serializado inline como coordenadas
3. **Sprites/Assets visuais** → ignorados no export (servidor não precisa)

```csharp
// AbilityExporter.cs — Unity Editor script
[MenuItem("Celestial Cross/Export Ability Graphs")]
public static void ExportAll()
{
    var allGraphs = Resources.LoadAll<AbilityGraphSO>(""); // ou AssetDatabase
    var exportData = new Dictionary<string, AbilityGraphExport>();

    foreach (var graph in allGraphs)
    {
        exportData[graph.name] = new AbilityGraphExport
        {
            Name = graph.abilityName,
            Type = graph.GetAbilityType().ToString(),
            Nodes = graph.NodeData.Select(n => new NodeExport
            {
                Guid = n.Guid,
                NodeType = n.NodeType,
                JsonData = n.JsonData
            }).ToList(),
            Links = graph.NodeLinks.Select(l => new LinkExport
            {
                BaseNodeGuid = l.BaseNodeGuid,
                PortName = l.PortName,
                TargetNodeGuid = l.TargetNodeGuid,
                TargetPortName = l.TargetPortName
            }).ToList(),
            Variables = graph.Variables.Select(v => new VarExport
            {
                Name = v.name,
                InitialValue = v.initialValue
            }).ToList(),
            Dependencies = ResolveDependencies(graph), // sub-grafos por nome
            Duration = graph.GetDuration(),
            IsBuff = graph.GetIsBuff(),
            CanStack = graph.GetCanStack(),
            MaxStacks = graph.GetMaxStacks(),
            IsPersistent = graph.GetIsPersistent(),
            MaxLevel = graph.MaxLevel
        };
    }

    // Injeta na seção "abilities" do gamedata.json
    string json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
    File.WriteAllText("Backend/gamedata_abilities.json", json);
}
```

> [!NOTE]
> O export gera um arquivo `gamedata_abilities.json` separado. Na Fase 2, este arquivo é migrado para PlayFab Title Data junto com o `gamedata.json` principal.

### 1b. `HeadlessGraphInterpreter.cs` — Coração do Core

A lógica é **estruturalmente idêntica** ao `AbilityGraphInterpreter.cs` da Unity, mas:
- **Síncrono** (sem coroutines/yield)
- **Sem visual** (pula VfxNode, não dispara DOTween, não mostra popups)
- **Emite `BattleEvent[]`** em vez de aplicar efeitos visuais
- **Usa `SimCombatContext`** (versão pura do `CombatContext`)

```csharp
public static class HeadlessGraphInterpreter
{
    /// <summary>
    /// Executa um grafo de habilidade headless sobre o BattleState.
    /// Retorna a porta de saída para encadeamento.
    /// </summary>
    public static void ExecuteGraph(
        AbilityGraphData graph,
        SimCombatContext context,
        SimCombatHook hook,
        List<BattleEvent> events)
    {
        if (graph == null || graph.Nodes.Count == 0) return;

        // Inicializa blackboard com valores do grafo
        foreach (var v in graph.Variables)
            context.Variables[v.Name] = v.InitialValue;

        var nodeMap = graph.Nodes.ToDictionary(n => n.Guid);

        // Encontra entry point
        NodeData currentNode = null;
        if (hook == SimCombatHook.OnManualCast)
        {
            currentNode = graph.Nodes.FirstOrDefault(n => n.NodeType == "StartNode");
        }
        else
        {
            currentNode = graph.Nodes.FirstOrDefault(n =>
            {
                if (n.NodeType != "TriggerNode") return false;
                var data = Deserialize<TriggerNodeData>(n.JsonData);
                return data.trigger == hook;
            });
        }

        if (currentNode == null) return;

        int safety = 0;
        while (currentNode != null && safety++ < 1000)
        {
            // Processa o nó e obtém a porta de saída
            string nextPort = ProcessNode(graph, currentNode, context, hook, events);

            if (nextPort == "Scheduled")
                break; // ScheduleExecution suspende o fluxo

            // Segue o link para o próximo nó
            var link = graph.Links.FirstOrDefault(l =>
                l.BaseNodeGuid == currentNode.Guid && l.PortName == nextPort);

            currentNode = link != null ? nodeMap[link.TargetNodeGuid] : null;
        }
    }

    private static string ProcessNode(
        AbilityGraphData graph,
        NodeData node,
        SimCombatContext context,
        SimCombatHook hook,
        List<BattleEvent> events)
    {
        switch (node.NodeType)
        {
            case "StartNode":
            case "TriggerNode":
            case "RamificationSpecNode":
                return "Out";

            case "TargetNode":
                return TargetNodeProcessor.Process(graph, node, context);

            case "DamageEffectNode":
                return DamageNodeProcessor.Process(node, context, events);

            case "HealEffectNode":
                return HealNodeProcessor.Process(node, context, events);

            case "MoveEffectNode":
                return MoveNodeProcessor.Process(node, context, events);

            case "StatModifierEffectNode":
                return StatModifierProcessor.Process(node, graph, context, events);

            case "ApplyModifierNode":
                return ApplyModifierProcessor.Process(node, graph, context, events);

            case "ConditionalFlowNode":
                return ControlFlowProcessors.ProcessConditionalFlow(graph, node, context, hook, events);

            case "LoopNode":
                return ControlFlowProcessors.ProcessLoop(node, context);

            case "LevelBranchNode":
                return $"Level {context.AbilityLevel}";

            case "RamificationNode":
                return ControlFlowProcessors.ProcessRamification(node, graph, context);

            case "VariableModifierNode":
                return VariableProcessors.ProcessModifier(node, context);

            case "UnitVariableNode":
                return VariableProcessors.ProcessUnitVariable(node, context);

            case "AttributeConditionNode":
            case "DistanceConditionNode":
            case "RangeConditionNode":
            case "FactionConditionNode":
            case "SpeedAdvantageConditionNode":
            case "TurnOrderConditionNode":
                return ConditionProcessors.Evaluate(node, context) ? "True" : "False";

            case "ModifyAPNode":
                return EffectProcessors.ProcessModifyAP(node, context, events);

            case "CleanseStatusNode":
                return EffectProcessors.ProcessCleanse(node, context, events);

            case "SacrificeHealthNode":
                return EffectProcessors.ProcessSacrifice(node, context, events);

            case "CostNode":
                return EffectProcessors.ProcessCost(node, context);

            case "LimitPerTurnNode":
                return EffectProcessors.ProcessLimit(node, context);

            case "ScheduleExecutionNode":
                return EffectProcessors.ProcessSchedule(node, graph, context);

            case "DurationNode":
                return "Out"; // Metadata, consumido pelo ApplyModifier

            case "VfxNode":
                return "Out"; // Ignorado no servidor — puramente visual

            default:
                return "Out";
        }
    }
}
```

> [!NOTE]
> **Cada `case` no switch mapeia 1:1 com os cases do `AbilityGraphInterpreter.ProcessNodeSync()` da Unity.** A diferença é que o servidor atualiza `UnitState` no `BattleState` e emite `BattleEvent`, enquanto a Unity atualiza `Unit.Health` e dispara VFX.

### 1c. `SimCombatContext.cs` — Versão Pura do CombatContext

```csharp
public class SimCombatContext
{
    public BattleState State;               // Estado mutável da batalha
    public string SourceUnitId;             // Quem está agindo
    public string TargetUnitId;             // Alvo principal
    public List<string> TargetUnitIds;      // Alvos (para AoE)
    public int Amount;                      // Dano/cura calculado
    public bool IsCritical;

    public int AbilityLevel = 1;
    public string SlotId = "";
    public SimpleVector2Int? TargetPos;

    public Dictionary<string, float> Variables;     // Blackboard do grafo
    public Dictionary<string, int> LoopCounters;    // Controle de LoopNode
    public SeededRandom Rng;                        // RNG determinístico

    // Helpers
    public UnitState GetSource() => State.Units.First(u => u.UnitId == SourceUnitId);
    public UnitState GetTarget() => State.Units.First(u => u.UnitId == TargetUnitId);
    public List<UnitState> GetTargets() => State.Units
        .Where(u => TargetUnitIds.Contains(u.UnitId)).ToList();
}
```

### 1d. `DamageCalculator.cs` — Mesma Fórmula do DamageProcessor.cs

```csharp
public static class DamageCalculator
{
    /// <summary>
    /// Réplica exata de DamageProcessor.ProcessAndApplyDamage(), mas pura.
    /// Usa SeededRandom em vez de UnityEngine.Random.
    /// </summary>
    public static void Calculate(SimCombatContext context, bool applyDefense,
                                  List<BattleEvent> events)
    {
        var source = context.GetSource();
        var target = context.GetTarget();

        // 1. Hooks Pre-Dano (passivas injetam nas Variables)
        PassiveProcessor.TriggerHook(SimCombatHook.OnBeforeDealDamage, context, events, source.UnitId);
        PassiveProcessor.TriggerHook(SimCombatHook.OnBeforeTakeDamage, context, events, target.UnitId);

        // 2. Base + bônus flat
        int totalBase = context.Amount;
        if (context.Variables.TryGetValue("bonus_flat_damage", out float flatDmg))
            totalBase += (int)Math.Round(flatDmg);

        // 3. Multiplicadores
        float multiplier = 1.0f;
        if (context.Variables.TryGetValue("damage_mult", out float md)) multiplier += md;

        // 4. Crítico (SeededRandom, não Random.Range!)
        int critChance = source.Stats.CriticalChance;
        if (context.Variables.TryGetValue("bonus_crit_chance", out float cb))
            critChance += (int)Math.Round(cb);

        context.IsCritical = context.Rng.Next(0, 100) < Math.Clamp(critChance, 0, 100);

        // 5. Dano bruto
        float dmgFloat = totalBase * multiplier;
        if (context.IsCritical)
        {
            float critMult = 1.0f + (source.Stats.CriticalDamage / 100f);
            if (context.Variables.TryGetValue("crit_mult_bonus", out float cm)) critMult += cm;
            if (context.Variables.TryGetValue("bonus_crit_damage", out float bcd)) critMult += (bcd / 100f);
            dmgFloat *= critMult;
        }

        // 6. Defesa
        int defense = 0;
        if (applyDefense)
        {
            defense = target.Stats.Defense;
            if (context.Variables.TryGetValue("defense_reduction", out float dr))
                defense = Math.Max(0, defense - (int)Math.Round(dr));
        }

        int finalDamage = Math.Max(1, (int)Math.Round(dmgFloat) - defense);

        // 7. Aplica no state
        target.CurrentHealth = Math.Max(0, target.CurrentHealth - finalDamage);

        // 8. Emite evento
        events.Add(new BattleEvent
        {
            EventType = "DamageDealt",
            SourceUnitId = source.UnitId,
            TargetUnitId = target.UnitId,
            Amount = finalDamage,
            IsCritical = context.IsCritical
        });

        // 9. Hooks Pós-Dano
        PassiveProcessor.TriggerHook(SimCombatHook.OnAfterDealDamage, context, events, source.UnitId);
        PassiveProcessor.TriggerHook(SimCombatHook.OnAfterTakeDamage, context, events, target.UnitId);

        // 10. Verifica morte
        if (target.CurrentHealth <= 0)
        {
            target.IsAlive = false;
            events.Add(new BattleEvent
            {
                EventType = "UnitDied",
                TargetUnitId = target.UnitId,
                SourceUnitId = source.UnitId
            });
            PassiveProcessor.TriggerHook(SimCombatHook.OnKill, context, events, source.UnitId);
        }
    }
}
```

### 1e. `PassiveProcessor.cs` — Hooks Disparam Sub-Grafos

> [!IMPORTANT]
> **Passivas no Celestial Cross são grafos `AbilityGraphSO` do tipo Condition,** executados via `TriggerNode` quando um `CombatHook` dispara. O servidor precisa replicar exatamente este comportamento.

```csharp
public static class PassiveProcessor
{
    /// <summary>
    /// Para cada condição ativa na unidade, verifica se tem TriggerNode
    /// para o hook atual e executa o sub-grafo headless.
    /// Replica PassiveManager.TriggerHook() da Unity.
    /// </summary>
    public static void TriggerHook(
        SimCombatHook hook,
        SimCombatContext context,
        List<BattleEvent> events,
        string unitId)
    {
        var unit = context.State.Units.FirstOrDefault(u => u.UnitId == unitId);
        if (unit == null) return;

        foreach (var condition in unit.ActiveConditions.ToList())
        {
            var graph = context.State.AbilityLibrary[condition.GraphId];
            if (graph == null) continue;

            // Anti-recursão: não re-executar o mesmo grafo
            if (condition.IsExecuting) continue;
            condition.IsExecuting = true;

            // Injeta stacks nas variables do contexto
            var subContext = context.Clone();
            subContext.Variables["stacks"] = condition.Stacks;
            subContext.SourceUnitId = unitId;

            HeadlessGraphInterpreter.ExecuteGraph(graph, subContext, hook, events);

            condition.IsExecuting = false;
        }
    }

    /// <summary>
    /// Decrementa duração das condições no fim do turno.
    /// Replica PassiveManager.TickConditionsOnTurnEnd().
    /// </summary>
    public static void TickConditions(UnitState unit, List<BattleEvent> events)
    {
        for (int i = unit.ActiveConditions.Count - 1; i >= 0; i--)
        {
            var cond = unit.ActiveConditions[i];
            if (cond.IsPersistent) continue;

            cond.RemainingTurns--;
            if (cond.RemainingTurns <= 0)
            {
                events.Add(new BattleEvent
                {
                    EventType = "ConditionRemoved",
                    TargetUnitId = unit.UnitId,
                    StatusId = cond.GraphId
                });
                unit.ActiveConditions.RemoveAt(i);
            }
        }
    }
}
```

### 1f. `CombatSimulator.cs` — Ponto de Entrada Único

```csharp
public static class CombatSimulator
{
    /// <summary>
    /// Dado o estado atual da batalha e uma ação do jogador,
    /// retorna o novo estado + todos os eventos que ocorreram.
    /// Este método é puro e determinístico (SeededRandom).
    /// </summary>
    public static TurnResult Simulate(BattleState state, TurnAction action)
    {
        var events = new List<BattleEvent>();
        var newState = state.DeepClone();
        var rng = new SeededRandom(newState.RngSeed);

        // 1. Validar ação
        var validation = TurnValidator.Validate(action, newState);
        if (!validation.IsValid)
            return new TurnResult { Success = false, ErrorMessage = validation.Error };

        // 2. Executar ação via grafo da habilidade
        var context = new SimCombatContext
        {
            State = newState,
            SourceUnitId = action.ActingUnitId,
            Variables = new Dictionary<string, float>(),
            LoopCounters = new Dictionary<string, int>(),
            Rng = rng,
            AbilityLevel = action.AbilityLevel,
            TargetPos = action.TargetPosition
        };

        switch (action.Type)
        {
            case TurnActionType.UseAbility:
                var graph = newState.AbilityLibrary[action.AbilityId];
                events.Add(new BattleEvent {
                    EventType = "AbilityUsed",
                    SourceUnitId = action.ActingUnitId,
                    AbilityId = action.AbilityId
                });
                // Pré-configura target se o cliente mandou
                if (action.TargetUnitId != null)
                    context.TargetUnitId = action.TargetUnitId;
                if (action.TargetPosition.HasValue)
                    context.TargetPos = action.TargetPosition;

                HeadlessGraphInterpreter.ExecuteGraph(
                    graph, context, SimCombatHook.OnManualCast, events);
                break;

            case TurnActionType.Move:
                MoveNodeProcessor.ProcessDirectMove(context, action.TargetPosition.Value, events);
                break;

            case TurnActionType.Wait:
                break;

            case TurnActionType.Surrender:
                newState.IsFinished = true;
                newState.WinnerTeam = TeamType.Enemy;
                events.Add(new BattleEvent { EventType = "CombatEnded",
                    Extra = new() { ["WinnerTeam"] = "Enemy" } });
                break;
        }

        // 3. Verificar mortes em cadeia
        CheckDeaths(newState, events, context);

        // 4. Verificar fim de combate
        if (CheckCombatEnd(newState, events))
        {
            newState.IsFinished = true;
        }

        // 5. Avançar turno
        if (!newState.IsFinished)
        {
            TurnOrderResolver.AdvanceTurn(newState, events);

            // 6. Se próxima unidade for inimiga → IA resolve e simula
            var nextUnit = newState.Units.First(u => u.UnitId == newState.CurrentTurnUnitId);
            if (nextUnit.Team == TeamType.Enemy)
            {
                var aiAction = AIResolver.DecideAction(newState, nextUnit);
                // Recursão: simula a ação da IA como se fosse outro turno
                var aiResult = Simulate(newState, aiAction);
                events.AddRange(aiResult.Events);
                newState = aiResult.NewState;
            }
        }

        // 7. Atualiza seed para próxima ação
        newState.RngSeed = rng.CurrentSeed;
        newState.ActionIndex++;
        newState.LastActionAtUtc = DateTime.UtcNow;

        return new TurnResult
        {
            Success = true,
            NewState = newState,
            Events = events
        };
    }
}
```

### Modelos Fundamentais

**`UnitState.cs`** — versão pura/serializável de `Unit.cs`:
```csharp
public class UnitState
{
    public string UnitId;
    public string DisplayName;
    public TeamType Team;                   // Player, Enemy
    public int CurrentHealth;
    public int MaxHealth;
    public int CurrentAP;
    public int MaxAP;
    public bool IsAlive;
    public bool HasMovedThisTurn;
    public bool HasActedThisTurn;
    public SimpleVector2Int GridPosition;
    public CombatStatsPure Stats;           // HP, ATK, DEF, SPD, Crit%, CritDmg%, Acc%, Res%
    public List<ActiveCondition> ActiveConditions;  // Passivas/buffs/debuffs ativos
    public Dictionary<string, int> AbilityCooldowns;
    public Dictionary<string, float> UnitVariables;  // UnitVariableStore puro
    public List<string> AbilityIds;          // IDs dos grafos de habilidade
    public LoadoutData Loadout;              // Ramification branches selecionados
}
```

**`ActiveCondition.cs`** — condição/buff/debuff ativo:
```csharp
public class ActiveCondition
{
    public string GraphId;                  // ID do AbilityGraphSO de condição
    public int RemainingTurns;
    public int Stacks;
    public int MaxStacks;
    public bool IsPersistent;               // DurationType.Infinite
    public bool IsBuff;
    public bool IsExecuting;                // Anti-recursão
    public List<StatModEntry> StatMods;     // Modificadores de stat ativos
}
```

**`BattleState.cs`** — estado completo e persistível:
```csharp
public class BattleState
{
    public string MatchId;
    public string OwnerPlayFabId;
    public string OpponentPlayFabId;        // null no PvE; preenchido no PvP
    public int RoundNumber;
    public List<string> TurnQueue;          // IDs em ordem de velocidade
    public string CurrentTurnUnitId;
    public List<UnitState> Units;
    public int ActionIndex;                 // Anti-replay
    public long RngSeed;                    // Semente determinística do servidor
    public bool IsFinished;
    public TeamType WinnerTeam;
    public string StageId;
    public DateTime CreatedAtUtc;
    public DateTime LastActionAtUtc;
    public Dictionary<string, ScheduledAction> ScheduledActions; // ScheduleExecutionNode
    public Dictionary<string, AbilityGraphData> AbilityLibrary;  // Grafos carregados
}
```

**`TurnAction.cs`** — intenção do jogador (1 por requisição):
```csharp
public class TurnAction
{
    public string MatchId;
    public string ActingUnitId;
    public TurnActionType Type;             // Move, UseAbility, Wait, Surrender
    public string AbilityId;                // ID do grafo (null se Move/Wait)
    public int AbilityLevel;
    public SimpleVector2Int? TargetPosition; // Posição no grid (move ou AoE)
    public string TargetUnitId;              // Alvo (para habilidades single-target)
    public List<SimpleVector2Int> TargetPositions; // Para multi-target / AoE
    public int ExpectedActionIndex;          // Anti-replay
}
```

**`TurnResult.cs`** — resposta completa do servidor:
```csharp
public class TurnResult
{
    public bool Success;
    public string ErrorMessage;
    public BattleState NewState;
    public List<BattleEvent> Events;
    // Eventos em ordem cronológica: dano → passivas → mortes → próximo turno → IA
}
```

> [!NOTE]
> **`Simulate()` é puro e determinístico:** dado o mesmo `BattleState` e `TurnAction`, sempre retorna o mesmo resultado graças ao `SeededRandom`. Isso facilita testes unitários, debug e futura re-simulação para validação anti-cheat.

### Tabela de Nós: Unity vs. Servidor

| Nó (`NodeType`) | Unity (`AbilityGraphInterpreter`) | Servidor (`HeadlessGraphInterpreter`) |
|---|---|---|
| `StartNode` | Entry point | ✅ Entry point |
| `TriggerNode` | Entry point de passiva | ✅ Entry point de passiva |
| `TargetNode` | Manual (wait input) ou Auto | ✅ Auto + `TurnAction.TargetPosition` |
| `DamageEffectNode` | Calcula + VFX + Popup | ✅ Calcula + BattleEvent |
| `HealEffectNode` | Calcula + VFX + Popup | ✅ Calcula + BattleEvent |
| `MoveEffectNode` | DOTween + Grid update | ✅ GridPosition update + BattleEvent |
| `StatModifierEffectNode` | Aplica + UI update | ✅ Aplica no UnitState + BattleEvent |
| `ApplyModifierNode` | PassiveManager.Apply | ✅ ActiveConditions.Add + BattleEvent |
| `LoopNode` | Loop coroutine | ✅ Loop síncrono |
| `VariableModifierNode` | context.Variables | ✅ context.Variables |
| `UnitVariableNode` | UnitVariableStore | ✅ UnitState.UnitVariables |
| `ConditionalFlowNode` | Avalia sub-nós | ✅ Avalia sub-nós |
| `AttributeConditionNode` | Avalia | ✅ Avalia |
| `DistanceConditionNode` | Avalia com GridMap | ✅ Avalia com GridState |
| `RangeConditionNode` | Avalia com GridMap | ✅ Avalia com GridState |
| `FactionConditionNode` | Avalia | ✅ Avalia |
| `SpeedAdvantageConditionNode` | Avalia | ✅ Avalia |
| `TurnOrderConditionNode` | Avalia com TurnManager | ✅ Avalia com TurnQueue |
| `LevelBranchNode` | Branch por level | ✅ Branch por level |
| `RamificationNode` | Branch por Loadout | ✅ Branch por LoadoutData |
| `RamificationSpecNode` | Passthrough | ✅ Passthrough |
| `ModifyAPNode` | Modifica AP | ✅ Modifica AP + BattleEvent |
| `CleanseStatusNode` | Remove condições | ✅ Remove + BattleEvent |
| `SacrificeHealthNode` | TakeDamage | ✅ Atualiza HP + BattleEvent |
| `CostNode` | Deduz custo | ✅ Valida e deduz |
| `LimitPerTurnNode` | UnitVariableStore | ✅ UnitVariables |
| `ScheduleExecutionNode` | PreparedActionManager | ✅ ScheduledActions no state |
| `DurationNode` | Metadata | ✅ Metadata (lido pelo ApplyModifier) |
| `VfxNode` | Dispara visual | ⏭️ **Ignorado** |

---

## FASE 2 — Azure Functions de Batalha + Migração Title Data

> Estimativa: 1~2 semanas.

### 2a. Migração do `gamedata.json` + `gamedata_abilities.json` para PlayFab Title Data

Alterar `GameDataService.cs` para carregar de Title Data:

```csharp
// ANTES (arquivo local):
string json = File.ReadAllText("gamedata.json");

// DEPOIS (PlayFab Title Data):
var result = await serverApi.GetTitleDataAsync(new GetTitleDataRequest
    { Keys = new List<string> { "MasterGameData", "AbilityGraphs" } });
string gameJson = result.Result.Data["MasterGameData"];
string abilityJson = result.Result.Data["AbilityGraphs"];
```

> [!NOTE]
> O resto do `GameDataService.cs` (GetPetSpecies, GetBanner, etc.) não muda em nada — só a origem do JSON. A nova seção `AbilityGraphs` contém os grafos exportados pelo `AbilityExporter.cs`.

### 2b. `BattleFunctions.cs` — Endpoints de Batalha

**`StartPveMatch`:**
1. Valida PlayFabId via `CallerEntityProfile`
2. Chama `ConsumeEnergy` (função já existente) — sem duplicar código
3. Verifica no PlayFab se o jogador não tem já uma `ActiveBattle` em andamento
4. Valida que as unidades/artefatos/pets do time pertencem ao inventário do jogador
5. Gera `RngSeed` aleatória no servidor
6. Carrega grafos de habilidade do `AbilityLibrary` (via `GameDataService`)
7. Monta `BattleState` inicial com `CombatSimulator.Core`
8. Salva em PlayFab Entity Objects com chave `"ActiveBattle_{MatchId}"`
9. Retorna `{ MatchId, InitialState }`

**`SubmitPveTurn`:**
1. Carrega `BattleState` do PlayFab
2. `TurnValidator.Validate(action, state)` — valida anti-replay, propriedade do turno, cooldown, alcance
3. `CombatSimulator.Simulate(state, action)` → `TurnResult`
4. Se `TurnResult.NewState.IsFinished` → chama `EndPveMatch` internamente
5. Salva `BattleState` atualizado
6. Retorna `TurnResult` (com todos os `BattleEvent` para animar)

**`EndPveMatch`:**
1. Carrega e valida que `IsFinished == true` e `WinnerTeam == Player`
2. Concede recompensas via `PlayFabServerAPI.GrantItemsToUser`
3. Remove `"ActiveBattle_{MatchId}"` do PlayFab
4. Registra progresso do StageNode
5. Retorna `{ Rewards }`

---

## FASE 3 — Adaptação do Cliente Unity

> Estimativa: 1~2 semanas. **Não apaga código existente — apenas acrescenta camada.**

### Novos Scripts (Pasta `Assets/Celestial-Cross/Scripts/Cloud/`)

#### `NetworkTurnProxy.cs`
- Serializa `TurnAction` e chama `NetworkGuard.ExecuteWithGuardAsync("SubmitPveTurn", action)`
- Bloqueia novos inputs enquanto aguarda resposta
- Recebe `TurnResult` e repassa ao `TurnResultApplier`
- Parâmetro `bool isPvp` preparado para futuro (false por padrão)

#### `TurnResultApplier.cs`
- Dicionário de handlers por `EventType` (padrão extensível)
- Processa `BattleEvent` em sequência, aguardando cada animação
- Atualiza estado local: `TurnManager.ApplyServerState(newState)`
- Handler de fallback para eventos desconhecidos

### Modificações nos Scripts Existentes

| Script | O que muda | Complexidade |
|---|---|---|
| `PlayerController.cs` | Chama `NetworkTurnProxy.SubmitAction()` em vez de `AbilityExecutor.Execute()` diretamente | **Baixa** |
| `TurnManager.cs` | **Adicionar** método `ApplyServerState(BattleState)` — código existente permanece intacto | **Baixa** |
| `DamageProcessor.cs` | **Adicionar** overload `ApplyVisualOnly(BattleEvent ev)` — sem cálculo de dano | **Baixa** |
| `PhaseManager.cs` | Remover chamada a `GrantRewardsAsync()` — recompensas agora vêm do `EndPveMatch` via `TurnResult` | **Média** |
| `AIBrain.cs` | **Zero mudanças** — servidor calcula, Unity só anima | **Nenhuma** |
| `AbilityExecutor.cs` | Adicionar overload para executar grafo em modo visual (sem calcular dano) | **Baixa** |
| `CombatInitializer.cs` | Substituir `TurnManager.StartCombat()` por chamada a `StartPveMatch` | **Baixa** |

---

## FASE 4 — PvP Síncrono (Próximo Semestre)

> [!NOTE]
> **Com as Fases 1-3 prontas, esta fase adiciona ~20% de código novo e zero reescrita.**

### Por que Azure SignalR?

| Tecnologia | Custo | Usa stack atual? | Complexidade |
|---|---|---|---|
| **Azure SignalR** ✅ | **$0** (20 conn. gratuitas — suficiente para TCC) | **Sim** | Baixa |
| PlayFab Sockets | $0 | Sim, mas separado | Média |
| Unity Netcode | $0 | **Não** (Relay pago) | Alta |
| Dedicated Server | $50+/mês | Sim | Muito Alta |
| Photon PUN | $0 até 20 CCU | Não | Média |

### O que adicionar no Backend
```xml
<!-- Backend.csproj -->
<PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.SignalRService" Version="1.14.0" />
```

**`PvpFunctions.cs`** (novo arquivo em `Backend/`):
- `Negotiate` → retorna token de conexão SignalR para o cliente
- `StartPvpMatch` → matchmaking, inicializa `BattleState` com dois times
- `SubmitPvpAction` → usa **exatamente o mesmo `CombatSimulator.Simulate()`** do PvE, depois emite `TurnResult` via SignalR para ambos os jogadores

### O que adicionar na Unity
- `PvpSignalRConnector.cs` → gerencia conexão WebSocket
- `NetworkTurnProxy.cs` já tem o parâmetro `bool isPvp` preparado: se `true`, envia via SignalR; se `false`, envia via HTTP (PvE atual)
- `TurnResultApplier.cs` **não muda** — processa `TurnResult` do mesmo jeito

---

## Como atualizar o sistema de combate no futuro

> **Exemplo prático: Adicionar passiva de "Roubo de Vida" (lifesteal)**

**Passo 1 — Editor Unity (criar o grafo):**
Cria um novo `AbilityGraphSO` do tipo `Condition` com:
- `TriggerNode` → hook: `OnAfterDealDamage`
- `VariableModifierNode` → lê `amount` do contexto, multiplica por 0.2
- `TargetNode` → targetsSelf: true
- `HealEffectNode` → usa variável calculada

**Passo 2 — Export:** Clica em `Celestial Cross > Export Ability Graphs`. O grafo é adicionado ao `gamedata_abilities.json` automaticamente.

**Passo 3 — Backend:** Zero mudanças. O `HeadlessGraphInterpreter` já sabe percorrer o novo grafo. Os `BattleEvent` de `HealApplied` já existem.

**Passo 4 — Unity visual:** Se o evento `HealApplied` já tem handler no `TurnResultApplier`, nada muda. Se for um tipo novo, adiciona uma linha.

**Total de arquivos alterados no código:** 0 (zero!). Só criou o grafo e exportou.

> [!TIP]
> **Compare com o fluxo antigo do plano:** antes, adicionar lifesteal exigia escrever código C# no `DamageCalculator.cs` do Core. Agora, é puramente data-driven — cria o grafo, exporta, pronto.

---

## Cronograma

```
FÉRIAS (Julho → Agosto)
├── Semana 1:   FASE 1a — Models puros + Export Pipeline (AbilityExporter.cs)
├── Semana 2:   FASE 1b — HeadlessGraphInterpreter + NodeProcessors essenciais
│               (Start, Target, Damage, Heal, Loop, Conditions)
├── Semana 3:   FASE 1c — PassiveProcessor + Hook system + todos os NodeProcessors
│               (StatModifier, ApplyModifier, Variable, Ramification, etc.)
├── Semana 4:   FASE 1d — Testes unitários (comparar resultados Unity vs Core)
│               + DamageCalculator + HealCalculator + AIResolver
└── Semana 5:   FASE 2 — BattleFunctions.cs + migração Title Data

SEMESTRE (Agosto → Dezembro)
├── Semana 1:   FASE 3 — NetworkTurnProxy + TurnResultApplier (Unity)
├── Semana 2:   Finalizar FASE 3 (PhaseManager refactor)
├── Semana 3-5: FASE 4 — SignalR Hub + PvpFunctions.cs
├── Semana 6:   FASE 4 — PvpSignalRConnector.cs na Unity
└── Semana 7+:  Testes, polish, matchmaking UI
```

---

## Resumo dos Arquivos Novos

### CombatSimulator.Core (C# puro)

| Arquivo | Propósito |
|---|---|
| `CombatSimulator.Core.csproj` | Biblioteca C# pura compartilhada (.NET Standard 2.1) |
| `Models/BattleState.cs` | Estado completo da partida |
| `Models/UnitState.cs` | Unidade sem MonoBehaviour |
| `Models/ActiveCondition.cs` | Condição/buff/debuff ativo |
| `Models/TurnAction.cs` | Intenção do jogador |
| `Models/TurnResult.cs` | Resposta completa do servidor |
| `Models/BattleEvent.cs` | Evento extensível (padrão central) |
| `Models/SimCombatContext.cs` | Versão pura do CombatContext |
| `Models/AbilityGraphData.cs` | Versão serializável do AbilityGraphSO |
| `Models/NodeDataModels.cs` | Cópia pura dos AbilityNodeRuntimeData |
| `Models/CombatStatsPure.cs` | CombatStats sem Unity |
| `Models/GridState.cs` | Estado do grid (tiles, ocupação) |
| `Interpreter/HeadlessGraphInterpreter.cs` | Percorre grafos headless |
| `Interpreter/NodeProcessors/*.cs` | 12 processors (1 por categoria de nó) |
| `Simulation/CombatSimulator.cs` | Ponto de entrada único da simulação |
| `Simulation/TurnOrderResolver.cs` | Fila por velocidade |
| `Simulation/DamageCalculator.cs` | Fórmula de dano (de DamageProcessor.cs) |
| `Simulation/HealCalculator.cs` | Fórmula de cura |
| `Simulation/PassiveProcessor.cs` | Hooks → executa sub-grafos |
| `Simulation/AIResolver.cs` | IA sem Unity (de AIBrain.cs) |
| `Simulation/SeededRandom.cs` | RNG determinístico |
| `Validation/TurnValidator.cs` | Segurança e anti-cheat |
| `Validation/GridValidator.cs` | Alcance e movimentação |

### Unity (novos scripts)

| Arquivo | Propósito |
|---|---|
| `Editor/AbilityExporter.cs` | Export pipeline: AbilityGraphSO → JSON |
| `Scripts/Cloud/NetworkTurnProxy.cs` | Envia ações ao servidor |
| `Scripts/Cloud/TurnResultApplier.cs` | Anima resultados do servidor |

### Backend (novos endpoints)

| Arquivo | Propósito |
|---|---|
| `Backend/BattleFunctions.cs` | Endpoints PvE |
| **[PvP]** `Backend/PvpFunctions.cs` | Endpoints PvP + SignalR |
| **[PvP]** `Scripts/Cloud/PvpSignalRConnector.cs` | WebSocket para PvP |
