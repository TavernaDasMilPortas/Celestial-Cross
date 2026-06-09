---
name: unity-ui-builder
description: Regras obrigatórias para geração e construção de Interfaces de Usuário (UI) na Unity
---

# Padrão de Construção de UI (UI Builder Pattern)

Esta skill define as regras obrigatórias que a Inteligência Artificial deve seguir ao criar novas interfaces de usuário no projeto.

## A REGRA DE OURO
**NUNCA** construa blocos de Interface de Usuário via código durante o runtime (tempo de execução).
Isso quer dizer: nunca crie scripts de UI com `new GameObject()`, `AddComponent<Image>()`, ou crie Canvas/Painéis dinamicamente nos métodos `Awake()`, `Start()`, ou eventos (como foi o caso do `VictoryRewardUI` primitivo).

**SEMPRE** utilize um Editor Script (UI Builder) com a anotação `[MenuItem(...)]`.

## O Motivo
A UI criada puramente via runtime não aparece na hierarquia (Hierarchy) antes de o jogo rodar, impossibilitando que o desenvolvedor ajuste visualmente o layout, as cores, os anchors, e os componentes (como TextMeshPro) no Unity Editor (Inspector).

## Guia de Implementação (Passo a Passo)

Sempre que a IA precisar criar uma nova tela de UI (como um modal de vitória, loja, inventário, etc.):

1. **Script de Controle (Runtime):** 
   Crie o MonoBehaviour focado em lógica, utilizando atributos `[SerializeField]` para receber as referências visuais que serão manipuladas via código (textos, imagens, prefabs, botões, containers).

2. **Builder (Editor Script):** 
   Crie um script dentro de uma pasta `Editor/` (ex: `NomeDaSuaUIBuilder.cs`), adicione a biblioteca `UnityEditor` e crie um método estático `[MenuItem("Tools/UI Builders/...")]`.

3. **O Escopo do Builder:**
   Dentro desse método do Builder, aí sim você estará autorizado a usar `new GameObject()`, `AddComponent<RectTransform>()` e afins. O objetivo do Builder é injetar uma estrutura nova de UI diretamente na *Scene* atual, para o desenvolvedor ajustar visualmente *antes* de rodar o jogo.

4. **Amarração:**
   O Builder também já deve anexar o Script de Controle (Passo 1) ao objeto principal da UI e preencher as referências `[SerializeField]` criadas.

*Desta forma, todo o poder de configuração de aparência fica na mão do usuário (dev) através da aba Hierarchy/Inspector, e a AI entrega a arquitetura 100% pronta com 1 clique de menu.*
