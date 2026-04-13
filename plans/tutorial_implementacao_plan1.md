# Tutorial Prático: Configurando e Testando o Sistema de Masmorras e Artefatos (Plano 1)

Este guia foi criado para te ajudar a configurar os assets na Unity e testar todo o sistema de **Dungeons, Drops Procedurais de Artefatos e a nova Tela de Vitória** em qualquer outro computador. Tudo já está programado, você só precisa "plugar" as peças dentro do Editor da Unity.

---

## Passo 1: Criando a sua primeira "Masmorra" (DungeonBaseSO)

Os drops de artefatos agora dependem de qual Masmorra o jogador concluiu.

1. Na aba **Project** da Unity, navegue até a pasta onde você salva seus dados (ex: `Assets/Celestial-Cross/Data/Dungeons`).
2. Clique com o botão direito na pasta e vá em: 
   `Create > RPG > Dungeon > Dungeon Base`
3. Selecione o arquivo criado e olhe a aba **Inspector**:
   - **Dungeon Name / Description:** Dê um nome para sua masmorra.
   - **Allowed Artifact Sets:** Clique no `+` e adicione os Sets de Artefato que podem dropar aqui.
   - **Levels:** Clique no `+` para adicionar os andares da masmorra.
     - **Level Ref:** Arraste o seu `LevelData` (A fase real de batalha).
     - **Artifacts To Drop:** Quantos artefatos o jogador vai ganhar se vencer (ex: 2).
     - **Drop Matrix:** Ajuste as porcentagens matemáticas de cair item Raro, Épico, 1 a 5 estrelas, etc.

## Passo 2: Criando o Catálogo de Masmorras

Para o jogo saber procurar a Masmorra certa quando uma fase for concluída, precisamos de um catálogo central.

1. Ainda na aba **Project**, clique com o botão direito e vá em:
   `Create > RPG > Dungeon > Dungeon Catalog`
2. Nomeie como `MainDungeonCatalog`.
3. Selecione esse catálogo, vá no **Inspector** no campo `Dungeons`, clique no `+` e **arraste a DungoenBaseSO** que você criou no Passo 1 para dentro desta lista.

## Passo 3: Linkando o Catálogo no Controlador do Hub (HubScene)

Quando o jogador for escolher a fase lá no menu principal, o controlador precisa puxar esse catálogo.

1. Abra a sua cena principal (`HubScene` ou similar).
2. Na aba **Hierarchy**, procure pelo GameObject que possui o script **Hub Scene Controller**.
3. No **Inspector** desse script, você verá um novo campo chamado **Dungeon Catalog**.
4. Arraste o arquivo `MainDungeonCatalog` (criado no Passo 2) para dentro deste campo.
5. Salve a cena (`Ctrl + S`).

## Passo 4: Gerando a Tela de Vitória com o UI Builder

Seguindo a nossa nova regra de ouro para UI, todas as telas são geradas visualmente no editor, não por código invisível.

1. Abra a Cena onde a batalha de fato acontece e termina (ou no próprio Hub, se o resultado vier de lá).
2. Na barra de ferramentas superior da Unity, clique em:
   `Tools > UI Builders > Generate Victory Reward UI`
3. A mágica acontece: Um canvas e o GameObject **VictoryRewardUI_Container** aparecerão instantaneamente na sua "Hierarchy".
4. **Opcional (Estilização):**
   - Você pode abrir a hierarquia desse UI, mudar as cores dos painéis, trocar fontes e ajustar o Canvas Group.
   - Existe um objeto chamado `ArtifactItem_PrefabProxy` oculto dentro do `ItemsContent`. Você pode editá-lo para ficar com a cara do seu jogo e transformá-lo num Prefab arrastando para sua pasta de Prefabs.
5. Salve a cena. O script `PhaseManager` vai procurar essa UI automaticamente quando a batalha terminar.

---

## Passo 5: Teste Final (Play!)

1. Dê **Play** no jogo a partir do Menu Inicial.
2. Escolha e entre na fase que você vinculou ao `DungeonBaseSO` lá no Passo 1.
3. Derrote todos os inimigos da fase.
4. Ao invés do jogo voltar para o Hub na hora, a **sua nova Tela de Vitória vai subir** mostrando Moedas, Energia e, o mais legal, **os cards dos artefatos recém-forjados** com status, estrelas e raridades geradas baseadas naquela sua Drop Matrix!
5. Clique no botão de Continuar na UI.
6. Volte no Inventário e veja seus itens prontinhos!