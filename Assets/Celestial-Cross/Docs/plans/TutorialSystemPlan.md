# Sistema de Tutorial Interativo — Celestial Cross

Sistema modular de tutoriais guiados que escurece a tela inteira exceto o elemento-alvo (botão, tile, unidade), bloqueia interações fora da área iluminada, e conduz o jogador passo-a-passo através de fases pré-scriptadas com resultados premeditados.

## Decisões de Design

- **Overlay na cena de combate:** O sistema utiliza um Canvas de alta prioridade que roda dentro da própria cena de combate, permitindo interagir diretamente com os elementos reais do jogo.
- **ScriptableObjects:** Cada módulo de tutorial é um `TutorialModuleSO`, facilitando a criação e edição via Inspector.
- **Sequestro de Input:** Flag `TutorialManager.IsActive` bloqueia inputs normais do `PlayerController` e avanço automático do `TurnManager`.
- **Módulos Iniciais:** Placement, Action e Movement.
- **Idioma:** Português (BR).
- **IA Desativada:** Inimigos seguem passos pré-scriptados.

---

## Arquitetura do Sistema

- **TutorialModuleSO**: Define passos, mapa/grid e unidades fixas.
- **TutorialStep**: Instrução, alvo de highlight, condição de avanço e resultados mockados.
- **TutorialManager**: Orquestrador singleton.
- **TutorialOverlayUI**: Gerencia o DimPanel, Spotlight, Banners e Setas.
- **Input Blocker**: Restringe cliques apenas ao spotlight.
- **MockCombat**: Intercepta cálculos de dano para resultados premeditados.

---

## Componentes Técnicos

### 1. Dados (ScriptableObjects)
- `TutorialModuleSO.cs`: Raiz do módulo.
- `TutorialStep.cs`: Dados de cada passo.
- `TutorialEnums.cs`: Enums (HighlightTarget, BannerPosition, AdvanceCondition).
- `TutorialUnitSetup.cs`: Unidades fixas iniciais.

### 2. Orquestrador
- `TutorialManager.cs`: Singleton que controla o fluxo e injeta comportamentos.

### 3. UI & Visual
- `TutorialOverlayUI.cs`: Gerencia o Canvas do tutorial.
- `TutorialSpotlight.shader`: Shader URP para o efeito de "buraco" no escuro.
- `TutorialSpotlightMask.cs`: Componente para o shader.

### 4. Lógica & Bloqueio
- `TutorialInputBlocker.cs`: Bloqueia cliques fora do spotlight.
- `TutorialMockCombat.cs`: Força resultados de dano/crítico.

---

## Modificações em Scripts Existentes

- **PlayerController.cs**: Guard clause no `Update` e métodos de força.
- **TurnManager.cs**: Guard no `NextTurn`.
- **ActionButtonUI.cs**: Notifica seleção de ação.
- **PlacementManager.cs**: Notifica posicionamento de unidade.
- **Unit.cs**: Injeção de mock no `CalculateAttack`.

---

## Plano de Verificação

### Testes Automatizados
- Verificar avanço de steps via console.
- Validar bloqueio de input.
- Validar aplicação de dano mockado.

### Verificação Manual
- Validar visual do spotlight e animações de banner.
- Testar comportamento em mobile (Touch).
- Verificar fluxo completo dos 3 módulos iniciais.
