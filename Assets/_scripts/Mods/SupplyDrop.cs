using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/SupplyDrop")]

    public class SupplyDrop : Mod
    {

        public int supplyDropMult = 4;

        public override int InitialUses => 1;
        public override string ModName => "Supply Drop";
        public override string description => $"Add '{supplyDropMult}x' your the current order size to your inventory.";
        public override string modUsedMessage => $"Added '{supplyDropMult}x' current order size to inventory!";

        public override ModType modType => ModType.Finite;
        public override void ResetMod()
        {
            // no need to reset since inventory is managed by game manager
        }
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