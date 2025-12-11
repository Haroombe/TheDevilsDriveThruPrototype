using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static Assets._scripts.OrderAlgorithm.OrderManager;

// The System.Text.Json using statement is now only relevant if you choose to serialize later, 
// but it is harmless to leave it, so I've kept it.

// --- 1. DATA STRUCTURES ---

public enum RandomnessType
{
    Normal,
    Uniform
}

public class RunLogData
{
    // Holds the static parameters captured at the start of the run (e.g., K, BaseComplexity)
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    // Holds the ComplexityResult for every single order generated in this run
    public List<ComplexityResult> DetailedOrderBreakdown { get; set; } = new List<ComplexityResult>();

    // You can add your ComplexitiesByShift summary here later if needed:
    // public Dictionary<int, List<Dictionary<string, object>>> ComplexitiesByShift { get; set; } 
}
public class AllocationRule
{
    public string ItemName { get; set; }
    public int PrioritySlot { get; set; }
    public float MinPct { get; set; }
    public float MaxPct { get; set; }
    public RandomnessType RandomnessType { get; set; }

    public float MidpointMu => (MinPct + MaxPct) / 2f;
}

public class AllocatedItem
{
    public string ItemName { get; set; }
    public int Quantity { get; set; }
    public float PctFinal { get; set; }
}

public class OrderComponentResult
{
    public int TargetVolumeCheck { get; set; }
    public List<AllocatedItem> Items { get; set; }
}

public class VolumeCalculationResult
{
    public float SigmaCalculated { get; set; }
    public float VolumeRaw { get; set; }
    public int VolumeFinal { get; set; }
}
// ------------------------------------------------------------------
// REQUIRED COMPANION STRUCTURES FOR LOGGING
// (Place these outside the OrderManager class)
// ------------------------------------------------------------------

/// <summary>
/// Structure to hold the data for a single complete run (Game Session).
/// </summary>


/// <summary>
/// Economic data snapshot captured for a specific order.
/// </summary>

public struct EconomicSnapshot
{
    // Static unit values
    public float BurgerCost;
    public float BurgerPrice;
    public float FriesCost;
    public float FriesPrice;
    public float SodaCost;
    public float SodaPrice;
    public int ClampedShiftNumber { get; set; }
    public float RunDifficultyMultiplier { get; set; }
    public float DifficultyCurveValue { get; set; }
    public float PriceMultiplier { get; set; }
    public float EffectiveCostRatio { get; set; }
    // **NEW DYNAMIC DERIVED VALUES**
    public float BurgerMargin; // Price - Cost
    public float FriesMargin;
    public float SodaMargin;

}


// NOTE: You must update ComplexityResult.cs to include the EconomicSnapshot:
// public class ComplexityResult { ... public EconomicSnapshot EconomicData { get; set; } ... }


public class ComplexityResult
{
    public int OrderNumberN { get; set; }
    public int OrdersPerShift { get; set; }
    public int ShiftNumberS { get; set; }
    public int LocalOrderO { get; set; }
    public float BetaIntraShiftCoefficient { get; set; }
    public float AlphaLinearCoefficient { get; set; }
    public float ExponentialDivider { get; set; }
    public float QuadraticTerm { get; set; }
    public float LinearTerm { get; set; }
    public float ExponentialMultiplier { get; set; }
    public float MeanVolumeMu { get; set; }
    public EconomicSnapshot EconomicData { get; set; }

    public VolumeCalculationResult StochasticResults { get; set; }
    public OrderComponentResult OrderComponents { get; set; }
}

// NOTE: SimulationOutput is often only needed for file logging, 
// but is harmless if left here.
public class SimulationOutput
{
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public Dictionary<int, List<Dictionary<string, object>>> ComplexitiesByShift { get; set; } = new Dictionary<int, List<Dictionary<string, object>>>();
    public List<ComplexityResult> DetailedOrderBreakdown { get; set; } = new List<ComplexityResult>();
}

// -------------------------------------------------------------------------------------

// --- 2. ORDER COMPLEXITY CALCULATOR (Level 1) ---

public class OrderComplexity
{
    // Exposed Public Properties for UI/Serialization
    public float BaseOrderComplexity { get; private set; }
    public float AlphaLinearCoefficient { get; private set; }
    public float ExponentialDivider { get; private set; }
    public float BetaIntraShiftCoefficient { get; private set; }

    // Dependencies
    private readonly StochasticVolumeRandomizer _randomizer;
    private readonly OrderComponentGenerator _componentGenerator;
    private List<AllocationRule> _currentRules;

