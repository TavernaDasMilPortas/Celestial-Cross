# Plano 1 & 2: Sistema de Capítulos e Progressão de História/Masmorras

Este plano detalha a criação de um fluxo de jogo onde o progresso é travado por etapas e os capítulos alternam entre diálogos e combates.

## 1. Estrutura de Dados (ScriptableObjects)
### ChapterData.cs
- `string ChapterID`: Identificador único.
- `string ChapterTitle`: Nome do capítulo.
- `List<StoryNode> Nodes`: Lista ordenada de eventos.
- `ChapterData RequiredChapter`: Capítulo anterior necessário para desbloquear este.
- `string RequiredUnitID`: (Opcional) ID da unidade que o jogador deve possuir para este capítulo aparecer (Fluxo de Diário).
- `bool IsDiaryChapter`: Se verdadeiro, este capítulo será exibido no menu de Diários/Afinidade em vez do modo História principal.

### StoryNode.cs (Abstract/SerializeReference)
- `string NodeID`: ID único para persistência.
- `string Title`: Título para exibir no botão do menu.
- `bool IsAutoPlay`: Se termina um e já tenta abrir o próximo.
- **Subclasses:**
    - `DialogueNode`: Referência a um `DialogueGraph` ou ID de conversa.
    - `CombatNode`: Referência a um `LevelData` ou `DungeonBaseSO`.

## 2. Sistema de Trava (Gating)
- **Account Progress:** No `Account.cs`, adicionar `HashSet<string> CompletedNodeIDs`.
- **Dungeon Lock:** Adicionar `string RequiredNodeID` ao `DungeonBaseSO`. O menu de seleção de masmorras só habilita o botão se o ID estiver no `HashSet` da conta.
- **Visual:** Botões trancados devem mostrar um ícone de cadeado e o nome do pré-requisito.

## 3. Fluxo de Diário (Affinity/Diary)
- **Visibility Logic:** Capítulos marcados como `IsDiaryChapter = true` só são instanciados no menu de Diário se `RequiredUnitID` estiver na lista `OwnedUnitIDs` da conta.
- **Story vs Diary:** 
    - O `ChapterManager` filtrará os capítulos por tipo.
    - O menu de Diário pode ser um sub-menu do Hub que destaca a história pessoal dos Pets/Personagens.

## 4. UI e Fluxo de Jogo
- **Chapter Menu:** UI dinâmica que lê o `ChapterData` e gera botões para cada `StoryNode`.
- **End Level Modal:** 
    - Ao vencer um combate ou terminar um diálogo, exibir modal com: "Recompensas", "Voltar ao Menu" e "**Próximo Passo**".
    - O botão "Próximo Passo" só aparece se houver um `StoryNode` subsequente no capítulo atual.

## 4. Modo de Teste Local (Debug)
- Implementar um `ProgressionCheater`: Um menu de desenvolvedor para marcar todos os `NodeIDs` como completos instantaneamente para testes de UI.
