# UI e Feedback Visual - Celestial Cross

A interface do usuário é projetada para fornecer clareza tática e feedback imediato para as ações do jogador.

## 1. Elementos de Feedback no Mundo
- **HealthBarUI**: Exibida acima de cada unidade. Mostra a vida atual e as condições ativas (buffs/debuffs).
- **DamagePopupManager**: Gerencia a criação de números flutuantes (`FloatingText`) ao redor da unidade quando ela recebe dano ou cura.
- **UnitOutlineController**: Destaca a unidade selecionada ou em foco.

## 2. Sistemas Globais de UI
- **TurnTimelineUI**: Localizada no topo da tela, mostra a ordem de execução dos turnos com portratos das unidades.
- **CombatForecastUI**: Mostra a previsão de dano e acerto antes de confirmar um ataque, permitindo que o jogador tome decisões informadas.
- **UnitPanelUI**: Exibe detalhes da unidade em foco (Stats completos e descrições de Skills).

## 3. Fluxo de Targeting
1.  O sistema de grid destaca os tiles válidos (cor azul para movimento, vermelho para ataque).
2.  Ao clicar em um tile inimigo, a UI de Forecast aparece.
3.  O jogador pode confirmar (ENTER ou Botão) ou cancelar (ESC).

## 4. Extensibilidade
Para adicionar novos popups ou feedbacks, utilize o `DamagePopupManager`. Ele centraliza a instância de efeitos visuais para evitar redundância em cada prefab de unidade.
