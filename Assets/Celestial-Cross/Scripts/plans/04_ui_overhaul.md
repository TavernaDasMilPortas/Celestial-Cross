# Plano 4: UI Overhaul — Inventário e Victory Screen via UI Builders

## Visão Geral
Atualizar as UIs de Inventário e Tela de Vitória para acomodar os novos sistemas (Nível, XP, Constelações, Insígnias). **Toda UI nova será construída via UIBuilders** (scripts de Editor com `MenuItem`), garantindo que o designer possa trocar a aparência depois.

## Padrão UI Builder
Cada `UIBuilder_*` é uma classe estática com `[MenuItem("Celestial Cross/UI Builders/...")]` que:
1. Encontra o `Canvas` ativo
2. Cria `GameObjects` com `RectTransform`, `Image`, `TMP_Text`, `Button` etc.
3. **Auto-vincula** referências serialzadas no MonoBehaviour alvo via `SerializedObject`
4. Registra undo para o Unity Editor

---

## Proposed Changes

### Builder 1: Painel de XP na Victory Screen

#### [NEW] [UIBuilder_VictoryXPPanel.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Giulia_UI/Editor/UIBuilder_VictoryXPPanel.cs)

Gera no Canvas (dentro do rootContainer do `VictoryRewardUI`):
- **Container horizontal** com 4 slots (1 por unidade no time)
- Cada slot contém:
  - `Image` — ícone da unidade
  - `Image` (fill) — barra de XP  
  - `TMP_Text` — "Lv. X" (ou "Lv. X → Y!" se leveled up)
  - `TMP_Text` — "+123 XP"
- Auto-vincula ao `VictoryRewardUI`:
  - `xpSlotsPanel` (Transform)
  - `xpSlotPrefab` (GameObject)

---

### Builder 2: Painel de Detalhes de Unidade no Inventário

#### [NEW] [UIBuilder_UnitDetailPanel.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Giulia_UI/Editor/UIBuilder_UnitDetailPanel.cs)

Gera o **Top Panel** da aba Unidades com:

**Seção 1: Identidade**
- Ícone grande da unidade
- Nome + Classe/Role badges
- "Lv. {level}" texto

**Seção 2: Stats + XP**
- Barra de XP com texto `{currentXP}/{xpToNext}`
- Grid de stats: HP | ATK | DEF | SPD | CRIT | ACC

**Seção 3: Constelação**
- 6 ícones de estrela (bright se desbloqueado, dim se não)
- Texto "Constelação: C{n}"
- Botão "Ascender" (disabled se sem insígnias)
- Texto "Insígnias: {count}"

**Seção 4: Habilidades** (já existente, apenas rearranjado)

Auto-vincula ao `InventoryUI`:
- `unitLevelText`, `unitXPBar`, `unitXPText`
- `constellationStars[]`, `constellationButton`, `insigniaCountText`

---

### Builder 3: Cards de Unidade no Grid

#### [NEW] [UIBuilder_UnitGridCard.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Giulia_UI/Editor/UIBuilder_UnitGridCard.cs)

Gera um **prefab** de card para a grid do inventário:
- Ícone da unidade
- Badge de nível no canto (Lv.X)
- Estrelas de constelação (mini, na base do card)
- Borda com cor por raridade/elemento

---

### Builder 4: Atualização da Victory Screen Completa

#### [MODIFY] [UIBuilder_VictoryModalComplete.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Giulia_UI/Editor/UIBuilder_VictoryModalComplete.cs)
Refatorar para que, além do modal de detalhes, ao rodar  o builder:
- Gere o panel de XP (chamando `UIBuilder_VictoryXPPanel`)
- Adicione seção de "Insígnias Ganhas" se houver duplicatas

---

### Runtime: VictoryRewardUI

#### [MODIFY] [VictoryRewardUI.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Giulia_UI/VictoryRewardUI.cs)
Adicionar campos:
```csharp
[Header("XP Panel")]
[SerializeField] private Transform xpSlotsPanel;
[SerializeField] private GameObject xpSlotPrefab;
```

Adicionar método:
```csharp
private IEnumerator AnimateXPBars(Dictionary<string, XPGainResult> results)
{
    // Para cada slot: 
    //   Preencher ícone e textos
    //   Animar fillAmount de oldXP% → newXP% over 1 segundo
    //   Se subiu de nível: flash + texto "LEVEL UP!"
}
```

---

### Runtime: InventoryUI

#### [MODIFY] [InventoryUI.cs](file:///d:/Arquivos/Documentos/GitHub/Bichinhos-Magicos/Assets/Celestial-Cross/Scripts/Giulia_UI/InventoryUI.cs)
Adicionar campos de referência que os UIBuilders vinculam:
```csharp
[Header("Leveling UI")]
public TMP_Text unitLevelText;
public Image unitXPBar;
public TMP_Text unitXPText;

[Header("Constellation UI")]
public Image[] constellationStars = new Image[6];
public Button constellationButton;
public TMP_Text insigniaCountText;
```

No método que popula detalhes de unidade selecionada, preencher esses campos.

---

## Verificação
- [ ] Rodar cada UIBuilder → elementos criados no Canvas
- [ ] Referências auto-vinculadas nos MonoBehaviours
- [ ] Barra de XP anima corretamente na Victory Screen
- [ ] Constellation panel mostra estrelas + botão funciona
- [ ] Cards no grid mostram nível e constelação
