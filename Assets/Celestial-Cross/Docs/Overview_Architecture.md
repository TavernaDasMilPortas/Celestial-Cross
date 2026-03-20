# Arquitetura Geral - Celestial Cross

O **Celestial Cross** é um RPG tático construído de forma modular em Unity. Ele utiliza o padrão de **ScriptableObjects** para definição de dados e componentes desacoplados para lógica de runtime.

## 1. Estrutura de Pastas (Scripts)
- `Camera`: Controladores de foco e zoom (Cinemachine/Custom).
- `Combat`: O coração do jogo, incluindo o **Weaver System** (Passivas e Condições).
- `Grid`: Gerenciamento do mapa de tiles, seleção e lógica de posicionamento.
- `PlayerController`: Ponto de entrada para comandos do jogador.
- `TurnManager`: Orquestrador dos turnos e rounds.
- `UI`: Interface de usuário, HP bars, popups de dano e portratos.
- `Unit`: Base para unidades (Pet, Enemy, UnitData).

## 2. Padrões de Design
- **Data-Driven**: Quase tudo (unidades, habilidades, ações) é definido via `ScriptableObject`. Isso permite criar conteúdo novo sem mexer no código core.
- **Hooks e Eventos**: O jogo utiliza eventos estáticos (ex: `TurnManager.OnTurnStarted`) para que sistemas UI e feedback reajam a mudanças de estado.
- **Efeitos Modulares**: Habilidades são compostas por `IAbilityEffect` reutilizáveis.

## 3. Fluxo de Execução
1.  `TurnManager` decide quem é a próxima unidade.
2.  `PlayerController` (ou IA) seleciona uma ação.
3.  A ação seleciona alvos no `GridMap`.
4.  Ao confirmar, a ação executa seus `Effects`.
5.  O **Weaver System** intercepta esses efeitos e dispara reações (Passivas).
