---
name: unity-refactoring-safety
description: Checklist obrigatório de segurança para refatoração de código Unity. Use SEMPRE que renomear, mover, deletar ou alterar assinaturas de scripts C#.
---

# Segurança em Refatoração Unity

Checklist de segurança obrigatório para qualquer operação de refatoração em projetos Unity (renomear classes, mover scripts, alterar assinaturas de métodos, deletar arquivos).

## Quando Usar

- Renomear classes, métodos, campos ou arquivos C#
- Mover scripts entre pastas ou namespaces
- Deletar scripts ou assets referenciados
- Alterar assinaturas de métodos públicos ou protegidos
- Refatorar múltiplos arquivos interdependentes

## Checklist Pré-Refatoração

Antes de iniciar qualquer refatoração:

1. **Identificar dependências:** Buscar todas as referências ao elemento que será alterado usando grep/busca no projeto
2. **Mapear impacto:** Listar todos os arquivos que serão afetados pela mudança
3. **Verificar serialização:** Se o campo é serializado (`[SerializeField]`, `public`), verificar Prefabs e ScriptableObjects que o referenciam

## Checklist Durante a Refatoração

Para cada arquivo modificado:

1. **Renomeação de campos serializados:**
   - OBRIGATÓRIO adicionar `[FormerlySerializedAs("nomeAntigo")]` antes do campo renomeado
   - Importar `using UnityEngine.Serialization;` se necessário

2. **Mudança de namespace:**
   - Atualizar todas as declarações `using` em arquivos que referenciam o namespace antigo
   - Verificar se o novo namespace está correto para a localização do arquivo

3. **Alteração de assinatura de método:**
   - Atualizar TODAS as chamadas ao método em todos os arquivos dependentes
   - Se o método é virtual/override, verificar toda a hierarquia de herança

4. **Deleção de classe/arquivo:**
   - Remover todas as referências antes de deletar
   - Verificar se nenhum Prefab ou ScriptableObject referencia o script

## Checklist Pós-Refatoração

Após completar todas as alterações:

1. **Limpeza de `using` statements:**
   - Remover todos os `using` não utilizados de cada arquivo modificado
   - Adicionar `using` necessários para novas dependências

2. **Verificação de consistência:**
   - Todos os nomes de classe correspondem aos nomes de arquivo?
   - Todos os namespaces correspondem à estrutura de pastas?
   - Nenhuma referência órfã permanece no código?

3. **Verificação de assinaturas Unity:**
   - Métodos de callback Unity (Awake, Start, OnEnable, OnCollisionEnter, etc.) têm a assinatura exata correta?
   - Métodos com `[SerializeField]` ainda estão no modificador de acesso correto?

4. **Verificação de compilação:**
   - Garantir que o projeto compila sem erros (CS0234, CS0246, etc.)

## Erros Comuns a Evitar

- ❌ Renomear campo serializado SEM `[FormerlySerializedAs]` → perda de dados no Inspector
- ❌ Mover script SEM atualizar namespace → erro de compilação CS0234
- ❌ Deletar script referenciado em Prefab → erro "Missing Script"
- ❌ Alterar assinatura de método virtual SEM atualizar overrides → erro de compilação
- ❌ Esquecer `using` statements após mover classes entre namespaces
- ❌ Deixar `using` statements órfãos de namespaces que não existem mais
