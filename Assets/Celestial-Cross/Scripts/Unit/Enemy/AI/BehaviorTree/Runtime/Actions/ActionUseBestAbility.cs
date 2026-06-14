namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Actions
{
    public class ActionUseBestAbility : ActionUseAbility
    {
        public ActionUseBestAbility()
        {
            ignoreCategoryFilter = true;
            minimumScoreThreshold = 5f;
        }
    }
}
