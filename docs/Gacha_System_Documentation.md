# Documentação do Sistema de Gacha (Celestial Cross)

O sistema de Gacha em Celestial Cross é responsável por gerenciar sorteios de Personagens (Units), Pets e Artefatos. Ele foi projetado para suportar banners customizáveis, um sistema robusto de *Pity* (garantias), escolhas de foco (*Epitomized Path* / 50-50) e suporte tanto para lógica local quanto em nuvem.

---

## 1. Arquitetura Geral

O sistema é construído ao redor das seguintes premissas:
* **GachaService**: O "cérebro" do sistema. Um Singleton (`GachaService.Instance`) que processa os sorteios, consome as moedas (`StarMaps`), calcula probabilidades e injeta as recompensas na conta do jogador.
* **Provedores (Providers)**: Através da interface `IGachaProvider`, o sistema pode executar sorteios de forma local (`LocalGachaProvider`) ou se comunicar com um backend (`CloudGachaProvider`).
* **ScriptableObjects (SOs)**: Os dados e configurações de como o banner deve se comportar, quais itens ele contém e quais as chances de drop residem no `GachaBannerSO`.
* **GachaPityState**: Um estado salvo por jogador (persistido no `Account`) para registrar quantos tiros já foram dados, se o último drop raro perdeu o 50/50 e qual é o item em destaque escolhido.

---

## 2. Scriptable Objects de Dependência

Os componentes visuais e de balanceamento dependem grandemente da injeção via Scriptable Objects do Unity.

### 2.1. GachaBannerSO
Este é o principal arquivo de configuração de um banner. Ele controla:

* **Configurações Básicas**:
  * `BannerID` e `BannerName`: Identificadores do banner.
  * `BannerSplashArt` e `Silhouette`: Artes utilizadas na UI do banner e nas animações de revelação do sistema.
  * `CostPerPull`: Quanto custa 1 tiro em *Mapas das Estrelas* (StarMaps).
* **Configuração Visual**:
  * `pullVisualConfig`: Referência para um `BannerPullVisualConfigSO` que dita como as 10 estrelas do pull múltiplo devem aparecer na tela.
* **Sistema de Garantia (Pity)**:
  * `SoftPityThreshold`: (Ex: 70) Número de tiros antes da probabilidade base do prêmio máximo (*Supreme*) começar a aumentar agressivamente.
  * `HardPityThreshold`: (Ex: 90) Tiro exato em que a raridade *Supreme* tem 100% de chance.
  * `GuaranteedAboveBaseEvery`: (Ex: 10) A cada quantos tiros o jogador é garantido de não tirar a pior raridade da lista (garante um *Uncommon* ou superior a cada 10 tiros).
* **Epitomized Path (Foco em Supremos)**:
  * `HasEpitomizedPath`: Um booleano. Se verdadeiro, permite ao jogador escolher um foco principal (um 50/50 para supremos focados).
  * `SupremeChoices`: Lista de `GachaRewardEntry` em destaque.
* **Tabelas de Drop (Pool e Probabilidades)**:
  * `BasicProbabilities`: Lista de `GachaRarityProbability` mapeando as chances base de cada raridade. (ex: Supreme - 0.6%, Base - 90%).
  * `TotalPool`: Todos os itens (`GachaRewardEntry`) disponíveis para sorteio dentro deste banner.

### 2.2. BannerPullVisualConfigSO
Usado puramente para organizar a animação de um *10-pull*.
* `pullPositions`: Uma lista de 10 coordenadas (`Vector2`) ditando onde cada estrela do sorteio de 10 deve aparecer fisicamente na UI.
* `connectionIndices`: Índices para desenhar as linhas de conexão entre os pontos da constelação (efeito puramente decorativo).

### 2.3. Outros SOs Referenciados
Dependendo do tipo de recompensa listada na *Total Pool* do banner, ele também referenciará:
* `UnitData` (Não SO, mas dados de personagem se for Unit).
* `PetSpeciesSO`: ScriptableObject com definições base caso a recompensa seja um Pet.
* `ArtifactSet`: ScriptableObject que define os buffs e o ID de conjunto caso a recompensa seja um Artefato.

