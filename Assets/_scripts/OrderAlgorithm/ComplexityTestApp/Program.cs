using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

// --- 1. DATA STRUCTURES ---

// Data structure returned by the StochasticVolumeRandomizer
public class VolumeCalculationResult
{
    public float SigmaCalculated { get; set; }
    public float VolumeRaw { get; set; }
    public int VolumeFinal { get; set; } // The final, integer volume (V)
}

// Detailed Result per Order (Combines Level 1 and Level 2 data)
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
}

// Final Container Structure for the JSON file
public class SimulationOutput
{
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    // Summary: Dictionary where Key=ShiftNumber (S), Value=List of MeanVolumeMu (mu)
    public Dictionary<int, List<float>> ComplexitiesByShift { get; set; } = new Dictionary<int, List<float>>();

    // Detailed Log: Full breakdown for every order
    public List<ComplexityResult> DetailedOrderBreakdown { get; set; } = new List<ComplexityResult>();
}


// --- 2. DETERMINISTIC CALCULATOR (Level 1) ---

public class DeterministicOrderComplexity
{
    // --- TUNING CONSTANTS ---
    public const float BaseOrderComplexity = 3.2f;
    public const float AlphaLinearCoefficient = 4.8f;
    public const float ExponentialDivider = 2.6f;
    public float BetaIntraShiftCoefficient = 0.6f; // Beta for intra-shift scaling

    // Dependency on the randomizer class
    private readonly StochasticVolumeRandomizer _randomizer;

    // Helper function (replaces Mathf.CeilToInt)
    private static int CeilToInt(float f) => (int)Math.Ceiling(f);

    // Constructor to inject the seed through the randomizer
    public DeterministicOrderComplexity(int gameSeed)
    {
        // Initialize the randomizer with the provided seed
        _randomizer = new StochasticVolumeRandomizer(gameSeed);
    }

    public float getSigmaScalingFactor()
    {
        return _randomizer.getSigma();
    }
    public ComplexityResult CalculateComplexity(int orderNumberN, int ordersPerShift)
    {
        if (ordersPerShift <= 0)
        {
            // Note: Must still fully initialize the required properties for ComplexityResult
            throw new ArgumentException("Orders per shift must be greater than zero.");
        }

        // --- 1. DERIVE DEPENDENT VARIABLES (S and O) ---
        int shiftNumberS = CeilToInt((float)orderNumberN / ordersPerShift);
        int localOrderO = ((orderNumberN - 1) % ordersPerShift) + 1;

        // --- 2. CALCULATE FORMULA COMPONENTS ---
        float localRatio = (float)localOrderO / ordersPerShift;

        // Quadratic Term (Intra-shift climax): Alpha * Beta * ( O(n) / X_shift )^2
        float quadraticTerm = AlphaLinearCoefficient * BetaIntraShiftCoefficient * (localRatio * localRatio);

        // Linear Term (Permanent growth per shift): Alpha * ( S(n) - 1 )
        float linearTerm = AlphaLinearCoefficient * (shiftNumberS - 1);

        // Exponential Multiplier (Late game acceleration): e^( S(n) / gamma )
        double exponentialMultiplier = Math.Exp((double)shiftNumberS / ExponentialDivider);

        // --- 3. FINAL MEAN VOLUME (MU) ---
        float complexityFactor = (quadraticTerm + linearTerm) * (float)exponentialMultiplier;
        float meanVolumeMu = BaseOrderComplexity + complexityFactor;


        // --- LEVEL 2: Call the new Randomizer Class ---
        var volumeResults = _randomizer.GenerateFinalVolume(meanVolumeMu, BaseOrderComplexity);


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
            StochasticResults = volumeResults // Attach the Level 2 results
        };
    }
}


// --- 3. STOCHASTIC VOLUME RANDOMIZER (Level 2) ---

public class StochasticVolumeRandomizer
{
    // --- 1. TUNING CONSTANTS ---
    public const float SigmaTuningPercentage = 0.01f; // 20% variance
    public float getSigma()
    {
        return SigmaTuningPercentage;
    }

    // --- 2. SEEDING for Reproducibility ---
    private readonly Random _rng; // Initialized in constructor

    // NEW: Constructor to accept the seed
    public StochasticVolumeRandomizer(int seed)
    {
        _rng = new Random(seed);
    }

