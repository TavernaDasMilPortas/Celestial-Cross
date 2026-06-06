using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CelestialCross.Progression
{
    public enum CompletionResetType
    {
        Never,    // Sem reset — conclusões são permanentes
        Daily,    // Reseta todo dia
        Weekly,   // Reseta toda semana
        Monthly,  // Reseta todo mês
        Custom    // Intervalo customizado
    }

    [Serializable]
    public class CompletionResetPolicy
    {
        [Tooltip("Define como e quando as conclusões deste nó são resetadas.")]
        public CompletionResetType ResetType = CompletionResetType.Never;
        
        [ShowIf("ResetType", CompletionResetType.Daily)]
        [Tooltip("Hora do dia (UTC) em que reseta. Ex: 3 = 3:00 AM UTC")]
        public int ResetHourUTC = 3;
        
        [ShowIf("ResetType", CompletionResetType.Weekly)]
        public DayOfWeek ResetDayOfWeek = DayOfWeek.Monday;
        
        [ShowIf("ResetType", CompletionResetType.Weekly)]
        public int WeeklyResetHourUTC = 3;
        
        [ShowIf("ResetType", CompletionResetType.Monthly)]
        [Range(1, 28)]
        public int ResetDayOfMonth = 1;
        
        [ShowIf("ResetType", CompletionResetType.Monthly)]
        public int MonthlyResetHourUTC = 3;
        
        [ShowIf("ResetType", CompletionResetType.Custom)]
        [Tooltip("Intervalo de reset em horas")]
        public float CustomResetIntervalHours = 24f;
    }
}