    private static int CeilToInt(float f) => (int)Math.Ceiling(f);
    private readonly List<AllocationRule> _defaultRules = new List<AllocationRule>
    {
        new AllocationRule { ItemName = "Burger", PrioritySlot = 1, MinPct = 0.2f, MaxPct = 0.6f, RandomnessType = RandomnessType.Normal },
        new AllocationRule { ItemName = "Fries", PrioritySlot = 2, MinPct = 0.2f, MaxPct = 0.7f, RandomnessType = RandomnessType.Normal },
        new AllocationRule { ItemName = "Soda", PrioritySlot = 3, MinPct = 0.3f, MaxPct = 0.4f, RandomnessType = RandomnessType.Normal }
    };

    public OrderComplexity(
        int gameSeed,
        float baseComplexity = 3.2f,
        float alphaCoefficient = 4.8f,
        float exponentialDivider = 2.6f,
        float betaCoefficient = 0.6f,
        float sigmaTuningPct = 0.01f,
        float normalDivisor = 6.0f,
        List<AllocationRule> currentRules = null)
    {
        BaseOrderComplexity = baseComplexity;
        AlphaLinearCoefficient = alphaCoefficient;
        ExponentialDivider = exponentialDivider;
        BetaIntraShiftCoefficient = betaCoefficient;

        
        if (_currentRules != null)
        {
            this._currentRules = currentRules;
        }
        else
        {
            this._currentRules = _defaultRules;
        }
       

        _randomizer = new StochasticVolumeRandomizer(gameSeed, sigmaTuningPct);
        _componentGenerator = new OrderComponentGenerator(gameSeed, normalDivisor);
    }

    public void UpdateAllocationRules(List<AllocationRule> newRules)
    {
        _currentRules = newRules ?? throw new ArgumentNullException(nameof(newRules));
    }

    public float GetSigmaScalingFactor() => _randomizer.SigmaTuningPercentage;
    public float NormalDistributionTuningDivisor => _componentGenerator.NormalDistributionTuningDivisor;
    public List<AllocationRule> GetCurrentRules() => _currentRules;

    public ComplexityResult CalculateComplexity(int orderNumberN, int ordersPerShift)
    {
        if (ordersPerShift <= 0 || orderNumberN <= 0)
        {
            throw new ArgumentException("Orders per shift and order number must be greater than zero.");
        }

        // --- Level 1 Calculations ---
        int shiftNumberS = CeilToInt((float)orderNumberN / ordersPerShift);
        int localOrderO = ((orderNumberN - 1) % ordersPerShift) + 1;
        float localRatio = (float)localOrderO / ordersPerShift;

        float quadraticTerm = AlphaLinearCoefficient * BetaIntraShiftCoefficient * (localRatio * localRatio);
        float linearTerm = AlphaLinearCoefficient * (shiftNumberS - 1);
        double exponentialMultiplier = Math.Exp((double)shiftNumberS / ExponentialDivider);
        float complexityFactor = (quadraticTerm + linearTerm) * (float)exponentialMultiplier;
        float meanVolumeMu = BaseOrderComplexity + complexityFactor;


        // --- LEVEL 2: Stochastic Volume Generation ---
        var volumeResults = _randomizer.GenerateFinalVolume(meanVolumeMu, BaseOrderComplexity);
        int finalVolumeTarget = volumeResults.VolumeFinal;


        // --- LEVEL 3: Component Allocation ---
        var componentResults = _componentGenerator.GenerateComponents(finalVolumeTarget, _currentRules);

        // --- 4. RETURN STRUCTURED DATA ---
        return new ComplexityResult
        {
            OrderNumberN = orderNumberN,
            OrdersPerShift = ordersPerShift,
            ShiftNumberS = shiftNumberS,
            LocalOrderO = localOrderO,
            BetaIntraShiftCoefficient = BetaIntraShiftCoefficient,
            AlphaLinearCoefficient = AlphaLinearCoefficient,
            ExponentialDivider = ExponentialDivider,
            QuadraticTerm = quadraticTerm,
            LinearTerm = linearTerm,
            ExponentialMultiplier = (float)exponentialMultiplier,
            MeanVolumeMu = meanVolumeMu,
            StochasticResults = volumeResults,
            OrderComponents = componentResults
        };
    }
}

// -------------------------------------------------------------------------------------

// --- 3. STOCHASTIC VOLUME RANDOMIZER (Level 2) ---

public class StochasticVolumeRandomizer
{
    public float SigmaTuningPercentage { get; private set; }
    private readonly Random _rng;

    public StochasticVolumeRandomizer(int seed, float sigmaTuningPct = 0.01f)
    {
        _rng = new Random(seed);
        SigmaTuningPercentage = sigmaTuningPct;
    }

    private float NextGaussian(float mu, float sigma)
    {
        double u1 = 1.0 - _rng.NextDouble();
        double u2 = 1.0 - _rng.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        return mu + sigma * (float)randStdNormal;
    }

