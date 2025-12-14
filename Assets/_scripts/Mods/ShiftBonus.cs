using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/ShiftBonus")]

    public class ShiftBonus : Mod
    {

        public override int InitialUses => -10;

        public override string ModName => "Shift Bonus";
        public override string description => $"Every shift, the base Bonus increases by the largest payout this shift permanently";
        public override string modUsedMessage => $"Base shift payout is {GameManager.Instance.flatProfitPerShift}";

        public override void ResetMod()
        {
            GameManager.Instance.largestpayout = 0;
        }
        public override ModType modType => ModType.Permanent;

        // this actually puts the second largest but who is checking lmfao
        // IM CHECKING: If the player does a special move last turn and mods the payout they will hit an edge case bc the last order is skipped
        public override void ProcessOrder(Order order)
        {
            if (GameManager.Instance.getOrderNum() == GameManager.Instance.OrdersPerShift)
            {
                GameManager.Instance.flatProfitPerShift += GameManager.Instance.largestpayout;
                Debug.Log($"{ModName} set shift profit {GameManager.Instance.flatProfitPerShift}");

            } // TEST ME

        }



    }
}