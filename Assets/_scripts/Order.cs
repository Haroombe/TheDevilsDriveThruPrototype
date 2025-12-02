using System.Collections.Generic;

using UnityEngine;

public class Order 
{
    public int burgerOrderAmount;
    public int friesOrderAmount;
    public int sodaOrderAmount;
    private float payout;
    private float fulfilledTime = 0f;
    private string fulfilledTimeString;

    public float FulfilledTime
    {
        get
        {
            return fulfilledTime;
        }
        set
        {
            fulfilledTime = value;
        }
    }



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

    public void CalculateFulfilledTimeString()
    {
        if (fulfilledTime == 0f)
        {
            return;
        }
        int minutes = Mathf.FloorToInt(fulfilledTime / 60f);
        int seconds = Mathf.FloorToInt(fulfilledTime % 60f);
        fulfilledTimeString = $"{minutes:00}:{seconds:00}";
    }

    public string getFulfilledTimeString()
    {
        return fulfilledTimeString;
    }
}
