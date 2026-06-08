# Devlog Celestial Cross - Rubens

Este log detalha a evolução cronológica do projeto, abrangendo mudanças estruturais, visuais e correções técnicas implementadas recentemente.

---

## 1. Revitalização Visual e Novos Tilesets
**Data: 08/05/2026 - 10/05/2026**
### **Problema:**
A estética inicial de lineart precisava ser substituída por um estilo mais polido e detalhado para elevar a qualidade visual do jogo.

### **Solução:**
*   **Transição de Assets:** Substituição completa dos tilesets antigos por modelos de alta fidelidade.
*   **Suporte a Camadas:** Melhoria no interpretador de mapas para suportar múltiplas camadas de sprites e transparências, permitindo mapas mais ricos.

---

## 2. Interface de Gacha e Loja
**Data: 11/05/2026**
### **Problema:**
O sistema de invocação e a loja precisavam de uma experiência de usuário (UX) mais fluida e atrativa.

### **Solução:**
*   **Gacha Visuals:** Adicionadas animações e materiais especiais para a sequência de invocação de unidades.
*   **Arrumação da UI:** Refatoração completa do layout da loja para facilitar a navegação em dispositivos mobile.

---

## 3. Sistema de Tutoriais e Spotlight Shader
**Data: 12/05/2026**
### **Problema:**
Dificuldade de novos jogadores em entender as mecânicas sem um guia visual direto.

### **Solução:**
*   **Spotlight System:** Desenvolvimento de um shader que destaca botões ou unidades específicas, escurecendo o restante da tela.
*   **Módulos Interativos:** Fluxos de tutorial que bloqueiam inputs externos até que o jogador execute a ação ensinada.

---

## 4. Otimização de Performance: Mesh-based Tiles
**Data: 13/05/2026**
### **Problema:**
Uso excessivo de GameObjects para o grid estava impactando o FPS (Draw Calls altas).

### **Solução:**
*   **Conversão para Mesh:** O grid foi otimizado para renderizar tiles via Mesh instanciada, reduzindo drasticamente o processamento necessário para o cenário.

---

## 5. Implementação da Heroína: Leidell
**Data: 14/05/2026**
### **Problema:**
Necessidade de testar o sistema de habilidades com uma unidade de alta complexidade.

### **Solução:**
*   **Abilities Setup:** Configuração das habilidades da Leidell via grafos, o que serviu como base para identificar gargalos no interpretador e no sistema de buffs.

---

## 6. Refatoração do Sistema de Atributos (Status Dinâmicos)
**Data: 14/05/2026**
### **Problema:**
Buffs e debuffs eram aplicados de forma estática, não atualizando o dano ou a UI em tempo real após a aplicação.

### **Solução:**
*   **Stats Dinâmicos:** A propriedade `Unit.Stats` agora recalcula bônus em tempo real consultando o `PassiveManager`.
*   **Cálculo Transparente:** Implementação de bônus planos e percentuais que se somam corretamente.

---

## 7. Otimização do CombatLogger e Debug Visual
**Data: 14/05/2026**
### **Problema:**
Falta de clareza sobre o estado interno das unidades durante o combate.

### **Solução:**
*   **Status Monitor (Live):** Adicionado monitor de atributos (ATK, DEF, SPD, HP) em tempo real no Inspector do Logger.
*   **Logs Granulares:** Detalhamento de cada etapa de execução dos grafos de habilidade.

---

## 8. Correções no Interpretador de Grafos (Distância e Facção)
**Data: 14/05/2026**
### **Problema:**
Bugs de desserialização faziam com que valores de distância fossem lidos como zero.

### **Solução:**
*   **Sincronização Editor/Runtime:** Alinhamento dos campos de dados entre a interface visual e o código de execução.
*   **Lógica de Facção:** Integração de filtros de Ally/Enemy diretamente no nó de distância.

---

