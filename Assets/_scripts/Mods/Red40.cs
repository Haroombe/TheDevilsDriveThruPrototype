using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets._scripts.OrderAlgorithm;


namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/Red40")]

    public class Red40 : Mod
    {
        public override int InitialUses => -10;

        public float weightReductionPercent = 0.30f;
        public bool isSet = false;
        public override string ModName => "Red40";
        public override string description => $"Sodas appear {weightReductionPercent*100}% less permanently";
        public override string modUsedMessage => $"Applied!";
        private float originalMaxPCT;
        private float originalMinPCT;


        public override void ResetMod()
        {
            isSet = false;

            OrderManager.Instance.ItemPrioritiesWeights[1].MaxPct = originalMaxPCT;
            OrderManager.Instance.ItemPrioritiesWeights[1].MinPct = originalMinPCT;
        }

        public override ModType modType => ModType.Permanent;


        public override void ProcessOrder(Order order)
        {
            if (!isSet) {
                originalMaxPCT = OrderManager.Instance.ItemPrioritiesWeights[1].MaxPct;
                originalMinPCT = OrderManager.Instance.ItemPrioritiesWeights[1].MinPct;
                OrderManager.Instance.ItemPrioritiesWeights[1].MaxPct *= (1-weightReductionPercent);
                OrderManager.Instance.ItemPrioritiesWeights[1].MinPct *= (1 - weightReductionPercent);
                Debug.Log($"{ModName}: set soda weights {OrderManager.Instance.ItemPrioritiesWeights[1].MinPct}-{OrderManager.Instance.ItemPrioritiesWeights[1].MaxPct}");
                isSet = true;
            }

        }

    }
}