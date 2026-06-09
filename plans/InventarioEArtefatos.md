# Planejamento: Sistema de Inventário e Artefatos

Este documento descreve a arquitetura e o plano de implementação passo a passo para o novo Sistema de Inventário Modular e o Sistema de Artefatos. O inventário permitirá o gerenciamento completo de unidades, pets e equipamentos, enquanto a arquitetura de artefatos introduzirá uma nova camada de profundidade no combate e progressão.

---

## Visão Geral da Arquitetura

### 1. O Sistema de Equipamentos (Artefatos)
Cada unidade (Personagem) terá exatamente **6 Slots de Artefatos** (podendo ou não incluir o Pet separadamente, dependendo do design, mas o padrão será de 6 espaços de equipamentos intercambiáveis).
- **Slots 1 a 6:** Peças de Artefato que compõem atributos e conjuntos.

**Propriedades de um Artefato:**
- **Estrelas (1 a 6):** As estrelas determinam a força dos atributos do artefato. Artefatos de 6 estrelas terão os maiores valores absolutos. Ressalta-se que **o Atributo Padrão (Main Stat) é fixo e estático**, ou seja, seu valor inicial e taxa de crescimento por nível são sempre predefinidos matematicamente (não tem rolagem de randomização).
- **Raridade (5 Níveis):** Comum, Incomum, Raro, Épico, Lendário. A raridade dita **exclusivamente a quantidade de substats** que o item possui (ex: Comum = 0~1 substat, Lendário = 4 substats).
- **Conjuntos (Sets):** Todo artefato pertence a uma "Família" ou "Set" (ex: Set do Gladiador, Set da Vida). Equipar 2, 4 ou 6 peças do mesmo conjunto na mesma unidade concederá **habilidades passivas extras ou bônus de atributos massivos**.
- **Geração Aleatória (RNG):** A criação de artefatos será procedural, rolada no momento do drop ou através de um menu de Geração/Crafting para testes e recompensa.
- **Nível de Aprimoramento (+1 a +15):** Artefatos podem receber upgrades consumindo recursos. Quanto maior o nível, maior será o Atributo Base (crescimento fixo). Além disso, a cada 3 níveis (+3, +6, +9, +12, +15), ocorre um evento de Substat via RNG: se o item tiver menos de 4 substats, ele **ganha um novo** substat aleatório; se já possuir 4, ele **dá um "upgrade" ("roll" numérico RNG) aleatoriamente** em um substat existente.
- **Range Variável (Apenas Substatus):** Enquanto o status principal cresce de forma fixa e padronizada, os Substatus, tanto no momento inicial (criação) quanto nos "saltos" de Upgrade (+3, +6...), recebem valores rolados aleatoriamente dentro de janelas (ranges) que variam de acordo com as Estrelas do artefato (Ex: Substats de Força em um 6* rolam entre 6-10 ao serem criados e aumentam um valor randômico entre +3 e +5 sempre que são sorteados no level up).

### 2. O Sistema de Inventário (UI Modular)
O Inventário será estruturado com um controlador mestre que gerencia diversas **Abas (Tabs)**.
- Implementação atual (Fase 2): reaproveita o componente `InventoryUI` da RestScene como controlador mestre + `InventoryTab` como botões de aba.
- Cada aba lida com **apenas 1 tipo de entidade**.
- Abas planejadas iniciais: **Aba de Unidades**, **Aba de Pets**, **Aba de Artefatos**.
- Interface de inspeção ao clicar numa unidade: Exibe status cumulativos, habilidades resultantes do base + equipamentos, e **6 slots** vazios/preenchidos para clicar e equipar artefatos.

---

## Mapa de Fases de Implementação

### Fase 1: Fundação do Sistema de Artefatos (Foco Atual)
Nesta fase, criaremos a matemática e a lógica orientada a dados para fazer artefatos existirem em código, e faremos as Units os calcularem em batalha. A UI não importa nesta fase, apenas a lógica e persistência.

