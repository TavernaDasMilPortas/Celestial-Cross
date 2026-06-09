using UnityEngine;
using CelestialCross.Combat;

namespace CelestialCross.Tutorial
{
    public static class TutorialMockCombat
    {
        public static bool ShouldMock => TutorialManager.Instance != null 
                                       && TutorialManager.Instance.IsActive 
                                       && TutorialManager.Instance.CurrentStepForceResult;
        
        public static AttackResult GetMockedResult()
        {
            var step = TutorialManager.Instance.CurrentStep;
            if (step == null) return new AttackResult(0, false);

            return new AttackResult(step.ForcedDamage, step.ForcedCrit);
        }
    }
}
