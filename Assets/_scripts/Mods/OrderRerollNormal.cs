using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/OrderRerollNormal")]

    public class OrderRerollNormal : Mod
    {

        public float volumeReduction = 0.30f;
        //public textMeshPro
        public override int InitialUses => GameManager.Instance.OrdersPerShift;
        public override string ModName => "Basic Order Reroll";
        public override string description => $"Reroll the current order with {volumeReduction*100}% reduction in size (Stackable). Only usable {GameManager.Instance.OrdersPerShift} times.";

        public override string modUsedMessage => $"Rerolled current order, ({UsesRemaining}/{InitialUses} uses left (Debug:Volume={GameManager.Instance.currentOrder.totalVolumeOrdered}))";
        public override ModType modType => ModType.Clickable;

        public override void ProcessOrder(Order order)
        {
 
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

        public override void ResetMod()
        { // game manager resets the order

        }
        public bool onModClick()
        {
            if (!this.ConsumeUsage())
            {

                return false;
            }
            setUsesTextField();

            Order rerolledOrder = GameManager.Instance.CreateOrder();

            GameManager.Instance.ReplaceCurrentOrder(
                fries: (int)Mathf.Round(rerolledOrder.friesOrderAmount * (1-volumeReduction)),
                sodas: (int)Mathf.Round(rerolledOrder.sodaOrderAmount * (1-volumeReduction)),
                burgers: (int)Mathf.Round(rerolledOrder.burgerOrderAmount * (1-volumeReduction))
                );
            FadingMessage.Instance.ShowMessage($"{modUsedMessage}", true, .6f, 1.7f);
            return true;
            //maybe add click notifs
        }

    }
}