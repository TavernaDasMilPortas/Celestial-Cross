# Plano 3: Arquitetura Avan�ada do Gacha (Banners H�bridos)

## 1. Vis�o Geral
O sistema de Banners � o �pice da economia de alto-n�vel. Ele n�o apenas consome moedas "premium" ou bilhetes e d� fragmentos de personagens, mas age como um "hub h�brido" capaz de invocar tamb�m inst�ncias de Artefatos e Pets em runtime, com aleatoriedade instant�nea anexada aos pux�es (pulls).

## 2. Modelagem de Dados (ScriptableObjects)
### 2.1. O Banner Configur�vel (GachaBannerSO)
Banners devem ser perfeitamente mold�veis via inspetor da Unity para suportar eventos cronometrados ou tempor�rios semanais.
*   **Economia:** Define a moeda demandada e a quantia por 1 Tiro e 10 Tiros.
*   **Rarity Drop Table Principal:** Percentuais brutos. Qual a chance do pux�o sequer ser um tier �pico ou lend�rio?
*   **Categorization Pools:** Listas que mesclam Personagens Fixos, PetSpeciesSO (Sendo Pets e n�o as inst�ncias), e ArtifactBaseSO (os sets brutos de artefato). 
    *   *Nota de Peso Interno:* Ap�s o sistema rolar (A raridade � Ouro/5 Estrelas), ele rola um peso secund�rio dentro da Pool Dourada (Ex: 50% chance de ser O Personagem Destaque, 25% ser o Pet X, 25% ser o Set de Artefato Y).

## 3. O Pity System (Sistema de Garantia)
Fundamental em mec�nicas Gacha. Uma flag persistente salva na conta que reseta apenas ao se tirar um pr�mio m�ximo (ex: 5-estrelas).
*   **Soft Pity:** Quando o jogador atinge X tiros acumulados de azar (ex: 70), a chance do pr�ximo tiro resultar num Rarity Supremo (Ex: 5 Estrelas) vai subindo substancialmente.
*   **Hard Pity:** Ao bater Y tiros (ex: 90), os pesos base s�o descartados e a raridade suprema � for�ada como recompensa do Gacha, rodando apenas a Pool secund�ria para ver qual dos itens lend�rios caiu.

## 4. O Fluxo de Execu��o H�brida em Runtime
A arquitetura "Dispatcher" que torna a m�gica poss�vel sem sobrecarregar classes:
1.  **Dedu��o de Saldo:** Autorizado.
2.  **Sorteio da Raridade (com Pity):** Sorteou, por exemplo, um 4-Estrelas. Adiciona +1 no Pity.
3.  **Sorteio do Pr�mio:** Acesso efetuado na Pool Rara do Banner. Sorteou um Artefato de Fogo.
4.  **Distribui��o Ass�ncrona (O Dispatcher):** O sistema nota o TIPO primitivo da recompensa.
    *   *Caso Personagem:* Se j� existe na base, vira fragmentos. Se n�o, destrava no save.
    *   *Caso Artefato/Pet:* Em tempo de execu��o, aciona as l�gicas centrais constru�das nos Planos 1 e 2. Pede pro Gerador gerar aquele Artefato/Pet fixando os par�metros com 4-Estrelas, gera o UUID e o resultado serializ�vel vai silenciosamente pro invent�rio.

## 5. UI/UX do Banner 
O momento do "Roll" � sagrado e tem de ser robusto. O resultado n�o pode ir direto pra conta sumariamente.
*   O retorno do Dispatcher deve popular uma lista de Interface Gr�fica ou Array tempor�rio de "Resultados do Tiro".
*   Os resultados devem ser mostrados em Anima��o (Sequencial) pro jogador.
*   Para pr�mios rand�micos (artefatos/pets), fornecer um gatilho de *inspect stats* logo na tela de recompensa do Gacha pra ele confirmar a qualidade do roll.

---

## 6. Passos para Implementa��o (Roadmap T�cnico)
1.  **Banco de Dados BannerSO:** Criar a estrutura base contendo DropTables, Custo e as tr�s tipagens de Loot (Personagem, BaseArtifact, BasePet).
2.  **Persist�ncia Pity:** Adicionar Dicion�rios de contagem no SaveData isolando o Pity Count por Categoria de Banner (Ex: Evento vs Banner Regular).
3.  **M�quina de Sorteios (Gacha Manager):** Escrever a classe est�tica com o m�todo Pull(). Implementar as curvas matem�ticas para o modelo de Soft Pity / Hard Pity.
4.  **Gacha Dispatcher (Pattern Abstract Factory):** Fazer a l�gica reativa que entende o tipo sorteado e conversa com seus devidos sub-geradores de ID �nico e Stats no mesmo frame do tiro.
5.  **Interface de Transi��o:** Montar as Telas Pop-Up 2D, com efeitos e part�culas distingu�veis pela raridade maxima do multi-pull e amarrando na hierarquia de cenas da Unity.
