# Módulo de Personagem (Garota Mágica + Pet)

## Objetivo
Representar um personagem jogável como a composição entre:
- **Humano (garota mágica)**
- **Pet equipado**

Essa abordagem garante que **qualquer pet possa ser equipado em qualquer humano**.

## Estrutura de dados

### `UnitData`
Contém dados base do humano:
- Nome
- `baseStats` (vida, ataque, defesa, velocidade, crítico, acerto de efeito)
- Habilidade do humano (`characterAbility`)
- Pet padrão opcional (`defaultPet`)

### `PetData`
Contém dados do pet:
- Nome
- `baseStats`
- Habilidade do pet (`ability`)

### `CombatStats`
Estrutura comum para status de combate:
- `health`
- `attack`
- `defense`
- `speed`
- `criticalChance`
- `effectAccuracy`

A soma de status é feita por operador (`human + pet`).

## Regra de composição

Status finais em batalha:

`statusFinal = statusBaseHumano + statusBasePet`

Exemplo:
- Humano: vida 30, ataque 10, defesa 6, velocidade 7, crítico 7, acerto efeito 1
- Pet: vida 5, ataque 2, defesa 0, velocidade 1, crítico 10, acerto efeito 0
- Final: vida 35, ataque 12, defesa 6, velocidade 8, crítico 17, acerto efeito 1

## Cálculo de dano

A resolução foi concentrada em `DamageModel`.

### Fórmula base
`danoBruto = ataque + bônusDeDano - defesa`

`danoMitigado = danoBruto - reduçãoDeDano`

Se crítico:
`danoFinal = danoMitigado * 2`

Caso não crítico:
`danoFinal = danoMitigado`

Sempre respeita mínimo de 1 de dano por acerto.

### Velocidade e múltiplos ataques

A regra implementada segue o requisito:
- Se `velocidadeAtacante >= velocidadeAlvo + 10`, atacante acerta **2 vezes**.
- Caso contrário, **1 vez**.

## Habilidades

Foi criado `AbilityData` para descrever habilidades de humano e pet de forma desacoplada.

Exemplos de habilidade que cabem nessa estrutura:
- "Ao iniciar combate, +10 de ataque"
- "Personagens com este pet recebem 5% de redução de dano"

## Efeitos ativos

Foi adicionado `ActiveCombatEffect` para permitir buffs/debuffs no personagem durante combate.

Cada efeito pode contribuir com:
- `DamageBonus` persistente
- `DamageReduction` persistente
- bônus/redução ativados em gatilho de início de combate
- redução extra ao receber ataque

No fluxo de ataque, os efeitos são aplicados automaticamente em `Unit.CalculateAttack`.
Adicionar um sistema de efeitos ativos (buff/debuff) por turno para aplicar:
- `DamageBonus`
- `DamageReduction`
- gatilhos de início de combate
- gatilhos ao receber ataque

---

## Como estruturar uma Unit na Unity para teste rápido

Este fluxo cria um cenário mínimo para validar a junção de **Character + Pet** em uma Unit.

### 1) Criar os ScriptableObjects

No Project, crie:

1. `Create > Units > Ability Data`
   - Ex.: `AB_Garota_Fogo`
2. `Create > Units > Ability Data`
   - Ex.: `AB_Pet_Lobo`
3. `Create > Units > Pet Data`
   - Ex.: `PET_Lobo`
   - Preencha `displayName`, `baseStats` e `ability` (apontando para `AB_Pet_Lobo`)
4. `Create > Units > Unit Data`
   - Ex.: `UNIT_Garota_A`
   - Preencha `displayName`, `baseStats`, `characterAbility` e `defaultPet` (apontando para `PET_Lobo`)

> Dica: para testar variações rapidamente, duplique o `PetData` e mude apenas os `baseStats`.

### 2) Preparar o prefab da Unit no cenário

Crie um `GameObject` (ex.: `TestUnit_A`) e adicione:

- Um componente que herde de `Unit` (ex.: `PlayerUnit`, `EnemyUnit` ou equivalente do projeto)
- `Health`
- `Collider` (Box/Sphere/Capsule)
- `UnitHoverDetector`
- `UnitOutlineController`

