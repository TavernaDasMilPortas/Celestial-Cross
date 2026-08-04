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

---

## Visão Geral da Estratégia

A chave para **minimizar retrabalho** é tomar uma decisão arquitetural central agora e segui-la:

> **O cliente Unity nunca calcula o resultado de um turno. Ele apenas envia intenções e anima resultados.**

O `TurnManager`, `DamageProcessor`, `PassiveManager` e `AIBrain` da Unity **continuam existindo**, mas com papéis completamente diferentes:
- **Antes:** calculavam e aplicavam resultados diretamente no jogo.
- **Depois:** servem apenas para **renderizar visualmente** o que o servidor mandou.

Quando o PvP Síncrono chegar, não haverá reescrita — apenas adição do canal de comunicação bidirecional (SignalR) por cima da mesma camada serverless.

---

## Mapa Arquitetural Final

```
┌─────────────────────────────────────────────────────────┐
│                      CLIENTE UNITY                      │
│                                                         │
│  PlayerInput → [NetworkTurnProxy] → HTTP / SignalR WS   │
│  SignalR WS → [TurnResultApplier] → Animações / VFX     │
│                                                         │
│  Componentes PERMANENTES (papel visual apenas):          │
│    TurnManager, PassiveManager, DamageProcessor,         │
│    AbilityExecutor, CombatHook, AIBrain (visuais)        │
└───────────────────┬─────────────────┬───────────────────┘
                    │ HTTP (PvE)       │ WebSocket (PvP)
                    ▼                 ▼
┌─────────────────────────────────────────────────────────┐
│                   AZURE FUNCTIONS (Serverless)           │
│                                                         │
│  BattleFunctions.cs                                      │
│  ├── StartPveMatch (valida energia, cria estado)         │
│  ├── SubmitPveTurn (valida, simula, retorna delta)       │
│  └── EndPveMatch   (valida vitória, concede recompensas) │
│                                                         │
│  [FUTURO PvP]  PvpFunctions.cs                           │
│  ├── Negotiate (token SignalR)                           │
│  ├── StartPvpMatch (matchmaking, conecta SignalR)        │
│  ├── SubmitPvpAction (MESMO CombatSimulator do PvE!)     │
│  └── EndPvpMatch                                         │
│                                                         │
│  Núcleo: CombatSimulator.Core.dll (C# puro)              │
│    ├── TurnOrderResolver, DamageCalculator               │
│    ├── StatusEffectProcessor, AIResolver                 │
│    └── BattleEventFactory → TurnResult                   │
└───────────────────┬─────────────────────────────────────┘
                    │ SDK PlayFab Server
                    ▼
┌─────────────────────────────────────────────────────────┐
│                   PLAYFAB (Persistência)                 │
│  Entity Objects → "ActiveBattle" (BattleState ativo)     │
│  User Data → inventário, energia, account save           │
│  Title Data → gamedata.json (migrar na Fase 2)           │
└─────────────────────────────────────────────────────────┘
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
    public Vector2Int? Position;  // Para eventos de movimento
    public Dictionary<string, string> Extra; // Dados extras ad-hoc para tipos novos
}
```

**Tipos de eventos iniciais** (lista viva — adicionar novos não quebra nada):
```
"DamageDealt"         → SourceUnitId, TargetUnitId, Amount, IsCritical
"HealApplied"         → SourceUnitId, TargetUnitId, Amount, IsCritical
"StatusApplied"       → SourceUnitId, TargetUnitId, StatusId, Duration
"StatusRemoved"       → TargetUnitId, StatusId
"StatusTick"          → TargetUnitId, StatusId, Amount (dano de veneno/queimadura)
"UnitMoved"           → SourceUnitId, Position (destino)
"UnitDied"            → TargetUnitId, SourceUnitId
"TurnStarted"         → SourceUnitId (quem começa o turno)
"TurnEnded"           → SourceUnitId
"RoundStarted"        → (nenhum extra)
"AbilityUsed"         → SourceUnitId, AbilityId
"CombatEnded"         → WinnerTeam
```

