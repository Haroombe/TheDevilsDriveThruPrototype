using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModManager : MonoBehaviour
{
    public static ModManager Instance { get; private set; }
    public List<Mod> activeMods = new List<Mod>();
    public List<Mod> allMods = new List<Mod>();

    [SerializeField] private Mod[] mods;

    public int seed = -1;
    [SerializeField] private const int PlayerModChoiceCount = 2;

    private List<List<Mod>> _modhistory = new List<List<Mod>>();

    private ModType[] autoApplyModTypes = new ModType[] {ModType.Permanent, ModType.Finite};
    public event Action<Mod> OnModAdded;
    public event Action<Mod> OnClickableModExpired;

    public event Action ResetClickableModsAction;


    public bool isResetting = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Mod mod in mods)
        {
            allMods.Add(mod);
        }
    }


    public void ResetMods()
    {
        isResetting = true;
        ResetClickableModsAction?.Invoke();
        _modhistory.Add(activeMods);
        foreach (Mod modInstance in activeMods)
        {
            modInstance.ResetMod();

            Destroy(modInstance);
        }

        activeMods = new List<Mod>(); //overwrite
        allMods = new List<Mod>(); //overwrite


        foreach (Mod modAsset in mods) // reset available mods list to include all mods
        {
            allMods.Add(modAsset);
        }

        isResetting = false;

        Debug.Log("Game state reset: Available mods repopulated from master templates.");
    }


    public List<Mod> ModChoicesForPlayer()
    {
        List<Mod> list = new List<Mod>();
        if (seed == -1)
        {
            Debug.LogWarning("Mod Manager seed not set. Will not generate Mod choices for player");
            return list;
        }

        if (allMods.Count == 0)
        {
            Debug.LogWarning("No mods available");
            return list;
        }

        // --- CRITICAL FIX 2: Use .Count property ---
        List<int> selectedIndices = ModSelectionUtility.GetSeededRandomIndices(
            seed,
            allMods.Count, // Corrected to use .Count property
            PlayerModChoiceCount
        );

        
        foreach (int mod_index in selectedIndices)
        {
            list.Add(allMods[mod_index]);
        }

        return list;
    }

    // Public hook for UI to call when a mod is chosen
    public void PlayerChoosesMod(Mod _chosenMod)
    {
        // 1. Initialize and activate the mod
        Mod chosenMod = Instantiate(_chosenMod);
        chosenMod.Initialize(); // Initialize uses
        activeMods.Add(chosenMod);
        OnModAdded?.Invoke(chosenMod);

        // 2. Remove from the available pool (assuming single-purchase)
        if (!allMods.Remove(_chosenMod))
        {
            Debug.LogWarning($"Failed to remove mod '{chosenMod.ModName}' from available pool. Was it already used?");
        }
    }


    public void OnOrderStartInitializeActiveMods()
    {
        //check expiry and remove
        RemoveExpiredActiveMods();

        foreach (Mod mod in activeMods) // run all mods except clickables
        {
            if (autoApplyModTypes.Contains(mod.modType))
            {
                mod.ProcessOrder(GameManager.Instance.currentOrder);
                Debug.Log(mod.modUsedMessage);
            }
        }

    }

    public void OnOrderFilledActiveMods()
    {
        foreach (Mod mod in activeMods) // run all mods except clickables
        {
            if (autoApplyModTypes.Contains(mod.modType))
            {
                mod.ConsumeUsage();
            }
        }
    }

    /// <summary>
    /// Checks active mods for expiration and removes them. 
    /// </summary>
    public void RemoveExpiredActiveMods()
    {
        int removedMods = 0;
        for (int i = activeMods.Count - 1; i >= 0; i--)
        {
            Mod mod = activeMods[i];

            if (mod.isExpired)
            {
                if (mod.modType == ModType.Clickable)
                {
                    OnClickableModExpired?.Invoke(mod);
                }
                activeMods.RemoveAt(i);
                Destroy(mod);
                removedMods++;
                Debug.Log($"Removed expired mod: {mod.ModName}");
            }
        }
        Debug.Log($"Scraped {removedMods} expired mods");
    }

    public void RemoveActiveMod(Mod modInstance)
    {
        if (activeMods.Contains(modInstance))
        {
            activeMods.Remove(modInstance);
            Destroy(modInstance); // Good practice to destroy the instance
        }
    }

    // --- Add GetMod<T>() helper method here for Reroll functionality ---
    public T GetMod<T>() where T : Mod
    {
        return activeMods.OfType<T>().FirstOrDefault();
    }

    public float GetTotalVolumeMultiplier()
    {
        float totalVolumeMultiplier = 1;
        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            totalVolumeMultiplier *= mod.GetVolumeMultiplier();
        }
        return totalVolumeMultiplier;
    }
    public float GetTotalPayoutMultiplier()
    {
        float totalVolumeMultiplier = 1f;
        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            totalVolumeMultiplier *= mod.GetPayoutMultiplier();
        }
        return totalVolumeMultiplier;
    }
    public float GetTotalBurgerPriceMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            total *= mod.GetBurgerPriceMultiplier();
        }
        return total;
    }

    public float GetTotalFriesPriceMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            total *= mod.GetFriesPriceMultiplier();
        }
        return total;
    }

    public float GetTotalSodaPriceMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            total *= mod.GetSodaPriceMultiplier();
        }
        return total;
    }

    public float GetTotalBulkPriceMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            total *= mod.GetBulkPriceMultiplier();
        }
        return total;
    }
    public float GetTotalBurgerCostMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            total *= mod.GetBurgerCostMultiplier();
        }
        return total;
    }

    public float GetTotalFriesCostMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            total *= mod.GetFriesCostMultiplier();
        }
        return total;
    }

    public float GetTotalSodaCostMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            total *= mod.GetSodaCostMultiplier();
        }
        return total;
    }
    // ModManager.cs

    public const float NO_OVERRIDE = -1.0f;
    public float getNO_OVERRIDE => NO_OVERRIDE;

    /// <summary>
    /// Finds the lowest absolute cost override for Burgers among all active mods.
    /// Returns -1.0f if no mod provides a valid override.
    /// </summary>
    public float GetTotalBurgerCostOverride()
    {
        float lowestOverrideCost = NO_OVERRIDE;

        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            float currentOverride = mod.GetBurgerCostOverride();

            // 1. Check if the current mod actually provides an override price (>= 0.0f)
            if (currentOverride >= 0.0f)
            {
                // 2. Check if this is the first valid override OR if the current override is cheaper
                if (lowestOverrideCost == NO_OVERRIDE || currentOverride < lowestOverrideCost)
                {
                    lowestOverrideCost = currentOverride;
                }
            }
        }
        return lowestOverrideCost;
    }

    /// <summary>
    /// Finds the lowest absolute cost override for Fries among all active mods.
    /// Returns -1.0f if no mod provides a valid override.
    /// </summary>
    public float GetTotalFriesCostOverride()
    {
        float lowestOverrideCost = NO_OVERRIDE;

        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            float currentOverride = mod.GetFriesCostOverride();

            if (currentOverride >= 0.0f)
            {
                if (lowestOverrideCost == NO_OVERRIDE || currentOverride < lowestOverrideCost)
                {
                    lowestOverrideCost = currentOverride;
                }
            }
        }
        return lowestOverrideCost;
    }

    /// <summary>
    /// Finds the lowest absolute cost override for Soda among all active mods.
    /// Returns -1.0f if no mod provides a valid override.
    /// </summary>
    public float GetTotalSodaCostOverride()
    {
        float lowestOverrideCost = NO_OVERRIDE;

        foreach (Mod mod in activeMods)
        {
            if (mod.modType == ModType.Clickable && !mod.isClicked())
            {
                continue;
            }
            float currentOverride = mod.GetSodaCostOverride();

            if (currentOverride >= 0.0f)
            {
                if (lowestOverrideCost == NO_OVERRIDE || currentOverride < lowestOverrideCost)
                {
                    lowestOverrideCost = currentOverride;
                }
            }
        }
        return lowestOverrideCost;
    }
    // ... (Your other methods like UpdateOrderModifiers/ProcessOrderMods should be added here) ...
}

