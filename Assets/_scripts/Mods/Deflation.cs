using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    public class Deflation : Mod
    {
        private float ItemDeflationMult = 0.30f;
        public override int InitialUses => GameManager.Instance.OrdersPerShift * 2;

        public override string ModName => "Deflation";
        public override string description => $"For 2 shifts, the cost of all items is reduced by {ItemDeflationMult*100}%";
        public override string modUsedMessage => $"{ItemDeflationMult}% item cost deflation is active!";


        public override ModType modType => ModType.Finite;

        public override float GetBurgerCostMultiplier()
        {
            return 1-ItemDeflationMult;
        }

        public override float GetFriesCostMultiplier()
        {
            return 1 - ItemDeflationMult;
        }
        public override float GetSodaCostMultiplier()
        {
            return 1 - ItemDeflationMult;
        }


        public override void ProcessOrder(Order order)
        {

        }

    }
}