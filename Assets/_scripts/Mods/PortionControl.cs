using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/PortionControl")]

    public class PortionControl : Mod
    {

        public int ItemWeightMult = 2;
        public override int InitialUses => -10; //arbitrary 

        public override string ModName => "Portion Control";
        public override string description => $"Each food item in your inventory weighs '{ItemWeightMult}x' permanently.fulfills '{ItemWeightMult}x' the order";
        public override string modUsedMessage => $"Food Items weigh '{ItemWeightMult}x'!";

        public override void ResetMod()
        {

        }
        public override ModType modType => ModType.Permanent;

        public override int _GetBurgerInventoryWeightMultiplier()
        {
            return ItemWeightMult;
        }

        public override int _GetFriesInventoryWeightMultiplier()
        {
            return ItemWeightMult;
        }

        public override int _GetSodaInventoryWeightMultiplier()
        {
            return ItemWeightMult;
        }
        public override void ProcessOrder(Order order)
        {
            GameManager.Instance.UpdateAllInventoryUI();
        }



    }
}