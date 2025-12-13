using System.Collections;
using UnityEngine;

namespace Assets._scripts.Mods
{
    public class ImBulking : Mod
    {

        private float bulkDiscountfl = 0.25f;
        public override int InitialUses => GameManager.Instance.OrdersPerShift;

        public override string ModName => "I'm Bulking";
        public override string description => $"Buying in Bulk gives you a {(int)(bulkDiscountfl*100)}% discount!";
        public override string modUsedMessage => $"Bulk discount {(int)(bulkDiscountfl * 100)}% applied";


        public override ModType modType => ModType.Permanent;

        public override float GetBulkPriceMultiplier()
        {
            return 1 - bulkDiscountfl;
        }
        public override void ProcessOrder(Order order)
        {

        }



    }
}