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
    // --- STATE & METADATA ---
    public abstract string ModName { get; }
    public abstract string description { get; }

    [Header("Uses Per Shift/Run")]
    [SerializeField]
    public int initialDuration = 1;
    public int UsesRemaining { get; protected set; }
    public bool isExpired { get; protected set; }

    public abstract ModType modType { get; }

    // Use a virtual method so the base class handles the counter
    public virtual void Initialize()
    {
        UsesRemaining = initialDuration;
        isExpired = false;
    }

    // CORE PLAYER-ACTION HOOK: Called when the player clicks the Reroll button.
    // Returns true if the use was consumed successfully.
public virtual bool TryUseAbility()
    {
        if (UsesRemaining > 0)
        {
            UsesRemaining--;
            return true;
        }
        // If the ability use fails, the mod is now fully exhausted.
        // It's often safer to set the expiration flag here.
        isExpired = true; 
        return false;
    }

    // --- PIPELINE INJECTION METHODS (Defaults to 1.0f) ---
    public virtual float GetVolumeMultiplier() { return 1.0f; }
    public virtual float GetPayoutMultiplier() { return 1.0f; }

    public virtual float GetBurgerCostMultiplier() { return 1.0f; }
    public virtual float GetBurgerPriceMultiplier() { return 1.0f; }

    public virtual float GetFriesPriceMultiplier() { return 1.0f; }
    public virtual float GetFriesCostMultiplier() { return 1.0f; }
    public virtual float GetSodeCostMultiplier() { return 1.0f; }
    public virtual float GetSodaPriceMultiplier() { return 1.0f; }

    // Add other pipeline methods as needed...

    // --- PER-ORDER LOGIC & CLEANUP ---
    // Called for every order. Returns true if the mod is finished/expired.
    public abstract bool ProcessOrder(Order order);
}