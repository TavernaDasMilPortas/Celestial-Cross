# Plano 7: Melhoria do Sistema de Progressão (Capítulos e StoryNodes)

## Visão Geral
O sistema atual de Capítulos é funcional mas básico. Vamos transformá-lo em uma engine de progressão completa com recompensas, rastreamento automático e uma interface de mapa visual.

---

## Proposed Changes

### Componente 1: Data Model (StoryNode & Chapter)

#### [MODIFY] [StoryNode.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Game Flow/StoryNode.cs)
- Adicionar `public RewardPackage rewards;` à classe base `StoryNode`.
- Adicionar evento `public static Action<string> OnNodeCompleted;`.
- Em `CombatStoryNode.Execute()`, garantir que ao vencer o combate (via `TurnManager`), o ID do node seja salvo.
- Adicionar `public Vector2 MapPosition;` para suportar interfaces de mapa.

#### [MODIFY] [ChapterData.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Game Flow/ChapterData.cs)
- Adicionar `public Sprite ChapterBanner;`.
- Melhorar `IsLocked()` para ser mais robusto (ex: checar se todos os nodes obrigatórios do capítulo anterior estão na lista).

---

### Componente 2: Sistema de Recompensas

#### [MODIFY] [AccountManager.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Account/AccountManager.cs)
- Adicionar método `CompleteStoryNode(string nodeID, RewardPackage rewards)`.
- Este método adiciona o ID à lista `CompletedNodeIDs` e aplica as recompensas à conta.

---

### Componente 3: UI & UX

#### [NEW] [ChapterMapView.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Game Flow/UI/ChapterMapView.cs)
Nova interface que:
- Instancia os nodes nas suas `MapPosition`.
- Desenha linhas (UGL ou simples Transforms) entre nodes conectados.
- Mostra um indicador de "Próximo Objetivo".

#### [NEW] [UIBuilder_ChapterMap.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Game Flow/Editor/UIBuilder_ChapterMap.cs)
Builder que:
- Cria o Canvas do Mapa.
- Automatiza a criação da grid/fundo do capítulo.
- Vincula os `StoryNodes` do SO aos GameObjects de UI.

---

## Verificação
- [ ] Completar um `DialogueStoryNode` → Recompensas aparecem no inventário.
- [ ] O próximo node no mapa muda de "Bloqueado" para "Disponível" visualmente.
- [ ] Tentar entrar em um capítulo trancado → Feedback visual claro.
- [ ] Builder gera o mapa corretamente a partir do SO.
