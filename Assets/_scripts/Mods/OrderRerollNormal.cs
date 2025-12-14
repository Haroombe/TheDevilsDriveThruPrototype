using System.Collections;

using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/OrderRerollNormal")]

    public class OrderRerollNormal : Mod
    {

        public float volumeReduction = 0.30f;

        public override int InitialUses => GameManager.Instance.OrdersPerShift;
        public override string ModName => "Basic Order Reroll";
        public override string description => $"Reroll the current order with {volumeReduction*100}% reduction in size (Stackable). Only usable {GameManager.Instance.OrdersPerShift} times.";

        public override string modUsedMessage => $"Rerolled current order, ({UsesRemaining}/{InitialUses} uses left (Debug:Volume={GameManager.Instance.currentOrder.totalVolumeOrdered}))";
        public override ModType modType => ModType.Clickable;

        public override void ProcessOrder(Order order)
        {
 
        }

        public override void ResetMod()
        { // game manager resets the order

        }
        public void OnRerollOrderPressed()
        {
            if (!this.ConsumeUsage())
            {
                return;
            }
            Order rerolledOrder = GameManager.Instance.CreateOrder();

            GameManager.Instance.ReplaceCurrentOrder(
                fries: (int)Mathf.Round(rerolledOrder.friesOrderAmount * (1-volumeReduction)),
                sodas: (int)Mathf.Round(rerolledOrder.sodaOrderAmount * (1-volumeReduction)),
                burgers: (int)Mathf.Round(rerolledOrder.burgerOrderAmount * (1-volumeReduction))
                );
            //maybe add click notifs
        }

    }
}