**Passo 1: Enums e Estruturas Base**
- Criar enum `ArtifactType`: 6 slots de tipos posicionais (ex: Capacete, Peitoral, Luvas, Botas, Colar, Anel).
- Criar enum `ArtifactRarity`: `Common`, `Uncommon`, `Rare`, `Epic`, `Legendary` (define a quantidade de substats).
- Criar enum de `ArtifactStars`: 1 a 6 (define os valores minímo e maxímo dos atributos).
- Criar scriptable object `ArtifactSet`: Contém ID, Nome, e arrays com habilidades/bônus dados ao equipar peças (ex: Bonus 2 peças = Passiva 1, 4 peças = Passiva 2).
- Criar struct `StatModifier`: Identifica qual status (Attack, Defense, Health, etc) e o valor.

**Passo 2: Definição de Dados e Ferramentas (Tuning vs Instâncias)**
- Diferente de `UnitData` que são moldes, artefatos precisam ser gerados com diferentes substats, estrelas e conjuntos.
- Arquitetura atual (Fase 1): a matemática é centralizada em `ArtifactGenerator` (runtime/editor) e pode ser dirigida por um asset de tuning `ArtifactGenerationTuning` em `Resources`.
- Construção de **Ferramentas de Editor (Custom Interfaces no Unity)** para podermos criar e verificar a matemática desses processos inteiramente **Fora de Runtime**, em tempo de projeto.
  - **Menu/Janela de Forja (ArtifactCreator)**: Permite gerar ScriptableObjects de instâncias determinantes ou sorteadas apontando: Slot selecionado, Atributo estático, Raridade, Estrelas, e que produzirá o arquivo do item testável na maleta do projeto com os substats já sorteados pelas regras da janela de estrelas.
  - **Menu/Janela de Upgrade (ArtifactUpgrader)**: Onde você rastreia e insere aquela instância já forjada para upar ela de +1 a +15, ativando as rolagens e verificando se o bônus triggou (no lvl 3,6,9,12,15) o acréscimo de status RNG em um substat antigo ou materializou um novo de acrodo com a falta do 4º substatus.
- Modelo de instância para testes de editor: `ArtifactInstance` (ScriptableObject).
- Modelo de instância para save/runtime (conta): `ArtifactInstanceData` (classe serializável em `Account`).

**Passo 3: Mapeamento de Equipamentos na Conta (`AccountManager`)**
- Atualizar a classe `Account` para salvar:
  - Uma lista de `ArtifactInstanceData` globais do jogador (o inventário de itens dele).
  - Um dicionário/modelo relacional dizendo quem veste o quê: `Dictionary<string, UnitLoadout>`.
  - Criar `UnitLoadout` (Classe): Guarda as _strings_ (GUIDs) das 6 instâncias de artefatos equipados.

**Passo 4: Atualizar Lógica da `Unit` em Combate**
- Atualizar `UnitRuntimeConfigurator` e `Unit.Initialize` para receberem o `UnitLoadout` contendo as requisições de Pets e Artefatos.
- Modificar o Get de `Stats` da `Unit`:
  - `Base Stats` + `Pet Stats` + `Somas e passivas de Set de todos os 6 Artefatos`.
- Modificar `InitializeActions` da `Unit`:
  - Deve incluir habilidades nativas, do Pet e Habilidades de Set de artefato caso a cota do Set esteja atingida.

### Fase 2: O Gerenciador de Inventário Modular (Reestruturação da UI Base)
A tela de Inventário atual da `RestScene` (criada originariamente pela Giulia) será readaptada para um layout de **Tela Dividida (Split-Screen)** ideal para jogos gacha em formato Portrait 16:9:
- **Ambiente Superior (Dinâmico):** Painel de Especificação. O formato e os dados exibidos aqui mudam drasticamente dependendo da Aba selecionada (ex: Aba Unidade mostra o modelo 3D/Foto, status globais e slots de equipamento; Aba Artefato mostra raridade, nível, botão de upgrade e substats).
- **Ambiente Inferior (Grid de Seleção):** Área com *ScrollRect* e *GridLayoutGroup* exibindo as miniaturas das instâncias disponíveis na conta (o que o jogador possui). Clicar num item de grade injeta e desenha seus dados detalhados no Ambiente Superior em destaque.
- **Abas Principais:** As antigas 3 abas serão substituídas por Unidades, Pets e Artefatos. O sistema de deslize de tela (Swipe) mudará qual Grid Inferior está visível e fará a transição do respectivo Painel Superior.

