# Plano de Ação: Correção de Múltiplos Alvos na IA

**Arquivo a ser modificado:** `Assets/Celestial-Cross/Scripts/Abilities/Graph/Runtime/AbilityGraphInterpreter.cs`

## Contexto
A IA simula corretamente o uso de habilidades em múltiplos alvos usando o seu motor de pontuação. Contudo, quando vai executar de fato a habilidade, o motor do grafo (`AbilityGraphInterpreter`) sofre um "Early Exit" (interrompe o processamento prematuramente) assim que adiciona o primeiro alvo passado pela IA, ignorando completamente que a magia tinha `multipleTargets = true` e vagas de alvos sobrando.

## O Que Deve Ser Feito

No método `HandleTargeting` da classe `AbilityGraphInterpreter.cs`, localize o trecho em que a IA repassa o alvo (por volta da linha em que há `if (context.source != null && context.source.Team != Team.Player)`).

Deve-se substituir o bloco atual de validação do alvo forçado da IA pela versão corrigida abaixo:

```csharp
            if (context.source != null && context.source.Team != Team.Player)
            {
                // Para a IA, se o Behavior Tree forneceu um alvo explícito (targetPos), ele tem prioridade
                if (context.targetPos.HasValue)
                {
                    Debug.Log($"[Interpreter] Usando alvo do Behavior Tree ({context.targetPos.Value}) para a IA.");
                    
                    // 1. Se for um ataque em área (AoE geográfico)
                    if (data.mode == GraphTargetMode.Area && data.areaPattern != null)
                    {
                        var areaPoints = AreaResolver.ResolveCells(context.targetPos.Value, data.areaPattern, data.preferredDirection);
                        context.targets.Clear();
                        if (GridMap.Instance != null)
                        {
                            foreach (var pt in areaPoints)
                            {
                                var tile = GridMap.Instance.GetTile(pt);
                                if (tile != null && tile.OccupyingUnit != null)
                                {
                                    bool isAlly = tile.OccupyingUnit.Team == context.source.Team;
                                    if (data.factionType == GraphFactionType.Ally && !isAlly) continue;
                                    if (data.factionType == GraphFactionType.Enemy && isAlly) continue;
                                    
                                    context.targets.Add(tile.OccupyingUnit);
                                }
                            }
                        }
                    }
                    // 2. CORREÇÃO: Se for seleção múltipla de alvo único (ex: 3 flechadas distintas)
                    else if (data.mode == GraphTargetMode.Single && data.multipleTargets && data.maxTargets > 1)
                    {
                        var dataCopy = data;
                        dataCopy.range = resolvedRange;
                        
                        // Pedir ao resolver para achar os melhores alvos próximos baseado nos filtros
                        var auxTargets = AutoTargetResolver.Resolve(context.source, dataCopy);
                        
                        // Garantir que o alvo prioritário escolhido pelo Behavior Tree da IA seja o 1º da lista
                        var primaryUnit = GridMap.Instance?.GetTile(context.targetPos.Value)?.OccupyingUnit;
                        
                        context.targets.Clear();
                        if (primaryUnit != null) context.targets.Add(primaryUnit);
                        
                        // Preencher o restante das vagas da habilidade
                        foreach (var t in auxTargets)
                        {
                            if (context.targets.Count >= data.maxTargets) break;
                            if (t == primaryUnit && !data.allowSameTargetMultipleTimes) continue;
                            
                            context.targets.Add(t);
                        }
                    }
                    
                    yield break;
                }
            }
```

## Como pedir para o Agente IA (Gemini/Antigravity) executar isso no futuro:
Se você estiver em outro PC, basta abrir a conversa e dizer:
*"Por favor, leia o plano em `docs/Plano_MultiTarget_IA.md` e execute a refatoração proposta lá para corrigir as habilidades multi-alvo da IA."*
