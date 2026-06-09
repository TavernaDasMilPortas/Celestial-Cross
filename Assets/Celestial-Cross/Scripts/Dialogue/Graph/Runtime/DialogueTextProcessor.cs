using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CelestialCross.Dialogue.Runtime
{
    /// <summary>
    /// Utilitário para quebrar textos longos em pedaços menores para a UI.
    /// </summary>
    public static class DialogueTextProcessor
    {
        public static string[] SplitDialogueText(string text, int maxCharactersPerPage)
        {
            if (string.IsNullOrEmpty(text)) return new string[0];
            if (text.Length <= maxCharactersPerPage) return new string[] { text };

            // Expressão regular para tentar quebrar em frases (pontos finais, exclamações, etc)
            // Ou apenas quebrar por limite de caracteres se não achar pontuação
            var pages = new List<string>();
            string remainingText = text;

            while (remainingText.Length > 0)
            {
                if (remainingText.Length <= maxCharactersPerPage)
                {
                    pages.Add(remainingText);
                    break;
                }

                // Encontrar o último espaço ou pontuação antes do limite
                int cutIndex = maxCharactersPerPage;
                
                // Tenta achar um ponto final ou quebra de linha por perto primeiro para ficar natural
                int preferredCut = remainingText.LastIndexOfAny(new char[] { '.', '!', '?', '\n' }, maxCharactersPerPage);
                
                if (preferredCut > maxCharactersPerPage * 0.7f) // Se a pontuação estiver perto do limite
                {
                    cutIndex = preferredCut + 1;
                }
                else
                {
                    // Senão procura o último espaço
                    int lastSpace = remainingText.LastIndexOf(' ', maxCharactersPerPage);
                    if (lastSpace != -1) cutIndex = lastSpace;
                }

                string pageText = remainingText.Substring(0, cutIndex).Trim();
                
                // Adiciona reticências se não for a última página e não terminar com pontuação
                if (remainingText.Length > cutIndex && !pageText.EndsWith("...") && !pageText.EndsWith(".") && !pageText.EndsWith("!") && !pageText.EndsWith("?"))
                {
                    pageText += "...";
                }

                pages.Add(pageText);
                remainingText = remainingText.Substring(cutIndex).Trim();
            }

            return pages.ToArray();
        }
    }
}
