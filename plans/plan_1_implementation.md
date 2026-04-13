# Implementação do Plano 1: Masmorras e Economia de Artefatos

## Relatório de Tarefas Restantes

Este documento detalha as pendências exatas para finalizar e integrar completamente a economia procedural de artefatos com o fluxo atual do jogo.

### Parte 1: Ajustes na Modelagem (Backend)
- **Tarefa 1: Vincular o LevelData à Masmorra (DungeonBaseSO / DungeonLevelNode)**
  - O `PhaseManager` (ou `GameFlowManager`) precisa saber em qual *Dungeon* estamos para poder ler a `ArtifactDropMatrix` correspondente. Existem duas rotas: 
    1. Adicionar uma referência `DungeonBaseSO` + `DungeonLevelNode` ao `LevelData`.
    2. Criar um `DungeonCatalog` que consiga mapear qual LevelData pertence a qual DungeonBaseSO. Vamos pela rota mais simples / direta (adicionando campos no LevelData, se fizer sentido para a sua arquitetura, ou criando um catálogo).

### Parte 2: Geração Dinâmica no Fluxo da Batalha
- **Tarefa 2: Integração de Drop de Artefatos no `PhaseManager`**
  - Ao executar `GrantRewards()`, o `PhaseManager` deverá chamar o `ArtifactLootService.GenerateLoot(...)`.
  - Pegar os artefatos devolvidos pela geração procedural e incluí-los na `RuntimeReward.GeneratedArtifacts`.
  - Com isso, a chamada que já existe para o `AccountManager` salvará automaticamente esses itens no inventário.

### Parte 3: UI Pós-Batalha (A Parte de "Gerar UI" como antes)
- **Tarefa 3: Tela de Vitória e Exibição de Loot**
  - Mudar o comportamento atual de "ir para o Hub instantaneamente" após `GrantRewards()`.
  - Ao invés de um log no console, queremos abrir um Prafab/Painel (`VictoryRewardUI`) na Scene de Batalha (ou Hub) que exiba para o jogador:
    - Quanto de **Dinheiro** e **Energia** ele ganhou.
    - Componentes Visuais (Cards/Ícones) listando os Artefatos procedurais gerados (mostrando a Raridade, Slot, e Estrelas).
  - Adicionar um Botão "Voltar ao Hub" nessa UI.

### Parte 4: Refinos na Economia 
- **Tarefa 4: Ajuste de Desequipamento Segura**
  - Na função de venda (`ArtifactEconomyService.TrySellArtifact`), atualmente o artefato apenas sai da lista `Account.OwnedArtifacts`. 
  - É crucial iterar sobre todos os `UnitLoadout` da conta para limpar o `idGUID` do artefato caso ele estivesse vestido em algum Pet ou Unidade, prevenindo bugs onde o sistema procura um idGUID inexistente.