No componente da Unit:

- Arraste `UNIT_Garota_A` para o campo **Unit Data**
- (Opcional) Defina **Equipped Pet** para sobrescrever o `defaultPet` em runtime

Essa configuração é importante porque `Unit` possui `[RequireComponent]` e depende desses componentes no `Awake`.

### 3) Criar dois alvos para validar dano e velocidade

Para testar combate de forma clara:

- `TestUnit_A` com foco em velocidade alta
- `TestUnit_B` com velocidade menor e defesa maior

Use combinações diferentes de pet para cada um. Assim você verifica:

- Soma de status (`GetCombinedStats`)
- Ataque duplo por velocidade (`GetAttacksAgainst`)
- Dano final (`CalculateAttack` + `DamageModel`)

### 4) Checklist de validação em Play Mode

Ao dar Play, confira no Inspector/Logs:

1. **Vida máxima** bate com `Stats.health` combinado
2. **Speed** muda ao trocar pet
3. Unit rápida realiza **2 ataques** quando diferença de velocidade for `>= 10`
4. Sem pet equipado, Unit usa `defaultPet` do `UnitData`

### 5) Cenário mínimo recomendado para testes repetíveis

Padronize uma cena de teste com:

- 2 prefabs de Unit fixos
- 2 a 3 `PetData` de referência (ofensivo, tanque, veloz)
- 1 botão/atalho para trocar pet em runtime (quando o sistema de UI estiver pronto)

Com isso, você consegue validar rapidamente regressões sempre que alterar fórmula de dano, status ou regras de turno.


---

## Roadmap: reestruturar Habilidades (ativas/passivas) com foco em ativas estilo Action

Objetivo desta fase: transformar habilidades ativas em um fluxo equivalente ao de `actions`, com suporte a **multi-target** e **área por matriz**, mantendo criação total via Inspector + drawers/custom editors.

### Visão de arquitetura (alvo)

- `AbilityData` passa a ter tipo: **Passive** ou **Active**.
- Habilidade ativa referencia um `ActionDefinition` (ou estrutura equivalente) para execução em runtime.
- Targeting e área viram blocos configuráveis por dados:
  - `TargetingRuleData` (single, multi, self, aliados/inimigos, limites)
  - `AreaPatternData` (matriz 2D que define footprint da área, com pivô/origem)
- Runtime separa 3 partes:
  1. Seleção de origem/alvo(s)
  2. Resolução da área (tiles atingidos)
  3. Aplicação dos efeitos

---

### Task 1 — Modelar domínio de Habilidades (base limpa)

**Meta**: separar claramente passivas e ativas no modelo de dados.

1. Evoluir `AbilityData` para conter:
   - `abilityType` (`Passive` / `Active`)
   - metadados comuns (`id`, nome, descrição, ícone, custo/cooldown futuro)
2. Criar dados específicos:
   - `PassiveAbilityData` (gatilhos/efeitos passivos)
   - `ActiveAbilityData` (referência de targeting + área + efeitos)
3. Garantir compatibilidade com `UnitData.characterAbility` e `PetData.ability`.

**Critério de aceite**: no Inspector, consigo criar habilidade passiva e ativa sem editar código.

---

### Task 2 — Unificar “Action” e “Active Ability” por definição de execução

**Meta**: habilidade ativa funcionar "próxima às actions" sem duplicar lógica.

1. Introduzir um contrato de definição executável (ex.: `IActionDefinitionData`).
2. Fazer `UnitActionData` e `ActiveAbilityData` compartilharem pipeline de runtime.
3. Extrair parâmetros comuns de execução:
   - alcance base
   - regras de seleção
   - regras de confirmação/cancelamento

**Critério de aceite**: uma habilidade ativa entra no mesmo estado de fluxo (`SelectingTargets -> ReadyToConfirm -> Resolving`) usado por action.

---

### Task 3 — Targeting avançado (single/multi com restrições)

**Meta**: expandir além de 1 alvo.

1. Criar `TargetingRuleData` com:
   - `mode`: `Single`, `Multiple`, `AreaFromPoint`, `AreaFromTarget`
   - `maxTargets`, `minTargets`
   - filtros: self, ally, enemy, dead/alive
