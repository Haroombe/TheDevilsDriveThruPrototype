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
    public abstract string modUsedMessage { get; }


    public abstract int InitialUses { get; }
    public int UsesRemaining { get; protected set; }
    public bool isExpired { get; protected set; }

    public abstract ModType modType { get; }

    public virtual void Initialize()
    {
        UsesRemaining = InitialUses;
        isExpired = false;
    }

    public abstract void ResetMod();

    public virtual bool TryUseAbility()
    {
        if (modType == ModType.Permanent)
            return true;

        if (UsesRemaining > 0)
        {
            UsesRemaining--;

            if (UsesRemaining <= 0)
            {
                isExpired = true;
            }

            return true; // Use succeeded!
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

    public virtual int GetSodaInventoryWeightMultiplier() { return 1; }
    public virtual int GetBurgerInventoryWeightMultiplier() { return 1; }
    public virtual int GetFriesInventoryWeightMultiplier() { return 1; }






    public abstract void ProcessOrder(Order order);
}