**Na Unity — `TurnResultApplier.cs`:**
```csharp
// Registrar handlers por tipo de evento
private static Dictionary<string, Action<BattleEvent>> _handlers = new()
{
    ["DamageDealt"]  = e => DamageProcessor.ApplyVisual(e),
    ["HealApplied"]  = e => HealVFX.Play(e),
    ["StatusApplied"]= e => PassiveManager.ApplyVisual(e),
    ["UnitDied"]     = e => UnitDeathHandler.Play(e),
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
> **Ao adicionar uma nova passiva de "escudo":** Você adiciona o cálculo no `Core` e um novo tipo de evento `"ShieldApplied"`. No dia seguinte, registra o handler visual na Unity. Enquanto o handler não existe, o jogo não quebra — usa o fallback.

---

## FASE 1 — Biblioteca `CombatSimulator.Core`

> [!IMPORTANT]
> **Fase mais crítica. Todo o resto depende dela.**  
> Estimativa: 2~3 semanas.

### Onde criar
```
c:\Users\Rubens\Bichinhos-Magicos\
└── CombatSimulator.Core\          ← Projeto C# puro (.NET Standard 2.1)
    ├── CombatSimulator.Core.csproj
    ├── Models\
    │   ├── BattleState.cs
    │   ├── UnitState.cs
    │   ├── TurnAction.cs
    │   ├── TurnResult.cs
    │   └── BattleEvent.cs         ← Padrão extensível
    ├── Simulation\
    │   ├── TurnOrderResolver.cs   ← De TurnManager.cs (lógica de velocidade)
    │   ├── DamageCalculator.cs    ← De DamageProcessor.cs (fórmulas de dano)
    │   ├── StatusEffectProcessor.cs← Ticks de passivas/buffs/debuffs
    │   ├── AIResolver.cs          ← De AIBrain.cs (sem MonoBehaviour)
    │   └── CombatSimulator.cs     ← Orquestra tudo; ponto de entrada único
    └── Validation\
        ├── TurnValidator.cs       ← Validações de segurança
        └── GridValidator.cs       ← Alcance, movimento, células livres