public static class ModSelectionUtility
{
    /// <summary>
    /// Returns a list of unique, random indices from the range [0, availableModCount - 1],
    /// based on the provided seed.
    /// </summary>
    /// <param name="seed">The integer seed for predictable randomness.</param>
    /// <param name="availableModCount">The total size of the array/list to choose from.</param>
    /// <param name="modsToShow">The number of unique indices to select.</param>
    /// <returns>A List of unique integers representing the chosen indices.</returns>
    public static List<int> GetSeededRandomIndices(int seed, int availableModCount, int modsToShow)
    {
        // 1. Safety check
        if (modsToShow <= 0 || availableModCount <= 0 || modsToShow > availableModCount)
        {
            Debug.LogError("Invalid parameters for index selection.");
            return new List<int>();
        }

        // 2. Initialize Seeded RNG
        System.Random rng = new System.Random(seed);

        // 3. Create a list of all possible indices (0, 1, 2, 3, ...)
        List<int> allIndices = Enumerable.Range(0, availableModCount).ToList();

        // 4. Perform a partial Fisher-Yates Shuffle and Selection
        List<int> chosenIndices = new List<int>();
        int workingSize = availableModCount;

        for (int i = 0; i < modsToShow; i++)
        {
            // Get a random index within the remaining working size
            // rng.Next(max) returns a value between 0 (inclusive) and max (exclusive)
            int randomIndex = rng.Next(workingSize);

            // The chosen index is the value currently at randomIndex
            chosenIndices.Add(allIndices[randomIndex]);

            // Swap the chosen element with the last element of the working list
            // This is the efficient way to "remove" an element without resizing the list
            // and ensures the next random pick is from the unchosen elements.
            workingSize--;

            int temp = allIndices[randomIndex];
            allIndices[randomIndex] = allIndices[workingSize];
            allIndices[workingSize] = temp;
        }

        return chosenIndices;
    }
}