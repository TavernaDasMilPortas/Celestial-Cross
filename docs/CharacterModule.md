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
