# Plano de Arquitetura: Sistema de Habilidades por Nodes (GraphView)

## 1. Visão Geral e Compatibilidade com o Legado
O objetivo deste sistema é criar uma interface visual (Grafo) para o design de habilidades, mantendo 100% de compatibilidade com o executor atual de combate. 
O jogo não executará os Nodes em tempo real. Em vez disso, o Grafo funcionará como uma **ferramenta de autoria**. Quando o designer salvar o Grafo, um compilador lerá os Nodes e **preencherá automaticamente um `AbilityBlueprint`** legado. Dessa forma, o código atual do executor (`AbilityExecutor`, `DamageProcessor`) não sofrerá quebras.

## 2. Janela do Editor e Base da Interface Visual (GraphView)
Para viabilizar este sistema, as seguintes etapas de base no Unity Editor serão criadas:

*   **AbilityGraphWindow**: Uma janela customizada (`EditorWindow`) que abrigará o canvas do grafo. Ela será acessada através de um novo botão nos `AbilityBlueprints` ou pelo menu do Unity.
*   **AbilityGraphView**: Uma classe herdando de `GraphView` (Unity Editor Experimental) que servirá como o canvas. Ela gerenciará a grade de fundo, o zoom, o pan e as regras de conexão (quem pode conectar em quem).
*   **BaseNode**: Uma classe de onde todos os nodes derivarão. Ela lidará com a UI padrão (título, cor do cabeçalho) e terá métodos para renderizar portas de Entrada (Input) e Saída (Output).
*   **GraphSaveUtility / Compiler**: Um script responsável por varrer o grafo a partir do `Start Node`, caminhar pelas portas Flow e traduzir os dados em instâncias de `EffectStep`, `TargetingStrategyData` e `EffectData`, injetando isso no ScriptableObject do `AbilityBlueprint`.

---

## 3. Estrutura Detalhada dos Nodes

### 🟨 Nodes de Fluxo e Inicialização

**1. Node Start**
*   **Função**: O ponto de partida do grafo e das configurações base da habilidade.
*   **Portas**: 1 Output (Flow)
*   **Campos**:
    *   **Dropdown de Tipo**: Passiva / Ativa / Condição
    *   *Dinâmico:* Se "Condição" for selecionado, os seguintes campos extras aparecem:
        *   `Duration in Turns` (Int)
        *   Ao compilar, o sistema setará `isPersistentCondition = true`.

**2. Node de Trigger (Gatilho)**
*   **Função**: Seta o momento/evento em que os próximos efeitos conectados ocorrerão.
*   **Portas**: 1 Input (Flow), 1 Output (Flow)
*   **Campos**:
    *   **Dropdown de Hook**:
        *   OnManualCast, OnRoundStart, OnRoundEnd, OnTurnStart, OnTurnEnd, OnBeforeAction, OnAfterAction, OnBeforeTakeDamage, OnAfterTakeDamage, OnBeforeDealDamage, OnAfterDealDamage, OnBeforeTakeHeal, OnAfterTakeHeal, OnBeforeDealHeal, OnAfterDealHeal, OnBeforeApplyCondition, OnAfterApplyCondition, OnBeforeRemoveCondition, OnAfterRemoveCondition, OnDeath, OnKill, OnMoveStart, OnMoveEnd.

**3. Conditional Node (Ramificação Múltipla)**
*   **Função**: Avalia diferentes condições consecutivas e direciona o fluxo para a primeira que for verdadeira (um switch/If-Else).
*   **Portas**: 
    *   1 Input principal (Flow).
    *   Múltiplos Inputs (Data) para plugar os nós do tipo *Condition Effect Data*.
    *   Múltiplos Outputs (Flow) (True) baseados na quantidade de condições.
    *   1 Output genérico de (False / Else).
*   **Campos**: Botão "+" para adicionar novas ramificações de condição na UI do Node.

---

### 🟦 Nodes de Contexto

