# Devlog Celestial Cross - Giuliauri

Este log detalha a evolução cronológica do projeto sob a perspectiva de engenharia de UI, sistemas de jogo, transição para a direção de arte e produção de assets visuais.

---

## 1. Estruturação do Menu Principal e Módulo de Inventário
**Data: 21/03/2026**
### **Problema:**
Ausência de uma cena inicial de navegação e de um sistema centralizado para o gerenciamento de itens, abas e missões do jogador.

### **Solução:**
* **Cena MainMenu:** Criação da cena com suporte a Canvas, elementos de menu, controle de carregamento com controle deslizante (slider) e atualização nos metadados de renderização do TextMesh Pro.
* **Módulo de Inventário:** Implementação do `InventoryTab` e `InventoryUI`, estabelecendo uma grade de 3 abas com navegação por deslize através do `SwipeDetector`.
* **Painéis de Suporte:** Adicionados o `MissionsPanel` com animação de deslize e o `RestSceneManager` para controle do dimensionamento do Canvas (Canvas Scaler).

---

## 2. Transição da Cena RestScene e Configurações de Compilação
**Data: 31/03/2026**
### **Problema:**
A cena antiga `SceneHall` precisava ser substituída pelo novo fluxo da `RestScene`, exigindo reconfiguração nas dependências de compilação mobile.

### **Solução:**
* **Migração de Cenas:** Substituição completa da cena antiga pela `RestScene` na estrutura do projeto.
* **Build Android:** Atualização dos arquivos de configuração do Gradle (`mainTemplate.gradle`, `gradleTemplate.properties` e `settingsTemplate.gradle`) e do resolvedor de dependências do Android.

---

## 3. Sistema de Diálogos e Interação com Elementos
**Data: 06/04/2026**
### **Problema:**
Falta de um sistema de narrativa interativa para apresentar falas textuais, nomes de personagens e sprites de forma dinâmica.

### **Solução:**
* **Core do Sistema de Diálogo:** Criação dos scripts `DialogueEntry`, `DialogueSequence`, `DialogueManager` e `DialogueUI` com efeito de digitação (*typewriter*) e evento de encerramento (`OnDialogueEnd`).
* **Componente ChibiInteraction:** Implementação de script para disparar diálogos através de cliques do mouse (exigindo Collider2D) e ocultação automática da interface ao iniciar a cena (`Awake`).
* **Integração na SceneHall:** Inclusão do asset `ChibiTestDialogue` e fiação dos componentes visuais (`SpeakerNameText`, `DialogueText` e indicador de continuação).

---

## 4. Escolhas com Ramificação e Gerenciador de Flags de Diálogo
**Data: 12/04/2026**
### **Problema:**
Os diálogos eram totalmente lineares, sem suporte para escolhas do jogador, respostas condicionais ou salvamento de decisões tomadas durante a conversa.

### **Solução:**
* **Ramificações na UI:** Criação do prefab `ButtonResposta` (configurado em *full-stretch* para melhor comportamento responsivo) e do contêiner dinâmico `ChoicesContainer` com layout vertical automático.
* **DialogueFlagManager:** Implementação de um gerenciador *Singleton* persistido via `PlayerPrefs` para rastrear termos de escolhas (`aceitou_shany`, `recusou_shany`, etc.).
* **Gating de Escolhas:** Atualização da UI para desativar ou exibir em cinza as opções de diálogo cujos requisitos de flags não sejam atendidos pelo jogador.

---

## 5. Mudança de Escopo: Foco em UI e Direção de Arte
**Data: 05/05/2026**
### **Problema:**
O ritmo de entrega do time de arte estava lento, criando gargalos no avanço visual e na identidade estética do jogo.

### **Solução:**
* **Transição de Função:** Migração oficial das tarefas de programação para focar exclusivamente no desenvolvimento de UI e Arte.
* **Pipeline de Assets:** Início da exportação direta do Adobe Illustrator para a pasta de recursos gráficos do Unity e criação do arquivo PSD focado na Splash Art da personagem Leidell.

---

## 6. Evolução dos Tilesets: Linha de Placeholder para Arte Final
**Data: 11/05/2026 - 19/05/2026**
### **Problema:**
O cenário do jogo utilizava assets provisórios em formato lineart (linhas pretas simples), necessitando de texturas completas e variações de arestas para compor os mapas.

### **Solução:**
* **Remoção de Placeholders:** Substituição e deleção dos arquivos antigos de linha de canto (`Lineart_Grama` e `Lineart_Pedra`).
* **Entrega de Assets Finais:** Criação e importação das texturas finais `Grama_Tileset (1 a 5)` e `Pedra_Tileset (1 a 5)`, acompanhadas de variações para as laterais esquerda e direita.
* **Centralização no PSD:** Atualização e organização do arquivo base estruturado `Tilesets.psd` incluindo as novas texturas de madeira e pedra.

---

## 7. Produção de Identidade Visual e Criação de Ícones
**Data: 27/05/2026 - 08/06/2026**
### **Problema:**
Necessidade de novos elementos de interface para diferenciar as abas do inventário e ilustrar as habilidades específicas das personagens inseridas no jogo.

### **Solução:**
* **Splash Art da Leidell:** Substituição e atualização completa do arquivo de ilustração com o asset de alta fidelidade `Splash Leidell.psd`.
* **Identidade de Personagens:** Criação e renderização do pacote de ícones dedicados para as habilidades e exibição das personagens Shanny e Marie.
* **Ícones de Navegação:** Desenvolvimento de novos vetores no Illustrator para as abas do inventário e ajustes gerais de layout.

---

## 8. Polimento Geral de Interface e Ajustes de UI
**Data: 09/06/2026 - 21/06/2026**
### **Problema:**
Pequenas inconsistências visuais, falta de refinamento em telas e necessidade de ícones sob demanda conforme os novos sistemas progrediam.

### **Solução:**
* **Série de Commits de Polimento:** Execução de ajustes contínuos de usabilidade nas interfaces existentes, exportação de novas peças sob medida do Illustrator para os pacotes gráficos e refinamento fino de UI de acordo com as necessidades levantadas no desenvolvimento atual.

---

## Próximos Passos
* Continuar com o fornecimento de ícones sob demanda para os novos sistemas (como o de Pets).
* Avaliar o comportamento responsivo dos novos painéis exportados do Illustrator nas resoluções mobile mais críticas.
