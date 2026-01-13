using UnityEngine;

public class GrillTop : Interactable
{
    private string _name = "Burger";
    public override bool canBuy => true;


    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {
        GameManager.Instance.BuyBurger();
    }
    public override void BulkInteract()
    {
        int amountToBuy = GetOrderBulkAmount();
        GameManager.Instance.BuyBurger(amountToBuy,true);

    }
    public override string getDialogue()
    {
        // --- Option 1: Bulk Buy (The default Interact action) ---
        int bulkAmount = GetOrderBulkAmount();
        float unitCost = EconomyManager.Instance.CurBurgerCost;
        float bulkCost = unitCost * bulkAmount * ModManager.Instance.GetTotalBulkPriceMultiplier();

        string bulkOption = $"'c' to Buy {bulkAmount} Burgers (-${bulkCost:N2})";

        // --- Option 2: Single Unit Buy (Optional action, needs separate input) ---
        float singleCost = unitCost;
        string singleOption = $"Buy 1 Burger (-${singleCost:N2})";

        // Combine into a single, two-line dialogue prompt
        // NOTE: Your UI/Input system needs to handle [E] and [1] separately.
        if (bulkAmount <= 1)
        {
            // If bulk amount is 1 or less, only show single option
            return "Buy Burger";
        }
        return $"{singleOption}\n{bulkOption}";
    }

    /// <summary>
    /// Helper to safely retrieve the fixed bulk amount from the current order.
    /// </summary>
    private int GetOrderBulkAmount()
    {
        if (GameManager.Instance.currentOrder == null)
        {
            // Default to buying 1 if no order is active
            return 1;
        }

        // Pull the fixed, pre-calculated bulk amount from the Order object
        return GameManager.Instance.currentOrder.BurgerBulkBuyAmount;
    }
}
