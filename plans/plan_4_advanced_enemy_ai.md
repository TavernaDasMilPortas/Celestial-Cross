# Plano de Implementação: IA Avançada de Inimigos

## Visão Geral
Este plano descreve a refatoração e expansão do sistema de Inteligência Artificial dos inimigos, inspirando-se em sistemas complexos e robustos de jogos como *Summoners War*, *Fire Emblem* e *Cookie Run*. O objetivo é criar uma IA altamente configurável, orientada a dados, perfeitamente integrada ao sistema de `Unit` existente, e com uma interface de desenvolvimento bilíngue (Português/Inglês) para facilitar o trabalho da equipe de design.

## Objetivos Principais
1. **Acoplamento Inteligente `AIBrain` <-> `Unit`**: Associar explicitamente o componente `Unit` ao `AIBrain` para acesso direto a habilidades (`Skills`), atributos, buffs/debuffs e estado atual.
2. **Injeção de Inimigos nas Fases**: Garantir que as instâncias de inimigos sejam completamente moldadas por `ScriptableObjects` (já existente no `LevelData` e `UnitData`, mas vamos expandir para *Patterns* de IA específicos por fase).
3. **Configuração Máxima de Comportamento (IA Baseada em Regras e Pesos)**: Expandir o `AIBehaviorProfile` para suportar condições complexas (ex: "Usar Cura se HP < 30%", "Focar inimigo do elemento Vento", "Proteger aliado com menos HP").
4. **Localização do Inspector (PT/EN)**: Implementar tooltips, cabeçalhos e um botão (via Custom Editor) no `AIBrain` para alternar o idioma do Inspector, facilitando o uso por toda a equipe.

---

## Passo a Passo da Implementação

### Fase 1: Refatoração do `AIBrain` e Integração com `Unit`

Atualmente, o `AIBrain` busca ações via `GetComponents<UnitActionBase>()`. Vamos aprimorar isso para que o `AIBrain` atue como o "cérebro" verdadeiro, tendo acesso a todo o estado da unidade.

*   [ ] **Atualizar a classe `AIBrain`:**
    *   Adicionar referência explícita para `Unit` (`public Unit MyUnit { get; private set; }`).
    *   No método `Awake` ou `Initialize`, capturar e armazenar a referência do `Unit`.
    *   Permitir que o `AIBrain` acesse e avalie o impacto de `Skills` específicas, não apenas ações genéricas.
*   [ ] **Expansão da Inicialização Data-Driven:**
    *   Garantir que, durante o `UnitRuntimeConfigurator.Initialize()`, o `AIBehaviorProfile` específico daquela fase/instância possa ser sobrescrito (permitindo que o mesmo inimigo tenha IAs diferentes em fases diferentes, como chefes).

### Fase 2: Expansão do `AIBehaviorProfile` (Inspiração em Fire Emblem e Summoners War)

Jogos táticos exigem IAs que entendam o campo de batalha, fraquezas elementais e papéis (Tank, Healer, DPS).

*   [ ] **Criar Estrutura de Condições (`AICondition`)**:
    *   Condições baseadas no próprio estado: `SelfHpBelowPercentage`, `HasStatusEffect`, `CooldownReady`.
    *   Condições baseadas no ambiente/alvo: `TargetHpBelowPercentage`, `AdvantageousElementAvailable`, `AllyNeedsHealing`.
*   [ ] **Criar Estrutura de Prioridades de Alvo (`AITargetingRule`)**:
    *   `LowestHP`: Focar em unidades fracas.
    *   `ElementalAdvantage`: Focar (ou evitar) com base em vantagens do sistema (Estilo *Summoners War*).
    *   `ClosestUtility`: Focar Healers primeiro.
    *   `Proximity`: Atacar o alvo mais próximo (padrão de agressão burra/zumbi).
*   [ ] **Sistemas de Pesos (Utility AI)**:
    *   Modificar a avaliação de ações. Em vez de apenas verificar se pode usar, cada ação terá um escore calculado com base no `AIBehaviorProfile`. A ação com o maior escore final é executada.

### Fase 3: Localização Dinâmica do Inspector (Ferramenta de Design)

Para facilitar a configuração por level designers, o Inspector do `AIBrain` e seus `ScriptableObjects` derivados precisam ser amigáveis e flexíveis em relação ao idioma.

*   [ ] **Criar `AIBrainEditor` (Custom Editor)**:
    *   Criar script em pasta `Editor` estendendo `Editor` para focar em `AIBrain` e `AIBehaviorProfile`.
    *   Implementar um `GUILayout.Button("Tradução: PT / EN")` ou `Toggle` global salvo na sessão (via `EditorPrefs`) para alternar.
*   [ ] **Dicionário de Tradução Integrado**:
    *   Mapear os `SerializedProperties` e alterar seus displays. Exemplo: `TargetingRule` pode ser exibido como "Regra de Mira" (PT) ou "Targeting Rule" (EN).
    *   Adicionar tooltips traduzidos descrevendo exatamente o que cada campo e peso faz na IA.

### Fase 4: Integração de Habilidades Especiais (Cookie Run Style)

Em jogos como *Cookie Run*, inimgos/chefes têm padrões de ataque baseados em ciclo de tempo ou fases de "Raiva".

*   [ ] **Adicionar Suporte a Fases/Gatilhos Emocionais (`AIPatterns`)**:
    *   Permitir que a IA mude seu `AIBehaviorProfile` no meio da batalha (Ex: se HP < 50%, entra em *Enrage Profile*, focando Dano em Área e ignorando cura).
    *   Suportar Habilidades de Assinatura que ignoram a fila de pesos e têm prioridade absoluta quando prontas e a condição é atingida.

---

## Cronograma de Tarefas Resumido (`TODO.md` Sugerido)

1. Criar novo galho/branch de IA.
2. Refatorar `AIBrain` para manter `MyUnit`.
3. Criar Enum e Classes base para Regras de IA (`AICondition`, `AIPatterns`).
4. Desenvolver o `AIBrainEditor` com sistema de troca de linguagem PT/EN.
5. Atualizar ou criar um novo `AIBehaviorProfile` com suporte a pesos dinâmicos.
6. Aplicar o novo perfil a um chefe de teste para validar o comportamento (testar ataques com vantagem elemental e cura em aliados feridos).
7. Validar injeção via fase (LevelData definindo e sobrescrevendo padrões de IA).