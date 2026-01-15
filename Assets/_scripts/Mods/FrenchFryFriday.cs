using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/FrenchFryFriday")]

    public class FrenchFryFriday : Mod
    {

        public float paymentMult = 3f;
        public float priceMult = .33f;

        public override int InitialUses => GameManager.Instance.OrdersPerShift;

        public override string ModName => "French Fries Friday";
        public override string description => $"For 1 shift, Fries selling price is '{paymentMult}x'";
        public override string modUsedMessage => $"'{paymentMult}x' selling price multiple on Fries!";
        private bool isUsed = false;

        public override void ResetMod()
        {
            EconomyManager.Instance.UpdateShiftEconomy(GameManager.Instance.shiftNum);
            isUsed = false;
        }

        public override ModType modType => ModType.Finite;

        public override float _GetFriesCostMultiplier()
        {
            if (isUsed)
            {
                return priceMult;
            }
            return base._GetFriesCostMultiplier();
        }

        public override float _GetFriesPriceMultiplier()
        {
            if (isUsed)
            {
                return 1 + paymentMult;
            }
            return base._GetFriesPriceMultiplier();
        }

        public override void ProcessOrder(Order order)
        {
            if (!isUsed)
            {
                isUsed = true;
                Debug.Log($"{ModName} applied fries payout multiplier {paymentMult}x");
                EconomyManager.Instance.UpdateShiftEconomy(GameManager.Instance.shiftNum);
            }

        }



    }
}