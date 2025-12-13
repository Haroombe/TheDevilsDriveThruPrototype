using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    public class JustInTime : Mod
    {
        public override int InitialUses => GameManager.Instance.OrdersPerShift;
        private float volumeReducMult = 0.90f;
        public override string ModName => "Just In Time";
        public override string description => $"Order Volume for the current shift is reduced by {volumeReducMult*100}%";
        public override string modUsedMessage => $"Order Volume decreased by {volumeReducMult*100}%!";

        public override void ResetMod()
        {
            
        }

        public override ModType modType => ModType.Finite;

        public override float GetVolumeMultiplier()
        {
            return 1-volumeReducMult;
        }


        public override void ProcessOrder(Order order)
        {

        }

    }
}