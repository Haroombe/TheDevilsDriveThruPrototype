using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/FireSale")]

    public class FireSale : Mod
    {
        public float foodItemReductionDecimal = 0.90f;
        public override int InitialUses => 1;
        private bool _isClicked = false;

        public override bool isClicked()
        {
            return _isClicked;
        }
        private TextMeshPro UsesRemainingTextField;
        private void getUsesTextField()
        {
            UsesRemainingTextField = ModManager.Instance.getClickModUsesTextField(this.ModName);
        }
        public override void Initialize()
        {
            base.Initialize();
            getUsesTextField();
            setUsesTextField();
        }
        public void setUsesTextField()
        {
            UsesRemainingTextField.text = $"Uses: ({UsesRemaining}/{InitialUses})";
        }
        public override string ModName => "Fire Sale";
        public override string description => $"(Single Use) For 1 order, the cost of all food item is reduced %{foodItemReductionDecimal*100}";
        public override string modUsedMessage => $"Food items cost %{foodItemReductionDecimal * 100} less!";

        public override void ResetMod()
        {
            _isClicked = false;
            EconomyManager.Instance.UpdateShiftEconomy(GameManager.Instance.shiftNum);
        }

        public override ModType modType => ModType.Clickable;

        public override float _GetBurgerCostMultiplier()
        {
            return _isClicked? foodItemReductionDecimal : base._GetBurgerCostMultiplier(); 
        }

        public override float _GetFriesCostMultiplier()
        {
            return _isClicked ? foodItemReductionDecimal : base._GetFriesCostMultiplier();
        }
        public override float _GetSodaCostMultiplier()
        {
            return _isClicked ? foodItemReductionDecimal : base._GetSodaCostMultiplier();
        }     
        public override void ProcessOrder(Order order)
        {

        }
        public bool onModClick()
        {
            if (!this.ConsumeUsage())
            {
                return false;
            }
            setUsesTextField();

            _isClicked = true;
            
            EconomyManager.Instance.UpdateShiftEconomy(GameManager.Instance.shiftNum);
            FadingMessage.Instance.ShowMessage($"{modUsedMessage}", true, .6f, 1.7f);
            return true;  
        }





    }
}