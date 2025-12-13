using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets._scripts.OrderAlgorithm;


namespace Assets._scripts.Mods
{
    public class Red40 : Mod
    {
        public override int InitialUses => -10;

        public float weightReductionPercent = 0.30f;
        public bool isSet = false;
        public override string ModName => "Red40";
        public override string description => $"Sodas appear {weightReductionPercent*100}% less permanently";
        public override string modUsedMessage => $"Applied!";


        public override ModType modType => ModType.Permanent;


        public override void ProcessOrder(Order order)
        {
            if (!isSet) { 
                OrderManager.Instance.ItemPrioritiesWeights[1].MaxPct *= 1-weightReductionPercent;
                OrderManager.Instance.ItemPrioritiesWeights[1].MinPct *= 1 - weightReductionPercent;

            }

        }

    }
}