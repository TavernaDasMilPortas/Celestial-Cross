using UnityEngine;

namespace CelestialCross.Tutorial
{
    public enum TutorialHighlightTarget 
    { 
        None, 
        UIButton, 
        GridTile, 
        Unit, 
        UIPanel 
    }

    public enum TutorialBannerPosition 
    { 
        Top, 
        Center, 
        Bottom 
    }

    public enum TutorialAdvanceCondition 
    { 
        ClickHighlighted, 
        ClickAnywhere, 
        WaitSeconds, 
        ActionConfirmed, 
        TurnEnded, 
        UnitPlaced 
    }
}