```

### `CombatSimulator.cs` — Ponto de Entrada Único
Este é o único método que a Azure Function precisa chamar. Toda a complexidade interna (passivas em cadeia, contra-ataques, ticks) fica dentro do Core.

```csharp
public static class CombatSimulator
{
    /// <summary>
    /// Dado o estado atual da batalha e uma ação do jogador,
    /// retorna o novo estado + todos os eventos que ocorreram.
    /// Este método NÃO tem efeitos colaterais — é puro e testável.
    /// </summary>
    public static TurnResult Simulate(BattleState state, TurnAction action)
    {
        var events = new List<BattleEvent>();
        var newState = state.DeepClone();

        // 1. Validar
        // 2. Executar ação (move, ability, wait)
        // 3. Processar passivas em cadeia (hooks OnAfterDealDamage, etc.)
        // 4. Verificar mortes → gerar "UnitDied"
        // 5. Verificar fim de combate → gerar "CombatEnded"
        // 6. Avançar fila de turnos → gerar "TurnStarted" para próxima unidade
        // 7. Se próxima unidade for inimigo → AIResolver.Decide() e simular

        return new TurnResult { NewState = newState, Events = events };
    }
}
```

> [!NOTE]
> **`Simulate()` é puro e determinístico:** dado o mesmo `BattleState` e `TurnAction`, sempre retorna o mesmo resultado. Isso facilita testes unitários, debug e futura re-simulação para validação anti-cheat.

### Modelos Fundamentais

**`UnitState.cs`** — versão pura/serializável de `Unit.cs`:
```csharp
public class UnitState
{
    public string UnitId;
    public string DisplayName;
    public TeamType Team;       // Enum simples: Player, Enemy
    public int CurrentHealth;
    public int MaxHealth;
    public int CurrentAP;
    public int MaxAP;
    public bool HasMovedThisTurn;
    public bool HasActedThisTurn;
    public SimpleVector2Int GridPosition; // Struct puro, sem UnityEngine
    public CombatStatsPure Stats;         // CombatStats sem atributos Unity
    public List<ActiveCondition> ActiveConditions;
    public Dictionary<string, int> AbilityCooldowns;
    public bool IsAlive => CurrentHealth > 0;
}
```

**`BattleState.cs`** — estado completo e persistível:
```csharp
public class BattleState
{
    public string MatchId;
    public string OwnerPlayFabId;        // PvE: dono; PvP: será expandido
    public string OpponentPlayFabId;     // null no PvE; preenchido no PvP
    public int RoundNumber;
    public List<string> TurnQueue;       // IDs em ordem de velocidade
    public string CurrentTurnUnitId;
    public List<UnitState> Units;
    public int ActionIndex;              // Anti-replay: incrementa a cada ação
    public long RngSeed;                 // Semente determinística do servidor
    public bool IsFinished;
    public TeamType WinnerTeam;
    public string StageId;
    public DateTime CreatedAtUtc;
    public DateTime LastActionAtUtc;
}
```

**`TurnAction.cs`** — intenção do jogador (1 por requisição):
```csharp
public class TurnAction
{
    public string MatchId;
    public string ActingUnitId;
    public TurnActionType Type;     // Move, UseAbility, Wait, Surrender
    public string AbilityId;        // null se Move ou Wait
    public int AbilityLevel;
    public SimpleVector2Int? TargetPosition;
    public string TargetUnitId;
    public int ExpectedActionIndex; // Anti-replay: deve igualar BattleState.ActionIndex
}
```

**`TurnResult.cs`** — resposta completa do servidor:
```csharp
public class TurnResult
{
    public bool Success;
    public string ErrorMessage;
    public BattleState NewState;        // Estado completo pós-ação
    public List<BattleEvent> Events;    // Tudo que aconteceu (cadeia completa)
    // Eventos em ordem cronológica: dano direto → passivas → mortes → próximo turno
}
```

---

## FASE 2 — Azure Functions de Batalha + Migração Title Data

> Estimativa: 1~2 semanas.

### 2a. Migração do `gamedata.json` para PlayFab Title Data

Alterar `GameDataService.cs` para carregar de Title Data:

```csharp
// ANTES (arquivo local):
string json = File.ReadAllText("gamedata.json");

// DEPOIS (PlayFab Title Data):
var result = await serverApi.GetTitleDataAsync(new GetTitleDataRequest
    { Keys = new List<string> { "MasterGameData" } });