## 9. Robustez na Seleção de Áreas e Habilidades
**Data: 14/05/2026**
### **Problema:**
Áreas de efeito de habilidades anteriores persistiam no grid após a troca de ação.

### **Solução:**
*   **Cleanup Agressivo:** O executor de habilidades agora garante a destruição de qualquer seletor antigo antes de iniciar o próximo.

---

## 10. Projeção de Popups de Dano (Suporte a RenderTexture)
**Data: 14/05/2026**
### **Problema:**
Popups 3D não apareciam corretamente quando o jogo era exibido através de uma RawImage.

### **Solução:**
*   **World-to-UI Projection:** Sistema que projeta o dano do mundo 3D diretamente para o espaço de tela da UI, ajustando escala e posição para garantir nitidez.

---

## 11. Minimalismo e UX na Shop Scene
**Data: 15/05/2026**
### **Problema:**
A interface da loja exibia excesso de texto informativo (ex: "Mapas: X", "Poeira: Y"), poluindo o visual e ocupando espaço desnecessário em telas mobile.

### **Solução:**
*   **Limpeza de UI:** Refatoração das strings de exibição para mostrar apenas os valores numéricos brutos das moedas e do contador de pity, adotando um visual mais limpo e direto.

---

## 12. Implementação de UI: Shop Scene (Assets Marina)
**Data: 15/05/2026**
### **Problema:**
A interface da loja ainda utilizava placeholders e layouts temporários que não condiziam com a direção artística do projeto.

### **Solução:**
*   **Integração de Assets:** Implementação da nova interface visual desenvolvida pela Marina, incluindo splash arts de banners e elementos decorativos, garantindo a fidelidade ao design proposto.

---

## 13. Posicionamento de Unidades, Câmera e Sincronização de Popups
**Data: 19/05/2026 - 20/05/2026**
### **Problemas:**
*   A fase de posicionamento de unidades permitia início automático antes de confirmações e não permitia retratar unidades já colocadas de volta para a mão.
*   A câmera dependia de cálculos de altura e apresentava distorções de estiramento no RenderTexture na inicialização.
*   Popups de dano usavam prefabs com Canvas aninhados duplicados, geravam posições incorretas sob escalas do CanvasScaler e a câmera mudava de foco prematuramente antes dos popups terminarem sua animação.

### **Soluções:**
*   **Fluxo Sequencial de Posicionamento:** Seleção ordenada automática da próxima unidade após confirmação; capacidade de recolher unidades confirmadas do mapa (removendo a confirmação e liberando a unidade de volta para o jogador).
*   **Confirmação Visual (Tiles Verdes):** Destaque verde (`IsConfirmed`) nos tiles de unidades confirmadas que piscam em verde e desaparecem após 0.3s na transição de início de combate.
*   **Zoom Exclusivo por Largura:** Refatoração de zoom baseada inteiramente na largura desejada de tiles no grid (`initialTilesWidthToSee`).
*   **Sincronização de Aspect Ratio:** Inicialização atrasada (`WaitForEndOfFrame`) do gerenciador da RenderTexture para obter as proporções corretas da tela e prevenir estiramento visual.
*   **Popups Puros e Ajuste de Escala:** Remoção do Canvas aninhado nos prefabs de popup. Projeção precisa via `WorldToCanvasWorldPoint` e fallback usando `RectTransformUtility.ScreenPointToWorldPointInRectangle` para a câmera principal. Adição do parâmetro `uiScale` para controle de tamanho customizado.
*   **Sincronização do Ciclo de Vida do Dano:** Alteração do método `Follow()` da câmera e do final das ações/fim de turno no interpretador para aguardar até que todas as animações dos popups ativos terminem antes de mover a câmera ou iniciar um novo turno.

---

## 14. Expansão de Status Base e Resistências
**Data: 23/05/2026**
### **Problema:**
O combate baseava-se muito em dano plano e as condições de controle de grupo/status careciam de mitigação defensiva apropriada.

