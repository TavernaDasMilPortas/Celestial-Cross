# Planejamento: Ciclo de Encerramento de Fase e Limpeza de Unidades

Este documento descreve a implementação do sistema que detecta o fim de um combate/fase e gerencia a limpeza das unidades derrotadas do campo e da lógica do jogo.

## 🎯 Objetivos
- Implementar um sistema de morte que remova unidades da grid e do sistema de turnos.
- Detectar vitória ou derrota quando uma facção inteira for eliminada.
- Criar o fluxo de transição pós-combate.

---

## 📅 Roadmap de Implementação

### Fase 1: Sistema de Morte e Limpeza Técnica
*Implementar a lógica que garante que uma unidade morta pare de existir para os sistemas de jogo.*

- [ ] **Task 1.1: Evento de Morte em `Health.cs`**
    - Adicionar `public event Action<Unit> OnUnitDied;`
    - Modificar `Die()` para passar a referência da unidade no evento.
- [ ] **Task 1.2: Remoção do TurnManager**
    - Fazer o `TurnManager` ouvir `OnUnitDied`.
    - Criar lógica para remover a unidade da `Queue<Unit>` de turnos.
- [ ] **Task 1.3: Limpeza da Grid**
    - Criar método `ClearTile(Vector2Int position)` em `GridMap.cs` ou `GridTile.cs`.
    - Definir `IsOccupied = false` e `OccupyingUnit = null`.
- [ ] **Task 1.4: Remoção Física/Visual**
    - Implementar a destruição do GameObject ou desativação pós-morte.

### Fase 2: Gestão de Facções e Vitória/Derrota
*Controlar quem ainda está vivo e quando a fase deve acabar.*

- [ ] **Task 2.1: Implementar `BattleManager.cs`**
    - Criar Singleton `BattleManager`.
    - Manter listas: `List<Unit> playerUnits` e `List<Unit> enemyUnits`.
- [ ] **Task 2.2: Monitoramento de Estado**
    - Atualizar contagem sempre que `OnUnitDied` for disparado.
    - Checar: `if (enemyUnits.Count == 0)` -> Vitória.
    - Checar: `if (playerUnits.Count == 0)` -> Derrota.
- [ ] **Task 2.3: Gatilho de Encerramento**
    - Criar método `EndPhase(Faction winner)`.
    - Parar todas as corrotinas de combate (TurnManager, AIBrain).

### Fase 3: Fluxo de Interface e Transição
*O que o jogador vê quando a luta acaba.*

- [ ] **Task 3.1: UI de Resultado**
    - Criar tela simples de "Victory" e "Defeat".
    - Exibir estatísticas básicas (opcional).
- [ ] **Task 3.2: Botão de Continuidade**
    - Configurar ações de transição: Próxima Fase, Menu Principal ou Loot.

---

## 🛠️ Arquivos Envolvidos
- `Assets/Celestial-Cross/Scripts/Unit/HealthSystem/Health.cs`
- `Assets/Celestial-Cross/Scripts/TurnManager/TurnManager.cs`
- `Assets/Celestial-Cross/Scripts/Grid/GridMap.cs`
- `Assets/Celestial-Cross/Scripts/Combat/BattleManager.cs` (A ser criado)

---

## ⚠️ Considerações Técnicas
- **Corrotinas:** Garantir que o `TurnManager` pare imediatamente para evitar que uma unidade morta tente realizar uma ação pendente.
- **GUIDs de Meta:** Atenção para não quebrar referências ao mover ou criar novos arquivos (sempre usar a Unity ou o Agent para manipulação).
- **Overheal:** O sistema de vitória deve contar unidades vivas, ignorando se possuem vida extra (Overheal).
