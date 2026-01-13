using UnityEngine;

public class SodaMachine : Interactable
{
    private string _name = "Soda";
    public override bool canBuy => true;

    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {
        GameManager.Instance.BuySoda(1);
    }
    public override void BulkInteract()
    {
        int amountToBuy = GetOrderBulkAmount();
        GameManager.Instance.BuySoda(amountToBuy, true);

    }
    public override string getDialogue()
    {
        int bulkAmount = GetOrderBulkAmount();
        float unitCost = EconomyManager.Instance.CurSodaCost;
        float bulkCost = unitCost * bulkAmount * ModManager.Instance.GetTotalBulkPriceMultiplier();

        string bulkOption = $"'c' to Buy {bulkAmount} Sodas (-${bulkCost:N2})";

        // --- Option 2: Single Unit Buy (Optional action, needs separate input) ---
        float singleCost = unitCost;
        string singleOption = $"Buy 1 Soda (-${singleCost:N2})";
        if (bulkAmount <= 1)
        {
            // If bulk amount is 1 or less, only show single option
            return "Buy Soda";
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
        return GameManager.Instance.currentOrder.SodaBulkBuyAmount;
    }
}