2. Adaptar `TargetSelector` para respeitar `TargetingRuleData`.
3. Suportar seleção incremental com feedback visual por estado (válido/inválido/selecionado).

**Critério de aceite**: configurar no asset uma habilidade de 3 alvos e selecionar exatamente até o limite.

---

### Task 4 — Área por matriz com ScriptableObject

**Meta**: habilitar skills tipo "bola de fogo" com footprint configurável.

1. Criar `AreaPatternData : ScriptableObject` contendo:
   - dimensão (`width`, `height`)
   - pivô (`originX`, `originY`)
   - matriz booleana (células ativas)
   - flags opcionais (rotacionável/simétrica)
2. Criar utilitário `AreaResolver`:
   - recebe célula de origem + rotação/direção
   - retorna lista de células afetadas
3. Integrar com grid atual para converter célula -> units atingidas.

**Critério de aceite**: consigo criar padrões (linha, cruz, cone, explosão) só no Inspector e ver impacto correto no mapa.

---

### Task 5 — Catálogo de efeitos desacoplados

**Meta**: skill/action com composição de efeitos (não hardcode por tipo).

1. Criar `EffectData` base (dano, cura, buff, debuff, deslocamento etc.).
2. `ActiveAbilityData`/`UnitActionData` passam a ter lista de `EffectData`.
3. Runtime executa cadeia de efeitos por alvo afetado.

**Critério de aceite**: mesma habilidade aplica dano + burn (ou buff + shield) sem classe runtime nova.

---

### Task 6 — Drawers e Editors (foco em produtividade de designer)

**Meta**: melhorar criação no Inspector com máximo de detalhe.

1. `CustomEditor` para `AbilityData` com UI condicional por tipo.
2. `PropertyDrawer` para:
   - `TargetingRuleData`
   - `AreaPatternData` (preview em grid clicável)
   - listas de `EffectData` com botão de adicionar por tipo
3. Melhorar `UnitDataEditor` para associar:
   - actions
   - habilidades ativas/passivas do personagem/pet
4. Validadores inline (warnings/erros no inspector).

**Critério de aceite**: criar habilidade completa no Inspector sem editar JSON/código manual.

---

### Task 7 — Runtime de preview e UX de seleção

**Meta**: previsibilidade para o jogador antes de confirmar.

1. Ao mirar alvo/origem, desenhar preview da área (`AreaPatternData`) no grid.
2. Destacar unidades afetadas e estimativa de efeito (dano previsto).
3. Confirmar/cancelar com ciclo consistente para action e active ability.

**Critério de aceite**: feedback visual mostra exatamente quem será afetado antes do Enter.

---

### Task 8 — Migração dos assets atuais

**Meta**: não quebrar conteúdo existente.

1. Script de migração de `AbilityData` antigo para novo formato.
2. Fallback para manter leitura de campos legados por uma versão.
3. Atualizar assets de exemplo (`UnitData`, pets, ações de teste).

**Critério de aceite**: projeto abre sem referência quebrada e cenas antigas continuam jogáveis.

---

### Task 9 — Testes e validação

**Meta**: garantir estabilidade.

1. Testes de domínio (edit mode):
   - soma de stats
   - resolução de área por matriz
   - filtros de targeting
2. Testes de integração (play mode):
   - fluxo seleção/confirm/cancel
   - multi-target + área
3. Checklist manual em cena de QA.

**Critério de aceite**: cenários principais passam sem regressão de turno/ação.

---

### Ordem recomendada de execução (task a task)

1. **Task 1** (modelo base)
2. **Task 2** (pipeline unificado)
3. **Task 3** (targeting avançado)
4. **Task 4** (área por matriz)
5. **Task 6** (drawers/editors) — já com dados estáveis
6. **Task 5** (efeitos compostos)
7. **Task 7** (preview UX)
8. **Task 8** (migração)
9. **Task 9** (testes)

> Sugestão: começar pela **Task 1** no próximo passo, definindo os novos tipos de dados sem quebrar assets atuais.
