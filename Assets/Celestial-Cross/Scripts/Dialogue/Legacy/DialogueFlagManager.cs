using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton que armazena as flags de diálogo ativas.
/// Flags são marcadores (strings) que registram escolhas do jogador.
/// Persiste automaticamente via PlayerPrefs.
/// 
/// Uso:
///   DialogueFlagManager.Instance.SetFlag("ajudou_shany");
///   bool ajudou = DialogueFlagManager.Instance.HasFlag("ajudou_shany");
/// </summary>
public class DialogueFlagManager : MonoBehaviour
{
    private const string PREFS_KEY = "DialogueFlags";

    public static DialogueFlagManager Instance { get; private set; }

    private HashSet<string> _flags = new HashSet<string>();

    /// <summary>
    /// Garante que o DialogueFlagManager exista em qualquer cena,
    /// mesmo que não tenha sido colocado manualmente na Hierarchy.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance != null) return;

        var go = new GameObject("DialogueFlagManager (Auto)");
        go.AddComponent<DialogueFlagManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFlags();
    }

    /// <summary>
    /// Ativa uma flag. Se já existir, não faz nada.
    /// </summary>
    public void SetFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;

        if (_flags.Add(flag))
        {
            Debug.Log($"[DialogueFlags] Flag ativada: \"{flag}\"");
            SaveFlags();
        }
    }

    /// <summary>
    /// Verifica se uma flag está ativa.
    /// </summary>
    public bool HasFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return false;
        return _flags.Contains(flag);
    }

    /// <summary>
    /// Remove uma flag específica.
    /// </summary>
    public void ClearFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;

        if (_flags.Remove(flag))
        {
            Debug.Log($"[DialogueFlags] Flag removida: \"{flag}\"");
            SaveFlags();
        }
    }

    /// <summary>
    /// Remove todas as flags (útil para "Novo Jogo").
    /// </summary>
    public void ClearAll()
    {
        _flags.Clear();
        SaveFlags();
        Debug.Log("[DialogueFlags] Todas as flags foram removidas.");
    }

    private void SaveFlags()
    {
        string data = string.Join(",", _flags);
        PlayerPrefs.SetString(PREFS_KEY, data);
        PlayerPrefs.Save();
    }

    private void LoadFlags()
    {
        _flags.Clear();

        if (!PlayerPrefs.HasKey(PREFS_KEY)) return;

        string data = PlayerPrefs.GetString(PREFS_KEY, string.Empty);

        if (string.IsNullOrEmpty(data)) return;

        string[] flags = data.Split(',');
        foreach (string flag in flags)
        {
            string trimmed = flag.Trim();
            if (!string.IsNullOrEmpty(trimmed))
                _flags.Add(trimmed);
        }

        Debug.Log($"[DialogueFlags] {_flags.Count} flag(s) carregada(s).");
    }
}
