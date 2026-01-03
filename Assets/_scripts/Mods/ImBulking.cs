using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    [CreateAssetMenu(fileName = "NewMod_", menuName = "Game/Mod/ImBulking")]

    public class ImBulking : Mod
    {

        public float bulkDiscountfl = 0.35f;
        public override int InitialUses => GameManager.Instance.OrdersPerShift;

        public override string ModName => "I'm Bulking";
        public override string description => $"Buying in Bulk gives you a {(int)(bulkDiscountfl*100)}% discount!";
        public override string modUsedMessage => $"Bulk discount {(int)(bulkDiscountfl * 100)}% applied";

        public override void ResetMod()
        {
            
        }

        public override ModType modType => ModType.Permanent;

        public override float _GetBulkPriceMultiplier()
        {
            return 1 - bulkDiscountfl;
        }
        public override void ProcessOrder(Order order)
        {

        }



    }
}