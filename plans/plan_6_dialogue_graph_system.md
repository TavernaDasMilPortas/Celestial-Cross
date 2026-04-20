# Plano de Implementação: Sistema de Diálogos por Grafo (Visual)

Este plano descreve a transição do sistema de diálogos linear atual para um sistema baseado em nós (grafo), permitindo uma edição visual semelhante ao Shader Graph e lógica de ramificação complexa baseada em variáveis.

## 1. Objetivos
*   **Editor Visual:** Interface para criar, conectar e organizar falas e decisões.
*   **Lógica de Condições:** Suporte a variáveis (`int`, `bool`, `string`) para bloquear ou liberar caminhos de diálogo.
*   **Flexibilidade:** Permitir saltos (loops, retornos) que o sistema linear atual não suporta.
*   **Compatibilidade:** Manter a capacidade de salvar os dados em `ScriptableObjects`.

## 2. Estrutura Proposta

### A. Core de Dados (Backend)
- `DialogueGraph` (ScriptableObject): O contêiner principal que armazena todos os nós e conexões.
- `NodeData`: Base para todos os tipos de blocos:
    - `SpeechNode`: Texto, personagem, animação.
    - `ChoiceNode`: Opções para o jogador.
    - `ConditionNode`: Checa variáveis (ex: `if amizade > 10`).
    - `VariableNode`: Altera variáveis (ex: `set flag_encontro = true`).
- `ConnectionData`: Armazena qual nó está ligado a qual.

### B. Blackboard (Variáveis)
- Sistema para definir variáveis globais de diálogo.
- Integração com o `DialogueFlagManager` existente, expandindo para tipos numéricos.

### C. Editor Visual (Editor)
- Uso da API `UnityEditor.Experimental.GraphView`.
- Janela de edição onde os blocos podem ser arrastados e linkados.
- Mini-mapa e Blackboard visível.

### D. Engine de Execução (Runtime)
- `DialogueGraphInterpreter`: Um novo componente ou módulo para o `DialogueManager` que sabe navegar pelo grafo.
- Adaptador para manter a UI atual (`DialogueUI.cs`) funcionando sem mudanças drásticas.

## 3. Etapas de Desenvolvimento

### Fase 1: Fundação de Dados
1. Criar classes base `DialogueNode` e `NodeConnection`.
2. Implementar `DialogueGraph` como ScriptableObject.
3. Criar o sistema de variáveis (Blackboard).

### Fase 2: Janela do Editor (GraphView)
1. Criar `DialogueGraphWindow` e `DialogueGraphView`.
2. Implementar a criação de nós (Speech, Choice).
3. Implementar o sistema de conexões (Edges).
4. Adicionar funcionalidade de Salvar/Carregar o gráfico no ScriptableObject.

### Fase 3: Lógica Condicional
1. Adicionar nós de condição (Comparação).
2. Adicionar nós de execução (Set Variable).
3. Integrar com o sistema de persistência de flags.

### Fase 4: Integração com o Jogo
1. Criar o `DialogueGraphInterpreter`.
2. Modificar o `DialogueManager` para aceitar um `DialogueGraph`.
3. Testar a transição entre nós e a exibição de escolhas na UI existente.

## 4. Riscos e Mitigações
*   **Complexidade da API de Grafo:** A API `GraphView` é experimental. *Mitigação: Usar padrões consolidados da comunidade Unity.*
*   **Migração de Dados:** Converter diálogos antigos para o novo formato. *Mitigação: Manter o sistema antigo funcionando em paralelo até que o novo esteja estável.*

---
**Próximo Passo:** Definir as classes base de dados para o Grafo (`DialogueNode` e `DialogueGraph`).
