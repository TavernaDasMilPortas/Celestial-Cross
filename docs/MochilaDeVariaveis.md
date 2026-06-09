# Mochila de Variáveis (Combat Variables Backpack)

A "Mochila de Variáveis" é um sistema flexível de troca de dados implementado no `CombatContext` e no `AbilityExecutionContext`. Ele permite que **Efeitos Base (EffectData)** guardem valores calculados durante a execução de uma habilidade para que **Efeitos Futuros** na mesma execução possam acessá-los.

## 🧳 O que é?

Tecnicamente, é um dicionário (`Dictionary<string, float> Variables`) inicializado quando uma habilidade começa a ser conjurada. 
Durante o fluxo em `AbilityExecutor.cs`, essa *mesma* mochila é passada de mão em mão para todo `CombatContext` criado em cada etapa (Effect Step) da habilidade.

Isso previne que habilidades complexas precisem de dezenas de propriedades "hardcoded" nas classes de combate principais.

---

## 🔑 Variáveis Padrão (Já Implementadas)

Atualmente, o sistema gera as seguintes chaves de forma automática através dos seus respectivos efeitos:

### `UltimoDanoCausado`
* **Quem gera:** `DamageEffectData.cs`
* **Tipo:** `float`
* **Exemplo de Valor:** `15.0` (Dano final deduzido da vida do alvo, após cálculos de defesa, bônus e críticos).
* **Para que serve:** Pode ser lido por outros efeitos logo em seguida para causar Roubo de Vida (Lifesteal), refletir dano (Thorns), ou gerar escudos baseados na força do soco.

---

## 🛠️ Como usar na prática (Exemplo: Roubo de Vida)

Para criar uma habilidade em que o Herói atinge um Inimigo causando 10 de dano e em seguida se cura nesse exato valor (10 de HP), você deve configurar o `AbilityBlueprint` dividindo a lógica em **dois passos (Effect Steps)**:

### 🎯 Passo 1: Causar Dano
* **Trigger:** `OnManualCast`
* **Targeting:** Seleção Manual (Alvo Inimigo ou Ponto)
* **Effects:** `DamageEffectData`
    * `Amount:` 10
    * Ao final deste cálculo, o script vai silenciosamente adicionar `["UltimoDanoCausado"] = 10` na mochila.

### 💖 Passo 2: Receber a Cura
* **Trigger:** `OnManualCast`
* **Targeting:** `SelfTargetingStrategy` *(Alvo automático: o próprio conjurador)*
* **Effects:** `HealEffectData`
    * `UseDynamicVariable:` **True (Marcado)**
    * `VariableName:` **UltimoDanoCausado**
    * O script vai abrir a mochila, procurar pela palavra "UltimoDanoCausado", encontrar o número `10` lá dentro, e aplicar a cura no Herói.

---

## 💻 Criando Novas Variáveis no Código

Se você criar um novo tipo de Efeito (ex: `StealGoldEffectData`), você pode facilmente salvar uma nova variável na mochila usando:

```csharp
public override void Execute(CombatContext context)
{
    float goldStolen = CalculateGold();
    
    // Salva na mochila
    if (context.Variables != null)
    {
        context.Variables["OuroRoubado"] = goldStolen;
    }
}
```

E para ler essa variável em outro efeito futuro (ex: `GainStatsBasedOnGoldEffectData`):

```csharp
public override void Execute(CombatContext context)
{
    float goldRecebido = 0;
    
    // Tenta ler da mochila
    if (context.Variables != null && context.Variables.TryGetValue("OuroRoubado", out float val))
    {
        goldRecebido = val;
    }

    // Aplica status usando 'goldRecebido'
}
```

## ⚠️ Limitações e Dicas
1. **Case-Sensitive:** O nome da variável precisa ser exatamente igual na hora de escrever e ler (ex: `"UltimoDanoCausado"` != `"ultimoDanoCausado"`). Copie e cole na interface da Unity!
2. **Ordem de Execução:** A ordem dos *Effect Steps* no `AbilityBlueprint` importa muito. Nunca tente Ler uma variável no **Step 1** se ela só será Gravada no **Step 2**.
3. **Escopo local:** A mochila de variáveis é **esvaziada** e destruída assim que as rotinas do `AbilityBlueprint` terminam de ser processadas. Ela não salva dados permanentemente para o próximo turno.