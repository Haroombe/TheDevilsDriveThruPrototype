using UnityEngine;
using System; // For Math.Pow

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    // --- Configuration Constants (Set in Inspector) ---
    [Header("Difficulty Tuning")]
    [Tooltip("Base rate for hyper-inflation (e.g., 1.12).")]
    [SerializeField] public float baseInflationRate = 1.2f;

    [Tooltip("Constant K. Controls how fast prices get out of hand (Lower K = Faster growth).")]
    [SerializeField] public float difficultyConstantK = 6.5f;

    [Tooltip("Max Shift index where price and margin acceleration stops.")]
    [SerializeField] public int maxDifficultyShift = 15; // S_max

    [Tooltip("Linear increase in difficulty per subsequent run (e.g., 0.05 = 5%).")]
    [SerializeField] public float runDifficultyIncreaseRate = 0.05f; // R_D

    [Tooltip("The highest R_Mult can ever go (e.g., 1.50 = 50% max cost increase).")]
    [SerializeField] public float maxRunDifficulty = 1.50f; // Max R_Mult

    [Tooltip("X-Axis = Shift Number. Y-Axis = Cost Ratio (The Inverse Bell Curve).")]
    [SerializeField] public AnimationCurve difficultyCurve;
    public float curbaseCostRatio;
    public float priceMultiplier;
    public float effectiveCostRatio;
    // --- Base Prices (Set in Inspector) ---
    [Header("Base Prices (Shift 0)")]
    [SerializeField] private float baseBurgerPrice = 5.00f;
    [SerializeField] private float baseFriesPrice = 3.00f;
    [SerializeField] private float baseSodaPrice = 2.00f;

    // --- Private Fields (The storage locations for 'ref' usage) ---
    private float _curBurgerPrice;
    private float _curBurgerCost;
    private float _curFriesPrice;
    private float _curFriesCost;
    private float _curSodaPrice;
    private float _curSodaCost;

    public int clampedshift;

    // --- Public Properties (Read-Only access for other scripts) ---
    public float CurBurgerPrice => _curBurgerPrice;
    public float CurBurgerCost => _curBurgerCost;
    public float CurFriesPrice => _curFriesPrice;
    public float CurFriesCost => _curFriesCost;
    public float CurSodaPrice => _curSodaPrice;
    public float CurSodaCost => _curSodaCost;

    // Global variable storing the meta-difficulty for the current run
    public float CurrentRunMultiplier { get; private set; }

    // --- Initialization and Run Difficulty Calculator ---

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("EconomyManager initializing.");
            InitializeEconomy(1);
            // Initialization is now managed by the GameManager calling InitializeEconomy()
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Calculates the clamped Run Multiplier (R_Mult).
    /// </summary>
    /// <param name="totalAttempts">Total number of runs/attempts the player has started (1st run = 1).</param>
    public float CalculateRunMultiplier(int totalAttempts)
    {
        // Difficulty increase starts after the first attempt (Attempt 1 = 0 increase)
        int attemptsOverBase = Math.Max(0, totalAttempts - 1);

        // Linear scaling: 1.00 + (Attempts * 0.05)
        float baseMultiplier = 1.0f + (attemptsOverBase * runDifficultyIncreaseRate);

        // Clamp the final multiplier at the defined maximum (e.g., 1.50)
        return Mathf.Clamp(baseMultiplier, 1.0f, maxRunDifficulty);
    }

    /// <summary>
    /// Call this from a Game Reset or Initialization function.
    /// </summary>
    public void InitializeEconomy(int totalAttempts)
    {
        // Calculate and set the meta-difficulty for this run
        CurrentRunMultiplier = CalculateRunMultiplier(totalAttempts);

        // Start the game at Shift 1 prices and costs
        UpdateShiftEconomy(1);
        Debug.Log($"ECONOMY INITIALIZED: Run Multiplier set to {CurrentRunMultiplier:F2}");
    }

    // --- Per-Shift Update ---

    /// <summary>
    /// The core method called at the start of every shift to update all item economics.
    /// </summary>
    /// <param name="shiftNumber">The current shift index (e.g., 1, 2, 3...).</param>
    public void UpdateShiftEconomy(int shiftNumber)
    {
        if (shiftNumber < 1) shiftNumber = 1;

        // Apply Max Difficulty Clamp to the Shift Number
        int clampedShift = Mathf.Min(shiftNumber, maxDifficultyShift);

        // --- 1. Calculate Global Economic Drivers ---

        // a. Hyper-Exponential Price Multiplier (The Scale Cap)
        // Formula: (Rate_base)^(S_clamped^2 / K)
        float exponent = (float)(clampedShift * clampedShift) / difficultyConstantK;
        priceMultiplier = Mathf.Pow(baseInflationRate, exponent);

        // b. Get the Difficulty Ratio (The Margin Collapse)
        // Formula: R_C.Evaluate(S_clamped) * R_Mult
        float baseCostRatio = difficultyCurve.Evaluate(clampedShift);
        curbaseCostRatio = difficultyCurve.Evaluate(clampedShift);
        effectiveCostRatio = baseCostRatio * CurrentRunMultiplier;

        // Final hard limit on difficulty (preventing unstable costs)
        effectiveCostRatio = Mathf.Clamp(effectiveCostRatio, 0.05f, 5.0f);

        // --- 2. Update All Item Prices and Costs ---

        // Use the private fields for the ref arguments
        UpdateItemValues(ref _curBurgerPrice, ref _curBurgerCost, baseBurgerPrice, priceMultiplier, effectiveCostRatio);
        UpdateItemValues(ref _curFriesPrice, ref _curFriesCost, baseFriesPrice, priceMultiplier, effectiveCostRatio);
        UpdateItemValues(ref _curSodaPrice, ref _curSodaCost, baseSodaPrice, priceMultiplier, effectiveCostRatio);

        // Log for tuning/debugging
        float marginPct = (1f - effectiveCostRatio) * 100f;
        Debug.Log($"--- SHIFT {shiftNumber} ECONOMY UPDATE ---");
        Debug.Log($"Price Multiplier: {priceMultiplier:F2} | Cost Ratio: {effectiveCostRatio:F2} | Margin: {marginPct:F1}%");
        Debug.Log($"Burger Price: ${CurBurgerPrice:F2} | Cost: ${CurBurgerCost:F2}");
        Debug.Log($"Fries Price: ${CurFriesPrice:F2} | Cost: ${CurFriesCost:F2}");
        Debug.Log($"Soda Price: ${CurSodaPrice:F2} | Cost: ${CurSodaCost:F2}");
    }

    // --- Helper Function ---

    private void UpdateItemValues(ref float currentPrice, ref float currentCost,
                                  float basePrice, float multiplier, float costRatio)
    {
        // Price = Base Price * Multiplier (The illusion of scale)
        currentPrice = basePrice * multiplier;

        // Cost = Current Price * Effective Cost Ratio (The true difficulty)
        currentCost = currentPrice * costRatio;
    }
}