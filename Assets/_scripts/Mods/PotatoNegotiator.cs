using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/PotatoNegotiator")]

    public class PotatoNegotiator : Mod
    {

        public float costMult = .5f;

        public override int InitialUses => -10;

        public override string ModName => "Potato Negotiator";
        public override string description => $"The cost to make fries is reduced {(int)(costMult*100)}%";
        public override string modUsedMessage => $"{(int)(costMult * 100)}% reduction on the cost of production for fries active!";
        private bool isUsed = false;

        public override void ResetMod()
        {
            EconomyManager.Instance.UpdateShiftEconomy(GameManager.Instance.shiftNum);
            isUsed = false;
        }

        public override ModType modType => ModType.Permanent;

        public override float _GetFriesCostMultiplier()
        {
            if (isUsed)
            {
                return costMult;
            }
            return base._GetFriesCostMultiplier();
        }



        public override void ProcessOrder(Order order)
        {
            if (!isUsed)
            {
                isUsed = true;
                Debug.Log($"{ModName} applied fries csot multiplier {costMult }");
                EconomyManager.Instance.UpdateShiftEconomy(GameManager.Instance.shiftNum);
            }

        }



    }
}