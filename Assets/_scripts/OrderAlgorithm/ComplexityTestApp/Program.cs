using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

// --- 1. DATA STRUCTURES ---

// Dynamic Rule Structures for Level 3
public enum RandomnessType
{
    Normal,
    Uniform
}

public class AllocationRule
{
    // The number of standard deviations the Min/Max range covers (6 = 99.7% coverage)
    public const float NormalDistributionTuningDivisor = 6f;

    public required string ItemName { get; set; }
    public required int PrioritySlot { get; set; } // e.g., 1 (highest priority), 2, 3...
    public required float MinPct { get; set; }     // e.g., 0.4 (40%)
    public required float MaxPct { get; set; }     // e.g., 0.6 (60%)
    public required RandomnessType RandomnessType { get; set; }

    // Default mu for Normal distribution (midpoint of Min/Max)
    public float MidpointMu => (MinPct + MaxPct) / 2f;

    // Calculated sigma for Normal distribution
    public float CalculatedSigma => (MaxPct - MinPct) / NormalDistributionTuningDivisor;
}

public class AllocatedItem
{
    public required string ItemName { get; set; }
    public required int Quantity { get; set; }
    public required float PctFinal { get; set; }
}

// Data structure for the Level 3 (Order Component) results
public class OrderComponentResult
{
    public required int TargetVolumeCheck { get; set; } // Should equal V_final
    public required List<AllocatedItem> Items { get; set; }
}

// Data structure returned by the StochasticVolumeRandomizer (Level 2)
public class VolumeCalculationResult
{
    public float SigmaCalculated { get; set; }
    public float VolumeRaw { get; set; }
    public int VolumeFinal { get; set; } // The final, integer volume (V)
}

// Detailed Result per Order (Combines Level 1, 2, and 3 data)
public class ComplexityResult
{
    // Inputs and Derived Variables (Level 1)
    public required int OrderNumberN { get; set; }
    public required int OrdersPerShift { get; set; }
    public required int ShiftNumberS { get; set; }
    public required int LocalOrderO { get; set; }

    // Tuning Coefficients (for reference)
    public required float BetaIntraShiftCoefficient { get; set; }
    public required float AlphaLinearCoefficient { get; set; }

    // Component Terms (Level 1)
    public required float QuadraticTerm { get; set; }
    public required float LinearTerm { get; set; }
    public required float ExponentialMultiplier { get; set; }

    // Mean Volume (Level 1 Result)
    public required float MeanVolumeMu { get; set; }

    // Stochastic Results (Level 2 Result)
    public required VolumeCalculationResult StochasticResults { get; set; }

    // Order Component Breakdown (Level 3 Result)
    public required OrderComponentResult OrderComponents { get; set; }
}

// Final Container Structure for the JSON file
public class SimulationOutput
{
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    // Summary: Dictionary where Key=ShiftNumber (S), Value=List of Order Volumes & Item Counts
    public Dictionary<int, List<Dictionary<string, object>>> ComplexitiesByShift { get; set; } = new Dictionary<int, List<Dictionary<string, object>>>();

    // Detailed Log: Full breakdown for every order
    public List<ComplexityResult> DetailedOrderBreakdown { get; set; } = new List<ComplexityResult>();
}


// --- 2. DETERMINISTIC CALCULATOR (Level 1) ---

public class OrderComplexity
{
    // --- TUNING CONSTANTS ---
    public const float BaseOrderComplexity = 3.2f;
    public const float AlphaLinearCoefficient = 4.8f;
    public const float ExponentialDivider = 2.6f;
    public float BetaIntraShiftCoefficient = 0.6f; // Beta for intra-shift scaling

    // Dependencies
    private readonly StochasticVolumeRandomizer _randomizer;
    private readonly OrderComponentGenerator _componentGenerator;

