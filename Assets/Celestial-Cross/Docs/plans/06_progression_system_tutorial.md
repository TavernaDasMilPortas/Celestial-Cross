# Tutorial: Expandindo a Progressão (Capítulos e StoryNodes)

O jogo já possui uma base sólida de progressão via **Capítulos**. Este tutorial ensina como usar e expandir esse sistema para criar uma jornada épica.

---

## 1. Conhecendo a Base: StoryNodes

A progressão acontece em **StoryNodes**. Atualmente temos `DialogueStoryNode` e `CombatStoryNode`. 

### Como Adicionar Recompensas (Melhoria)
Para que completar um node signifique progresso real, adicione o campo de recompensas:

```csharp
// Em StoryNode.cs
public RewardPackage completionRewards;

// Exemplo de uso no Inspector:
// Node: "O Resgate do Pet"
// Recompensa: 100 Ouro, 1x Poção
```

---

## 2. Rastreamento Automático

O progresso é salvo na coleção `CompletedNodeIDs` da conta. Garanta que ao final de um diálogo ou combate, o node seja marcado como completo:

```csharp
public static void MarkNodeAsComplete(string nodeID)
{
    var account = AccountManager.Instance.PlayerAccount;
    if (!account.CompletedNodeIDs.Contains(nodeID))
    {
        account.CompletedNodeIDs.Add(nodeID);
        // Mostrar popup de "Novo Capítulo Desbloqueado"
        AccountManager.Instance.SaveAccount();
    }
}
```

---

## 3. Criando Conteúdo Procedural (Missions)

Você pode usar o sistema de Capítulos para criar "Missões Diárias" gerando Capítulos temporários no código:

```csharp
// Exemplo: Gerando um capítulo de Missão Diária
public ChapterData GenerateDailyMission()
{
    ChapterData daily = ScriptableObject.CreateInstance<ChapterData>();
    daily.ChapterTitle = "Missão do Dia: " + DateTime.Now.ToShortDateString();
    
    // Adicione um CombatNode aleatório
    CombatStoryNode combat = new CombatStoryNode {
        NodeID = "daily_" + DateTime.Now.Ticks,
        Title = "Vencer 3 Gosmas",
        // ...
    };
    daily.Nodes.Add(combat);
    
    return daily;
}
```

---

## 4. UI Builder: Gerador de Mapa de Capítulos

Para visualizar o progresso, use o **UI Builder** de Mapa (`Plano 7`). Ele transformará sua lista de botões em um mapa visual:

1. Defina as `MapPosition` (X, Y) em cada StoryNode.
2. Rode o Builder via `Celestial Cross > UI Builders > Generate Chapter Map`.
3. O Builder criará as conexões visuais entre os nodes.

---

## 5. Próximos Passos
- **Side Quests**: Crie um `ChapterCatalog` separado apenas para missões secundárias.
- **Diários de Unidade**: Use o campo `RequiredUnitID` nos capítulos para criar histórias que só abrem se o jogador possuir um personagem específico.
- **Eventos Globais**: Adicione datas de validade (`StartDate`/`EndDate`) aos capítulos para eventos de tempo limitado.

