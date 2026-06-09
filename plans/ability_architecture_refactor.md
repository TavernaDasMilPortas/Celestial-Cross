# Plano de Refatoração Arquitetural: Sistema de Habilidades e Passivas

Este documento detalha a reestruturação do sistema de habilidades do jogo, com o objetivo de criar uma arquitetura limpa, escalável e baseada em dados, dividida firmemente em 3 camadas: **Targets**, **Conditions** e **Effects/Modifiers**. A refatoração inclui também a completa extinção e substituição do sistema antigo de "Passive Effects" fragmentados por uma engine centralizada de `ModifierData`.

---

## 1. Visão Geral do Sistema

Atualmente, habilidades e efeitos passivos geram redundância de código através da criação contínua de classes ultra-específicas. O objetivo desta refatoração é "diluir" comportamentos fechados em **blocos de construção genéricos que o game designer monta no Inspector**.

*   As **Conditions** passam a ser globais e declaradas de forma modular em uma "piscina" na Habilidade (Blueprint).
*   Os **EffectSteps** (ativos/visuais) e **ModifierDatas** (passivos/matemáticos) simplesmente "assinam" uma ou mais condições através de índices numéricos.
*   Se as condições requeridas não forem atendidas, **apenas** aquele o Step ou Modificador que as exigia é silenciosamente ignorado. O restante da habilidade continua sem falhar.

---

## 2. As Três Camadas Arquiteturais

### Camada 1: Targets (Estratégias de Alvo)
A responsabilidade desta camada é exclusivamente gerir as malhas matemáticas que respondem à pergunta: *"Quem será afetado?"*

*   **Tipos Básicos:** `ManualTargeting` (baseado no clique no Grid UI) e `AutoTargetingStrategy` (baseado em LINQ espacial).
*   **Múltiplos Alvos no AutoTargeting:** O `AutoTargetingStrategy` será expandido para possuir o campo `public int maxTargetCount = 1`, utilizando `.Take(maxTargetCount)` para alvejar simultaneamente múltiplos "Inimigos Mais Próximos" ou "Aliados com Menor HP".

### Camada 2: Conditions (A Piscina Global)
Responsável por gerenciar as regras lógicas estritas (*"O alvo está com menos de 50% de HP?", "O alvo está envenenado?"*).

*   **Onde Ficam:** Adição da lista `public List<AbilityConditionData> globalConditions = new List<AbilityConditionData>();` dentro de `AbilityBlueprint`. O inspector desta classe agirá como um dicionário principal de condições.
*   Nenhum efeito ou modificador instanciará as próprias condições; eles cruzarão dados lendo desta lista mestre no instante que abrirem o `CombatContext`.

### Camada 3: Effects (Assíncronos) e Modifiers (Síncronos)
Aqui ocorre o impacto direto no jogo (Dano, Cura, Movimento). Esta camada foi bifurcada para atender corretamente os turnos x equações:

*   **EffectData (Ações Assíncronas):** Roda em `Coroutines` (`IEnumerator`). Usado para visuais de habilidade, como projéteis que levam tempo para alcançar o alvo, curas animadas, dano da skill base.
*   **ModifierData (Ações Síncronas/Matemáticas):** O **Substituto Definitivo das antigas "Passive Effects"**. Interfere de forma linear e imediata nas contas (sem coroutinas), aplicando buffs em status, multiplicando danos nos `Hooks` corretos (ex: `OnBeforeDealDamage`) e conferindo bônus lógicos baseados em *checkboxes* do modo `Data-Driven`.

Ambos possuirão o campo `public List<int> activeConditionIndices = new List<int>();` para checar com a Piscina de Condições no rodar das engrenagens.

---

## 3. A Estrutura do ModifierData (O Substituto das Passivas)

Criar uma classe separada por "Passive" em Scripts estava sobrecarregando o código. O novo `ModifierData` centraliza e dilui tudo isso em opções de caixas booleanas ("Checkboxes"):

```csharp
public abstract class ModifierData
{
    public CombatHook triggerHook; // Ex: OnBeforeDealDamage
    public List<int> activeConditionIndices = new List<int>(); // Ligação lógica
    
    // Método instantâneo e matemático. Evita uso de yield coroutines!
    public abstract void ApplyModifier(CombatContext context);
}
```

Em vez de criar uma classe específica para um Pet "Dar 10% a mais de dano conforme a distância" ou "Stackar HP perdido", você criará poucos modificadores amplamente personalizáveis, por exemplo:

**`StatMultiplierModifier : ModifierData`** (Multiplicador de Status Flexível)
Pode ter caixas booleanas no Inspector de Unit:
*   `[ ] Usar HP Perdido` (Aumenta o dano dependendo da vida que está faltando).
*   `[X] Escalonar por Distância` (Multiplica dano se estiver nas pontas das casas do Grid).

---

## 4. Sistema de Expiração e Status (Stateful Modifiers)

Para suportar efeitos passivos e buffs que são temporários (ex: Veneno, ou redução de dano nos próximos 3 ataques), introduziremos um sistema de envoltórios (Wrappers) e contadores, separando logicamente *"o que o efeito faz"* de *"quanto tempo ele dura"*.

A classe abstrata `ModifierData` (e até mesmo algumas `EffectData` específicas como as de Status) irá incorporar um Sub-Struct de Duração:

```csharp
public enum DurationType { Momentary, UntilEndOfTurn, Turns, Charges, Infinite }

[Serializable]
public struct ModifierDurationSettings
{
    public DurationType type;
    public int durationValue; // Ex: 2 para "2 Turnos" ou 3 para "3 Cargas/Ataques"
}
```

