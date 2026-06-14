---
name: review-implementing
description: Processa e implementa feedback de code review sistematicamente. Use quando o usuário fornecer comentários de revisão, feedback de PR, notas de review, ou pedir para implementar sugestões de revisões.
---

# Implementação de Feedback de Code Review

Processa e implementa alterações sistematicamente com base em feedback de revisão de código.

## Quando Usar

- O usuário fornece comentários de revisores ou feedback
- O usuário cola notas de revisão de PR
- O usuário menciona implementar sugestões de revisão
- O usuário diz "aplique esses comentários" ou "implemente o feedback"
- O usuário compartilha lista de alterações solicitadas por revisores

## Fluxo de Trabalho Sistemático

### 1. Analisar Notas do Revisor

Identifique itens individuais de feedback:
- Separe listas numeradas (1., 2., etc.)
- Trate bullet points ou feedback não numerado
- Extraia solicitações de alteração distintas
- Esclareça itens ambíguos antes de começar

### 2. Criar Lista de Tarefas

Crie tarefas acionáveis:
- Cada item de feedback se torna uma ou mais tarefas
- Quebre feedback complexo em tarefas menores
- Torne as tarefas específicas e mensuráveis

Exemplo:
```
- Adicionar null checks na função de extração
- Corrigir lógica de detecção de tags duplicadas
- Atualizar comentários no PlayerController.cs
- Adicionar teste para caso extremo de dano negativo
```

### 3. Implementar Alterações Sistematicamente

Para cada tarefa:

**Localizar código relevante:**
- Usar grep para buscar funções/classes
- Buscar arquivos por padrão
- Ler implementação atual

**Fazer alterações:**
- Usar ferramentas de edição para modificações
- Seguir convenções do projeto (`.ia-rules`)
- Preservar funcionalidade existente a menos que a alteração de comportamento seja intencional

**Verificar alterações:**
- Checar correção de sintaxe
- Executar testes relevantes se aplicável
- Garantir que as alterações atendem à intenção do revisor
- **IMPORTANTE:** Verificar referências — nenhuma `using` órfã, nenhuma referência a classes/métodos renomeados ou removidos

**Atualizar status:**
- Marcar tarefa como concluída imediatamente após finalizar
- Avançar para a próxima tarefa

### 4. Tipos de Feedback

**Alterações de código:**
- Usar ferramentas de edição para código existente
- Seguir convenções de tipo e nomenclatura do C#
- Manter estilo consistente

**Novas funcionalidades:**
- Criar novos arquivos se necessário
- Adicionar testes correspondentes
- Atualizar documentação

**Documentação:**
- Atualizar comentários seguindo estilo do projeto
- Modificar arquivos markdown conforme necessário
- Manter explicações concisas

**Testes:**
- Escrever testes claros e descritivos
- Usar nomes que descrevam o cenário testado

**Refatoração:**
- Preservar funcionalidade
- Melhorar estrutura do código
- Executar testes para verificar ausência de regressões
- **CRÍTICO:** Verificar que nenhuma referência foi quebrada pela refatoração

### 5. Validação

Após implementar alterações:
- Verificar se há erros de compilação
- Checar referências quebradas (using, namespaces, campos serializados)
- Verificar que as alterações não quebram funcionalidade existente
- Remover `using` statements não utilizadas

### 6. Comunicação

Mantenha o usuário informado:
- Atualizar lista de tarefas em tempo real
- Pedir esclarecimentos sobre feedback ambíguo
- Reportar bloqueios ou desafios
- Resumir alterações ao concluir

## Casos Extremos

**Feedback conflitante:**
- Pedir orientação ao usuário
- Explicar o conflito claramente

**Alterações que quebram compatibilidade:**
- Notificar o usuário antes de implementar
- Discutir impacto e alternativas

**Testes falham após alterações:**
- Corrigir testes antes de marcar tarefa como concluída
- Garantir que todos os testes relacionados passam

**Código referenciado não existe:**
- Pedir esclarecimento ao usuário
- Verificar entendimento antes de prosseguir

## Diretrizes Importantes

- **Marcar tarefas como concluídas imediatamente** após cada item
- **Perguntar** quando houver feedback pouco claro
- **Executar testes** se alterações afetam código testado
- **Seguir convenções do `.ia-rules`** para todas as alterações de código
- **SEMPRE verificar referências** após alterações (usando, namespace, campos serializados)