### **Solução:**
*   **Novas Métricas:** Adicionados `CriticalDamage` e `EffectResistance` aos status globais (`CombatStats`) das Unidades e dos Pets (`PetSpeciesSO` e `RuntimePetData`).
*   **Refatoração do DamageProcessor:** O multiplicador crítico base agora escala perfeitamente e ataques baseados em debuff passam pela verificação de Resistência do defensor antes da aplicação final.

---

## 15. Sistema Dinâmico de Escalonamento (Scaling)
**Data: 23/05/2026**
### **Problema:**
Grafos de habilidade operavam com números inteiros ou flutuantes diretos, limitando builds criativas (ex: "Bate com o valor de defesa" ou "Soma HP Máximo").

### **Solução:**
*   **Escalonamento Multi-Status:** `DamageNodeData` e `HealNodeData` agora utilizam a listagem `StatScalingData`, lendo e processando múltiplos status (ex: 100% de Ataque + 25% de Velocidade) via `AbilityGraphInterpreter` sem hardcoding.
*   **Armazenamento de Variáveis Globais:** Criado o `UnitVariableStore` onde nodos de grafos de Habilidades agora conseguem ler e escrever variáveis transitórias e permanentes.

---

## 16. Fundações da Skill Tree e Modificadores
**Data: 23/05/2026**
### **Problema:**
Inflexibilidade no carregamento de habilidades; o jogador não conseguia modificar o que as ações faziam, deixando a progressão tática limitada.

### **Solução:**
*   **Divisão de Tiers:** Separação do grafo central com uso do `SkillBranchNode` dentro do editor gráfico. A execução das habilidades passou a depender das escolhas dos Tiers feitos pelo usuário no novo sistema de Configuração (`SkillTreeConfigSO` e `SkillBranchTree`).
*   **Persistência em Conta:** As modificações feitas nas ramificações da Skill Tree são mantidas em `UnitLoadout` que integra perfeitamente o sistema de gravação `AccountManager`.

---

## 17. Novos Modais de UI e Builders para Habilidades
**Data: 23/05/2026**
### **Problema:**
A interface não mostrava quais passivas e condições estavam ativas em tempo real e não havia local para interagir com o novo sistema de árvore de habilidades.

### **Solução:**
*   **Componentes Modulares Dinâmicos:** Desenvolvimento do `SkillTabUI`, `SkillSelectionModal`, e `SkillBranchModal` para a tela de Inventário.
*   **Lista de Passivas em Combate:** `PassiveListModal` exibe todos os buffs momentâneos da unidade vinculada (`PassiveManager.GetActiveConditionNames()`).
*   **Integração Automatizada:** Ferramentas de utilidade de Editor criadas na janela superior (`Celestial Cross -> UI Builders -> Skills`) para injetar instantaneamente as Canvas prontas na cena sem perda de referências.

## 18. Otimização do Modal de Passivas de Combate e Ajustes Lógicos das Habilidades de Slot
**Data: 25/05/2026**
### **Problema:**
O modal de passivas possuía um layout confuso de colunas que limitava a legibilidade e usava bordas quadradas pesadas de agrupamento. Além disso, as passivas baseadas em grafo (como a `Full House` de Leidell) sofriam de atrasos na aplicação (corrotinas assíncronas no início da rodada) e eram injetadas de forma incondicional no combate mesmo quando não estavam equipadas nos slots de combate do `Loadout`. Os modificadores numéricos no combate também não exibiam seus ícones de origem adequadamente.

