using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/BusinessRelations")]

    public class BusinessRelations : Mod
    {

        public float discount = 0.30f;
        public override int InitialUses => -10;

        public override string ModName => "Business Relations";
        public override string description => $"The cost of all food items drops by {discount*100}% permanently!";
        public override string modUsedMessage => $"Applied!";

        public override void ResetMod()
        {
        }

        public override ModType modType => ModType.Permanent;


        public override float _GetBurgerCostMultiplier()
        {
            return 1-discount;
        }

        public override float _GetFriesCostMultiplier()
        {
            return 1-discount;
        }

        public override float _GetSodaCostMultiplier()
        {
            return 1 - discount;
        }
        public override void ProcessOrder(Order order)
        {

        }



    }
}