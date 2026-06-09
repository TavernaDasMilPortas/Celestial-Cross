# Tasks - Weaver System (Passivas e Condições)

## 1. Infraestrutura do Pipeline (Hooks)
- [ ] Criar `CombatHook.cs` (Enumeração de gatilhos: OnTurnStart, OnAfterDamage, etc.)
- [ ] Atualizar `TurnManager.cs` para incluir lógica de **Rounds** e disparar gatilhos globais.
- [ ] Atualizar `Health.cs` para incluir hooks de **Pre-Damage** e **Post-Damage**.
- [ ] Criar `PassiveManager` na unidade para gerenciar inscrições de passivas e condições.

## 2. Experiência do Editor (Drawers)
- [ ] Criar `AbilityEffectDrawer.cs` (Custom Property Drawer para `[SerializeReference]`).
    - [ ] Implementar dropdown de seleção de tipo de efeito.
    - [ ] Garantir que a UI seja dinâmica e limpa.

## 3. Biblioteca de Efeitos Modulares
- [ ] Definir `IAbilityEffect` e `EffectContext`.
- [ ] Implementar efeitos básicos (configuráveis via Inspector):
    - [ ] `DamageEffect` (Dano direto/percentual)
    - [ ] `HealEffect` (Cura direta/percentual)
    - [ ] `ModifyStatEffect` (Aumento/diminuição de força, velocidade, etc.)
    - [ ] `ApplyConditionEffect` (Aplica um status effect)

## 4. Módulo de Condições (Status Effects)
- [ ] Criar `ConditionData` (ScriptableObject)
    - [ ] Lista de efeitos para: `OnApply`, `OnTick` (Turn Start), `OnExpire`.
- [ ] Implementar sistema de duração e stack de condições.

## 5. Habilidades Passivas (Reações)
- [ ] Criar `PassiveAbilityData` (ScriptableObject)
    - [ ] Configuração: `CombatHook` + `Filtros` + `Lista de Efeitos`.
- [ ] Integrar no `UnitData` para que passivas sejam carregadas automaticamente.

## 6. Evolução das Ações Ativas
- [ ] Criar `CompositeUnitAction` que herda de `UnitActionBase`.
    - [ ] Permitir que uma única ação (ex: Ataque) execute múltiplos `IAbilityEffect` em sequência.
