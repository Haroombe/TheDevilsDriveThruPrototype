using System.Collections;

using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/VipReroll")]

    public class VipReroll : Mod
    {

        public float payoutMult = 4f;
        private bool _isClicked = false;

        public override int InitialUses => 1;
        public override string ModName => "VIP Customer";
        public override string description => $"(Single Use) Reroll the current order with '{payoutMult}x' payout!";

        public override string modUsedMessage => $"Rerolled current order!)";
        public override ModType modType => ModType.Clickable;

        public override void ProcessOrder(Order order)
        {
 
        }

        public override void ResetMod()
        {
            _isClicked = false;

        }

        public override float _GetPayoutMultiplier()
        {
            return _isClicked ? payoutMult : base._GetPayoutMultiplier();

        }
        public bool onModClick()
        {
            //TODO QOL: SFX
            if (!this.ConsumeUsage())
            {
                return false;
            }
            _isClicked = true;
            GameManager.Instance.currentOrder = GameManager.Instance.CreateOrder();
            FadingMessage.Instance.ShowMessage($"{modUsedMessage}", true, .6f, 1.7f);
            return true;
        }



    }
}