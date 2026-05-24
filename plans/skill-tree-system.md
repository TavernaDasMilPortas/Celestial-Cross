# Sistema de Árvore de Habilidades + Atributos + Variáveis Globais — Plano de Implementação v3

> **Versão:** 3.0  
> **Data:** 2026-05-23  
> **Status:** Aguardando aprovação  
> **Escopo:** Aliados (Player Units). Inimigos mantêm o sistema atual por enquanto.

---

## 1. Visão Geral

### 1.1 Este plano cobre 5 pilares de mudança

| Pilar | Descrição |
|---|---|
| **A. Novos Atributos** | `CriticalDamage` e `EffectResistance` integrados em TODA a cadeia |
| **B. Árvore de Habilidades** | Slots customizáveis + ramos de modificação cumulativos |
| **C. Variáveis Globais da Unidade** | Atributos e custom vars expostos no `UnitVariableStore` (global + per-slot) |
| **D. Scaling Dinâmico no Grafo** | Dano e Cura escalam por % de atributos (sem valores flat); Novos Nós |
| **E. UI & Builders** | Sub-abas de inventário, modal de passivas no combate, utilitários de editor |

---

## 2. Pilar A — Novos Atributos: CriticalDamage e EffectResistance

### 2.1 Auditoria do Estado Atual

> [!WARNING]
> **Bugs existentes descobertos na auditoria** que serão corrigidos nesta implementação:
> - `CriticalDamage` e `EffectResistance` já existem no `StatType` enum dos artefatos e no `StatModifierEffectNode`, mas **nunca foram integrados** ao `CombatStats`
> - Artefatos podem rolar essas stats, mas elas são **silenciosamente descartadas** em `Unit.Stats` (L181: comentário explícito admitindo)
> - `DamageProcessor` usa multiplicador crítico **hardcoded de 2.0f** ao invés de ler um stat
> - O sistema de passivas escreve `bonus_crit_damage` mas o processador lê `crit_mult_bonus` — **nomes diferentes**!
> - Nenhum check de `EffectResistance vs EffectAccuracy` existe em lugar algum do código

### 2.2 Fórmulas

**Dano Crítico:**
```
critMultiplier = 1.0 + (baseCritDamage + bonusCritDamage) / 100
// baseCritDamage = 50 (implícito para todos)
// Exemplo: 50 base + 0 bônus = 1.5x = 150% do dano
// Exemplo: 50 base + 100 bônus = 2.5x = 250% do dano
```

**Resistência a Efeitos (2 camadas):**
```
1. A habilidade rola: rand(0,100) < effectAccuracy do atacante?
   - Se falhou → efeito não aplica (fim)
   - Se passou → vai para camada 2

2. Defensor resiste: rand(0,100) < max(0, effectResistance_defensor - effectAccuracy_atacante)?
   - Se resistiu → efeito bloqueado
   - Se não resistiu → efeito aplicado
```

### 2.3 Mudanças em CombatStats