*   **Momentary (Momentâneo):** Ocorre unicamente como um flash na memória do Contexto de Combate e depois se dissipa (ex: Olhos de Lince no turno de ataque, o multiplicador age no momento exato do *hook* e nunca é salvo na Unidade como passiva para turnos futuros).
*   **Turns/UntilEndOfTurn (Turnos):** Salva o modificador na `Unit` alvo com o status de vida por tempo. O `TurnManager` fará o *"tick down"* (decrementar) no fim de cada round em que aquele personagem agir.
*   **Charges (Cargas):** Salva o modificador e só desgasta mediante uso. Exemplo: *"Diminui o dano dos próximos 3 ataques"*. O modificador se instala engatilhado no `OnTakeDamage` e, a cada execução de sucesso de sua própria matemática, ele deduz 1 carga. Ao chegar em 0, destrói e solta do personagem (estimulando o design onde o jogador adversário lança ataques multi-hit menores para quebrar as blindagens antes do Golpe Forte).
*   **O "ActiveModifier":** Para separar o asset puro (ScriptableObject) do que roda no jogo, será criada a classe dinâmica `ActiveModifier` instanciada nos personagens durantes os conflitos. Ela guarda qual é o referencial original e qual o seu *Tempo de Vida/Current Charges*, que se esvazia até expirar.

---

## 5. Novos Componentes a Serem Criados (Análise de Escopo Faltante)

Após avaliação do modelo de tática, proponho o desenvolvimento dos seguintes novos componentes base para preenchimento de todas as possbilidades na arquitetura:

### Novas Conditions
1.  **`HasStatusCondition`**: Verifica no `Target` se ele encontra-se "Envenenado", "Atordoado", "Enraizado", etc. Exige ligação e checagem da lista de Status/Debuffs ativas na unidade.
2.  **`FactionCondition`**: Permite restringir um passo da habilidade (Step/Modifier) "Apenas para Pets" ou "Apenas para Aliados/Inimigos".

### Novos EffectDatas (Ações Ativas/Visuais)
1.  **`CleanseEffectData`**: Remove classes filhas de Debuffs/DoTs no alvo instantaneamente. (Ideal para curandeiros e passes de limpeza).
2.  **`ApplyStatusEffectData`**: Adiciona DoT, Envenenamento, etc. Baseado puramente no sistema de duration de Turnos.

### Novos ModifierDatas (Ações Passivas/Síncronas)
1.  **`DistanceScalingModifier`**: Aplica um multiplicador (ex: x1.5) momentâneo baseado puramente no valor de `Vector2Int.Distance(Caster, Target)`. (A habilidade "Olhar de Lince").
2.  **`ShieldChargesModifier`**: Invoca redução/bloqueio de dano atrelada ao tipo `Charges`. Bloqueia o dano recebido e consome -1 charge, se deletando ao chegar em 0.
3.  **`TurnStatFlatBuffModifier`**: Aumenta atributos fixos (ex: Speed) atrelado ao DurationType `UntilEndOfTurn` ou `Turns`.
4.  **`LifestealModifier`**: Um recupador simples momentâneo atado ao `OnAfterDealDamage` (Caster recebe HP baseado em Dano Causado * Lifesteal%).

---

## 6. Fluxo Prático Atualizado

**Exemplo: Habilidade "Flecha Vampírica com Veneno"**

**Piscina de Condições Globais na Habilidade (AbilityBlueprint):**
*   `Index [0]` -> FactionCondition: O Alvo é Inimigo.
*   `Index [1]` -> HasStatusCondition: O Alvo NÃO possui a Tag [Envenenado].

**Estruturação de Execução em Steps e Modifiers:**
*   **Modifier 1 (DistanceScaling | SubType: Momentary):** Atado na Habilidade. Índica de condição exigido: `[0]`. No instante da engatinhagem matemática, gera calculo extra de distância e dissolve sem gastar nada da memória do alvo.
*   **Step 1 / EffectData 1 (Dano Base Perfurante):** Coroutine Visual. Condição: Nenhuma. Ocorre SEMPRE.
*   **Modifier 2 (Lifesteal | SubType: Momentary):** Condição: Nenhuma. Cura instantaneamente o atacante após o dano causado.
*   **Step 2 / Modifier 3 (Status de Veneno | SubType: Turns, Value: 2):** Condição exigida: `[1]`. Como a vítima falhou na tentativa de achar Veneno nela antes, agora adquire uma engrenagem (ActiveModifier) que tira HP dela nos próximos 2 ciclos de turno perfeitamente.

---

## 7. Arquivos e Entregáveis

1.  **Modificar:** `AbilityBlueprint.cs` (Lista global).
2.  **Modificar:** `EffectStep.cs` e `EffectData.cs` (Referências de índices de condição e loop de avaliação).
3.  **Modificar:** `AbilityExecutor.cs` (Adaptação da injeção das listagens globais).
4.  **Criar:** **`ModifierData.cs`** (Como Base Class) com estrutura `ModifierDurationSettings`.
5.  **Criar:** A classe rodante de memória `ActiveModifier.cs` gerida pelo `StatusManager` ou `Unit` do alvo afetado.
6.  **Criar:** Novos modifiers `DistanceScalingModifier.cs`, `ShieldChargesModifier.cs`, `TurnStatFlatBuffModifier.cs`.
7.  **Criar:** Novas condições `HasStatusCondition.cs`, `FactionCondition.cs`.