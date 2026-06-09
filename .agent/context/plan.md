# Arquitetura de Passivas, Condições e Pipeline de Combate

Este plano estabelece uma infraestrutura robusta para habilidades complexas, permitindo que passivas e status effects reajam a eventos em múltiplos níveis (Round, Turn, Action, Damage).

## User Review Required
> [!IMPORTANT]
> Esta mudança é uma reestruturação profunda. Ações vao deixar de ser scripts únicos (ex: AttackAction) e passarão a ser composições de **Efeitos** (ex: CompositeAction com DamageEffect + ApplyConditionEffect).

## Proposed Changes

### [Core: Combat Pipeline (Hooks)]
Estabelecer os pontos de intercepção onde passivas e condições podem "ouvir" o jogo.

#### [NEW] [CombatHook.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Combat/CombatHook.cs)
- Enumeração de todos os momentos: `OnRoundStart`, `OnTurnStart`, `OnBeforeDamage`, `OnAfterDamage`, `OnTurnEnd`, etc.

#### [MODIFY] [TurnManager.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/TurnManager/TurnManager.cs)
- Implementar contador de **Rounds**.
- Disparar eventos de Round Start/End.

#### [MODIFY] [Health.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Unit/HealthSystem/Health.cs)
- Adicionar hooks de intercepção: `OnBeforeTakeDamage` (permite que passivas reduzam dano ou o ignorem).

---

### [Module: Conditions (Status Effects)]
Sistema de buffs e debuffs configuráveis via Inspector.

#### [NEW] [ConditionData.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Combat/Conditions/ConditionData.cs)
- ScriptableObject que define o efeito (ícone, duração, efeitos por turno).

#### [NEW] [ConditionInstance.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Combat/Conditions/ConditionInstance.cs)
- Representação em runtime de uma condição aplicada a uma unidade.

---

### [Module: Composite Effects]
Decompor ações em blocos reutilizáveis.

#### [NEW] [IAbilityEffect.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Combat/Effects/IAbilityEffect.cs)
- Interface base: `void Execute(EffectContext context)`.

#### [NEW] Efeitos Atômicos:
- `DamageEffect`: Causa dano.
- `HealEffect`: Cura vida.
- `ApplyConditionEffect`: Aplica um `ConditionData`.
- `TeleportEffect`: Move a unidade instantaneamente.

---

### [Module: Passive Abilities]
Habilidades que reagem ao Pipeline.

#### [NEW] [PassiveAbilityData.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Combat/Passives/PassiveAbilityData.cs)
- Define a condição de ativação (Gatilho) e os efeitos resultantes.

## Verification Plan

### Automated Tests
- Criar unidade com passiva "Counter-Attack" (Trigger: OnAfterDamage, Effect: DamageEffect no atacante).
- Validar se o dano é retribuído automaticamente.
- Aplicar condição "Poison" e validar dano no `OnTurnStart`.

### Manual Verification
- Testar no Inspector a criação de uma habilidade que aplica 2 condições diferentes e causa dano em área.