### **Solução:**
*   **Refatoração Visual e Stacked (Design Clean):** Reformulado o `PassiveListModal` e o construtor dinâmico de UI `CombatUISetupUtility` para posicionar as 3 seções verticalmente uma embaixo da outra (empilhadas de largura completa). Removemos as bordas quadradas escuras para dar um visual mais livre e premium. O texto dos bônus agora omite a indicação literal de fonte, exibindo apenas a duração restante.
*   **Submodal de Detalhes e Ícones Integrados:** Criado o `PassiveDetailModal` para exibir detalhes completos de condições temporárias ao clicar em seus ícones. Adicionados componentes de imagem nos prefabs de bônus e passivas, garantindo que o ícone original da habilidade apareça ao lado de suas respectivas informações textuais.
*   **Injeção Síncrona de Grafos de Passivas:** Modificado o `AbilityGraphInterpreter` para disponibilizar interpretações 100% síncronas (`ExecuteGraphSync` e `ProcessNodeSync`). O `PassiveManager` executa passivas instantaneamente no mesmo frame em que os hooks do round/turno iniciam, aplicando os bônus sem atraso de frames.
*   **Injeção Seletiva via Loadout:** Atualizado o `Unit.Initialize()` e o `PassiveManager.GetStaticPassives()` para registrar no `PassiveManager` apenas as passivas ativas nativas (ataque básico/movimentação) e as passivas selecionadas e equipadas nos slots 1 e 2 do `Loadout`. Habilidades passivas que estão na árvore geral mas não foram equipadas não são mais injetadas no combate e nem exibidas no modal.
*   **Preservação de Sprites Dinâmicos:** O interpretador de grafos agora copia o ícone original da passiva/habilidade para os blueprints de modificadores gerados dinamicamente, permitindo que os ícones de origem apareçam corretamente na listagem dos bônus ativos.

---

## 19. Refatoração Master da Behavior Tree (V2)
**Data: 28/05/2026**
### **Problema:**
A visualização e a modularidade da Behavior Tree eram limitadas. Nós compostos escondiam o fluxo em uma única porta invisível "Children", e havia uma proliferação de nós de condição e ação muito específicos e inflexíveis (ex: `ConditionHPPercent`, `ActionAttack`), tornando difícil a reutilização e criação de lógicas complexas. A interface também era inteiramente em inglês, dificultando a leitura de alguns nós.

### **Solução:**
*   **Compostos com Portas Dinâmicas:** `Selector` (OU) e `Sequence` (E) agora mostram explicitamente o fluxo através de portas numeradas (`Passo_0`, `Passo_1`, etc.), adicionadas dinamicamente com o botão "+ Adicionar Passo".
*   **Extração Genérica de Dados:** Implementado o sistema de leitura de dados (`BTGetNumericData`), permitindo extrair HP, Distância, Turnos ou Contagem de aliados vivos no momento exato e de forma independente do controle de fluxo.
*   **Condições e Switches Modulares:** Criados os nós genéricos `BTCheckValue` e `BTValueSwitch` que avaliam os dados recebidos pelas suas portas de entrada. Os nós antigos rígidos foram deletados e substituídos por essa arquitetura limpa de "Dado + Verificação".
*   **Ações Consolidadas:** Diversas ações específicas foram mescladas (ex: `Patrol` virou intent `Wander` no `ActionMove`). O `GetTarget` agora suporta filtragem nativa por `Tag`.
*   **Localização:** Adicionado o `BTLocalizationManager`, permitindo traduzir a UI do editor para português sem quebrar a estrutura de dados.
*   **Presets Atualizados:** O gerador de IAs padrão (`BTAIPresetGenerator`) foi inteiramente reconstruído para usar essa nova arquitetura de visual scripting puro.

---

## 20. Estabilização da Cena de Inventário, Correção de Escala e Tradução de Filtros
**Data: 31/05/2026**
### **Problemas:**
*   A transição da cena Hub para a cena de Inventário resultava em uma tela vazia. A causa era o `Canvas_Inventory` instanciado com escala `(0.29, 0.29, 0.29)` e posição deslocada `(157, 279)` devido a preview settings ativos no Unity Editor durante a execução do script Builder.
*   O `PetReleaseManager` no Canvas causava auto-destruição do Canvas inteiro devido a referências estáticas duplicadas que não limpavam após o Play Mode (Domain Reload desativado).
*   Os dropdowns de filtros de artefatos mostravam strings brutas do enum em inglês, tornando a experiência de usuário confusa.

