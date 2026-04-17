# Plano de Implementação: Animação de Gacha (Estilo Scrapbook / Magical Girl)

## Visão Geral
Criar uma sequência de animação in-scene para o Gacha, temática de "Navegadoras Espaciais/Magical Girl" focada no estilo *Scrapbook* (caderno de colagem). A animação deve reagir dinamicamente a 1 ou 10 invocações, revelar a cor da maior raridade e revelar os prêmios em formato de selos colados, ordenados do pior para o melhor.

## Fase 1: Preparação de Assets e UI (A Página em Branco)
1. **Background UI**: Um painel base (Image) ocupando a tela toda com uma textura de folha de caderno pautado, textura de pergaminho ou diário estelar.
2. **Prefabs**:
   - **Estrela (Sticker)**: Um Prefab de UI simples contendo a imagem de uma estrela branca/neutra (estilo recorte de papel, com borda branca).
   - **Selo de Prêmio**: O `resultItemPrefab` atual repaginado com uma borda que lembre um selo postal ou polaroid colada com fita adesiva (washi tape).
3. **Pacote de Áudio**: Som de virar página, papel amassando/sendo colado, "plins" mágicos, e um som de clímax sonoro para quando a estrela muda de cor.

## Fase 2: O Motor de Animação (`GachaAnimationController`)
Criar um novo script `GachaAnimationController.cs` (anexado ao `ShopSceneUI` ou num Canvas próprio de GachaAnim). Ele receberá a `List<GachaRewardEntry>` resultante do `PerformPulls`.

**Fluxo da Coroutine Principal (`PlayGachaSequence`)**:
1. Ligar o painel do Caderno.
2. **Passo 1: Colagem das Estrelas**:
   - Se for 1 pull: Instancia 1 estrela no centro. Som de *slap* (papel colando).
   - Se for 10 pulls:
     - Instancia 1 estrela (centro). (Delay 0.3s)
     - Instancia 3 estrelas espalhadas. (Delay 0.3s)
     - Instancia 6 estrelas espalhadas.
   - *Como fazer o efeito de colagem*: A estrela nasce com `Scale = 2`, `Alpha = 0` e uma rotação Z aleatória (ex: -15 a 15 graus). Em 0.15s, ela vai para `Scale = 1`, `Alpha = 1` com easing de "OutBack" (para dar aquele soquinho elástico).

## Fase 3: Pulsar e Revelar a Maior Raridade
Ainda dentro da mesma Coroutine, depois que as estrelas estão coladas:
1. **Pulso 1**: Todas as estrelas crescem para 1.2x e voltam para 1x (Delay rápido, som de sino leve).
2. **Pulso 2**: Crescem para 1.2x e voltam para 1x (Delay rápido, som crescendo).
3. **Pulso 3 (O Clímax)**:
   - O algoritmo vasculha a `List<GachaRewardEntry>` recebida e acha a **maior raridade** conquistada.
   - Determina a cor visual (ex: Base = Azul, Supremo = Dourado Brilhante/Arco-Íris).
   - A estrela principal do meio (ou a maior delas) pulsa absurdamente (1.5x) e **pinta-se** dessa cor, talvez disparando um sistema de partículas 2D de glitter / estrelinhas por trás dela. Som de impacto mágico forte.

## Fase 4: A Revelação dos Selos (Prêmios)
Após a revelação da cor:
1. Limpar as estrelas da tela (ou fazer a página "virar" com um Fade/Slide).
2. **Ordenação**: Pegar a `List<GachaRewardEntry>` e ordená-la do valor Mais Baixo (Base) para o Mais Alto (Supreme).
3. **Colando os Selos**:
   - Instanciar um a um os ícones (num grid ou posições pseudo-aleatórias na tela imitando colagem desordenada).
   - Aplicar a mesma animação de colagem de selo: `Scale 2 -> 1`, Rotação Z aleatória, e som de carimbo/papel para cada prêmio.
   - Pausa curta entre os baratos, pausa **longa dramática** antes de colar os ícones de tier épico/supremo, com efeitos de luz por trás.

## Fase 5: Integração com o que já temos (`ShopSceneUI`)
1. No método `DoPull`, no lugar de iterar instantaneamente o `results` no grid de visualização do modal:
   - Oculte o botão de Pull.
   - Chame: `animationController.PlayGachaSequence(results, OnAnimationFinished)`.
2. O método `OnAnimationFinished` reabilita a UI (para que o jogador possa apertar o botão de fechar / continuar na janela final que mostrará o resumo das coisas que tirou, ou aproveitar o Canvas de selos colados como próprio popup de resultado).
