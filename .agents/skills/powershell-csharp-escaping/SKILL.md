---
name: powershell-csharp-escaping
description: Prevents syntax errors when writing C# code via PowerShell by enforcing single-quoted here-strings (@'...') instead of double-quoted ones.
---

# PowerShell C# Escaping Rules

When using the `run_in_terminal` tool (PowerShell) to replace, append, or overwrite C# code files (e.g., `Set-Content`, `-replace`), you **MUST** follow these rules to avoid breaking the script with improperly escaped double quotes (e.g., `""Title""` instead of `"Title"`) and triggering errors like `Unterminated raw string literal`.

## Rule 1: Use Single-Quoted Here-Strings (`@'` ... `'@`)
Never use double-quoted here-strings (`@"` ... `"@`) when injecting C# code, as PowerShell will try to expand variables and process double quotes, requiring tedious and error-prone escaping that often destroys string literals in the target file.

Always wrap literal C# code blocks in single-quoted here-strings:

**CORRECT:**
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

**INCORRECT:**
```powershell
$csharpCode = @"
public class VictoryRewardUI : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log(""Hello World""); // INCORRECT ESCAPING: BREAKS COMPILER
    }
}
"@
```

## Rule 2: Single-Quote Strings for Regex Replacements
If you are doing a `-replace` operation in the terminal, use single quotes `' '` for the PowerShell string wrapper so that you can type double quotes `"` internally exactly as they are without side effects.

**CORRECT:**
```powershell
(Get-Content "file.cs" -Raw) -replace 'Debug\.Log\("old"\);', 'Debug.Log("new");'
```

## Rule 3: Prefer Native Tools First
Whenever possible, prefer using the `replace_string_in_file` native tool over executing PowerShell regex replacements via terminal. It mitigates the risk of script parsing errors and string escaping entirely. Use PowerShell regex strings for multiline blocks only when the regular tool fails due to indentation/whitespace issues mismatch.