---
name: powershell-csharp-escaping
description: Previne erros de sintaxe ao escrever código C# via PowerShell, forçando o uso de here-strings com aspas simples (@'...') em vez de aspas duplas.
---

# Regras de Escaping C# no PowerShell

Ao usar o terminal (PowerShell) para substituir, adicionar ou sobrescrever arquivos C# (ex: `Set-Content`, `-replace`), você **DEVE** seguir estas regras para evitar quebra de scripts com aspas duplas indevidamente escapadas (ex: `""Title""` em vez de `"Title"`) e erros como `Unterminated raw string literal`.

## Regra 1: Use Here-Strings com Aspas Simples (`@'` ... `'@`)

Nunca use here-strings com aspas duplas (`@"` ... `"@`) ao injetar código C#, pois o PowerShell tentará expandir variáveis e processar as aspas duplas, exigindo escaping tedioso e propenso a erros que frequentemente destrói strings literais no arquivo de destino.

Sempre envolva blocos de código C# literal em here-strings com aspas simples:

**CORRETO:**
```powershell
$csharpCode = @'
public class VictoryRewardUI : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("Hello World");
    }
}
'@
Set-Content "file.cs" -Value $csharpCode -Encoding UTF8
```

**INCORRETO:**
```powershell
$csharpCode = @"
public class VictoryRewardUI : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log(""Hello World""); // ESCAPING INCORRETO: QUEBRA O COMPILADOR
    }
}
"@
```

## Regra 2: Strings com Aspas Simples para Substituições Regex

Se estiver fazendo operações `-replace` no terminal, use aspas simples `' '` para delimitar a string no PowerShell para que você possa digitar aspas duplas `"` internamente sem efeitos colaterais.

**CORRETO:**
```powershell
(Get-Content "file.cs" -Raw) -replace 'Debug\.Log\("old"\);', 'Debug.Log("new");'
```

## Regra 3: Prefira Ferramentas Nativas Primeiro

Sempre que possível, prefira usar as ferramentas nativas de edição de arquivo (`replace_file_content`, `multi_replace_file_content`) em vez de executar substituições regex via PowerShell no terminal. Isso mitiga completamente o risco de erros de parsing e escaping de strings. Use regex no PowerShell somente quando a ferramenta nativa falhar por problemas de indentação/whitespace.