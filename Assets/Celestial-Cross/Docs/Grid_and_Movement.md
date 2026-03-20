# Sistema de Grid e Movimentação - Celestial Cross

O jogo utiliza um sistema de grid 2D para gerenciar o posicionamento de unidades e o cálculo de áreas de efeito.

## 1. Componentes Core
- **GridMap**: O gerenciador central do grid. Ele mantém as referências de todos os `GridTile`. Ele é responsável pelo spawn inicial do grid e pela busca de tiles por coordenadas.
- **GridTile**: Representa uma célula individual do grid. Ele contém informações sobre:
    - Ocupação (tem uma unidade?).
    - Feedback Visual (cor de destaque).
    - Tipo de Terreno (via `TileDefinition`).
- **AreaResolver**: Responsável por converter padrões (`AreaPatternData`) em listas de tiles afetados a partir de um centro.

## 2. Movimentação de Unidades
A movimentação é tratada como uma `MoveAction` (que herda de `UnitActionBase`).
- **Range**: Definido via `MovementStats` na unidade.
- **Pathfinding**: O `GridMap` fornece utilitários para calcular caminhos válidos, considerando tiles ocupados ou intransponíveis.
- **Hooks**: O sistema de combate dispara `OnMoveStart` e `OnMoveEnd` no pipeline para permitir reações (ex: "Aura que causa dano ao se mover").

## 3. Seleção de Alvos
- Durante uma ação, o jogador clica em tiles.
- O sistema valida se o tile está dentro do range da ação.
- Em caso de habilidades de área, o `AreaResolver` destaca os tiles vizinhos de acordo com o padrão configurado no asset.
