# Guia de Montagem e Fluxo: Telas UI Fases 2, 3 e 4

Este documento detalha o processo de configuração e a lógica por trás da **UI Modular do Inventário e Artefatos**, abrangendo as Fases 2 (Inventário Base), 3 (Slots de Equipamento por Unidade) e 4 (Tela de Seleção e Equipamento de Artefatos).

## Visão Geral da Arquitetura (Split-Screen)

Para evitar reconfigurações massivas na cena (e conflitos de merge de YAML nas scenes), grande parte do layout é agora montado de maneira **dinâmica (em runtime)**. 

O script `InventoryUI` gerencia um layout com a tela dividida:
- **TabsBar (Topo):** Construída com abas que permanecem no ponto mais alto, navegáveis por clique.
- **Top Panel:** Um terço ou quase metade superior da tela reservada para detalhes dinâmicos (Fase 3 vive aqui).
- **Bottom Scroll:** A lista ou grid, encapsulada num `ScrollRect` (para rolagem vertical fluída), permitindo ainda que o evento global do *SwipeDetector* alterne as abas horizontalmente.

---

## Fase 2: O Inventário Modular

### Configuração do GameObject (RestScene)
Para que a interface base funcione:
1. No seu `InventoryPanel` adicione o script `InventoryUI`.
2. Habilite a flag `Auto Build Split Layout` para fatiar e configurar os ScrollRects e Painéis automaticamente no `Start()`.
3. Preencha a lista de **Tabs** vinculando os botões de Abas (`InventoryTab`).
4. Preencha os **Grid Containers** com os `RectTransforms` que manterão as listas dos itens (Unidades / Pets / Artefatos).
5. Certifique-se de referenciar o componente `SwipeDetector`.

### Fluxo de Código
No `Start()`, o script chamará `EnsureSplitLayout()`, que injetará as barras e reparentará os grids sob contentores dimensionados adequadamente com barra de rolagem (ScrollRect e Masks criados on-the-fly). A popularáção das células é orientada pela classe base `Account`, pegando todos os itens em inventário via `AccountManager.Instance.PlayerAccount`.

---

## Fase 3: Detalhes da Unidade & Painel de Equipamentos

Ao selecionar a aba "Unidades" e clicar em uma personagem da lista inferior, o foco é transferido ao Painel Superior (Top Panel). 

### Como é montado e configurado:
Na implementação, o painel do `InventoryKind.Units` não recebe apenas um texto bruto. O código provê:
1. **Detalhes Básicos:** Foto/Nome/Stats.
2. **Grade de Status / 6 Slots:** Uma área construída com `GridLayoutGroup` contendo 6 slots fixos (Helmet, Chestplate, Gloves, Boots, Necklace, Ring).
3. **População Automática do Status:** O framework puxa o `UnitLoadout` da conta para este ID da unidade, permitindo identificar se há algum artefato associado em um slot específico, mostrando seu ícone ou botão genérico vazio.

### Lógica de Clique:
Ao clicar em um desses sub-slots no **Top Panel**:
1. É guardada a intenção em memória no próprio `InventoryUI`: "Estou equipando a unidade **X**, especificamente para o slot **Y**".
2. O sistema salta automaticamente a visão atual para a aba de **Artefatos (Aba 2)**, iniciando a fase 4.

---

## Fase 4: Modo de Seleção de Artefato (Filtro e Equipamento)

Quando o usuário chega na aba de Artefatos vindo de um clique proveniente da tela de seleção de slot da unidade na **Fase 3**, o estado muda para o **Selection Mode**.

### Como ocorre a Filtragem:
1. Ao montar a grade do Inventário de Artefatos, o script checa o estado `IsSelectingArtifact`.
2. Se ativado de forma casada com um `Slot` específico, na hora do loop, o grid **exclui** da fila todo o artefato que for de tipo diferente daquele slot.
3. Isso garante que a rolagem inferior apresentará apenas anéis se o clique anterior buscou trocar o anel, etc.

### Ações Possíveis, Botão Equipar e Cancelar:
No Top Panel da aba de seleção, criam-se componentes condicionais durante a configuração:
- **Botão "Cancelar/Voltar":** Remove a seleção e volta para a aba das "Unidades", restaurando a visão normal.
- Ao clicar num Artefato inferior da lista filtrada, no topo aparecerão os detahes da peça e o botão **"Equipar"**.
- Clicar em "Equipar" envia uma instrução ao `AccountManager`, no `UnitLoadout` adequado, para associar o  `InstanceData.idGUID` do artefato ao Slot, e salvar a ação. No ato a tela limpa o modo seleção e retorna para o painel atualizado da Unidade.

---
## Resumo do Uso do Código (`InventoryUI.cs`)

Caso você queira personalizar prefabs ao invés do layout gerado via código (que é a base principal de sustentação atualmente):
- `EnsureSplitLayout()`: Local das injeções de UI cruas (geração dos botões das Fases 3 e 4).
- `PopulateTab()`: Regras que populam botões iterando na `Account`. Filtragens da Fase 4 encontram-se aqui validando a variavel booleana de estado da seleção de slot.
- `FormatArtifactDetails()` ou o equivalente de UI criado via injeção: Cuida dos displays de Status em tela.

Siga sempre esses parâmetros definindo as ancoragens usando rectTransform `offsetMin` / `offsetMax` se customizar algo em runtime.