    // Default dynamic profile 
    private readonly List<AllocationRule> _defaultRules = new List<AllocationRule>
    {
        // Item 1: Burger - Note: RandomnessType must be Normal for CalculatedSigma to be used
        new AllocationRule { ItemName = "Burger", PrioritySlot = 1, MinPct = 0.2f, MaxPct = 0.6f, RandomnessType = RandomnessType.Normal }, 
        // Item 2: Fries - Mid priority, moderate allocation, chaotic
        new AllocationRule { ItemName = "Fries", PrioritySlot = 2, MinPct = 0.2f, MaxPct = 0.7f, RandomnessType = RandomnessType.Normal },
        // Item 3: Soda - Lowest priority, takes the remainder 
        new AllocationRule { ItemName = "Soda", PrioritySlot = 3, MinPct = 0.3f, MaxPct = 0.4f, RandomnessType = RandomnessType.Normal }
    };

    // Helper function (replaces Mathf.CeilToInt)
    private static int CeilToInt(float f) => (int)Math.Ceiling(f);

    public OrderComplexity(int gameSeed)
    {
        _randomizer = new StochasticVolumeRandomizer(gameSeed);
        _componentGenerator = new OrderComponentGenerator(gameSeed);
    }

    public float GetSigmaScalingFactor()
    {
        return StochasticVolumeRandomizer.SigmaTuningPercentage;
    }

    public List<AllocationRule> GetDefaultRules() => _defaultRules;


    public ComplexityResult CalculateComplexity(int orderNumberN, int ordersPerShift)
    {
        if (ordersPerShift <= 0)
        {
            throw new ArgumentException("Orders per shift must be greater than zero.");
        }

        // --- Level 1 Calculations (Derivations) ---
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
        var componentResults = _componentGenerator.GenerateComponents(finalVolumeTarget, _defaultRules);


        // --- 4. RETURN STRUCTURED DATA ---
        return new ComplexityResult
        {
            OrderNumberN = orderNumberN,
            OrdersPerShift = ordersPerShift,
            ShiftNumberS = shiftNumberS,
            LocalOrderO = localOrderO,
            BetaIntraShiftCoefficient = BetaIntraShiftCoefficient,
            AlphaLinearCoefficient = AlphaLinearCoefficient,
            QuadraticTerm = quadraticTerm,
            LinearTerm = linearTerm,
            ExponentialMultiplier = (float)exponentialMultiplier,
            MeanVolumeMu = meanVolumeMu,
            StochasticResults = volumeResults,
            OrderComponents = componentResults // Assign Level 3 results
        };
    }
}


// --- 3. STOCHASTIC VOLUME RANDOMIZER (Level 2) ---

public class StochasticVolumeRandomizer
{
    // --- 1. TUNING CONSTANTS ---
    public const float SigmaTuningPercentage = 0.2f; // 20% volatility

    private readonly Random _rng;

    public StochasticVolumeRandomizer(int seed)
    {
        _rng = new Random(seed);
    }

    // Helper method for the Normal Distribution (Box-Muller)
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


// --- 4. ORDER COMPONENT GENERATOR (Level 3) ---

public class OrderComponentGenerator
{
    private readonly Random _rng;

    public OrderComponentGenerator(int seed)
    {
        _rng = new Random(seed);
    }

    // Helper method for the Normal Distribution (Box-Muller)
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
                pctFinal = (float)volumeRemaining / (float)targetVolume;
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
                    // Uses the CalculatedSigma from AllocationRule
                    rawPct = NextGaussian(rule.MidpointMu, rule.CalculatedSigma);
                }

                // Clamp and finalize the percentage
                pctFinal = Math.Min(rule.MaxPct, Math.Max(rule.MinPct, rawPct));

                // Calculate Quantity (based on remaining volume)
                quantity = (int)Math.Round(volumeRemaining * pctFinal);

                // Safety Check: Must not allocate more than what's left
                quantity = Math.Max(0, Math.Min(quantity, volumeRemaining));

                // Update Budget
                volumeRemaining -= quantity;
            }

            float truePctFinal = (float)quantity / (float)targetVolume;

            // When adding to the list:
            allocatedItems.Add(new AllocatedItem
            {
                ItemName = rule.ItemName,
                Quantity = quantity,
                // Use the newly calculated percentage (truePctFinal) for logging
                PctFinal = truePctFinal
            });
        }

        return new OrderComponentResult
        {
            TargetVolumeCheck = targetVolume,
            Items = allocatedItems
        };
    }
}


