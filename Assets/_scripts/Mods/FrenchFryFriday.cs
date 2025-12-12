using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    public class FrenchFryFriday : Mod
    {

        private float paymentMult = 3f;
        public override int InitialUses => GameManager.Instance.OrdersPerShift;

        public override string ModName => "French Fries Friday";
        public override string description => $"For 1 shift, Fries payout '{paymentMult}x'";
        public override string modUsedMessage => $"'{paymentMult}x' payout on Fries!";


        public override ModType modType => ModType.Finite;

        public override float GetFriesPriceMultiplier()
        {
            return paymentMult;
        }

        public override void ProcessOrder(Order order)
        {

        }



    }
}