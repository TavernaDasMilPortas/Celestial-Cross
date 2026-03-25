# Mecânicas de Combate - Celestial Cross

O combate é baseado em turnos táticos em um grid 2D, com foco em composição de habilidades e reatividade.

## 1. Fluxo de Combate
O combate é dividido em **Rounds** e **Turns**:
- **Turno**: O momento individual de uma unidade agir.
- **Round**: Um ciclo completo onde todas as unidades agiram uma vez. No início e fim de cada Round e Turno, o `PassiveManager` dispara hooks (`OnRoundStart`, `OnTurnEnd`, etc.).

## 2. Sistema de Ações
As unidades possuem **Native Actions** (ataque padrão) e **Abilities**. Todas herdam de `UnitActionBase`.
- **Granularidade**: Ataques e habilidades de múltiplos hits são processados individualmente, permitindo que passivas e condições reajam a cada hit (ex: chance de veneno por acerto).
- **Execution Pipeline**: Toda ação dispara o hook `OnAfterAction` no final de sua execução, permitindo gatilhos de pós-ataque.

## 3. Cálculo de Dano e Cura
O `DamageModel.cs` realiza a matemática base simplificada:
- **Fórmula**: `Dano = (Ataque - Defesa)`.
- **Mutação via Contexto**: O valor final não é mais alterado por parâmetros fixos. Em vez disso, o `CombatContext` é passado por uma pipeline de hooks (`OnBeforeTakeDamage`) onde passivas e condições multiplicam ou somam valores diretamente no contexto antes da aplicação final.

## 4. Weaver System (Hooks e Passivas)
O núcleo de reatividade do combate. O sistema utiliza um padrão de **Hooks** para interceptar eventos:
- **Hooks de Dano**: `OnBeforeTakeDamage` permite modificar o dano recebido.
- **Hooks de Turno**: `OnTurnStart`, `OnTurnEnd`, `OnRoundStart`.
- **Hooks de Ação**: `OnAfterAction` disparado após qualquer habilidade/ataque.
- **Hooks de Condição**: `OnBeforeApplyCondition` permite que passivas impeçam ou modifiquem a aplicação de status (ex: Imunidade).

As **Passivas** são definidas via `PassiveAbilityBlueprint` (ScriptableObject), contendo uma lista de `PassiveEffect` que filtram por qual `CombatHook` devem ser ativados.

## 5. Feedback Visual
- **Damage Popups**: Números flutuantes que indicam dano/cura.
- **Turn Timeline**: UI que mostra a fila de turnos atual.
- **Targeting Visuals**: Destaque de tiles no grid durante a seleção de alvos.
