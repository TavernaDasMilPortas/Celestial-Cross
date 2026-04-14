# Plano 3: Arquitetura Avançada do Gacha (Shop & Banners Híbridos)

## 1. Visão Geral
O sistema de Banners é o ápice da economia de alto-nível e agora está encapsulado dentro de uma cena de Loja (ShopScene). A loja adota uma arquitetura baseada em abas e modais (similar e adaptada da arquitetura do Inventário). Inicialmente teremos:
- **Aba de Banners (Gacha):** Onde ocorrem os "pulls" ou invocações. Age como um "hub híbrido" capaz de invocar personagens e também instâncias de Artefatos e Pets em runtime.
- **Aba de Compra/Câmbio:** Onde se pode converter moedas in-game por bilhetes de invocação ou compra direta de sorteios para rolar os banners.

## 2. Modelagem de Dados (ScriptableObjects)
### 2.1. O Banner Configurável (GachaBannerSO)
Banners devem ser perfeitamente moldáveis via inspetor da Unity. O sistema agora prevê **múltiplos personagens da raridade máxima** (Ex: múltiplos 6-Estrelas) em destaque, dividindo a pool.
*   **Economia:** Define a aba de loja onde será exibido, qual a moeda/bilhete cobrada e a quantia por 1 Tiro e 10 Tiros.
*   **Rarity Drop Table Principal:** Percentuais brutos. Qual a chance do puxão ser Raro, Épico, Lendário ou Supremo?
*   **Categorization Pools:** Listas que mesclam Personagens Fixos (agora com sistema de Estrelas), PetSpeciesSO, e ArtifactBaseSO.
    *   *Nota de Peso Interno:* Após sortear a raridade máxima (ex: 6-Estrelas), rola-se um peso secundário dentro desta Pool. Exemplo: se há dois heróis Supremos 6-estrelas focados, as chances são divididas (50%/50%, ou outras distribuições).

### 2.2. Sistema de Estrelas de Personagens
Personagens agora possuem um Sistema de Estrelas (Star Rating).
*   A raridade base determina com quantas estrelas eles começam (4, 5 ou 6 estrelas).
*   Diferente de pets e artefatos (que são gerados proceduralmente), personagens repetidos caem como **Fragmentos de Estrela**. Estes fragmentos servem para realizar o "Awakening" - aumentando o nível máximo de estrela do personagem, elevando muito seus parâmetros base e liberando bônus nas skills.

## 3. O Pity System (Sistema de Garantia)
*   **Soft Pity:** A partir de X tiros de azar (ex: 70), a chance do próximo tiro ser a Raridade Suprema (6-Estrelas) vai aumentando sucessivamente.
*   **Hard Pity / Supreme Pity:** Ao atingir Y tiros (ex: 90), os pesos base são ignorados e a Raridade Suprema é garantida. 
    *   Em Banners com múltiplos prêmios 6-estrelas, caso exista o **Supreme Pity**, perder o "caminho certo" ou o "50/50" em pulls anteriores irá forçar com que o próximo Prêmio Supremo sorteado venha focado no personagem preferencial/escolhido.

## 4. O Fluxo de Execução Híbrida em Runtime
A arquitetura "Dispatcher" que torna a mágica possível sem sobrecarregar classes:
1.  **Dedução de Saldo:** Autorizado na Aba de Câmbio/Banners da ShopScene.
2.  **Sorteio da Raridade (com Pity):** Sorteou. Adiciona +1 no Pity correspondente daquele tipo de banner.
3.  **Sorteio do Prêmio:** Acesso efetuado na Pool Rara do Banner. Sorteou o Herói X.
4.  **Distribuição Assíncrona (O Dispatcher):** O sistema nota o TIPO primitivo:
    *   *Caso Personagem:* Checa o banco; se já tiver, dá Fragmentos. Se não, destrava o Herói no save.
    *   *Caso Artefato/Pet:* Em tempo de execução, aciona as lógicas centrais. Pede pro Gerador buildar, gera o UUID e o resultado vai para o inventário.

## 5. UI/UX: Modais da Aba Shop
A ShopScene será rica visualmente, usando um formato de Modais baseados no sistema atual de inventário.
*   **Aparência do Banner (Modal):** Cada banner terá um modal/janela própria. O modal conta com a **Splash Art do(s) Personagem(ns)** destaque preenchendo a tela.
*   **Visualização de Info e Drops (Preview):** 
    *   Haverá um botão para consultar os Status base, Skills e lore do Personagem de forma detalhada.
    *   O jogador pode visualizar uma lista do restante dos possíveis prêmios que dividem essa roleta (demais personagens, espécies de Pets presentes ou as tags dos tipos de artefatos).
*   **Roll Reveal (Animação):** Após rolar (1x ou 10x), o sistema exibe os resultados em uma sequência ou grid.
    *   Para os heróis, mostra a imagem/fragmentos brilhando de forma destacada.
    *   Para Artefatos/Pets (Drop Procedural), fornecer um botão/gatilho de *inspect stats* na própria tela de fim do gacha para ele verificar na hora as Sub-Status geradas.

---

## 6. Passos para Implementação (Roadmap Técnico)
1.  **Base ShopScene & Modal System:** Construir a base da UI (ShopScene), implementando abas (Câmbio / Loja) e um sistema de modais adaptado do já existente para inventário.
2.  **Sistema de Estrelas (Characters):** Criar a estrutura base de progressão por Estrelas (Star Awakening) para personagens, e seu sistema de fragmentos no save de jogador.
3.  **Banco de Dados BannerSO:** Refatorar a estrutura base contendo DropTables suportando Múltiplos Heróis 6-Estrelas na mesma pool, e mesclando com Fragmentos / Drop Procedural.
4.  **UI/Modal Rico do Banner:** Criar os painéis que inserem e animam a *Splash Art*, e o modal expandido que renderiza o painel de status/skills de Preview do gacha atual.
5.  **Máquina de Sorteios & Supreme Pity:** Escrever a classe de lógica de Gacha. Implementar as curvas de chance do Soft Pity, Hard Pity e as regras para o Pity Supremo focado e dividido.
6.  **Gacha Dispatcher (Pattern Abstract Factory):** Fazer a lógica reativa que entende o tipo sorteado, instanciando pets/artefatos ou criando/despertando heróis no mesmo frame do tiro de gacha, refletindo no pop up e inventário.