### **Soluções:**
*   **Inicialização Segura do Canvas:** Atualizados os construtores de criação de Canvas nos scripts de build (`UIBuilder_InventoryScene.cs` e `UIBuilder_UnitScene.cs`) para redefinir e resetar de forma absoluta o `RectTransform` (pos=(0,0), escala=(1,1,1), e âncoras em full stretch).
*   **Ajuste de Singleton:** Modificado `PetReleaseManager` para destruir apenas a si mesmo (`Destroy(this)`) em caso de duplicatas e resetar `Instance = null` no `OnDestroy`.
*   **Tradução Dinâmica dos Dropdowns:** No `ArtifactFilterModal.cs`, adicionado mapeamento dinâmico que puxa os valores de `StatType` e os formata em português amigável (`Vida (Fixo)`, `Ataque (%)`, etc.) nos dropdowns, mapeando de volta para a string do enum bruto no momento de aplicar os filtros no inventário.

## 21. Fluxo de Câmera e Foco da Inteligência Artificial (AI)
**Data: 02/06/2026**
### **Problema:**
*   A IA agia instantaneamente ou sem feedback visual claro, impossibilitando que o jogador acompanhasse qual inimigo estava agindo, qual habilidade usava e qual o seu respectivo alvo.
*   Erros de compilação relacionados à proteção de acesso (`CameraController.targetProjectedPoint`) bloqueavam a compilação do projeto ao tentar referenciar posições de câmera diretamente no script da IA.

### **Solução:**
*   **Fluxo Sequencial de Câmera em 4 Passos:** Implementado no `AIBrain.ExecutePlanRoutine` uma rotina visual para ações de inimigos:
    1. A câmera foca no inimigo conjurador por um tempo determinado.
    2. A área/alcance de ação da IA é exibida. Caso a habilidade possua alcance longo, o zoom da câmera é reduzido automaticamente para mostrar o contexto da ação.
    3. A câmera foca na posição do alvo escolhido (unidade ou tile) e restaura o zoom anterior.
    4. A ação é finalmente executada.
*   **Ajuste de Acessibilidade:** Corrigido o acesso de proteção em `AIBrain.cs` alterando a chamada para a propriedade pública `TargetProjectedPoint` no `CameraController`.
*   **Filtro de Foco por Turno:** Atualizado o `CameraController.SetActionFocus` para ignorar snaps de câmera indesejados durante o turno de IA inimiga.

## 22. Ferramentas de Produtividade UI e Correção de Migração
**Data: 06/06/2026**
### **Problema:**
*   A ferramenta de migração de `Image` para `BetterImage` falhava em manter as cores customizadas porque o pacote BetterUI usava setters internos em C# (`Config.Set`) que a serialização de cópia nativa do Unity ignorava.
*   Ajustar âncoras manualmente para interface responsiva tornava o processo de UI muito lento.
*   Importar novas fontes e criar os Font Assets do TextMeshPro era um processo manual, repetitivo e demorado.
*   O componente de XP no `UnitMainPanel` usava o antigo `Slider` nativo, o que impedia animações modernas de interface (requerendo `Image.Filled`).

