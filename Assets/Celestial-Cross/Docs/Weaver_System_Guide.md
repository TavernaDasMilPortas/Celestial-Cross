# Weaver System - Guia Técnico (Passivas e Condições)

O **Weaver System** é o motor de reatividade e efeitos modulares do Celestial Cross. Ele permite que habilidades, itens e status effects interajam com o pipeline de combate de forma dinâmica.

## 1. O Pipeline de Hooks
O sistema baseia-se em interceptar momentos específicos do jogo, definidos no enum `CombatHook`:
- `OnRoundStart / OnRoundEnd`: Útil para efeitos globais ou recarga.
- `OnTurnStart / OnTurnEnd`: Dispara efeitos de "tick" (ex: Veneno) ou buffs temporários.
- `OnBeforeTakeDamage / OnAfterTakeDamage`: Permite que passivas reduzam dano recebido ou reajam a ele (contra-ataque).
- `OnBeforeAction / OnAfterAction`: Reações ao uso de habilidades.

## 2. PassiveManager
Cada unidade possui um componente `PassiveManager`. Ele é o cérebro que:
1.  Escuta eventos globais do `TurnManager`.
2.  Gerencia as `PassiveAbilityData` (passivas fixas).
3.  Gerencia as `ConditionInstance` (status effects ativos).
4.  Expõe o método `TriggerHook(hook, context)` para ser chamado por outros sistemas (como o `Health`).

## 3. Efeitos Modulares (IAbilityEffect)
Em vez de programar cada habilidade do zero, você compõe efeitos:
- **DamageEffect**: Causa dano direto.
- **HealEffect**: Cura vida.
- **StatModifierEffect**: Modifica atributos (ATK, DEF, SPD, etc) da unidade alvo.
- **ApplyConditionEffect**: Aplica um status effect.

### Como criar novos efeitos:
Basta criar uma classe que herde de `AbilityEffectBase` e implementar o método `Execute(CombatContext context)`. Graças ao `AbilityEffectDrawer`, ele aparecerá automaticamente no dropdown do Inspector.

## 4. Criando uma Passiva de Contra-Ataque
1.  Crie um asset: `Create > Combat > Weaver Passive`.
2.  Nome: "Counter-Attack".
3.  Trigger: `OnAfterTakeDamage`.
4.  Effects: Adicione um `DamageEffect`.
5.  Adicione este asset na lista de passivas do `PassiveManager` da unidade.

## 5. Criando uma Condição de Veneno (Poison)
1.  Crie um asset: `Create > Combat > Weaver Condition`.
2.  Duração: 3 turnos.
3.  Tick Effects: Adicione um `DamageEffect` (amount: 2).
4.  Agora, use um `ApplyConditionEffect` em qualquer ataque para aplicar este veneno ao alvo.
