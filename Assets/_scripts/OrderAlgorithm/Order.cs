using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public int burgerOrderAmount;
    public int friesOrderAmount;
    public int sodaOrderAmount;
    public int totalVolumeOrdered;

    // --- Financial Data ---
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

    public float FulfilledTime
    {
        get { return fulfilledTime; }
        set { fulfilledTime = value; }
    }

    public Order(int burgers, int fries, int sodas, int total_volume, int ShiftNum)
    {
        shiftNum = ShiftNum;
        burgerOrderAmount = burgers;
        friesOrderAmount = fries;
        sodaOrderAmount = sodas;
        totalVolumeOrdered = total_volume;

        // **NEW: Calculate and cache the fixed bulk amounts**
        CalculateBulkBuyAmounts(burgers, fries, sodas);

        CalculateFinancials();
    }

    // **NEW METHOD: Calculates the fixed 10% batch size (minimum 1)**
    private void CalculateBulkBuyAmounts(int burgers, int fries, int sodas)
    {
        // 10% of the ordered amount, rounded up, with a minimum of 1
        BurgerBulkBuyAmount = Mathf.Max(1, Mathf.CeilToInt(burgers * 0.10f));
        FriesBulkBuyAmount = Mathf.Max(1, Mathf.CeilToInt(fries * 0.10f));
        SodaBulkBuyAmount = Mathf.Max(1, Mathf.CeilToInt(sodas * 0.10f));
    }

    public float CalculatePayout()
    {
        // ... (Existing implementation for CalculatePayout, which updates TotalPayout, TotalCost, NetProfit)
        // Ensure this method is setting TotalPayout, TotalCost, and NetProfit
        // using EconomyManager.Instance.Cur...Price and EconomyManager.Instance.Cur...Cost

        // NOTE: The previous refactor placed this logic in CalculateFinancials(). 
        // Ensure only one method (e.g., CalculateFinancials) handles the financial calculation.
        return TotalPayout; // Return the final TotalPayout/Revenue
    }

    public void CalculateFinancials()
    {
        // 1. Calculate Revenue (Payout)
        float burgerRevenue = EconomyManager.Instance.CurBurgerPrice * burgerOrderAmount;
        float friesRevenue = EconomyManager.Instance.CurFriesPrice * friesOrderAmount;
        float sodaRevenue = EconomyManager.Instance.CurSodaPrice * sodaOrderAmount;
        float flatBonus = GameManager.Instance.flatProfitPerOrder;

        TotalPayout = burgerRevenue + friesRevenue + sodaRevenue;
        Debug.Log($"Order Payout Calculation: Burgers(${burgerRevenue:F2}) + Fries(${friesRevenue:F2}) + Sodas(${sodaRevenue:F2}) + Bonus(${flatBonus * shiftNum:F2}) = TotalPayout(${TotalPayout:F2})");

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