using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    public class HighRoller : Mod
    {

        private int volumeMult = 2;
        private int paymentMult = 5;
        public override int InitialUses => GameManager.Instance.OrdersPerShift;

        public override string ModName => "High Roller";
        public override string description => $"For 1 shift, The order size is '{volumeMult}x', but the payouts are '{paymentMult}x'";
        public override string modUsedMessage => $"'{volumeMult}x' order size, but '{paymentMult}x' payouts!";

        public override void ResetMod()
        {
            
        }

        public override ModType modType => ModType.Finite;

        public override float GetVolumeMultiplier()
        {
            return volumeMult;
        }

        public override float GetPayoutMultiplier()
        {
            return paymentMult;
        }
        public override void ProcessOrder(Order order)
        {

        }



    }
    }