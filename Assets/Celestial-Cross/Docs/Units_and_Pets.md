# Unidades e Bichinhos (Pets) - Celestial Cross

O sistema de personagens é baseado em uma hierarquia de classes e no uso extensivo de ScriptableObjects para dados.

## 1. Unit (Base)
Todas as entidades do combate (heróis, inimigos, aliados) herdam de `Unit`.
- **Health**: Componente obrigacional que gerencia vida e hooks de dano.
- **PassiveManager**: Gerencia as passivas e condições da unidade.
- **UnitData**: Referência ao asset que define os status base.

## 2. Bichinhos (Pets)
Unidades podem equipar **Pets**.
- **PetData**: ScriptableObject que adiciona status bônus ao dono e fornece uma **Ability** única.
- **Pet (Runtime)**: Algumas vezes representados visualmente ou processados pelo `TurnManager`.

## 3. Status e Progressão (CombatStats)
Todos os status são encapsulados em `CombatStats`:
- `health`: Vida máxima.
- `attack`: Poder ofensivo.
- `defense`: Redução de dano.
- `speed`: Define a ordem na fila de turnos.
- `accuracy` e `luck`: Afetam chance de acerto e críticos.

## 4. Gerenciamento de Dados (UnitData)
O asset `UnitData` centraliza:
- Nome e status base.
- Lista de **Abilities** (Habilidades de personagem).
- Lista de **Native Actions** (Comandos fixos como "Atacar", "Defender").
- Lista de **Passives** (Inseridas via Weaver System).
