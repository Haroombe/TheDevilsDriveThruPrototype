using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public int burgerOrderAmount;
    public int friesOrderAmount;
    public int sodaOrderAmount;
    public int totalVolumeOrdered;

    // --- Financial Data ---
    public float TotalPayoutNonMod { get; private set; }
    public float TotalPayout { get; private set; }
    public float TotalCost { get; private set; }
    public float NetProfit { get; private set; }

    // --- New Bulk Buy Properties ---
    public int BurgerBulkBuyAmount { get; private set; }
    public int FriesBulkBuyAmount { get; private set; }
    public int SodaBulkBuyAmount { get; private set; }

    public float fulfilledTime = 0f;
    public string fulfilledTimeString;
    public int shiftNum;

    public float orderPayoutMultiplier = 1f;
    public float FulfilledTime
    {
        get { return fulfilledTime; }
        set { fulfilledTime = value; }
    }

    private float bulkBuyPercent = .25f;

    public Order(int burgers, int fries, int sodas, int total_volume, int ShiftNum)
    {
        shiftNum = ShiftNum;
        burgerOrderAmount = burgers;
        friesOrderAmount = fries;
        sodaOrderAmount = sodas;
        totalVolumeOrdered = total_volume;

        // **NEW: Calculate and cache the fixed bulk amounts**

        orderPayoutMultiplier = ModManager.Instance.GetTotalPayoutMultiplier();
        
        CalculateBulkBuyAmounts(burgers, fries, sodas);

        CalculateFinancials();
    }

    // **NEW METHOD: Calculates the fixed 10% batch size (minimum 1)**
    private void CalculateBulkBuyAmounts(int burgers, int fries, int sodas)
    {
        // 10% of the ordered amount, rounded up, with a minimum of 1
        BurgerBulkBuyAmount = Mathf.Max(1, Mathf.CeilToInt(burgers * bulkBuyPercent));
        FriesBulkBuyAmount = Mathf.Max(1, Mathf.CeilToInt(fries * bulkBuyPercent));
        SodaBulkBuyAmount = Mathf.Max(1, Mathf.CeilToInt(sodas * bulkBuyPercent));
    }



    public void CalculateFinancials()
    {
        // 1. Calculate Revenue (Payout)
        float burgerRevenue = EconomyManager.Instance.CurBurgerPrice * burgerOrderAmount;
        float friesRevenue = EconomyManager.Instance.CurFriesPrice * friesOrderAmount;
        float sodaRevenue = EconomyManager.Instance.CurSodaPrice * sodaOrderAmount;
        float flatBonus = GameManager.Instance.flatProfitPerOrder;

        TotalPayoutNonMod = burgerRevenue + friesRevenue + sodaRevenue;
        TotalPayout = TotalPayoutNonMod * orderPayoutMultiplier;
        Debug.Log($"Order Payout Calculation: Burgers(${burgerRevenue:N2}) + Fries(${friesRevenue:N2}) + Sodas(${sodaRevenue:N2}) + Bonus(${flatBonus * shiftNum:N2}) = TotalPayout(${TotalPayout:N2})");

        // 2. Calculate Cost
        float burgerCost = EconomyManager.Instance.CurBurgerCost * burgerOrderAmount;
        float friesCost = EconomyManager.Instance.CurFriesCost * friesOrderAmount;
        float sodaCost = EconomyManager.Instance.CurSodaCost * sodaOrderAmount;

        TotalCost = burgerCost + friesCost + sodaCost;

        // 3. Calculate Net Profit
        NetProfit = TotalPayout - TotalCost + (flatBonus * shiftNum);
    }


    public float getPayout()
    {
        return TotalPayout;
    }

    public void CalculateFulfilledTimeString()
    {
        if (fulfilledTime == 0f) return;

        int minutes = Mathf.FloorToInt(fulfilledTime / 60f);
        int seconds = Mathf.FloorToInt(fulfilledTime % 60f);
        fulfilledTimeString = $"{minutes:00}:{seconds:00}";
    }

    public string getFulfilledTimeString()
    {
        return fulfilledTimeString;
    }
}