using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/HeavenlyHand")]

    public class HeavenlyHand : Mod
    {
        public override int InitialUses => -10;
        private bool isSet = false;
        private float shiftbasedMult = 0f; // Initialize to 0 (or -1, if 0 is a valid multiplier)

        // Helper property to determine the multiplier value to display
        private float DisplayMultiplier =>
            shiftbasedMult > 0f ? shiftbasedMult : (float)GameManager.Instance.shiftNum;
        public override string ModName => "Heavenly Hand";
        public override string description => $"Each order payout is permanently multiplied based on the immediate shift number! ('{DisplayMultiplier}x')";
        public override string modUsedMessage => $"Payout will increase {DisplayMultiplier}x!";

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
                shiftbasedMult = (float)GameManager.Instance.shiftNum;
                isSet = true;
            }

        }

    }
}