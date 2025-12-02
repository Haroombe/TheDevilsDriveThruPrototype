using System.Collections.Generic;

using UnityEngine;

public class Order 
{
    public int burgerOrderAmount;
    public int friesOrderAmount;
    public int sodaOrderAmount;
    private float payout;

    public Order(int burgers, int fries, int sodas)
    {
        burgerOrderAmount = burgers;
        friesOrderAmount = fries;
        sodaOrderAmount = sodas;
        CalculatePayout();
    }

    public float CalculatePayout()
    {
        float curBurgerProfit = GameManager.Instance.BurgerProfit * burgerOrderAmount;
        float curFriesProfit = GameManager.Instance.FriesProfit * friesOrderAmount;
        float curSodaProfit = GameManager.Instance.SodaProfit * sodaOrderAmount;
        payout = curBurgerProfit + curFriesProfit + curSodaProfit + GameManager.Instance.flatProfitPerOrder;

        return payout;
    }

    public float getPayout()
    {
        return payout;
    }
}
