# Mecânicas de Combate - Celestial Cross

O combate é baseado em turnos táticos em um grid 2D, com foco em composição de habilidades e reatividade.

## 1. Fluxo de Combate
O combate é dividido em **Rounds** e **Turns**:
- **Turno**: O momento individual de uma unidade agir. A ordem é baseada na `Speed`.
- **Round**: Um ciclo completo onde todas as unidades agiram uma vez. No início de cada Round, o `RoundCounter` incrementa e hooks globais são disparados.

## 2. Sistema de Ações
As unidades possuem **Native Actions** (ataque padrão) e **Abilities** (habilidades de personagem ou pet). Todas herdam de `UnitActionBase` e podem ser:
- **Ativas**: Requerem seleção de alvo e confirmação.
- **Passivas**: Processadas automaticamente pelo **Weaver System**.

## 3. Cálculo de Dano e Cura
O `DamageModel.cs` gerencia a matemática base:
- **Dano Físico/Mágico**: Calcula a diferença entre ataque e defesa.
- **Bônus e Redução**: Aplicados via `DamageBonus` e `DamageReduction`.
- **Críticos**: Calculados com base na sorte/probalidade.

## 4. Weaver System (Passivas e Condições)
Este sistema permite que efeitos reajam a eventos de combate.
- **Passivas**: Habilidades permanentes da unidade (ex: "Counter-Attack").
- **Condições (Status Effects)**: Efeitos temporários (ex: "Poison", "Speed Buff").
- **Hooks**: Pontos de entrada como `OnBeforeTakeDamage` ou `OnTurnStart`.

## 5. Feedback Visual
- **Damage Popups**: Números flutuantes que indicam dano/cura.
- **Turn Timeline**: UI que mostra a fila de turnos atual.
- **Targeting Visuals**: Destaque de tiles no grid durante a seleção de alvos.
