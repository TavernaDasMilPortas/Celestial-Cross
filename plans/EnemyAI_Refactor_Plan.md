# Plano de Refatoração da IA de Inimigos

Este documento centraliza as decisões e diretrizes estabelecidas para a futura refatoração do sistema de Inteligência Artificial dos inimigos em Celestial Cross.

## 1. Visão Geral da Arquitetura
- **Substituição Total:** O atual sistema de `AIBehaviorProfile` (baseado em listas de prioridade em ScriptableObjects) será **completamente substituído**. Todos os inimigos, desde os mais simples até os chefes, utilizarão o novo sistema.
- **Editor Visual Dedicado:** Será criado um editor visual próprio (Node-based) para as **AI Behavior Trees**, estruturalmente independente do atual sistema de `AbilityGraph`.
- **Abordagem Híbrida (Tree + Utility):** 
  - A **Behavior Tree** fará a tomada de decisão em alto nível (o "O QUÊ" fazer).
  - O sistema de **Utility AI scoring** continuará existindo internamente nos nós de ação comuns (o "COMO" fazer), calculando o melhor tile para se mover ou o melhor alvo para atacar baseado em pesos.

## 2. Tipos de Nós (Nodes) Planejados
O editor de Behavior Tree deverá suportar inicialmente a seguinte gama de nós:

### 2.1. Nós de Controle (Core Behavior Tree)
- **Selector:** Executa seus filhos da esquerda para a direita até que UM retorne sucesso.
- **Sequence:** Executa seus filhos da esquerda para a direita até que UM retorne falha.

### 2.2. Nós de Condição
- **Básicas:** HP% (próprio e de aliados), quantidade de aliados vivos, distância ao alvo, verificação de isolamento ("está sozinho?").
- **De Turno/Tempo:** É o primeiro turno?, turno atual == X, recarga de habilidade disponível.
- **Táticas:** Existe alvo no alcance de ataque?, existe aliado precisando de cura?, alvo possui buff/debuff ativo?
- **AoE (Área de Efeito):** Habilidade atingiria N+ alvos a partir de tile X?, existe um tile que maximiza alvos atingidos?

### 2.3. Nós de Ação
- **Ações Comuns (com Utility Scoring automático):** 
  - *Atacar* (escolhe o alvo com maior score)
  - *Mover* (escolhe o tile com maior score agressivo/defensivo)
  - *Usar Habilidade* (dano, cura, buff, debuff)
  - *Esperar/Pular Turno*
- **Ações Avançadas (Seleção Explícita):** 
  - *Recuar para tile seguro* (busca especificamente zonas fora de ameaça)
  - *Proteger aliado específico* (ex: ficar adjacente ao Boss/Healer)
  - *Focar alvo específico* (prioridade rígida baseada em Tag, Classe ou Role)
  - *Patrulhar área* (movimentação predefinida)

## 3. Próximos Passos (Roadmap de Implementação)
1. **Estrutura de Dados:** Criar as classes base para a Behavior Tree (`AINode`, `AISelector`, `AISequence`, `AICondition`, `AIAction`).
2. **Editor Window:** Desenvolver a janela visual com Unity GraphView API focada nestes nós.
3. **Integração do Blackboard:** Criar um "Blackboard" ou cache de turno para que os nós não precisem rodar funções custosas múltiplas vezes (ex: evitar múltiplos `FindObjectsByType` listados no diagnóstico).
4. **Portabilidade do Utility Scoring:** Mover a lógica de scoring atual do monolítico `AIBrain` para dentro dos novos nós de *Ações Comuns*.
5. **Migração:** Criar templates básicos de Behavior Tree equivalentes aos `BehaviorType` atuais (Agressivo, Defensivo, Suporte) e associá-los aos inimigos existentes.