**4. Node de Target (Alvo)**
*   **Função**: Define quem receberá os próximos efeitos da habilidade.
*   **Portas**: 1 Input (Flow), 1 Output (Flow).
*   **Campos e Dropdowns Dinâmicos**:
    *   **Checkbox**: `Reuse Previous Targets` (Se ativado, esconde todos os campos abaixo e usa os alvos do Step anterior).
    *   **Dropdown de Tipo Principal**: Manual Target / Auto Target Strategy
    *   *Se Manual Target:*
        *   **Dropdown de Modo**: Single / Área
        *   *Se Área:* 
            *   Object Field para o SO `AreaPattern`.
            *   Dropdown `Preferred Direction`.
            *   Checkbox `Auto Rotate Área`.
        *   **Dropdown de Origem do Target**: Point (Qualquer tile) / Unit (Apenas unidades).
    *   *Se Auto Target Strategy:*
        *   **Dropdown com as possibilidades**: Closest Unit / Farthest Unit / Lowest Attribute / Highest Attribute / Self / Main Target / Random Target.
        *   *Se Closest/Farthest Unit:* Dropdown (Closest ou Farthest).
        *   *Se Lowest/Highest Attribute:* Dropdown (Lowest ou Highest) e Dropdown de Atributo (HP, Ataque, Defesa, Speed).
        *   *Se Random Target:* Dropdown de Facção (Ally ou Enemy) e Campo Numérico para `Target Number`.

---

### 🟥 Nodes de Efeito (EffectData)

*(Todos os nós de efeito abaixo contêm 1 porta Input Flow, 1 porta Output Flow, e 1 porta Input de Data para plugar condições opcionais que barram o efeito individualmente).*

**5. Damage Effect Node**
*   **Função**: Causa dano a um alvo.
*   **Campos**:
    *   **Dropdown de Tipo**: Flat / Percentage.
    *   **Amount** (Int).
    *   *Se Percentage:* Dropdown de Atributo Base (HP, Attack).
    *   **Checkbox**: `Scale With Distance`
        *   *Se Ativado:* Aparece campo numérico `Distance Scale Factor` (Float).

**6. Heal Effect Node**
*   **Função**: Cura um alvo.
*   **Campos**:
    *   **Dropdown de Tipo**: Flat / Percentage.
    *   **Amount** (Int).
    *   *Se Percentage:* Dropdown de Atributo Base (HP, MaxHP, Attack).
    *   **Checkbox**: `Can Crit Heal`.
    *   **Checkbox**: `Allow Overheal`.

**7. Apply Condition Effect Node**
*   **Função**: Aplica um modificador persistente ou estado negativo/positivo (tipo envenenamento) no alvo. *(Substituirá a obsoleta ApplyConditionEffectData e usará ApplyModifierEffectData)*.
*   **Campos**:
    *   **Object Field**: Blueprint da Condição/Modificador (`AbilityBlueprint`).
    *   **Dropdown**: Apply Type (Add, Remove, Reset Duration).

**8. Move Effect Node**
*   **Função**: Move uma unidade.
*   **Campos**:
    *   **Checkbox**: `Move Target` (True = Move o alvo; False = Move quem usou a habilidade).
    *   **Range** (Int).
    *   **Checkbox**: `Manual Destination`.
    *   **Checkbox**: `Allow Occupied Tiles`.

**9. Stat Modifier Effect Node**
*   **Função**: Adiciona ou subtrai atributos brutos enquanto a habilidade ou passiva estiver ativa. Deve escalar com a distância.
*   **Campos**:
    *   Campos numéricos para `Attack`, `Defense`, `CritChance`, etc.
    *   **Checkbox**: `Scale With Distance`
        *   *Se Ativado:* Aparece campo numérico `Distance Scale Factor` (Float).

**10. Cost / Countdown Node**
*   **Função**: Subtrai recursos para que a habilidade ocorra ou injeta um tempo de recarga. *(Exigirá adição de novo código de EffectData na base do projeto).*
*   **Campos**:
    *   **Dropdown de Recurso**: Mana / Action Points / HP Cost / Cooldown Turns.
    *   **Amount** (Int).
    *   **Dropdown Timing**: Pay On Cast / Pay On Hit.

---

### 🟩 Nodes de Dados (Condições)

*(Estes nodes não passam "fluxo", mas sim "dados". Possuem apenas Output de Data, que são valores de verdadeiro ou falso).*

**11. Condition Effect Data Node (Avaliador de Condições)**
*   **Função**: Cria regras para verificar se um fluxo ou efeito deve ocorrer. É plugado no Input do *Conditional Node* ou nos efeitos.
*   **Campos Dinâmicos**:
    *   **Dropdown de Tipo de Condição**: Attribute Check / Has Status / Distance Check.
    *   *Se Attribute Check:*
        *   Dropdown: Target / Source.
        *   Dropdown Atributo: HP, Ataque, etc.
        *   Dropdown Operador: >, <, ==, >=, <=.
        *   Campo Valor (Int).
    *   *Se Has Status:*
        *   Dropdown: Target / Source.
        *   Object Field do Status (AbilityBlueprint).
    *   *Se Distance Check:*
        *   Dropdown Operador: >, <, ==.
        *   Campo Valor (Int).
