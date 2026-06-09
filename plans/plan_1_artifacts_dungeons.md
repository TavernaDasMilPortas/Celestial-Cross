# Plano 1: Sistema Estrutural de Masmorras e Economia de Artefatos

## 1. Visão Geral
Este sistema será responsável por distribuir os artefatos (cuja lógica de geração procedural já está pronta) baseando-se no progresso do jogador nas masmorras. Além da distribuição, este plano prevê a governança econômica dos artefatos (upgrades até o nível +15 e sistema de venda/reciclagem) e a expansão do sistema de Reward já existente no jogo para englobar a entrega destes artefatos.

## 2. Modelagem de Dados (ScriptableObjects)
### 2.1. O Catálogo de Masmorras (DungeonLevelSO)
Cada fase/dificuldade de uma masmorra será representada por um ScriptableObject que define as recompensas. A mecânica principal aqui é a **Randomização Conjunta de Raridade e Estrelas**.
*   **AllowedArtifactSets:** Lista de IDs ou referências aos sets de artefatos que podem cair nesta masmorra.
*   **DropChanceMatrix:** Uma estrutura que define a probabilidade de drop. Precisamos de duas rolagens cruzadas:
    *   **Rarity Weights:** (Ex: Comum: 50%, Incomum: 30%, Raro: 15%, Épico: 4%, Lendário: 1%)
    *   **Star Rating Weights:** (Ex: 1 Estrela: 60%, 2 Estrelas: 30%, 3 Estrelas: 10%)
    *   *Nota:* A rolagem conjunta permite que o sistema sorteie a Raridade primeiro (definindo a cor/tiers de status base) e depois cruze com a rolagem de Estrelas (multiplicador/potencial do item).

### 2.2. Integração com o Reward Manager (Artefatos)
O jogo já possui um método de Reward. Ele deverá ser refatorado para suportar a injeção do sistema procedural de artefatos.
*   **Distribuição de Artefatos:** O mesmo método de Reward que lê a vitória na fase será ajustado para injetar os novos Artefatos gerados proceduralmente na conta do jogador.
*   A interface da caixa de recompensa/loot deve conseguir ler a estrutura de atributos do artefato para exibir os status recém-gerados da peça.

## 3. Lógica de Geração em Runtime
Quando o jogador conclui a masmorra (ou abre um baú de loot) via Reward Manager:
1.  **Roll de Tipo:** Sorteia qual Set e qual Peça de artefato será dropada da lista do DungeonLevelSO.
2.  **Roll Duplo (Raridade + Estrelas):** O sistema gera um número para definir a Raridade, e outro para definir as Estrelas, usando os pesos configurados na fase atual.
3.  **Instanciação:** Chama o gerador de artefatos já existente, passando os parâmetros sorteados (Set, Peça, Raridade, Estrelas).
4.  **Atribuição ao Inventário:** O gerador retorna uma instância de atributo dinâmico que recebe um UUID (Unique IDentifier) e é salva no SaveData vinculado ao jogador via Reward Manager.

## 4. Sistema de Progressão: Upgrade (+15)
*   **Lógica de Custos:** Cada nível de aprimoramento (0 a 15) tem um custo em "Dinheiro" da conta. Este custo cresce de forma não-linear, configurável globalmente ou por raridade.
*   **Efeito do Upgrade:** Aumentar os status primários do artefato. Podemos atrelar a revelação de sub-status (caso existentes na sua lógica atual) em marcos como +3, +6, +9, +12, +15.

## 5. Sistema de Economia: Venda e Descarte
Quando um artefato fica defasado, o jogador pode lucrar vendendo-o.
*   **Valor Mínimo (Base Value):** Cada artefato tem um valor em Dinheiro correspondente à sua Raridade e Estrela base.
*   **Retenção de Investimento:** Se o artefato tiver nível superior a 0, o sistema adiciona uma porcentagem do dinheiro total gasto nos upgrades ao valor original.
    *   *Fórmula matemática:* SellPrice = BaseValue + (TotalGoldInvested * 0.45).
    *   O fato do valor de venda ser rigorosamente menor que o custo total incentiva escolhas estratégicas na hora de dar upgrades pesados.

---

## 6. Passos para Implementação (Roadmap Técnico)
1.  **Melhorar Método de Reward:** Ajustar a classe já existente que dá o reward para aceitar a chamada de criação em runtime de Artefatos.
2.  **Criar o DungeonLevelSO:** Desenvolver a classe contendo as matrizes de drop chance de Raridade e de Estrelas.
3.  **Serviço de RNG Conjunto:** Criar a função que lê o DungeonLevelSO e retorna o Set, Tipo, Raridade e Estrelas definitivas do loot.
4.  **Integração ao Gerador Atual:** Chamar o método do gerador que você já tem, alimentando-o com os dados do passo 3, devolvendo ao Reward.
5.  **UUIDs e Persistência:** Implementar GUIDs persistentes para que os artefatos salvos não se sobrescrevam e garantam sincronia no SaveData.
6.  **Interface de Upgrade (+15):** Criar a UI que permite gastar conta-moeda para subir o nível do UID do artefato selecionado (+1 até +15).
7.  **Interface de Venda e Cálculo:** Adicionar função de venda e garantir o repasse fracionado dos upgrades na equação econômica.
