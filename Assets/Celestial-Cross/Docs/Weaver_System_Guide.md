# Weaver System - Guia Técnico (Passivas e Condições)

O **Weaver System** é o motor de reatividade e efeitos modulares do Celestial Cross. Ele permite que habilidades, itens e status effects interajam com o pipeline de combate de forma dinâmica.

## 1. O Pipeline de Hooks
O sistema baseia-se em interceptar momentos específicos do jogo, definidos no enum `CombatHook`:
- `OnRoundStart / OnRoundEnd`: Útil para efeitos globais ou recarga.
- `OnTurnStart / OnTurnEnd`: Dispara efeitos de "tick" (ex: Veneno) ou buffs temporários.
- `OnBeforeTakeDamage`: Permite que passivas reduzam dano recebido ou reajam a ele (mutando o `CombatContext.Amount`).
- `OnBeforeApplyCondition`: Permite interceptar a aplicação de status (ex: Imunidade).
- `OnAfterAction`: Reações ao uso de habilidades disparadas no final de cada ação.

## 2. PassiveManager
Cada unidade possui um componente `PassiveManager`. Ele é o cérebro que:
1.  Escuta eventos globais do `TurnManager`.
2.  Gerencia as `PassiveAbilityBlueprint` (passivas fixas).
3.  Gerencia as `ConditionInstance` (status effects ativos).
4.  Expõe o método `TriggerHook(hook, context)` para ser chamado por outros sistemas.

## 3. PassiveAbilityBlueprint e PassiveEffect
As passivas agora são modulares. Cada `PassiveAbilityBlueprint` contém uma lista de `PassiveEffect`:
- **PassiveEffect**: Uma classe base que define a lógica de reação.
- **Trigger Filter**: O efeito só é executado se o `CombatHook` atual corresponder ao configurado no blueprint.

## 4. O CombatContext
O objeto que transporta dados entre os hooks. Ele permite que uma passiva:
- Mude o valor de dano (`Amount`).
- Verifique quem é o `Source` e o `Target`.
- Cancele um efeito (`WasInterrupted = true`).
- Acesse o blueprint do status sendo aplicado (`ConditionBlueprint`).

## 5. Como Criar uma Passiva de Contra-Ataque
1.  Crie um script herdando de `PassiveEffect` (ex: `CounterAttackEffect`).
2.  No Editor, crie um asset `PassiveAbilityBlueprint`.
3.  Adicione o `CounterAttackEffect` à lista de efeitos.
4.  Configure o trigger para `OnAfterAction` ou `OnBeforeTakeDamage`.
5.  Atribua o Blueprint à unidade.
