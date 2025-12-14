using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/SittingOnGold")]

    public class SittingOnGold : Mod
    {

        public int inventoryMultiplier = 5;

        public override int InitialUses => 1;
        public override string ModName => "Sitting on Gold";
        public override string description => $"Instantly '{inventoryMultiplier}x' your current inventory.";
        public override string modUsedMessage => $"{inventoryMultiplier}x'ed inventory! (fries:{GameManager.Instance.playerFries.ToString()}, burger:{GameManager.Instance.playerBurgers.ToString()}, soda:{GameManager.Instance.playerSoda.ToString()})";

        public override ModType modType => ModType.Finite;

        public override void ResetMod()
        {
                // no need to reset since inventory is managed by game manager
        }
        public override void ProcessOrder(Order order)
        {
            
            GameManager.Instance.addAmount(
                ref GameManager.Instance.playerFries,
                GameManager.Instance.playerFries * (inventoryMultiplier - 1)
                );

            GameManager.Instance.addAmount(
                ref GameManager.Instance.playerBurgers,
                GameManager.Instance.playerBurgers * (inventoryMultiplier - 1)
                ); 
            GameManager.Instance.addAmount(
                ref GameManager.Instance.playerSoda,
                GameManager.Instance.playerSoda * (inventoryMultiplier - 1)
                );
            GameManager.Instance.UpdateAllInventoryUI();

            this.isExpired = true;

        }



        }
}