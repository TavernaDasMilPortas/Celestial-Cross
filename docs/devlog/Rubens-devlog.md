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

## Próximos Passos
*   Arrumar anchors da Shop Scene para garantir responsividade em diferentes resoluções.
*   Aprofundar as mecânicas das Inteligências Artificiais usando o novo sistema de Variáveis de Unidade.
*   Verificar em Play Mode as transições visuais e o submodal de detalhes de status durante batalhas reais.

