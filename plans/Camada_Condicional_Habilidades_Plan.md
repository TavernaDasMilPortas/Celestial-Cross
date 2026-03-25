# Plano: Camada de Ação Condicional nas Habilidades

Este documento detalha o plano de criação de uma "camada condicional" nas habilidades e ataques, removendo hardcodes (como os golpes extras gerados estritamente por estatísticas base de Velocidade) e migrando essas regras para um sistema de efeitos e condições avaliados em tempo real.

## 1. O Problema Atual
Anteriormente, certas lógicas de combate estavam presas ao código principal dos modelos de dano e ações nativas. Por exemplo, a lógica de velocidade (`DamageModel.GetAttackCountBySpeed`) verificava se `velocidade_atacante >= velocidade_alvo + 10` para forçar `2` hits.
Isso se torna inflexível e limitante, impossibilitando que passivas alterem dinamicamente essa regra, ou que exista ataques imunes a isso, que ganham esse bônus de outros meios (por Ex: Condições específicas no alvo).

## 2. Abordagem da Camada Condicional
A lógica será convertida em **Condition Evaluators** dentro do pipeline de dados de habilidade e dos Efeitos (Effects).

**Pila de funcionamento esperado:**
1. **Contexto de Combate (CombatContext):** Passará a suportar e carregar as avaliações (quem ataca, quem defende, se já foi avaliada uma mecânica extra).
2. **Evaluator Nodes / Conditionals:** Objetos como `SpeedDifferenceCondition`, `HasStatusCondition` poderão ser anexados aos Blueprints (nas listas de Efeitos ou como multiplicadores de Casts)
3. **Execution Modifier:** Se uma condição de "Velocidade > x" for verdadeira, o Efeito será instruído a rodar `N` repetições no AbilityExecutor ou a aplicar o modificador de Hit extra via Passiva/AbilityData.

## 3. Tarefas de Implementação

*   [ ] **1. Criação Base das Condições (Evaluators):** Criar uma classe abstrata `AbilityCondition` com um método `bool Evaluate(CombatContext context)`.
*   [ ] **2. Implementar Condição de Velocidade:** Criar classe herdeira `SpeedAdvantageCondition` que implemente a lógica de diferença `A.Speed >= B.Speed + X`.
*   [ ] **3. Implementar Condição de Status/Health:** Criar `TargetHasStatusCondition` (ex: isBurned) e `HealthThresholdCondition` (Ex: isLowHealth) para que o designer possa criar habilidades com efeitos variáveis em alvos feridos.
*   [ ] **4. Modificador de Execução (Repeater/Multiplexer):** Integrar ao `AbilityExecutor` (ou aos nós de repasses individuais de `EffectData`) a validação dessas condições. Exemplo: Um array de `ConditionalEffects` no `AbilityBlueprint`. Se as condições baterem, adicione esse efeito/hit extra à execução padrão do Blueprint.
*   [ ] **5. Teste e Validação:** Criar Blueprints na Unit de teste, contendo dois hits, sendo 1 hit condicional apenas se a velocidade cobrir a antiga regra `X + 10`. Validar os mesmos resultados obtidos anteriormente no log.

## 4. Benefícios Práticos
- Ataques duplos se tornam uma *mecânica de habilidade* ou de *passiva* (podendo ser ativada/desativada nativamente como qualquer buff), em vez de uma matemática global impossível de ignorar.
- A IA do Inimigo agora pode avaliar via AIBrain os Effects listados antes do cast condicional, descobrindo com o Blueprint se valerá a pena lançar a habilidade no Alvo A (onde o condicional da duplo hit) versus o Alvo B (onde ele causaria um simples tick).