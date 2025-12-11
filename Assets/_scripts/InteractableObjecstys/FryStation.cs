using UnityEngine;

public class FryStation : Interactable
{
    private string _name = "Fries";

    public override string InteractableName
    {
        get { return _name; }
    }
    public override bool canBuy => true;

    /// <summary>
    /// The default interaction remains the Bulk Buy (10% batch).
    /// </summary>
    public override void Interact()
    {
        GameManager.Instance.BuyFries(1);
    }


    public override void BulkInteract()
    {
        int amountToBuy = GetOrderBulkAmount();
        GameManager.Instance.BuyFries(amountToBuy);

    }

    /// <summary>
    /// Displays dialogue showing both the Bulk Buy option (default) 
    /// and the Single Unit Buy option.
    /// </summary>
    public override string getDialogue()
    {
        // --- Option 1: Bulk Buy (The default Interact action) ---
        int bulkAmount = GetOrderBulkAmount();
        float unitCost = EconomyManager.Instance.CurFriesCost;
        float bulkCost = unitCost * bulkAmount;

        string bulkOption = $"'b' Buy {bulkAmount} Fries (-${bulkCost:F2})";

        // --- Option 2: Single Unit Buy (Optional action, needs separate input) ---
        float singleCost = unitCost;
        string singleOption = $"Buy 1 Fry (-${singleCost:F2})";
        if (bulkAmount <= 1)
        {
            // If bulk amount is 1 or less, only show single option
            return "Buy Fries";
        }
        // Combine into a single, two-line dialogue prompt
        // NOTE: Your UI/Input system needs to handle [E] and [1] separately.
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
        return GameManager.Instance.currentOrder.FriesBulkBuyAmount;
    }
}