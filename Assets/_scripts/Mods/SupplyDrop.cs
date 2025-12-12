using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    public class SupplyDrop : Mod
    {

        private int supplyDropMult = 4;

        public override int InitialUses => 1;
        public override string ModName => "Supply Drop";
        public override string description => $"Add '{supplyDropMult}x' your the current order to your inventory.";
        public override string modUsedMessage => $"Added '{supplyDropMult}x' current order to inventory!";

        public override ModType modType => ModType.Finite;

        public override void ProcessOrder(Order order)
        {

            GameManager.Instance.addAmount(
                ref GameManager.Instance.playerFries,
                GameManager.Instance.currentOrder.friesOrderAmount*supplyDropMult
                );

            GameManager.Instance.addAmount(
                ref GameManager.Instance.playerBurgers,
                GameManager.Instance.currentOrder.burgerOrderAmount * supplyDropMult
                );
            GameManager.Instance.addAmount(
                ref GameManager.Instance.playerSoda,
                GameManager.Instance.currentOrder.sodaOrderAmount * supplyDropMult
                );
            GameManager.Instance.UpdateAllInventoryUI();

            this.isExpired = true;

        }



    }
}