using System.Collections;

using UnityEngine;

namespace Assets._scripts.Mods
{
    public class VipReroll : Mod
    {

        public float payoutMult = 4f;

        public override int InitialUses => 1;
        public override string ModName => "VIP Customer";
        public override string description => $"(Single Use) Reroll the current order with '{payoutMult}x' payout!";

        public override string modUsedMessage => $"Rerolled current order!)";
        public override ModType modType => ModType.Clickable;

        public override void ProcessOrder(Order order)
        {
 
        }

        public override void ResetMod()
        {
            
        }

        public override float GetPayoutMultiplier()
        {
            return payoutMult;
        }
        public void OnRerollOrderPressed()
        {
            //TODO QOL: SFX
            if (!this.TryUseAbility())
            {
                return;
            }
            GameManager.Instance.currentOrder = GameManager.Instance.CreateOrder();
        }



    }
}