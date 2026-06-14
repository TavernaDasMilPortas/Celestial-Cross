---
name: unity-code-validation
description: Checklist de validação para código C# gerado pela IA em projetos Unity. Use para revisar e corrigir outputs antes de aplicar ao projeto.
---

# Validação de Código Unity Gerado por IA

Checklist para validar código C# gerado por IA antes de considerá-lo pronto para o projeto Unity.

## Quando Usar

- Após gerar qualquer script C# novo
- Após modificar scripts existentes
- Quando o usuário reportar erros de compilação ou comportamento inesperado
- Em revisões de código gerado

## Checklist de Validação

### 1. APIs e Referências

- [ ] Todas as classes, métodos e propriedades usadas existem na API da Unity atual?
- [ ] Nenhuma API deprecated foi utilizada? (ex: `GUIText`, `WWW`, `Application.LoadLevel`)
- [ ] Nenhuma classe, namespace ou método foi "inventado" (alucinado)?
- [ ] Todos os `using` statements são necessários e válidos?
- [ ] Nenhum `using` está faltando para tipos utilizados no código?

### 2. Ciclo de Vida Unity

- [ ] `Awake()`, `Start()`, `OnEnable()`, `OnDisable()`, `OnDestroy()` são usados corretamente?
- [ ] Nenhuma chamada a APIs Unity ocorre em construtores?
- [ ] `GetComponent` NÃO é chamado dentro do `Update()` ou loops quentes? (deve ser cacheado)
- [ ] Referências são inicializadas em `Awake()` ou `Start()` e validadas com null check?

### 3. Null Safety

- [ ] Verificações de nulo usam `== null` (nunca `ReferenceEquals` para objetos Unity)?
- [ ] `TryGetComponent<T>()` é usado em vez de `GetComponent<T>()` onde o componente pode não existir?
- [ ] Referências que podem ser destruídas entre frames são verificadas antes do acesso?
- [ ] Guard clauses são usadas no início de métodos para retornar cedo em caso de referências nulas?

### 4. Performance

- [ ] Nenhuma alocação desnecessária dentro de `Update()` (sem `new`, LINQ, ou concatenação de strings)?
- [ ] `CompareTag()` é usado em vez de `== "tag"`?
- [ ] Object Pooling é utilizado para objetos instanciados frequentemente?
- [ ] Coroutines ou eventos são usados em vez de polling constante?

### 5. Serialização

- [ ] Campos expostos usam `[SerializeField] private` em vez de `public`?
- [ ] Nenhum tipo não-serializável está marcado como serializado? (Dictionary, arrays multidimensionais)
- [ ] Campos renomeados possuem `[FormerlySerializedAs]`?
- [ ] `OnValidate()` é implementado para validar dados no Inspector quando apropriado?

### 6. Arquitetura

- [ ] O código segue os padrões definidos no `.ia-rules`?
- [ ] Nenhum `GameObject.Find()` ou `FindObjectOfType()` é usado em código de gameplay?
- [ ] Comunicação entre sistemas usa eventos em vez de referências diretas?
- [ ] O código não reinventa funcionalidades já existentes no projeto?

### 7. Limpeza Final

- [ ] Todos os `using` statements não utilizados foram removidos?
- [ ] Nenhum `Debug.Log` de depuração temporária foi deixado no código final?
- [ ] Comentários explicam o "porquê" de lógicas complexas (não o "o quê")?
- [ ] O nome da classe corresponde ao nome do arquivo?
