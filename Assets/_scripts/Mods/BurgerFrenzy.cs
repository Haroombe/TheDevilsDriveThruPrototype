using Assets._scripts.OrderAlgorithm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/BurgerFrenzy")]
    public class BurgerFrenzy : Mod
    {
        public override int InitialUses => GameManager.Instance.OrdersPerShift;

        public float burgerweightordervolume = .80f;
        public float burgercostDiscount = .80f;
        public float burgerpayoutIncrease = .80f;
        private bool setWeight = false;

        public override string ModName => "Burger Frenzy";
        public override string description => $"For 1 shift, burgers will be {burgerweightordervolume*100}% of orders, burger cost discounted {burgercostDiscount * 100}%, and burger payout will be {burgerpayoutIncrease * 100}% more!";
        public override string modUsedMessage => $"Applied!";


        public override ModType modType => ModType.Finite;


        public override float _GetBurgerCostMultiplier()
        {
            return 1 - burgercostDiscount;
        }

        public override float _GetBurgerPriceMultiplier()
        {
            return 1 - burgerpayoutIncrease;

        }

        // TODO 
        // ADD how to handle this, change it back after shift
        public override void ProcessOrder(Order order)
        {
            if (GameManager.Instance.getOrderNum() == GameManager.Instance.OrdersPerShift) // reset
            {
                ResetMod();
            }else if (setWeight == false)
            {
                OrderManager.Instance.ItemPrioritiesWeights[0].MaxPct = .8f;
                OrderManager.Instance.ItemPrioritiesWeights[0].MinPct= .8f;
                setWeight = true;

            }

        }


        /// <summary>
        /// Will be called 
        /// - If Shift ends
        /// - If player resets game
        /// No possibility of discarding without resetting 
        /// </summary>
        public override void ResetMod()
        {
            Debug.Log("Reset Burger frenzy burger weights");
            setWeight = false;
            OrderManager.Instance.ItemPrioritiesWeights[0] = OrderManager.Instance.OrderGenerator._defaultRules[0];
            OrderManager.Instance.OrderGenerator.UpdateAllocationRules(OrderManager.Instance.OrderGenerator._defaultRules);
        }
    }
}