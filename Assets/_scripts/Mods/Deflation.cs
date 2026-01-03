using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/Deflation")]
    public class Deflation : Mod
    {
        public float ItemDeflationMult = 0.30f;
        public override int InitialUses => GameManager.Instance.OrdersPerShift * 2;

        public override string ModName => "Deflation";
        public override string description => $"For 2 shifts, the cost of all items is reduced by {ItemDeflationMult*100}%";
        public override string modUsedMessage => $"{ItemDeflationMult}% item cost deflation is active!";


        public override ModType modType => ModType.Finite;

        public override void ResetMod()
        {
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

        }

    }
}