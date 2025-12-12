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
    [SerializeField] private int PlayerModChoiceCount = 2;


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


    private List<Mod> ModChoicesForPlayer()
    {
        if (seed == -1)
        {
            Debug.LogWarning("Mod Manager seed not set. Will not generate Mod choices for player");
            return null;
        }

        if (allMods.Count == 0)
        {
            Debug.LogWarning("No mods available");
            return null;
        }

        // --- CRITICAL FIX 2: Use .Count property ---
        List<int> selectedIndices = ModSelectionUtility.GetSeededRandomIndices(
            seed,
            allMods.Count, // Corrected to use .Count property
            PlayerModChoiceCount
        );

        List<Mod> list = new List<Mod>();
        foreach (int mod_index in selectedIndices)
        {
            list.Add(allMods[mod_index]);
        }

        return list;
    }

    // Public hook for UI to call when a mod is chosen
    public void PlayerChoosesMod(Mod chosenMod)
    {
        // 1. Initialize and activate the mod
        chosenMod.Initialize(); // Initialize uses
        activeMods.Add(chosenMod);

        // 2. Remove from the available pool (assuming single-purchase)
        if (!allMods.Remove(chosenMod))
        {
            Debug.LogWarning($"Failed to remove mod '{chosenMod.ModName}' from available pool. Was it already used?");
        }
    }

    /// <summary>
    /// Checks active mods for expiration and removes them. 
    /// Should be called after every order is processed.
    /// </summary>
    public void RemoveExpiredActiveMods()
    {
        // --- CRITICAL FIX 3: Safe Backward For Loop ---
        for (int i = activeMods.Count - 1; i >= 0; i--)
        {
            Mod mod = activeMods[i];

            // Note: You still need a method to check expiration.
            // The isExpired flag should be set in Mod.ProcessOrder() or Mod.TryUseAbility().
            if (mod.isExpired)
            {
                // Optional: Call mod.Revert() if the mod made permanent changes
                activeMods.RemoveAt(i);
                Debug.Log($"Removed expired mod: {mod.ModName}");
            }
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
            totalVolumeMultiplier *= mod.GetVolumeMultiplier();
        }
        return totalVolumeMultiplier;
    }
    public float GetTotalPayoutMultiplier()
    {
        float totalVolumeMultiplier = 1;
        foreach (Mod mod in activeMods)
        {
            totalVolumeMultiplier *= mod.GetVolumeMultiplier();
        }
        return totalVolumeMultiplier;
    }
    public float GetTotalBurgerPriceMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            total *= mod.GetBurgerPriceMultiplier();
        }
        return total;
    }

    public float GetTotalFriesPriceMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            total *= mod.GetFriesPriceMultiplier();
        }
        return total;
    }

    public float GetTotalSodaPriceMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            total *= mod.GetSodaPriceMultiplier();
        }
        return total;
    }

    public float GetTotalBulkPriceMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            total *= mod.GetBulkPriceMultiplier();
        }
        return total;
    }
    public float GetTotalBurgerCostMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            total *= mod.GetBurgerCostMultiplier();
        }
        return total;
    }

    public float GetTotalFriesCostMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            total *= mod.GetFriesCostMultiplier();
        }
        return total;
    }

    public float GetTotalSodaCostMultiplier()
    {
        float total = 1.0f;
        foreach (Mod mod in activeMods)
        {
            total *= mod.GetSodaCostMultiplier();
        }
        return total;
    }
    // ModManager.cs

    private const float NO_OVERRIDE = -1.0f;

    /// <summary>
    /// Finds the lowest absolute cost override for Burgers among all active mods.
    /// Returns -1.0f if no mod provides a valid override.
    /// </summary>
    public float GetTotalBurgerCostOverride()
    {
        float lowestOverrideCost = NO_OVERRIDE;

        foreach (Mod mod in activeMods)
        {
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