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
- **Data-Driven (Blueprints)**: Unidades, habilidades e passivas são definidas via `ScriptableObject`. As `PassiveAbilityBlueprint` definem o comportamento reativo.
- **Hook Registry**: O `PassiveManager` serve como o hub de eventos reativos para cada unidade.
- **Pipelines de Mutação**: Em vez de passar muitos parâmetros, o sistema usa o `CombatContext` que é passado e mutado por múltiplos sistemas antes da execução final.

## 3. Fluxo de Execução
1.  `TurnManager` inicia o turno de uma unidade. O `PassiveManager` dispara o hook `OnTurnStart` para a unidade.
2.  Um comando (Jogador ou IA) seleciona uma ação.
3.  A ação é enviada para o `AbilityExecutor`.
4.  O executor percorre os efeitos e, para cada um, dispara os hooks relevantes (ex: `OnBeforeApplyCondition`, `OnBeforeTakeDamage`).
5.  O `PassiveManager` intercepta estes hooks e executa as passivas/condições, mutando o `CombatContext`.
6.  O dano é aplicado e se a habilidade termina, o hook `OnAfterAction` é disparado.

## 4. Fluxo de Cenas (Hub → Preparação → Batalha)
O jogo utiliza gerenciadores persistentes (`DontDestroyOnLoad`) para manter dados entre cenas:

- `AccountManager`: dados persistentes do jogador (dinheiro/energia/posses).
- `GameFlowManager`: dados da sessão atual (fase escolhida + formação).

Guia de configuração no Editor (passo-a-passo):

- [Scene_Flow_Setup.md](Scene_Flow_Setup.md)
