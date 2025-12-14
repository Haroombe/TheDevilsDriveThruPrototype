using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/FrenchFryFriday")]

    public class FrenchFryFriday : Mod
    {

        public float paymentMult = 3f;
        public override int InitialUses => GameManager.Instance.OrdersPerShift;

        public override string ModName => "French Fries Friday";
        public override string description => $"For 1 shift, Fries payout '{paymentMult}x'";
        public override string modUsedMessage => $"'{paymentMult}x' payout on Fries!";

        public override void ResetMod()
        {
            
        }

        public override ModType modType => ModType.Finite;

        public override float _GetFriesPriceMultiplier()
        {
            return paymentMult;
        }

        public override void ProcessOrder(Order order)
        {

        }



    }
}