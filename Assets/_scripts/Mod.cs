using UnityEngine;

// Use ScriptableObject for asset creation and management

public enum ModType
{
    Permanent,
    Finite,
    Clickable
}
public abstract class Mod : ScriptableObject
{
    public abstract string ModName { get; }
    public abstract string description { get; }

    [Header("Uses Per Shift/Run")]
    [SerializeField]
    public int initialDuration = 1;
    public int UsesRemaining { get; protected set; }
    public bool isExpired { get; protected set; }

    public abstract ModType modType { get; }

    public virtual void Initialize()
    {
        UsesRemaining = initialDuration;
        isExpired = false;
    }

    public virtual bool TryUseAbility()
    {
        if (modType == ModType.Permanent)
            return true;
        if (UsesRemaining > 0)
        {
            UsesRemaining--;
            return true;
        }
        isExpired = true; 
        return false;
    }

    public virtual float GetVolumeMultiplier() { return 1.0f; }
    public virtual float GetPayoutMultiplier() { return 1.0f; }

    public virtual float GetBurgerCostMultiplier() { return 1.0f; }
    public virtual float GetBurgerPriceMultiplier() { return 1.0f; }

    public virtual float GetFriesPriceMultiplier() { return 1.0f; }
    public virtual float GetFriesCostMultiplier() { return 1.0f; }
    public virtual float GetSodaCostMultiplier() { return 1.0f; }
    public virtual float GetSodaPriceMultiplier() { return 1.0f; }
    public virtual float GetBulkPriceMultiplier() { return 1.0f; }

    public virtual float GetBurgerCostOverride() { return -1.0f; }
    public virtual float GetFriesCostOverride() { return -1.0f; }
    public virtual float GetSodaCostOverride() { return -1.0f; }




    // Add other pipeline methods as needed...

    // --- PER-ORDER LOGIC & CLEANUP ---
    // Called for every order. Returns true if the mod is finished/expired.
    public abstract void ProcessOrder(Order order);
}