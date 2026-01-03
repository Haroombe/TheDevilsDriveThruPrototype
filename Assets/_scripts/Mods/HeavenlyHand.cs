using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/HeavenlyHand")]

    public class HeavenlyHand : Mod
    {
        public override int InitialUses => -10;
        private bool isSet = false;
        private float shiftbasedMult = 2f; // Initialize to 0 (or -1, if 0 is a valid multiplier)

        // Helper property to determine the multiplier value to display
        public override string ModName => "Heavenly Hand";
        public override string description => $"Each order payout is permanently multiplied by {shiftbasedMult}!";
        public override string modUsedMessage => $"Payout will increase {shiftbasedMult}x!";

        public override void ResetMod()
        {
            
        }
        public override ModType modType => ModType.Permanent;

        public override float _GetPayoutMultiplier()
        {
            return shiftbasedMult;
        }


        public override void ProcessOrder(Order order)
        {
            if (!isSet)
            {
                shiftbasedMult = 2;
                isSet = true;
            }

        }

    }
}