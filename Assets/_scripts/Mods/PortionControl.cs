using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    public class PortionControl : Mod
    {

        private int ItemWeightMult = 2;
        public override int InitialUses => -10; //arbitrary 

        public override string ModName => "Portion Control";
        public override string description => $"Each food item in your inventory weighs '{ItemWeightMult}x' permanently.fulfills '{ItemWeightMult}x' the order";
        public override string modUsedMessage => $"Food Items weigh '{ItemWeightMult}x'!";


        public override ModType modType => ModType.Permanent;

        public override int GetBurgerInventoryWeightMultiplier()
        {
            return ItemWeightMult;
        }

        public override int GetFriesInventoryWeightMultiplier()
        {
            return ItemWeightMult;
        }

        public override int GetSodaInventoryWeightMultiplier()
        {
            return ItemWeightMult;
        }
        public override void ProcessOrder(Order order)
        {

        }



    }
}