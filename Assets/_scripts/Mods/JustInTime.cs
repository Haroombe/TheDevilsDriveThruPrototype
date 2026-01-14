using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/JustInTime")]

    public class JustInTime : Mod
    {
        public override int InitialUses => GameManager.Instance.OrdersPerShift*2;
        public float volumeReducMult = 0.80f;
        public override string ModName => "Just In Time";
        public override string description => $"Order Volume for the next 2 shifts is reduced by {volumeReducMult*100}%";
        public override string modUsedMessage => $"Order Volume decreased by {volumeReducMult*100}%!";

        public override void ResetMod()
        {
            
        }

        public override ModType modType => ModType.Finite;

        public override float _GetVolumeMultiplier()
        {
            return 1-volumeReducMult;
        }


        public override void ProcessOrder(Order order)
        {

        }

    }
}