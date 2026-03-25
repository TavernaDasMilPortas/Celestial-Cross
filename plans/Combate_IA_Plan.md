# Plano Mestre: Sistema de Combate Granular, IA e Gestão de Condições/Status

Este documento detalha a reestruturação completa do ciclo de vida de combate do Celestial Cross. O objetivo é garantir granularidade total (por hit), inteligência para a IA via Blueprints, e um sistema robusto de Condições (Status) com durações e empilhamento configuráveis.

---

## 1. Visão Geral das Mudanças

*   **Granularidade de Dano:** Atualmente os hooks agrupam o dano de multi-hits em um único evento. Eles passarão a disparar individualmente por hit, acionando passivas (ex: +2 Atk ao sofrer dano) múltiplas vezes em um ataque triplo.
*   **Reintegração da IA:** Scripts antigos como AttackAction rodando stand-alone quebram a inteligência das unidades inimigas. A IA passará a ler os AbilityBlueprint diretamente e chamar o AbilityExecutor, usando lógica de contagem de utilidade baseada nos Efeitos.
*   **Pipeline do CombatContext:** O TakeDamage usará como base o context.amount final *após* todas as passivas de OnBeforeTakeDamage terem alterado o seu valor.
*   **Motor de Expiração (Duration Engine):** Modificadores e passivas não são mais permanentes se não desejado. Um sistema unificado contará turnos ou rodadas baseadas no gatilho escolhido (OnTurnEnd, TurnStart, OtherUnitTurns, etc).
*   **Empilhamento de Efeitos (Stack & Cap):** Bônus (como StatModifierEffectData) agora têm maxStacks ou maxAmount (ex: máximo de 10 de aumento no status base) com IDs únicos gerenciados pelo PassiveManager.

---

## 2. Tabela de Tarefas e Implementação

| ID | Categoria | Descrição | Arquivo Alvo / Foco | Dependência |
|:---|:---|:---|:---|:---|
| **F1.1** | Infra/Hooks | **Granularidade de Dano**: Alterar AttackAction para processar cada hit em um laco de TakeDamage individual, em vez de acumular. | AttackAction.cs | - |
| **F1.2** | Infra/Hooks | **Pipeline de Dano**: Atualizar Health.TakeDamage para aplicar o mount final recebido do CombatContext após o hook OnBeforeTakeDamage. | Health.cs | - |
| **F1.3** | Infra/Hooks | **Hooks de Causa**: Disparar OnBeforeDealDamage e OnAfterDealDamage no PassiveManager da unidade que *causa* o dano. | Health.cs | F1.2 |
| **F1.4** | Infra/Hooks | **Ciclo de Turno**: Implementar escuta de OnTurnEnd no PassiveManager vinculado à Action atual do TurnManager. | PassiveManager.cs | - |
| **F1.5** | Infra/Hooks | **Ciclo de Ação**: Garantir o disparo de OnAfterAction ao finalizar qualquer UnitActionBase. | UnitActionBase.cs | - |
| **F2.1** | IA | Substituir busca rígida por UnitActionBase na IA para realizar leitura dos AbilityBlueprint via unit.Data. | AIBrain.cs | - |
| **F2.2** | IA | Criar método CalculateUtility (Scorer) na IA para avaliar utilidade do EffectData (Dano, Cura, Buffs) nos Blueprints. | AIBrain.cs | F2.1 |
| **F2.3** | IA | Trocar a execução manual de ações velhas por chamada ao método AbilityExecutor.Instance.ExecuteAbility. | AIBrain.cs | F2.2 |
| **F3.1** | Condições | **Duration Engine**: Criar estrutura de dados DurationType (Rounds, UnitsTurns, NextTurnStart, OtherUnitTurns). | Novo: ConditionInstance.cs | - |
| **F3.2** | Condições | **Condition Object**: Criar a classe encapsuladora ConditionInstance para rastrear ticks, stacks, bônus e Blueprint pai. | ConditionInstance.cs | F3.1 |
| **F3.3** | Condições | **Manager Update**: Atualizar o PassiveManager para gerenciar a \List<ConditionInstance>\ e disparar os expurgos/ticks nos hooks. | PassiveManager.cs | F3.2 |
| **F3.4** | Status/Stacks| **Stat Modifiers**: Integrar StatModifierEffectData à nova lógica criando variáveis de ID único, maxStacks e limitadores (maxAmount). | StatModifierEffectData.cs | F3.3 |
| **F4.1** | Validação | **Teste Queimadura**: Criar Burn (dá dano no OnTurnStart sem chance de cura, dura 3 rodadas cheias) e rodar logger. | Blueprints (Unity) | F3.3 |
| **F4.2** | Validação | **Teste Veneno**: Criar Poison (dá dano no OnTurnEnd, duração: 2 turnos apenas da unidade dona) e validar logs. | Blueprints (Unity) | F3.3 |
| **F4.3** | Validação | **Teste Ferocidade Multi-Hit**: Criar passiva com +2 ATK (max 10) procastinando cada hit recebido, com fim garantido na prox rodada. Validação essencial da Fase 1 x Fase 3. | Blueprints (Unity) | F1.1, F3.4 |

---

## 3. Dinâmica Exemplificada da Implementação Final

Após aplicar essas mudanças e executar as sessões focadas nisso, o TurnManager e AbilityExecutor atuarão assim na prática:

1. A Unidade aliada ativa **Ferocidade** no inicio do combate, ou de uma rodada (registra ID na ConditionInstance via PassiveManager).
2. Uma IA Inimiga (AIBrain lendo o Scorer -> AbilityExecutor) dispara um **Ataque Triplo**.
3. O AttackAction avalia os status das unidades e dispara 3 "TakeDamage" em vez de um agrupado.
4. Para CADA HIT recebido, a "Ferocidade" engatilhada por OnBeforeTakeDamage aumentará o status de ataque da unidade alvo em +2, com o limite estrito da pilha de maxAmount = 10 ou maxStacks = 5.
5. Se a defesa absorver ou curar com outra passiva, ela muta o buffer do CombatContext.amount, refletindo no valor que perfura o escudo e abate CurrentHealth.
6. Quando o gatilho da condição (ex: OtherUnitTurns = 3) estourar o tick registrado no PassiveManager, o buff de ATK será zerado e retirado.

---
**Observações:** Inicie o novo chat pedindo direto para implementar a Fase 1 (Granularidade de Dano e Infra de Hooks).