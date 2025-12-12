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

        public override bool ProcessOrder(Order order)
        {
            
        }

    }
}