### Fase 3: Aba de Detalhes da Unidade (Tab: Units)
- **Ambiente Inferior (Grid):** Preenchido automaticamente com todos os personagens que o `AccountManager` acusar como desbloqueados.
- **Ambiente Superior:** Renderiza o personagem selecionado no momento.
  - Exibição de Status Final (Base + Artefatos) vs Status Nu.
  - Criação dos 6 botões posicionais de Slots de artefato correspondentes (mostra "+" vazio caso desequipado).
  - Detecção e destaque visual de Conjuntos Ativos (ex: "Bônus Gladiador 4 Peças Ativado!").
  - **Interação de Slot:** Ao clicar em um dos 6 slots do personagem, o *Ambiente Inferior* congela a lista de heróis e passa a exibir uma filtragem do seu inventário de Artefatos contendo apenas itens daquele slot posicional exato (para você visualizar e Equipar).

### Fase 4: Abas de Exervo (Tabs: Pets e Artefatos)
- **Tab de Artefatos:**
  - **Ambiente Inferior:** Um grid contendo a fusão de todos os `ArtifactInstanceData` da conta do jogador.
  - **Ambiente Superior:** Informações do item focado no grid. Mostra Ícone, Slot, Set pertencente, Estrelas, Level, MainStat e os Substats sorteados. No futuro, abrigará ali mesmo os botões reais de "Nivelar +1" gastando ouro (chamando a mesma nossa matemática criada no upgrader).
- **Tab de Pets:**
  - **Ambiente Inferior:** Grid apenas de Pets colecionados.
  - **Ambiente Superior:** Demonstração da Passiva Única/Skill oferecida pelo Pet aos atributos da equipe ou do personagem equipado.

---

## Detalhamento Técnico para Início Rápido (Fase 1)

Para iniciarmos a **Fase 1**, necessitaremos criar os seguintes arquivos em sequência (próxima instrução):

1. `ArtifactEnums.cs`: Conterá `ArtifactType` (os 6 slots), `ArtifactRarity` (dita quantidade de substats), `ArtifactStars` (1..6) e `StatType`.
2. `StatModifier.cs`: Estrutura simples de bônus (`StatType`, `float value`).
3. `ArtifactSet.cs`: ScriptableObject definindo os bônus ganhos quando a Unit usa (ex) 2, 4 ou 6 partes de conjuntos de idêntica família.
4. `ArtifactGenerationTuning.cs`: ScriptableObject (em `Resources`) com faixas por estrela para main/sub e increments; usado automaticamente por `ArtifactGenerator` quando `useTuning=true`.
5. `ArtifactGenerator.cs`: Matemática central de geração (main base, sub roll, increments) com overloads para `ArtifactStars` e clamp para compatibilidade.
6. `ArtifactInstance.cs`: ScriptableObject de instância (para forja/testes no Editor).
7. `ArtifactData.cs`: Modelo serializável de instância para Save/Conta (`ArtifactInstanceData`), incluindo clamp pós-deserialização (compatibilidade com saves legados).
8. **Ferramentas de Editor (Fora do Runtime):**
  - **`ArtifactCreatorWindow.cs`** e **`ArtifactUpgraderWindow.cs`**: Forja/upgrade de `ArtifactInstance` (ScriptableObject) para validar a matemática.
  - **`ArtifactDataForgerWindow.cs`** e **`ArtifactDataUpgraderWindow.cs`**: Alternativa (Option B) para forjar/upgradear diretamente itens do tipo `ArtifactInstanceData` dentro de uma conta (via `AccountManager` ou JSON).
  - **`ArtifactGenerationTuningWindow.cs`**: Janela para criar/editar rapidamente o asset `ArtifactGenerationTuning` em `Resources`.
9. Alterar `Account.cs` para suportar 6 espaços em `UnitLoadout` para acomodar essa estrutura.