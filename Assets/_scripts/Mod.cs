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

    public virtual bool isClicked()
    {
        return false; 
    }
    public abstract ModType modType { get; }

    public virtual void Initialize()
    {
        UsesRemaining = InitialUses;
        isExpired = false;
    }

    public abstract void ResetMod();

    public virtual bool ConsumeUsage()
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

    public float GetVolumeMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1.0f;
        return _GetVolumeMultiplier();
    }

    public float GetPayoutMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1.0f;
        return _GetPayoutMultiplier();
    }

    public float GetBurgerCostMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1.0f;
        return _GetBurgerCostMultiplier();
    }

    // ... continue this pattern for all other standard float multipliers ...

    public float GetBurgerPriceMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1.0f;
        return _GetBurgerPriceMultiplier();
    }

    public float GetFriesPriceMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1.0f;
        return _GetFriesPriceMultiplier();
    }
    public float GetFriesCostMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1.0f;
        return _GetFriesCostMultiplier();
    }
    public float GetSodaCostMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1.0f;
        return _GetSodaCostMultiplier();
    }
    public float GetSodaPriceMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1.0f;
        return _GetSodaPriceMultiplier();
    }
    public float GetBulkPriceMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1.0f;
        return _GetBulkPriceMultiplier();
    }

    public float GetBurgerCostOverride()
    {
        if (ModManager.Instance.isResetting) return ModManager.NO_OVERRIDE;
        return _GetBurgerCostOverride();
    }

    public float GetFriesCostOverride()
    {
        if (ModManager.Instance.isResetting) return ModManager.NO_OVERRIDE;
        return _GetFriesCostOverride();
    }

    public float GetSodaCostOverride()
    {
        if (ModManager.Instance.isResetting) return ModManager.NO_OVERRIDE;
        return _GetSodaCostOverride();
    }


    // --- INTEGER MULTIPLIERS (Returns 1 if resetting) ---

    public int GetSodaInventoryWeightMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1;
        return _GetSodaInventoryWeightMultiplier();
    }

    public int GetBurgerInventoryWeightMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1;
        return _GetBurgerInventoryWeightMultiplier();
    }

    public int GetFriesInventoryWeightMultiplier()
    {
        if (ModManager.Instance.isResetting) return 1;
        return _GetFriesInventoryWeightMultiplier();
    }
    public virtual float _GetVolumeMultiplier() { return 1.0f; }
    public virtual float _GetPayoutMultiplier() { return 1.0f; }

    public virtual float _GetBurgerCostMultiplier() { return 1.0f; }
    public virtual float _GetBurgerPriceMultiplier() { return 1.0f; }

    public virtual float _GetFriesPriceMultiplier() { return 1.0f; }
    public virtual float _GetFriesCostMultiplier() { return 1.0f; }
    public virtual float _GetSodaCostMultiplier() { return 1.0f; }
    public virtual float _GetSodaPriceMultiplier() { return 1.0f; }
    public virtual float _GetBulkPriceMultiplier() { return 1.0f; }

    public virtual float _GetBurgerCostOverride() { return ModManager.NO_OVERRIDE; }
    public virtual float _GetFriesCostOverride() { return ModManager.NO_OVERRIDE; }
    public virtual float _GetSodaCostOverride() { return ModManager.NO_OVERRIDE; }

    public virtual int _GetSodaInventoryWeightMultiplier() { return 1; }
    public virtual int _GetBurgerInventoryWeightMultiplier() { return 1; }
    public virtual int _GetFriesInventoryWeightMultiplier() { return 1; }






    public abstract void ProcessOrder(Order order);
}