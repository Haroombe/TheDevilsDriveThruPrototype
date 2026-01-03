using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/FireSale")]

    public class FireSale : Mod
    {
        public float ingredientOverrideCost = 0.01f;
        public override int InitialUses => 1;
        private bool _isClicked = false;

        public override bool isClicked()
        {
            return _isClicked;
        }

        public override string ModName => "Fire Sale";
        public override string description => $"(Single Use) For 1 order, every food item costs ${ingredientOverrideCost}";
        public override string modUsedMessage => $"Food items cost ${ingredientOverrideCost}!";

        public override void ResetMod()
        {
            _isClicked = true;
            EconomyManager.Instance.UpdateShiftEconomy(GameManager.Instance.shiftNum);
        }

        public override ModType modType => ModType.Clickable;

        public override float _GetBurgerCostOverride()
        {
            return _isClicked? ingredientOverrideCost : base._GetBurgerCostOverride(); 
        }

        public override float _GetFriesCostOverride()
        {
            return _isClicked ? ingredientOverrideCost : base._GetFriesCostOverride();
        }
        public override float _GetSodaCostOverride()
        {
            return _isClicked ? ingredientOverrideCost : base._GetSodaCostOverride();
        }     
        public override void ProcessOrder(Order order)
        {

        }
        public void onModClick()
        {
            _isClicked = true;
            EconomyManager.Instance.UpdateShiftEconomy(GameManager.Instance.shiftNum);

        }





    }
}