> **Arquivo:** [CombatStats.cs](file:///c:/Users/Rubens/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Unit/Character/CombatStats.cs)

```diff
 [System.Serializable]
 public struct CombatStats
 {
     [Min(1)] public int health;
     public int attack;
     public int defense;
     public int speed;
     [Range(0, 100)] public int criticalChance;
+    public int criticalDamage;        // Base: 50 (= 150% do dano em crit)
     [Range(0, 100)] public int effectAccuracy;
+    [Range(0, 100)] public int effectResistance;  // Base: 0

-    public CombatStats(int health, int attack, int defense, int speed, int criticalChance, int effectAccuracy)
+    public CombatStats(int health, int attack, int defense, int speed, int criticalChance, int criticalDamage = 50, int effectAccuracy = 0, int effectResistance = 0)
     {
         // ... todos os campos
     }
 }
```

### 2.4 Mudanças no DamageProcessor

> **Arquivo:** [DamageProcessor.cs](file:///c:/Users/Rubens/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Combat/Execution/DamageProcessor.cs)

```diff
 // ANTES (L34):
-float critMult = 2.0f;
-if (context.Variables.TryGetValue("crit_mult_bonus", out float cMult)) critMult += cMult;

 // DEPOIS:
+int baseCritDmg = context.source != null ? context.source.Stats.criticalDamage : 50;
+if (context.Variables.TryGetValue("bonus_crit_damage", out float cdBonus))
+    baseCritDmg += Mathf.RoundToInt(cdBonus);
+float critMult = 1.0f + (baseCritDmg / 100f);
```

### 2.5 Refatoração dos Pets

> **Arquivo:** [PetSpeciesSO.cs](file:///c:/Users/Rubens/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Data/Pets/PetSpeciesSO.cs)

```csharp
[System.Serializable]
public class PetStatRange
{
    public PetStatType stat;
    public float min;
    public float max;
}

public enum PetStatType
{
    Health, Attack, Defense, Speed,
    CriticalChance, CriticalDamage,
    EffectAccuracy, EffectResistance
}

// No PetSpeciesSO:
[Header("Stats (até 5)")]
public List<PetStatRange> statRanges = new List<PetStatRange>(); // Max 5
```

---

## 3. Pilar B — Árvore de Habilidades

### 3.1 Novos ScriptableObjects

- `SkillTreeConfigSO` — SO central por personagem
- `SkillEntry` — Habilidade individual com ramos
- `SkillSlotType` — Enum (Basic, Movement, Slot1, Slot2)
- `SkillBranchTree` — Árvore hierárquica de ramos
- `SkillBranchSelection` — Persistência de ramos escolhidos

### 3.2 Persistência e UnitData

Adicionar `SkillTreeConfigSO skillTreeConfig` ao `UnitData`.
Adicionar `Slot1SkillId`, `Slot2SkillId`, `List<SkillBranchSelection> branchSelections` ao `UnitLoadout`.

> [!IMPORTANT]
> `CriticalChance`, `CriticalDamage`, `EffectAccuracy` e `EffectResistance` **NÃO escalam com level**. Em `UnitData.GetStatsAtLevel`, eles retornam o valor base direto, sem interpolação.

---

## 4. Pilar C — Variáveis Globais e Atributos

### 4.1 Enum UnitVariable unificado

Para permitir que habilidades referenciem atributos base (como Speed) e variáveis customizadas (como BonusDamage), tudo será acessível via enum unificado:

```csharp
public enum UnitVariable
{
    // === Atributos Base (interceptados da Unit) ===
    Health, Attack, Defense, Speed,
    CriticalChance, CriticalDamage, EffectAccuracy, EffectResistance,

    // === Amplificação e Modificação ===
    BonusDamagePercent,      // % bônus de dano geral
    DamageReductionPercent,  // % redução de dano recebido
    HealingBonusPercent,     // % bônus de cura dada
    
    // === Alcance & Movimento ===
    ExtraRange,              // Alcance adicional (tiles)
    ExtraMoveRange,          // Movimento adicional (tiles)
    
    // === Contadores Livres ===
    Counter1, Counter2, Counter3
}
```

### 4.2 Implementação do `UnitVariableStore`

O store agora tem conhecimento da `Unit` à qual pertence para resolver os atributos base dinamicamente.

```csharp
[System.Serializable]
public class UnitVariableStore
{
    private Unit owner;
    private Dictionary<UnitVariable, float> globalVars = new();
    private Dictionary<SkillSlotType, Dictionary<UnitVariable, float>> slotVars = new();
    
    public void Initialize(Unit unit) => owner = unit;

    public float Get(UnitVariable variable, SkillSlotType? slot = null)
    {
        // Se for atributo base, retorna direto do CombatStats atual
        if (IsBaseStat(variable)) return GetBaseStat(variable);

        float total = globalVars.TryGetValue(variable, out float g) ? g : 0f;
        if (slot.HasValue && slotVars.TryGetValue(slot.Value, out var dict))
        {
            if (dict.TryGetValue(variable, out float s)) total += s;
        }
        return total;
    }
    
    // Set e Add implementados apenas para não-atributos. Atributos base são imutáveis por Set.
}
```

---

## 5. Pilar D — Scaling Dinâmico no Grafo

### 5.1 Fim do Dano Flat — Novo `StatScalingData`

Atendendo ao requisito de *não termos mais dano/cura flat*, e sim dependente de porcentagem e status:

```csharp
[Serializable]
public class StatScalingData
{
    public UnitVariable stat;          // Qual status/variável (ex: UnitVariable.Speed)
    public float percentage;           // Qual porcentagem desse status (ex: 10f para 10%)
    public string percentageVariable;  // Opcional: Variável local que sobrescreve a porcentagem
}
```

### 5.2 Modificação no DamageNodeData e HealNodeData

> **Arquivo:** [AbilityNodeRuntimeData.cs](file:///c:/Users/Rubens/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Abilities/Graph/Runtime/AbilityNodeRuntimeData.cs)

```diff
 [Serializable]
 public class DamageNodeData
 {
-    public string variableReference; 
-    public int baseAttribute = (int)AttributeCondition.AttributeType.Attack;
+    public List<StatScalingData> scalings = new List<StatScalingData>();
     public bool scaleWithDistance = false;
     public float distanceScaleFactor = 0.1f;
 }
```

**Como funciona no Interpretador:**
Se o nó de dano tiver `[{stat: Attack, percentage: 100}, {stat: Speed, percentage: 10}]`, o dano base (`context.amount`) será `(Attack * 1.0f) + (Speed * 0.10f)`.

### 5.3 Novos Nós de Variáveis

- **`SkillBranchNode`**: Nó visual para ramificações da SkillTree.
- **`ReadUnitVariableNode`**: Lê uma `UnitVariable` (agora incluindo os Atributos da Unidade) do Store e joga pra variável local do Grafo.
- **`WriteUnitVariableNode`**: Escreve numa `UnitVariable` do Store (somente não-atributos, como `BonusDamagePercent`).
- **TargetNode Range Dinâmico**: Flag `useExtraRangeVariable` para somar `ExtraRange` do Store.

---

## 6. Pilar E — Interface de Usuário & Builders

### 6.1 Sub-Abas no Painel de Unidade (Inventário)

Ao selecionar uma unidade, o painel terá 3 sub-abas:
| Sub-Aba | Conteúdo | Status |
|---|---|---|
| **Status & Equip** | 8 stats (com CritDamage e EffectResistance), artefatos, pet | Existente — expandir |
| **Constelação** | Árvore de constelação (atualmente modal) | Mover para aba |
| **Habilidades** | 4 slots + árvore de ramos | **NOVO** |

### 6.2 Modal de Passivas no Combate

A `ActionBarUI` passa a mostrar apenas os atalhos de ação (Mover, Atacar, etc) e um botão `[📋 Passivas]`.  
O modal tem 3 seções:
1. **Passivas** (Constelação, artefatos, pets)
2. **Condições** (Ativas no turno via grafo)
3. **Buffs/Debuffs** (Status modifiers)

### 6.3 Builders de Cena

- **`InventorySceneSetupUtility`**: Automatiza a criação das abas, slots de habilidades, e modais (`SkillSelectionModal`, `SkillBranchModal`).
- **`CombatUISetupUtility`**: Cria o botão de Passivas na ActionBar e o `PassiveListModal`.

---

## 7. Fases de Implementação

### Fase 1 — Fundação e Correções (Prioridade ALTA)
- [ ] Integrar `criticalDamage` e `effectResistance` no `CombatStats`
- [ ] Corrigir pipelines: `DamageProcessor` (crit dinâmico) e Artefatos (silencing stats)
- [ ] Implementar `EffectResistanceCheck.cs`

### Fase 2 — Refatoração de Pets
- [ ] Migrar `PetSpeciesSO` para listas dinâmicas de 5 stats

### Fase 3 — Variáveis Globais e Scaling Dinâmico
- [ ] Criar enum `UnitVariable` (com os atributos base) e classe `UnitVariableStore`
- [ ] Integrar `UnitVariableStore` na `Unit`
- [ ] Criar classe `StatScalingData`
- [ ] Alterar `DamageNodeData` e `HealNodeData` para listas de scaling
- [ ] Modificar Interpretador para calcular `context.amount` via scaling das variáveis

### Fase 4 — Fundação de Dados da SkillTree
- [ ] Criar SOs: `SkillTreeConfigSO`, `SkillEntry`, `SkillSlotType`, `SkillBranchTree`
- [ ] Expandir `UnitLoadout` (SaveSystem)
- [ ] Criar novos nós de grafo (`SkillBranchNode`, `ReadUnitVariableNode`, `WriteUnitVariableNode`)

### Fase 5 — Interface de Usuário
- [ ] Criar Builders Utilitários (Inventário e Combate)
- [ ] Desenvolver `SkillTabUI`, modais de seleção e ramos
- [ ] Refatorar Inventário para exibir as 8 stats
- [ ] Desenvolver `PassiveListModal` e atualizar ActionBar

### Fase 6 — Testes
- [ ] Fluxo completo e persistência
- [ ] Testar Scaling Dinâmico: Dano dependendo de `Speed` ou `% de HP`
- [ ] Garantir não quebra de inimigos sem SkillTree
