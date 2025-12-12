using System.Collections;

using UnityEngine;

namespace Assets._scripts.Mods
{
    public class OrderRerollNormal : Mod
    {

        public int volumeReductionPercent = 30;
        public override string ModName => "Order Reroll";
        public override string description => $"Reroll the current order with {volumeReductionPercent}% reduction in volume (Stackable). Only usable {GameManager.Instance.OrdersPerShift} times.";

        public override ModType modType => ModType.Clickable;

        public override void ProcessOrder(Order order)
        {
 
        }

        public void OnRerollOrderPressed()
        {
            if (!this.TryUseAbility())
            {
                return;
            }
            GameManager.Instance.ReplaceCurrentOrder(
                fries: (int)Mathf.Round(GameManager.Instance.currentOrder.friesOrderAmount * (volumeReductionPercent / 100)),
                sodas: (int)Mathf.Round(GameManager.Instance.currentOrder.sodaOrderAmount * (volumeReductionPercent / 100)),
                burgers: (int)Mathf.Round(GameManager.Instance.currentOrder.burgerOrderAmount * (volumeReductionPercent / 100))
                );
        }

    }
}