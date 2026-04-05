# Documentação do Sistema de Inventário Atual (RestScene)

Encontrei e analisei o sistema de inventário atual criado pela outra programadora. Ele está localizado na pasta `Assets/Celestial-Cross/Scripts/Giulia_UI/` e foi pensado para uma cena de descanso/hub chamada `RestScene`.

Neste documento, explico como ele funciona atualmente e como podemos usá-lo de base ou adaptá-lo para nossa nova arquitetura de equipamentos.

---

## 1. Visão Geral da Arquitetura Atual

A interface de inventário de Giulia é **puramente visual e voltada para Mobile (Portrait 16:9)**. Ela não possui lógica de persistência de dados conectada, focando apenas no comportamento, gerenciamento de abas, swipes (deslize de dedos na tela) e geração automática de grades de slots.

Os scripts principais são:
1. `RestSceneManager.cs`
2. `InventoryUI.cs`
3. `InventoryTab.cs`
4. `SwipeDetector.cs`

---

## 2. Como os Scripts Funcionam

### `RestSceneManager.cs`
É o cérebro da cena de descanso. Ele possui um padrão Singleton (`Instance`) e faz o controle de fluxo maciço da UI do jogador quando não está batalhando:
- Ele força o Canvas principal para a escala de resolução `1080x1920` (Mobile Portrait).
- Guarda as referências principais para o `MissionsPanel` (Janela de Missões) e o `InventoryPanel` (O próprio inventário).
- Fornece funções públicas para os botões do canvas chamarem: `OpenInventoryPanel()`, `CloseInventoryPanel()` e `ToggleInventoryPanel()`.

### `InventoryUI.cs`
É onde a mágica do inventário acontece de fato. Ele é atrelado ao GameObject do painel do inventário e gerencia o miolo da mochila:
- **As Abas (Tabs)**: Ele requer arrays de `InventoryTab` e arrays de `RectTransform` (conteúdos dos grids). Quando uma aba é clicada, ele esconde todos os grids errados e revela o correto.
- **Auto-Geração de Slots**: Uma coisa legal neste script é que ele dispensa o trabalho braçal de montar os bloquinhos do inventário no Canvas. Através do método `InitializeGrids()`, ele preenche automaticamente 36 slots vazios (uma grade 6x6) dentro de cada aba logo no `Start()`. Ele utiliza o componente `GridLayoutGroup` do próprio Unity para deixá-los alinhados e bonitos.
- **Gestos (Swipes)**: Em dispositivos móveis, ele se conecta com o `SwipeDetector` para trocar da aba de "Poções" para "Armas" apenas deslizando o dedo para a esquerda ou direita.

### `SwipeDetector.cs`
- É uma classe utilitária fantástica que lê os toques contínuos (`Input.touches` ou arrastar do Mouse) na tela. Calcula a distância matemática para ver se o dedo foi movido o suficiente e dispara um Evento (`OnSwipeLeft` ou `OnSwipeRight`) para o `InventoryUI` ouvir e agir.

---

## 3. Limitações e Desafios (Onde precisaremos atuar)

O modelo criado pela Giulia é uma carcaça lindíssima, responsiva e pronta para uso. Entretanto, ela foi elaborada antes da concepção do sistema profundo de Dados (ScriptableObjects e Save files json).

**Os problemas atuais com o que está lá:**
1. **Dados Fantasmas:** Os slots gerados pelo `InventoryUI` no método `InitializeGrids()` geram botões genéricos transparentes, mas esses botões não têm nenhum script conectado a eles capaz de guardar qual item ele representa. É só uma pintura que não faz nada ao ser clicada.
2. **Abas Hardcoded:** As três abas presumidas nos comentários originais são "Poções, Armas, Suprimentos". Precisamos reconfigurá-las para os nossos requisitos: "Unidades", "Pets", "Artefatos".

---

## 4. Plano de Integração (Como unificar as duas visões)

Não há motivo para criarmos um sistema de inventário 2D do zero, podemos tranquilamente enxertar o nosso "Plano de Implementação de Artefatos" (`InventarioEArtefatos.md`) dentro dessa carcaça. 

No lugar de focarmos na arquitetura bruta visual na "Fase 2" (que eu havia estabelecido antes de ler os scripts da sua colega), usaremos o código de interface da Giulia.

**Próximos Passos (Adaptando a Obra da Giulia):**

1. **Criar a classe de lógica do Slot (`InventorySlotUI.cs`):** 
   - Anexaremos esse script no prefab do Slot de Inventário dela.
   - Esse script vai conseguir receber e segurar dados. Exemplo: receber um `UnitData` e desenhar a carinha da unidade nele, ou receber um `ArtifactInstance` e pintar uma bota com qualidade épica roxa em volta.
2. **Conectar os Dados do Save:**
   - Vamos interferir no `InitializeGrids()` da Giulia. Em vez dela sempre criar 36 blocos de cor morta "falsos", faremos o código dela ler o `AccountManager.Instance.PlayerAccount.OwnedUnitIDs` ou o nosso futuro `OwnedArtifacts` e criar **apenas** a quantidade de slots equivalente ao número de pertences que o jogador tem, preenchendo as fotos com o que ele conquistou.
3. **Mecanismos de Interação:**
   - Adicionar uma função *OnClick* nos slots gerados pela `InventoryUI` para que, ao clicar num pet ou numa espada, um Mega Panel (O "Unit Details" que cravamos no plano original) abra lateralmente com a tela de equipar.