---

## 3. Mecânicas do Sorteio (Gacha Mechanics)

Toda a lógica de probabilidade acontece no método `ExecutePullsInternal` da classe `GachaService`. O fluxo para cada "tiro" é o seguinte:

### A. Validação de Moeda
A conta do jogador (`Account.StarMaps`) é verificada para garantir que ele pode pagar pelos tiros (`banner.CostPerPull * times`).

### B. Cálculo de Raridade (DetermineRarity)
1. **Hard Pity**: Verifica-se se o `PullsSinceLastSupreme >= HardPityThreshold`. Se sim, a raridade `Supreme` é forçada.
2. **Garantia de 10 (AboveBase)**: Se `PullsSinceLastOverBase >= GuaranteedAboveBaseEvery`, exclui-se a raridade mais baixa das probabilidades possíveis, forçando o tiro a ser de maior raridade.
3. **Soft Pity**: Se os tiros sem *Supreme* passaram do `SoftPityThreshold`, adiciona-se *4.5%* de chance de *Supreme* para cada tiro extra acima do limite.
4. O sistema gera um número randômico e faz o sorteio ajustado (rolagem roleta com pesos cumulativos).

### C. Seleção de Item (SelectRewardFromPool)
Com a raridade definida, busca-se na lista `TotalPool` todos os prêmios dessa mesma raridade. Se a raridade sorteada for `Supreme` e não existirem itens na `TotalPool` principal (ou por design o banner separa isso), o sistema olhará dentro da `SupremeChoices`.

### D. A Regra do 50/50 e Epitomized Path
Se o banner possui foco ativo (`HasEpitomizedPath`):
* Ao tirar uma raridade `Supreme`, o sistema verifica se ela é exatamente a que o jogador escolheu focar (`SelectedSupremeChoice`).
* Se não for o personagem focado (um *Rate-Off* do pool geral), o estado de pena é alterado para `Lost5050 = true`.
* **Hard Guarantee**: Na próxima vez que o jogador rolar qualquer recompensa `Supreme`, se `Lost5050` estiver marcado como *true*, o `GachaService` forçará que a recompensa sorteada seja o `SelectedSupremeChoice`. (Resetando o estado para *false* em seguida).

### E. Distribuição das Recompensas (DispatchReward)
Por fim, o item é entregue no inventário (`Account`):
* **Units (Personagens)**: Adiciona à `OwnedUnits`. Se for uma unidade duplicada, o sistema **não** entrega um novo personagem. Em vez disso, converte automaticamente em 20 *Fragmentos* e adiciona 1 *Insígnia Estelar* (item valioso do jogo).
* **Pets**: Adiciona um `RuntimePetData` em `OwnedRuntimePets` gerando os stats de forma instanciada.
* **Artefatos**: Executa lógicas do `ArtifactGenerator` para randomizar Sub-Status (Main stat fixado em HealthFlat no momento ou randomizado depois) e injetar o Artefato com a raridade do Gacha no `OwnedArtifacts`.

---

## 4. Estruturas Auxiliares Importantes

* **GachaRewardEntry**: Classe customizada e serializada que exibe abas dinâmicas no painel do Unity (Sirenix Odin Inspector). Baseado no `RewardType` (Unit, Pet, Artifact), ele oculta/mostra referências aos SOs correspondentes para preenchimento. Possui um `Weight` (Peso) para casos em que certos itens na mesma raridade são mais fáceis ou difíceis de obter do que os outros.
* **GachaPityState**: A classe serializada na conta do jogador. Memoriza `BannerID`, `PullsSinceLastSupreme`, `PullsSinceLastOverBase`, `Lost5050` e `SelectedSupremeChoice`. Fundamental para persistir o *Pity* entre as sessões de jogo.
