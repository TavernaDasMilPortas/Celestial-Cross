using System;

namespace CelestialCross.Progression
{
    [Serializable]
    public class InviteCost
    {
        public string InviteItemID;  // ex: "convite_leidell" ou "convite_generico"
        public int Amount;
        public string DisplayName;   // ex: "1 Convite de Leidell"
    }
}
