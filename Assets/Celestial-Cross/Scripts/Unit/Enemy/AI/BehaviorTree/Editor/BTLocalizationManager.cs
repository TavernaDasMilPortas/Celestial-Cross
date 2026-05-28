using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Editor
{
    public static class BTLocalizationManager
    {
        public enum Language
        {
            English,
            Portuguese
        }

        public static Language CurrentLanguage = Language.English;

        public static event System.Action OnLanguageChanged;

        public static void ToggleLanguage()
        {
            CurrentLanguage = CurrentLanguage == Language.English ? Language.Portuguese : Language.English;
            OnLanguageChanged?.Invoke();
        }

        private static readonly Dictionary<string, string> EnglishToPortuguese = new Dictionary<string, string>
        {
            { "Save Tree", "Salvar Árvore" },
            { "Load Tree", "Carregar Árvore" },
            { "Generate AI Presets", "Gerar Presets de IA" },
            { "Language: EN", "Idioma: PT-BR" },
            
            // Nodes
            { "Root Node", "Nó Raiz" },
            { "Sequence", "Sequence (E)" },
            { "Selector", "Fallback (OU)" },
            { "Action Use Ability", "Ação: Usar Habilidade" },
            { "Action Move", "Ação: Mover" },
            { "Action Wait", "Ação: Aguardar" },
            { "Condition Switch", "Switch Condicional" },
            { "Check Value", "Condição: Checar Valor" },
            { "Get Numeric Data", "Dado: Obter Número" },
            { "Get Target", "Dado: Obter Alvo" },
            { "Invert", "Inverter (NOT)" },
            { "Cooldown", "Recarga (Cooldown)" },
            
            // UI Elements
            { "Add Step", "Adicionar Passo" },
            { "Add Case", "Adicionar Condição" },
            { "Operator", "Operador" },
            { "Threshold", "Valor (Limite)" },
            { "Data Type", "Tipo de Dado" },
            
            // Enum Values
            { "Approach", "Aproximar" },
            { "Flee", "Fugir" },
            { "Flank", "Flanquear" },
            { "Wander", "Vagar" }
        };

        public static string GetString(string englishKey)
        {
            if (CurrentLanguage == Language.English)
                return englishKey;
            
            if (EnglishToPortuguese.TryGetValue(englishKey, out string ptValue))
                return ptValue;
            
            return englishKey; // fallback
        }
    }
}
