using UnityEngine;
using System;
using System.Collections.Generic;
using CelestialCross.Combat;

[Serializable]
public class AIBossPhase
{
    [Tooltip("Nome descritivo da fase (ex: 'Fase de Fúria')")]
    public string phaseName;

    [Tooltip("Trocar para esta fase quando HP for igual ou menor que este %")]
    [Range(0, 100)]
    public float triggerHpBelowPercent = 50f;

    [Tooltip("Novo perfil de comportamento que será ativado nesta fase")]
    public AIBehaviorProfile newBehaviorProfile;

    // Runtime state (não serializado para evitar lixo no Unity)
    [NonSerialized]
    public bool hasTriggered = false;
}

[CreateAssetMenu(fileName = "NewAIPattern", menuName = "Celestial Cross/AI/AI Pattern Data")]
public class AIPatternData : ScriptableObject
{
    [Tooltip("Perfil de comportamento inicial da unidade")]
    public AIBehaviorProfile initialProfile;

    [Tooltip("Lista de fases e seus gatilhos. Ordene da maior prioridade (menor HP) para a menor, ex: 30%, depois 50%")]
    public List<AIBossPhase> phases = new List<AIBossPhase>();
}