// --- 5. MAIN EXECUTION LOGIC ---

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- STARTING DETERMINISTIC + STOCHASTIC SIMULATION ---");

        // --- SIMULATION PARAMETERS ---
        int testOrdersPerShift = 10;
        int totalOrdersToTest = 100;
        const int GameSeed = 1338;

        OrderComplexity calculator = new OrderComplexity(GameSeed);

        List<ComplexityResult> allDetailedResults = new List<ComplexityResult>();

        // --- A. Run Simulation and Collect Detailed Data ---
        for (int n = 1; n <= totalOrdersToTest; n++)
        {
            var result = calculator.CalculateComplexity(n, testOrdersPerShift);
            allDetailedResults.Add(result);
        }

        // --- B. Group Data for Summary Section & Metadata ---
        var finalOutput = new SimulationOutput();
        // This assignment ensures all detailed data (L1, L2, L3) is included in the output.
        finalOutput.DetailedOrderBreakdown = allDetailedResults;

        // NEW LOGIC: Create the grouped dictionary (S -> [OrderDict1, OrderDict2, ...])
        finalOutput.ComplexitiesByShift = allDetailedResults
            .GroupBy(r => r.ShiftNumberS)
            .OrderBy(g => g.Key)
            .ToDictionary(
                group => group.Key,
                group => group.Select(r =>
                {
                    // For each order (r), create a dictionary summarizing V_final and item counts
                    var orderSummary = new Dictionary<string, object>();
                    orderSummary.Add("Volume_Final", r.StochasticResults.VolumeFinal);
                    orderSummary.Add("Local_Order_Number_N", r.LocalOrderO);
                    orderSummary.Add("Order_Number_N", r.OrderNumberN);

                    // Add all allocated item quantities
                    foreach (var item in r.OrderComponents.Items)
                    {
                        orderSummary.Add(item.ItemName + "_Count", item.Quantity);
                    }
                    return orderSummary;
                }).ToList()
            );

        // Populate Metadata
        finalOutput.Metadata.Add("Description", "Stochastic Order Volume Simulation Output (L1 Mean, L2 Volume, L3 Components)");
        finalOutput.Metadata.Add("GeneratedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        finalOutput.Metadata.Add("OrdersPerShift_X", testOrdersPerShift.ToString());
        finalOutput.Metadata.Add("GameSeed", GameSeed.ToString());

        finalOutput.Metadata.Add("V_base", OrderComplexity.BaseOrderComplexity.ToString("F2"));
        finalOutput.Metadata.Add("Alpha", OrderComplexity.AlphaLinearCoefficient.ToString("F2"));
        finalOutput.Metadata.Add("Gamma_Divisor", OrderComplexity.ExponentialDivider.ToString("F2"));
        finalOutput.Metadata.Add("Beta_IntraShift", calculator.BetaIntraShiftCoefficient.ToString("F2"));
        finalOutput.Metadata.Add("Sigma_Scaling_Pct", StochasticVolumeRandomizer.SigmaTuningPercentage.ToString("F2"));


        // --- C. Serialize and Save to JSON ---
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(finalOutput, options);

            string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss");
            string fileName = $"order_simulation_{formattedDateTime}_s{GameSeed}.json";

            File.WriteAllText(fileName, jsonString);

            Console.WriteLine($"\n--- SIMULATION COMPLETE ---");
            Console.WriteLine($"Detailed log (DetailedOrderBreakdown) includes Level 1, 2, and 3 results.");
            Console.WriteLine($"Wrote detailed results (1 to {totalOrdersToTest} orders) to: {Path.GetFullPath(fileName)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn error occurred while writing the file: {ex.Message}");
            Console.WriteLine("ACTION REQUIRED: Please ensure the 'System.Text.Json' NuGet package is installed.");
        }
    }
}