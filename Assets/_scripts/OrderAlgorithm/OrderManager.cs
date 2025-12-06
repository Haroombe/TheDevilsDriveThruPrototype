using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets._scripts.OrderAlgorithm
{
    public class OrderManager : MonoBehaviour
    {
        private List<ComplexityResult> _simulationResults = new List<ComplexityResult>();

        // Struct to hold the initial, default state of all parameters
        private struct InitialParameters
        {
            public float BaseComplexity;
            public float AlphaCoefficient;
            public float ExponentialDivider;
            public float BetaCoefficient;
            public float SigmaTuningPct;
            public float ItemPctNormalDistDivisor;
            public List<AllocationRule> ItemPrioritiesWeights;
        }

        private InitialParameters _initialParams;

        public OrderComplexity OrderGenerator { get; private set; }

        // Rules are public for Inspector/Runtime modification
        public List<AllocationRule> ItemPrioritiesWeights = new List<AllocationRule>
        {
            new AllocationRule { ItemName = "Burger", PrioritySlot = 1, MinPct = 0.2f, MaxPct = 0.6f, RandomnessType = RandomnessType.Normal },
            new AllocationRule { ItemName = "Fries", PrioritySlot = 2, MinPct = 0.2f, MaxPct = 0.7f, RandomnessType = RandomnessType.Normal },
            new AllocationRule { ItemName = "Soda", PrioritySlot = 3, MinPct = 0.3f, MaxPct = 0.4f, RandomnessType = RandomnessType.Normal }
        };

        public static OrderManager Instance { get; private set; }

        [Header("Order Complexity Algorithm Parameters - MODIFIABLE")]
        // These are now public fields (or public properties with private setters) for access/modification
        [field: SerializeField] public float BaseComplexity { get; set; } = 3.2f;
        [field: SerializeField] public float AlphaCoefficient { get; set; } = 4.8f;
        [field: SerializeField] public float ExponentialDivider { get; set; } = 2.6f;
        [field: SerializeField] public float BetaCoefficient { get; set; } = 0.6f;
        [field: SerializeField] public float SigmaTuningPct { get; set; } = 0.01f;
        [field: SerializeField] public float ItemPctNormalDistDivisor { get; set; } = 6.0f;

        private int _seed = -1;


        public void SetSeed(int newSeed)
        {
            if (_seed == -1)
            {
                _seed = newSeed;
                Debug.Log($"OrderManager seed set to: {_seed}");
                InitializeGenerator();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 1. CAPTURE INITIAL VALUES in Awake (before any Start() or external calls)
            CaptureInitialParameters();
        }

        /// <summary>
        /// Saves the initial values set in the Unity Inspector.
        /// </summary>
        private void CaptureInitialParameters()
        {
            _initialParams = new InitialParameters
            {
                BaseComplexity = BaseComplexity,
                AlphaCoefficient = AlphaCoefficient,
                ExponentialDivider = ExponentialDivider,
                BetaCoefficient = BetaCoefficient,
                SigmaTuningPct = SigmaTuningPct,
                ItemPctNormalDistDivisor = ItemPctNormalDistDivisor,
                // Deep clone the rules list to prevent modification of the initial state
                ItemPrioritiesWeights = ItemPrioritiesWeights.ConvertAll(rule => new AllocationRule
                {
                    ItemName = rule.ItemName,
                    PrioritySlot = rule.PrioritySlot,
                    MinPct = rule.MinPct,
                    MaxPct = rule.MaxPct,
                    RandomnessType = rule.RandomnessType
                })
            };
        }

        /// <summary>
        /// Initializes the OrderGenerator using the current serialized parameters and the seed.
        /// </summary>
        private void InitializeGenerator()
        {
            if (_seed == -1)
            {
                Debug.LogError("OrderGenerator cannot be initialized: Seed has not been set.");
                return;
            }

            OrderGenerator = new OrderComplexity(
                gameSeed: _seed,
                baseComplexity: BaseComplexity,
                alphaCoefficient: AlphaCoefficient,
                exponentialDivider: ExponentialDivider,
                betaCoefficient: BetaCoefficient,
                sigmaTuningPct: SigmaTuningPct,
                normalDivisor: ItemPctNormalDistDivisor
            );

            // Apply the current (potentially modified) rules
            OrderGenerator.UpdateAllocationRules(ItemPrioritiesWeights);

            Debug.Log("OrderGenerator initialized successfully.");
        }

        /// <summary>
        /// Resets the entire generator: restores all parameters to their initial Inspector state, 
        /// applies the new seed, and re-instantiates the generator.
        /// </summary>
        public void ResetGenerator(int newSeed)
        {
            // 1. RESTORE PARAMETERS TO INITIAL STATE
            BaseComplexity = _initialParams.BaseComplexity;
            AlphaCoefficient = _initialParams.AlphaCoefficient;
            ExponentialDivider = _initialParams.ExponentialDivider;
            BetaCoefficient = _initialParams.BetaCoefficient;
            SigmaTuningPct = _initialParams.SigmaTuningPct;
            ItemPctNormalDistDivisor = _initialParams.ItemPctNormalDistDivisor;

            // Restore the rules list by deep cloning the initial list back onto the current list
            ItemPrioritiesWeights = _initialParams.ItemPrioritiesWeights.ConvertAll(rule => new AllocationRule
            {
                ItemName = rule.ItemName,
                PrioritySlot = rule.PrioritySlot,
                MinPct = rule.MinPct,
                MaxPct = rule.MaxPct,
                RandomnessType = rule.RandomnessType
            });

            Debug.Log($"Parameters reset to initial defaults. Preparing to initialize with new seed: {newSeed}");

            // 2. UPDATE SEED AND RE-INITIALIZE GENERATOR
            _seed = newSeed;
            InitializeGenerator();
        }

        public Order generateOrder(int totalOrderNum, int ordersPershift)
        {
            if (OrderGenerator == null)
            {
                Debug.LogError("OrderGenerator is not initialized. Cannot generate order.");
                return null;
            }
            ComplexityResult orderdetails = OrderGenerator.CalculateComplexity(totalOrderNum, ordersPershift);

            _simulationResults.Add(orderdetails); // LOGGING: Add the full result to the buffer

            // --- Extract Item Quantities for Game Order ---
            int totalVolume = orderdetails.StochasticResults.VolumeFinal;

            int burgers = orderdetails.OrderComponents.Items
                .FirstOrDefault(item => item.ItemName == "Burger")?.Quantity ?? 0;

            int fries = orderdetails.OrderComponents.Items
                .FirstOrDefault(item => item.ItemName == "Fries")?.Quantity ?? 0;

            int sodas = orderdetails.OrderComponents.Items
                .FirstOrDefault(item => item.ItemName == "Soda")?.Quantity ?? 0;

            // 3. Create the final Order object
            Order newOrder = new Order(
                burgers: burgers,
                fries: fries,
                sodas: sodas,
                total_volume: totalVolume
            );

            // Optional: Log the new order details
            Debug.Log($"New Order N={totalOrderNum}: V={totalVolume}, B={burgers}, F={fries}, S={sodas}");

            return newOrder;
        }

        /// <summary>
        /// Unity Lifecycle Method: Called when the application is quitting or closing.
        /// </summary>
        public void OnApplicationQuit()
        {
            WriteSimulationToFile();
        }

        /// <summary>
        /// Serializes the collected simulation data into a JSON file with Metadata, 
        /// ComplexitiesByShift summary, and the DetailedOrderBreakdown.
        /// </summary>
        private void WriteSimulationToFile()
        {
            // Safety check: Don't write if no data or generator is missing.
            if (_simulationResults.Count == 0 || OrderGenerator == null)
            {
                Debug.Log("OrderManager: No simulation data or generator is missing. File not written.");
                return;
            }

            try
            {
                var finalOutput = new SimulationOutput();

                // --- 1. DETAILED ORDER BREAKDOWN ---
                finalOutput.DetailedOrderBreakdown = _simulationResults;

                // --- 2. COMPLEXITIES BY SHIFT (Summary Data) ---
                // Groups all results by the shift number and creates a summary dictionary for each order.
                finalOutput.ComplexitiesByShift = _simulationResults
                    .GroupBy(r => r.ShiftNumberS)
                    .OrderBy(g => g.Key)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(r =>
                        {
                            var orderSummary = new Dictionary<string, object>
                            {
                                { "Volume_Final", r.StochasticResults.VolumeFinal },
                                { "Local_Order_Number_N", r.LocalOrderO },
                                { "Order_Number_N", r.OrderNumberN }
                            };

                            // Add all allocated item quantities from OrderComponentResult
                            foreach (var item in r.OrderComponents.Items)
                            {
                                orderSummary.Add(item.ItemName + "_Count", item.Quantity);
                            }
                            return orderSummary;
                        }).ToList()
                    );

                // --- 3. METADATA ---

                // Get OrdersPerShift from the first entry (assuming it's constant)
                int ordersPerShift = _simulationResults.First().OrdersPerShift;

                finalOutput.Metadata.Add("Description", "Stochastic Order Volume Simulation Output (L1 Mean, L2 Volume, L3 Components)");
                finalOutput.Metadata.Add("GeneratedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                // Use the retrieved value
                finalOutput.Metadata.Add("OrdersPerShift_X", ordersPerShift.ToString());
                // Use the stored _seed variable
                finalOutput.Metadata.Add("StartingGameSeed", _seed.ToString());

                // Add complexity coefficients from the OrderGenerator object
                finalOutput.Metadata.Add("V_base", OrderGenerator.BaseOrderComplexity.ToString("F2"));
                finalOutput.Metadata.Add("Alpha", OrderGenerator.AlphaLinearCoefficient.ToString("F2"));
                finalOutput.Metadata.Add("Gamma_Divisor", OrderGenerator.ExponentialDivider.ToString("F2"));
                finalOutput.Metadata.Add("Beta_IntraShift", OrderGenerator.BetaIntraShiftCoefficient.ToString("F2"));
                finalOutput.Metadata.Add("Sigma_Scaling_Pct", OrderGenerator.GetSigmaScalingFactor().ToString("F2"));

                // --- 4. Serialize and Save ---

                // Use Newtonsoft.Json for serialization with formatting
                string jsonString = JsonConvert.SerializeObject(finalOutput, Formatting.Indented);

                string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = $"order_log_{formattedDateTime}_s{_seed}.json";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);

                File.WriteAllText(filePath, jsonString);

                Debug.Log($"OrderManager: Wrote simulation data ({_simulationResults.Count} records) to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"OrderManager: Failed to write simulation file! Error: {ex.Message}");
            }
        }
    }
}