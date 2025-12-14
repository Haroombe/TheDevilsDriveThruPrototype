using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/HeavenlyHand")]

    public class HeavenlyHand : Mod
    {
        public override int InitialUses => -10;

        public override string ModName => "Heavenly Hand";
        public override string description => $"Each order payout is permanently multiplied based on the shift number! (Currently '{GameManager.Instance.shiftNum}x')";
        public override string modUsedMessage => $"Payout will increase {GameManager.Instance.shiftNum}x!";

        public override void ResetMod()
        {
            
        }
        public override ModType modType => ModType.Permanent;

        public override float _GetPayoutMultiplier()
        {
            return (float) GameManager.Instance.shiftNum;
        }


        public override void ProcessOrder(Order order)
        {

        }

    }
}