    public VolumeCalculationResult GenerateFinalVolume(float meanVolumeMu, float baseOrderComplexity)
    {
        float sigma = meanVolumeMu * SigmaTuningPercentage;
        float volumeRaw = NextGaussian(meanVolumeMu, sigma);
        int volumeRounded = (int)Math.Round(volumeRaw);
        int volumeFinal = Math.Max(volumeRounded, (int)baseOrderComplexity);

        return new VolumeCalculationResult
        {
            SigmaCalculated = sigma,
            VolumeRaw = volumeRaw,
            VolumeFinal = volumeFinal
        };
    }
}

// -------------------------------------------------------------------------------------

// --- 4. ORDER COMPONENT GENERATOR (Level 3) ---

public class OrderComponentGenerator
{
    private readonly Random _rng;
    public float NormalDistributionTuningDivisor { get; private set; }

    public OrderComponentGenerator(int seed, float normalDivisor = 6.0f)
    {
        _rng = new Random(seed);
        NormalDistributionTuningDivisor = normalDivisor;
    }

    private float NextGaussian(float mu, float sigma)
    {
        double u1 = 1.0 - _rng.NextDouble();
        double u2 = 1.0 - _rng.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mu + sigma * (float)randStdNormal;
    }

    public OrderComponentResult GenerateComponents(int targetVolume, List<AllocationRule> rules)
    {
        int volumeRemaining = targetVolume;
        var allocatedItems = new List<AllocatedItem>();

        var sortedRules = rules.OrderBy(r => r.PrioritySlot).ToList();
        int lastItemIndex = sortedRules.Count - 1;

        for (int i = 0; i < sortedRules.Count; i++)
        {
            var rule = sortedRules[i];
            int quantity;
            float pctFinal;

            if (i == lastItemIndex)
            {
                // FINAL ITEM RULE: Takes all remaining volume
                quantity = volumeRemaining;
                // For the last item, pctFinal is always the percentage of the total volume
                pctFinal = (float)quantity / (float)targetVolume;
            }
            else
            {
                // GENERAL ALLOCATION FORMULA:
                float rawPct;
                if (rule.RandomnessType == RandomnessType.Uniform)
                {
                    rawPct = (float)_rng.NextDouble() * (rule.MaxPct - rule.MinPct) + rule.MinPct;
                }
                else
                {
                    float calculatedSigma = (rule.MaxPct - rule.MinPct) / NormalDistributionTuningDivisor;
                    rawPct = NextGaussian(rule.MidpointMu, calculatedSigma);
                }

                // Clamp and finalize the percentage (of V_rem)
                pctFinal = Math.Min(rule.MaxPct, Math.Max(rule.MinPct, rawPct));

                // Calculate Quantity (based on remaining volume)
                quantity = (int)Math.Round(volumeRemaining * pctFinal);

                // Safety Check: Must not allocate more than what's left
                quantity = Math.Max(0, Math.Min(quantity, volumeRemaining));

                // Update Budget
                volumeRemaining -= quantity;
            }

            // PctFinal now logs the instantaneous percentage (pctFinal) 
            // for non-final items, and the true final percentage for the last item.
            allocatedItems.Add(new AllocatedItem
            {
                ItemName = rule.ItemName,
                Quantity = quantity,
                PctFinal = pctFinal
            });
        }

        return new OrderComponentResult
        {
            TargetVolumeCheck = targetVolume,
            Items = allocatedItems
        };
    }
}

// --- 5. MAIN EXECUTION LOGIC (Example use of the new constructors) ---

// Note: The main logic needs to be run in a separate class/method that calls OrderComplexity.
// I will not render the full Program.Main class again, but show the instantiation:

/* public class Program
{
    public static void Main(string[] args)
    {
        int gameSeed = 1338;

        // Instantiate with default parameters
        OrderComplexity calculator = new OrderComplexity(gameSeed); 

        // Or instantiate with custom parameters
        OrderComplexity customCalculator = new OrderComplexity(
            gameSeed: 500,
            baseComplexity: 5.0f,
            betaCoefficient: 0.8f,
            sigmaTuningPct: 0.3f
        );

        // Example of updating rules mid-run:
        customCalculator.UpdateAllocationRules(new List<AllocationRule>
        {
            new AllocationRule { ItemName = "Burger", PrioritySlot = 1, MinPct = 0.5f, MaxPct = 0.8f, RandomnessType = RandomnessType.Uniform },
            new AllocationRule { ItemName = "Fries", PrioritySlot = 2, MinPct = 0.1f, MaxPct = 0.3f, RandomnessType = RandomnessType.Normal },
            new AllocationRule { ItemName = "Soda", PrioritySlot = 3, MinPct = 0.0f, MaxPct = 1.0f, RandomnessType = RandomnessType.Uniform }
        });

        // ... run simulation loop using customCalculator ...
    }
}
*/