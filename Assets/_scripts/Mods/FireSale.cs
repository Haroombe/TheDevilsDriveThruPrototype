using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    public class FireSale : Mod
    {
        private float ingredientOverrideCost = 0.01f;
        public override int InitialUses => 1;
        private bool isClicked = false;

        public override string ModName => "Fire Sale";
        public override string description => $"(Single Use) For 1 order, every food item costs ${ingredientOverrideCost}";
        public override string modUsedMessage => $"Food items cost ${ingredientOverrideCost}!";

        public override void ResetMod()
        {
            
        }

        public override ModType modType => ModType.Clickable;

        public override float GetBurgerCostOverride()
        {
            return isClicked? ingredientOverrideCost : base.GetBurgerCostOverride(); 
        }

        public override float GetFriesCostOverride()
        {
            return isClicked ? ingredientOverrideCost : base.GetFriesCostOverride();
        }
        public override float GetSodaCostOverride()
        {
            return isClicked ? ingredientOverrideCost : base.GetSodaCostOverride();
        }     
        public override void ProcessOrder(Order order)
        {

        }
        public void onModClick()
        {
            isClicked = true;
            // add method that is integrated with gamemanager that is used to caclulate the price of the items TODO

        }





    }
}