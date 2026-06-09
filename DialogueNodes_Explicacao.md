# Sistema de Nodes de Diálogo: Explicação e Redundâncias

Visão geral dos Nodes disponíveis no sistema de diálogos do `Celestial Cross`, suas utilidades e como podem estar causando sobreposição ou redundância entre si.

## 1. Speech Node (Node de Fala)
- **Utilidade:** É o node principal usado para exibir textos de diálogo. Ele permite definir o Nome do Personagem (Persona), o Texto do Diálogo e o Sprite (imagem do personagem).
- **Redundância/Atenção:** Ele possui um botão nativo **"New Choice"**, permitindo que ele mesmo atue como um divisor de caminhos (criando múltiplas saídas dependendo do que o jogador escolher).

## 2. Choice Node (Node de Escolhas)
- **Utilidade:** Serve exclusivamente para exibir escolhas para o jogador e bifurcar o caminho do gráfico. 
- **Redundância/Atenção:** Como o **Speech Node** já é capaz de criar múltiplos conectores de saída usando o botão "New Choice", o **Choice Node** acaba se tornando **totalmente redundante**. Vocês teriam a mesma funcionalidade apenas usando as portas de saída no próprio `Speech Node` que antecede a escolha, ou podem remover a capacidade de múltiplas escolhas do `Speech Node` para tornar o `Choice Node` o padrão centralizado de bifurcações.

## 3. Condition Node (Node de Condição)
- **Utilidade:** Usado para checar uma variável no Blackboard/GameState antes de prosseguir com uma ramificação narrativa.
- **Como funciona:** Você digita o nome de uma Variável (ex: `conheceu_rei`), escolhe o tipo de Condição (`Equals`, `Greater`, etc) e o Valor Alvo. Ele automaticamente gera duas portas de saída: `True` (Verdadeiro) e `False` (Falso).
- **Redundância:** Não há redundância direta. Ele é puramente voltado para checagens de estado (If/Else da história).

## 4. Action Node (Node de Ação)
- **Utilidade:** Modifica o estado do jogo ou das variáveis de história quando o nó é ativado, mas de forma invisível ao usuário (não aparece texto na tela). 
- **Como funciona:** Permite colocar o nome de uma variável e definir um valor para que o sistema salve essa indicação no State do jogo (Ex: setar `ganhou_espada = true`).
- **Redundância:** Não tem redundância. Ele complementa o Condition Node.

## 5. End Node (Node de Fim)
- **Utilidade:** É um nó que simplesmente avisa ao sistema que o diálogo acabou.
- **Como funciona:** Ele não tem portas de saída (`Outputs`), forçando a finalização daquela árvore (Node de folha morta).
- **Redundância:** Em vários sistemas de Grafos, deixar um nó sem Output já é considerado o encerramento da branch. Um `End Node` na UI muitas vezes é adotado só para dar um fechamento visual limpo.

---
### O que foi alterado recentemente no Sistema:
Conforme solicitado, corrigimos o problema de invocação (criação) de Nodes da interface. Antes, todos os Nodes eram empilhados na mesma coordenada inicial (Vector2.zero). Agora:
- Ao dar o **Clique Direito -> Create Node**, o nó será spawnado na exata localização do mouse na tela.