### **Solução:**
*   **Refatoração do Migration Tool:** O script `BetterUIMigrationTool` foi reescrito para utilizar atribuição direta das propriedades em C# (`newComp.color = savedColor;`), acionando os callbacks corretos do BetterUI e corrigindo definitivamente a perda de cores.
*   **Auto-Anchor Tool:** Desenvolvido o `RectTransformAnchorTools.cs` com atalhos de teclado (`Ctrl + ]` e `Ctrl + Shift + ]`) para grudar automaticamente as âncoras da UI nos limites visuais da tela.
*   **Batch Font Creator:** Criado o utilitário `BatchFontAssetCreator.cs`, permitindo selecionar dezenas de fontes (TTF/OTF) na aba Project e gerar todos os Font Assets SDF do TextMeshPro de uma só vez (via clique direito).
*   **XP Bar Atualizada:** O construtor `UIBuilder_UnitScene` e o painel principal foram alterados para abandonar o Slider e gerar uma barra de progresso horizontal moderna baseada em `Image (Filled)`.
*   **Sistematização de Máscaras e Sombras:** Esclarecimento e documentação sobre o funcionamento do `Mask` nativo (Stencil) do Unity com o BetterUI, estabelecendo o uso do `MPUIKit` para Drop Shadows suaves via GPU e técnicas de PNG "Baked" para sombras em sprites complexos.

---

## 23. Correção do Sistema de Recompensas e Loot (ProgressionService)
**Data: 08/06/2026**
### **Problemas:**
*   As **LootTables procedurais** estavam gerando os mesmos itens duas vezes após os combates (causado por múltiplos inimigos morrendo no mesmo frame, disparando a rotina de vitória mais de uma vez).
*   Na primeira vitória de uma fase (First Clear), a economia base configurada (Dinheiro e XP) desaparecia do log de recompensas.

### **Soluções:**
*   **Trava de Segurança:** Criada a flag `isPhaseEnded` no `PhaseManager` para garantir que os prêmios só sejam calculados e distribuídos exatamente 1 vez por combate, cortando o loop de mortes múltiplas.
*   **Refatoração do ProgressionService:** Removida a lógica mutuamente exclusiva (`if/else`) entre as recompensas. Agora a economia base (`RepeatRewards`) é **sempre** distribuída. O bônus de primeira vitória (`FirstClearRewards`) entra como uma soma adicional apenas na primeira vez em que a fase é jogada.

---

## 24. BetterUI: Depuração do "White Flash" e Transições
**Data: 08/06/2026**
### **Problema:**
*   Ícones baseados em `BetterImage` piscavam uma caixa branca ao carregar a tela pela primeira vez. Ao tentar contornar via script forçando re-renderização, os ícones dinâmicos passaram a sumir misteriosamente ou serem sobrescritos por placeholders após 0.1 segundos.

### **Solução:**
*   **Ajuste do Fixer:** O `BetterUIFixer` foi refatorado. Removemos os hacks agressivos (`sprite = null` e `enabled = false/true`) que estavam corrompendo a máquina de transições do BetterUI. Agora ele usa apenas chamadas nativas pesadas da GPU (`SetAllDirty()` e `SetMaterialDirty()`).
*   **Componente Plug & Play:** Criado o `BetterUIAutoRefresher`, um componente que pode ser anexado a qualquer Modal problemático. Ao dar `OnEnable`, ele espera 0.2s e re-renderiza apenas as BetterImages filhas daquele modal automaticamente.
*   **Descoberta Arquitetural:** Validado que a máquina de estados (`Transitions`) do BetterUI substitui imagens injetadas via código. A solução para animações dinâmicas é desmarcar a transição de **Sprite** no Inspector, ou preencher o Prefab com uma imagem placeholder (nunca `None`).

---

## 25. Correções Locais (Interface e Gameplay)
**Data: 08/06/2026**
### **Problemas Resolvidos:**
*   **Sincronização de Pets:** Pets equipados em unidades não eram levados para o tabuleiro de combate. (Resolvido).
*   **Trava Fantasma de Energia:** O sistema impedia o início da fase por falta de energia, mesmo quando a UI exibia 50 de energia na conta. (Resolvido).
*   **Modal de Habilidades do Pet:** Finalização e ajustes na interface de visualização de skills dos pets. (Resolvido).

---

## Próximos Passos
*   Aprofundar a arquitetura de features futuras para Pets e Habilidades.
*   Polimento visual e transições utilizando a nova fundação segura do BetterUI.
*   Verificar em Play Mode as transições visuais e o submodal de detalhes de status durante batalhas reais.