string json = result.Result.Data["MasterGameData"];
```

> [!NOTE]
> O resto do `GameDataService.cs` (GetPetSpecies, GetBanner, etc.) não muda em nada — só a origem do JSON.

### 2b. `BattleFunctions.cs` — Endpoints de Batalha

**`StartPveMatch`:**
1. Valida PlayFabId via `CallerEntityProfile`
2. Chama `ConsumeEnergy` (função já existente) — sem duplicar código
3. Verifica no PlayFab se o jogador não tem já uma `ActiveBattle` em andamento
4. Valida que as unidades/artefatos/pets do time pertencem ao inventário do jogador
5. Gera `RngSeed` aleatória no servidor
6. Monta `BattleState` inicial com `CombatSimulator.Core`
7. Salva em PlayFab Entity Objects com chave `"ActiveBattle_{MatchId}"`
8. Retorna `{ MatchId, InitialState }`

**`SubmitPveTurn`:**
1. Carrega `BattleState` do PlayFab
2. `TurnValidator.Validate(action, state)` — valida anti-replay, propriedade do turno, mana, cooldown, alcance
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
| `DamageProcessor.cs` | **Adicionar** overload `ApplyVisualOnly(DamageEvent ev)` — sem cálculo de dano | **Baixa** |
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

**Passo 1 — `CombatSimulator.Core` (único arquivo alterado):**
```csharp
// Em DamageCalculator.cs, após calcular dano:
if (attacker.Stats.HasLifesteal)
{
    int healAmount = Mathf.RoundToInt(finalDamage * attacker.Stats.LifestealPercent);
    events.Add(new BattleEvent { EventType = "HealApplied", SourceUnitId = attacker.UnitId,
        TargetUnitId = attacker.UnitId, Amount = healAmount });
    attacker.CurrentHealth = Math.Min(attacker.MaxHealth, attacker.CurrentHealth + healAmount);
}
```

**Passo 2 — Backend (`BattleFunctions.cs`):** Zero mudanças.

**Passo 3 — Unity (`TurnResultApplier.cs`):** Registrar handler visual:
```csharp
// "HealApplied" já existe no dicionário — lifesteal usa o mesmo!
// Se fosse um evento 100% novo, você apenas adiciona uma linha ao dicionário.
```

**Total de arquivos alterados:** 1 (ou 2 se o evento visual for novo).

---

## Cronograma

```
FÉRIAS (Julho → Agosto)
├── Semana 1-2: FASE 1 — CombatSimulator.Core + testes unitários
├── Semana 3:   FASE 2 — BattleFunctions.cs + migração Title Data
└── Semana 4:   FASE 3 — NetworkTurnProxy + TurnResultApplier (Unity)

SEMESTRE (Agosto → Dezembro)
├── Semana 1:   Finalizar FASE 3 (PhaseManager refactor)
├── Semana 2-4: FASE 4 — SignalR Hub + PvpFunctions.cs
├── Semana 5:   FASE 4 — PvpSignalRConnector.cs na Unity
└── Semana 6+:  Testes, polish, matchmaking UI
```

---

## Resumo dos Arquivos Novos

| Arquivo | Onde | Propósito |
|---|---|---|
| `CombatSimulator.Core.csproj` | `CombatSimulator.Core/` (novo) | Biblioteca C# pura compartilhada |
| `Models/BattleState.cs` | `CombatSimulator.Core/` | Estado completo da partida |
| `Models/UnitState.cs` | `CombatSimulator.Core/` | Unidade sem MonoBehaviour |
| `Models/TurnAction.cs` | `CombatSimulator.Core/` | Intenção do jogador |
| `Models/TurnResult.cs` | `CombatSimulator.Core/` | Resposta completa do servidor |
| `Models/BattleEvent.cs` | `CombatSimulator.Core/` | Evento extensível (padrão central) |
| `Simulation/CombatSimulator.cs` | `CombatSimulator.Core/` | Ponto de entrada único da simulação |
| `Simulation/TurnOrderResolver.cs` | `CombatSimulator.Core/` | Fila por velocidade |
| `Simulation/DamageCalculator.cs` | `CombatSimulator.Core/` | Fórmulas de dano/cura |
| `Simulation/StatusEffectProcessor.cs` | `CombatSimulator.Core/` | Ticks de buffs/debuffs |
| `Simulation/AIResolver.cs` | `CombatSimulator.Core/` | IA sem Unity |
| `Validation/TurnValidator.cs` | `CombatSimulator.Core/` | Segurança e anti-cheat |
| `Validation/GridValidator.cs` | `CombatSimulator.Core/` | Alcance e movimentação |
| `Backend/BattleFunctions.cs` | `Backend/` | Endpoints PvE |
| `Assets/.../NetworkTurnProxy.cs` | Unity `Scripts/Cloud/` | Envia ações ao servidor |
| `Assets/.../TurnResultApplier.cs` | Unity `Scripts/Cloud/` | Anima resultados do servidor |
| **[PvP]** `Backend/PvpFunctions.cs` | `Backend/` | Endpoints PvP + SignalR |
| **[PvP]** `Assets/.../PvpSignalRConnector.cs` | Unity `Scripts/Cloud/` | WebSocket para PvP |
