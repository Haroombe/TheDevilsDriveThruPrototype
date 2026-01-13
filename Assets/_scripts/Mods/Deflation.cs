using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/Deflation")]
    public class Deflation : Mod
    {
        public float ItemDeflationMult = 0.50f;
        public override int InitialUses => GameManager.Instance.OrdersPerShift * 2;

        public override string ModName => "Deflation";
        public override string description => $"For 2 shifts, the cost of all items is reduced by {ItemDeflationMult*100}%";
        public override string modUsedMessage => $"{ItemDeflationMult}% item cost deflation is active!";

        private bool isActive = false;

        public override ModType modType => ModType.Finite;

        public override void ResetMod()
        {
            isActive = false;
            EconomyManager.Instance.UpdateShiftEconomy(GameManager.Instance.shiftNum);
        }
        public override float _GetBurgerCostMultiplier()
        {
            return 1-ItemDeflationMult;
        }

        public override float _GetFriesCostMultiplier()
        {
            return 1 - ItemDeflationMult;
        }
        public override float _GetSodaCostMultiplier()
        {
            return 1 - ItemDeflationMult;
        }


        public override void ProcessOrder(Order order)
        {
            if (!isActive)
            {
                isActive = true;
                EconomyManager.Instance.UpdateShiftEconomy(GameManager.Instance.shiftNum);
            }

        }

    }
}