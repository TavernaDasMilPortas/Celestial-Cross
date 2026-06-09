# Guia Detalhado: Configuração da Combat UI na Cena

Siga este passo a passo para integrar todos os componentes de Interface de Combate no seu projeto Unity.

## 1. Organização do Canvas
1. **Criar Canvas**: Caso não tenha, crie um `Canvas` (`GameObject -> UI -> Canvas`).
2. **Configuração do Canvas Scaler**:
    - **UI Scale Mode**: `Scale With Screen Size`.
    - **Reference Resolution**: `1080 x 1920` (ou a que preferir, garantindo a proporção vertical/portrait).
    - **Screen Match Mode**: `Match Width or Height` (valor `0.5`).

## 2. Gerenciador de UI de Combate
1. **Criar GameObject**: Crie um objeto vazio chamado `CombatUI_Manager`.
2. **Adicionar Script**: Arraste o script `CombatUIManager.cs` para este objeto.
3. **Atribuições**: Este script servirá como o hub central para os outros componentes abaixo.

---

## 3. Painel de Unidade (UnitPanelUI)
*Exibe o nome, HP e ícone da unidade cujo turno começou.*
1. **Estrutura**: Crie um `Panel` chamado `UnitInfo_Panel`.
2. **Componente**: Adicione o script `UnitPanelUI.cs`.
3. **UI Elements**:
    - Crie textos (TMPro) para `Name`, `HP` e `Speed`.
    - Crie uma `Image` para o ícone (`Pet Icon`).
    - Crie um `Slider` para a barra de vida (`HP Slider`).
4. **Link**: Arraste estas referências para os campos do script `UnitPanelUI`.
5. **Configuração Global**: Arraste este painel para o campo `Unit Panel UI` no `CombatUIManager`.

---

## 4. Barra de Ações (ActionBarUI)
*Contém os botões de habilidades da unidade.*
1. **Estrutura**: Crie um objeto chamado `ActionBar_Container`.
2. **Componente**: Adicione o script `ActionBarUI.cs`.
3. **Layout**: Adicione um `Horizontal Layout Group` ou `Grid Layout Group` para organizar os botões automaticamente.
4. **Configuração Global**: Arraste este container para o campo `Action Bar UI` no `CombatUIManager`.

---

## 5. Prefab do Botão de Ação (ActionButtonUI)
1. **Criar Botão**: Crie um `Button` (UI) simples.
2. **Script**: Adicione `ActionButtonUI.cs`.
3. **Configuração**:
    - Arraste a imagem interna do botão para o campo `Icon Image`.
    - **Novo**: Crie uma imagem de destaque (ex: uma borda brilhante) e arraste para o campo `Selection Image`. Ela será ativada automaticamente quando a habilidade for selecionada.
    - Arraste o próprio componente `Button` para o campo `Button`.
4. **Importante**: Remova ou oculte os textos de "Name" e "Shortcut" que o botão possuía antes (agora usamos apenas ícones).
5. **Salvar como Prefab**: Arraste o botão para a pasta de Assets para criar o Prefab.
6. **Link no Container**: Arraste este Prefab para o campo `Button Prefab` do script `ActionBarUI`.

---

## 6. Modal de Detalhes (ActionModalUI) - **NOVO!**
*Aparece ao segurar um botão de ação.*
1. **Estrutura**: Crie um `Panel` flutuante acima da ActionBar.
2. **Script**: Adicione `ActionModalUI.cs`.
3. **UI Elements**:
    - Adicione textos (TMPro) para `Name`, `Stats` (Dano/Alcance) e `Description`.
    - **Visual Root**: Selecione o objeto que serve de "fundo" para o modal (ele será ativado/desativado automaticamente).
4. **Estado Inicial**: Deixe o modal desativado na hierarquia ou o script o desativará no `Awake`.

---

## 7. Previsão de Dano (CombatForecastUI)
*Mostra o dano estimado, chance de crítico e nome do alvo ao selecionar uma unidade inimiga.*
1. **Estrutura**: Crie um objeto chamado `Forecast_Panel`.
2. **Componente**: Adicione o script `CombatForecastUI.cs`.
3. **UI Elements**:
    - **Panel**: Objeto pai que contém o fundo da previsão (será escondido quando não houver alvo).
    - **Target Name**: Texto (TMPro) para o nome do alvo.
    - **Damage**: Texto (TMPro) para o dano estimado.
    - **Crit**: Texto (TMPro) para a porcentagem de crítico.
    - **Hit Count**: Texto (TMPro) para mostrar "x2", "x3" (opcional).
4. **Funcionamento**: A previsão aparece automaticamente quando você seleciona um alvo válido com uma habilidade de ataque ativa.
5. **Configuração Global**: Arraste este painel para o campo `Combat Forecast UI` no `CombatUIManager`.

---

## 8. Linha do Tempo (TurnTimelineUI)
*Exibe a ordem de quem vai agir no combate.*
1. **Estrutura**: Crie um objeto pai chamado `Timeline_Bar`.
2. **Componente**: Adicione o script `TurnTimelineUI.cs`.
3. **Container**: Crie um objeto interno (ex: `Portrait_Container`) com um `Horizontal Layout Group` para os retratos ficarem alinhados. Arraste-o para o campo `Container` no script.
4. **Prefab de Retrato**:
    - Crie um objeto pequeno com uma imagem quadrada.
    - Adicione o script `TurnPortraitUI.cs`.
    - Configure uma `Icon Image` (ícone do pet) e uma `Background Image` (para cores de time).
    - Salve como Prefab e arraste para o campo `Portrait Prefab` no `TurnTimelineUI`.
5. **Link Global**: Arraste o `Timeline_Bar` para o campo `Turn Timeline UI` no `CombatUIManager`.

---

### Dicas Finais:
- **EventSystem**: Certifique-se de ter um `EventSystem` na cena para que os botões e o "clique e segura" funcionem.
- **ScriptableObjects**: Lembre-se de preencher os campos `Icon` e `Description` nos seus `UnitActionData` para que as informações apareçam no jogo.
- **Z-Order**: Garanta que o `ActionModalUI` e o `Forecast_Panel` estejam à frente dos outros elementos na hierarquia do Canvas para não serem cobertos.