    // Helper method for the Normal Distribution (Box-Muller)
    private float NextGaussian(float mu, float sigma)
    {
        // Use two uniform random numbers (u1, u2) to generate a Gaussian distribution
        double u1 = 1.0 - _rng.NextDouble();
        double u2 = 1.0 - _rng.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        return mu + sigma * (float)randStdNormal;
    }

    /// <summary>
    /// Generates the final, stochastic integer volume based on the deterministic mean.
    /// </summary>
    public VolumeCalculationResult GenerateFinalVolume(float meanVolumeMu, float baseOrderComplexity)
    {
        // 1. Calculate Sigma (Standard Deviation scales with Mu)
        float sigma = meanVolumeMu * SigmaTuningPercentage;

        // 2. Generate Raw Volume (Gaussian Distribution)
        float volumeRaw = NextGaussian(meanVolumeMu, sigma);

        // 3. Round and Apply Floor
        int volumeRounded = (int)Math.Round(volumeRaw);

        // Apply Minimum Floor: Volume must be at least V_base
        int volumeFinal = Math.Max(volumeRounded, (int)baseOrderComplexity);

        // Return all calculated components for logging
        return new VolumeCalculationResult
        {
            SigmaCalculated = sigma,
            VolumeRaw = volumeRaw,
            VolumeFinal = volumeFinal
        };
    }
}


// --- 4. MAIN EXECUTION LOGIC ---

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- STARTING DETERMINISTIC + STOCHASTIC SIMULATION ---");

        // --- SIMULATION PARAMETERS ---
        int testOrdersPerShift = 10;
        int totalOrdersToTest = 100;
        const int GameSeed = 1338; // Now defined here

        // Initialize the calculator, passing the seed
        DeterministicOrderComplexity calculator = new DeterministicOrderComplexity(GameSeed);

        List<ComplexityResult> allDetailedResults = new List<ComplexityResult>();

        // --- A. Run Simulation and Collect Detailed Data ---
        for (int n = 1; n <= totalOrdersToTest; n++)
        {
            var result = calculator.CalculateComplexity(n, testOrdersPerShift);
            allDetailedResults.Add(result);
        }

        // --- B. Group Data for Summary Section & Metadata ---
        var finalOutput = new SimulationOutput();
        finalOutput.DetailedOrderBreakdown = allDetailedResults;

        // Create the grouped dictionary (S -> [mu1, mu2, ...])
        // NOTE: This summary currently only includes the MEAN (Mu)
        finalOutput.ComplexitiesByShift = allDetailedResults
            .GroupBy(r => r.ShiftNumberS)
            .OrderBy(g => g.Key)
            .ToDictionary(
                group => group.Key,
                group => group.Select(r => (float)r.StochasticResults.VolumeFinal).ToList()
            );

        // Populate Metadata
        finalOutput.Metadata.Add("Description", "Stochastic Order Volume Simulation Output (Level 1 + Level 2)");
        finalOutput.Metadata.Add("GeneratedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        finalOutput.Metadata.Add("OrdersPerShift_X", testOrdersPerShift.ToString());
        finalOutput.Metadata.Add("GameSeed", GameSeed.ToString()); // Log the seed!

        finalOutput.Metadata.Add("V_base", DeterministicOrderComplexity.BaseOrderComplexity.ToString("F2"));
        finalOutput.Metadata.Add("Alpha", DeterministicOrderComplexity.AlphaLinearCoefficient.ToString("F2"));
        finalOutput.Metadata.Add("Gamma_Divisor", DeterministicOrderComplexity.ExponentialDivider.ToString("F2"));
        finalOutput.Metadata.Add("Beta_IntraShift", calculator.BetaIntraShiftCoefficient.ToString("F2"));
        finalOutput.Metadata.Add("Standard_deviation_mean_percent", calculator.getSigmaScalingFactor().ToString()); // Log the seed!


        // --- C. Serialize and Save to JSON ---
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(finalOutput, options);

            string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss");
            string fileName = $"final_order_volume_v_{formattedDateTime}_s{GameSeed}.json"; // Added seed to filename

            File.WriteAllText(fileName, jsonString);

            Console.WriteLine($"\n--- SIMULATION COMPLETE ---");
            Console.WriteLine($"Seed used: {GameSeed}");
            Console.WriteLine($"Wrote detailed results (1 to {totalOrdersToTest} orders) to: {Path.GetFullPath(fileName)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn error occurred while writing the file: {ex.Message}");
            Console.WriteLine("ACTION REQUIRED: Please ensure the 'System.Text.Json' NuGet package is installed.");
        }
    }
}