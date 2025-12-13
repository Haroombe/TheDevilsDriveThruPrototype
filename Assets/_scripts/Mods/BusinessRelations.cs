using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    public class BusinessRelations : Mod
    {

        private float discount = 0.30f;
        public override int InitialUses => GameManager.Instance.OrdersPerShift;

        public override string ModName => "Business Relations";
        public override string description => $"The cost of all food items drops by {discount*100}% permanently!";
        public override string modUsedMessage => $"Applied!";

        public override void ResetMod()
        {
        }

        public override ModType modType => ModType.Permanent;


        public override float GetBurgerCostMultiplier()
        {
            return 1-discount;
        }

        public override float GetFriesCostMultiplier()
        {
            return 1-discount;
        }

        public override float GetSodaCostMultiplier()
        {
            return 1 - discount;
        }
        public override void ProcessOrder(Order order)
        {

        }



    }
}