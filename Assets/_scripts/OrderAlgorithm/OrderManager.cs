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
        //private List<ComplexityResult> _simulationResults = new List<ComplexityResult>();
        private List<RunLogData> _allRunsData = new List<RunLogData>();
        // Struct to hold the initial, default state of all parameters
        private RunLogData _currentRunData;
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
        public EconomicSnapshot EconomicData { get; set; }

        private InitialParameters _initialParams;

        public OrderComplexity OrderGenerator { get; private set; }

        // Rules are public for Inspector/Runtime modification
        public List<AllocationRule> ItemPrioritiesWeights = new List<AllocationRule>
        {
            new AllocationRule { ItemName = "Burger", PrioritySlot = 1, MinPct = 0.2f, MaxPct = 0.6f, RandomnessType = RandomnessType.Normal },
            new AllocationRule { ItemName = "Soda", PrioritySlot = 2, MinPct = 0.2f, MaxPct = 0.7f, RandomnessType = RandomnessType.Normal },
            new AllocationRule { ItemName = "Fries", PrioritySlot = 3, MinPct = 0.3f, MaxPct = 0.4f, RandomnessType = RandomnessType.Normal }
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
                ResetGenerator(_seed);
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
            _currentRunData = new RunLogData();
            _allRunsData.Add(_currentRunData); // Add to the master list immediately

            // --- METADATA CAPTURE ---
            _currentRunData.Metadata.Add("RunNumber", GameManager.Instance.runCount);
            _currentRunData.Metadata.Add("GeneratedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            _currentRunData.Metadata.Add("StartingGameSeed", _seed.ToString());

            // Order Generator Static Params
            _currentRunData.Metadata.Add("V_base", OrderGenerator.BaseOrderComplexity.ToString("N2"));
            _currentRunData.Metadata.Add("Alpha", OrderGenerator.AlphaLinearCoefficient.ToString("N2"));
            _currentRunData.Metadata.Add("Gamma_Divisor", OrderGenerator.ExponentialDivider.ToString("N2"));
            _currentRunData.Metadata.Add("Beta_IntraShift", OrderGenerator.BetaIntraShiftCoefficient.ToString("N2"));
            _currentRunData.Metadata.Add("Sigma_Scaling_Pct", OrderGenerator.GetSigmaScalingFactor().ToString("N2"));

            // Economy Manager Static Params
            if (EconomyManager.Instance != null)
            {
                var eco = EconomyManager.Instance;
                _currentRunData.Metadata.Add("Eco_BaseInflationRate", eco.baseInflationRate.ToString("F3"));
                _currentRunData.Metadata.Add("Eco_DifficultyConstantK", eco.difficultyConstantK.ToString("F3"));
                _currentRunData.Metadata.Add("Eco_MaxDifficultyShift", eco.maxDifficultyShift.ToString());
                _currentRunData.Metadata.Add("Eco_RunDifficultyIncreaseRate", eco.runDifficultyIncreaseRate.ToString("F3"));
                _currentRunData.Metadata.Add("Eco_MaxRunDifficulty", eco.maxRunDifficulty.ToString("F3"));
            }
        }

        public Order generateOrder(int totalOrderNum, int ordersPershift)
        {
            if (OrderGenerator == null)
            {
                Debug.LogError("OrderGenerator is not initialized. Cannot generate order.");
                return null;
            }

            if (_currentRunData == null)
            {
                // Safety check: This shouldn't happen if ResetGenerator is called at the start of a run.
                Debug.LogError("No active run data! Please call ResetGenerator first.");
                return null;
            }
            ComplexityResult orderdetails = OrderGenerator.CalculateComplexity(totalOrderNum, ordersPershift);
            Debug.Log("generating order details.");

            if (GameManager.Instance != null && EconomyManager.Instance != null) // Check EconomyManager.Instance access
            {
                var eco = EconomyManager.Instance;


                // You'll need the current shift number to get the curve value

                orderdetails.EconomicData = new EconomicSnapshot
                {
                    // Unit Values
                    ClampedShiftNumber = eco.clampedshift, // Assuming eco exposes this
                    RunDifficultyMultiplier = eco.CurrentRunMultiplier,
                    DifficultyCurveValue = eco.curbaseCostRatio, // Using the variable name from your script
                    PriceMultiplier = eco.priceMultiplier,   // Assuming eco exposes this
                    EffectiveCostRatio = 1.0f, // Assuming eco exposes this

                    // Final Unit Values
                    BurgerCost = eco.CurBurgerCost,
                    BurgerPrice = eco.CurBurgerPrice,

                    // Derived Margins (Always calculate Margin in the snapshot for consistency)
                    BurgerMargin = eco.CurBurgerPrice - eco.CurBurgerCost,
                    FriesCost = eco.CurFriesCost,
                    FriesPrice = eco.CurFriesPrice,
                    FriesMargin = eco.CurFriesPrice - eco.CurFriesCost,
                    SodaCost = eco.CurSodaCost,
                    SodaPrice = eco.CurSodaPrice,
                    SodaMargin = eco.CurSodaPrice - eco.CurSodaCost
                };
            }
            _currentRunData.DetailedOrderBreakdown.Add(orderdetails);

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
                total_volume: totalVolume,
                ShiftNum: GameManager.Instance.shiftNum
            );

            // Optional: Log the new order details
            Debug.Log($"New Order N={totalOrderNum}: V={totalVolume}, B={burgers}, F={fries}, S={sodas} BulkB={newOrder.BurgerBulkBuyAmount} BulkF={newOrder.FriesBulkBuyAmount} BulkS={newOrder.SodaBulkBuyAmount}");

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
        /// 
        private void WriteSimulationToFile()
        {
            // Safety check: Don't write if no data has been collected across any runs.
            if (_allRunsData.Count == 0 || OrderGenerator == null)
            {
                Debug.Log("OrderManager: No simulation data collected across any runs. File not written.");
                return;
            }

            try
            {
                int minutes = (int)(GameManager.Instance.TrueElapsedTime / 60f);
                int seconds = (int)(GameManager.Instance.TrueElapsedTime % 60f);
                string Timetext = $"{minutes:00}:{seconds:00}";
                
                var datacontainer = new
                {
                    TotalPlayTime = Timetext,
                    bonusperOrder = GameManager.Instance.flatProfitPerOrder,
                    bonusperShift = GameManager.Instance.flatProfitPerShift,
                    Runs = _allRunsData
                };
                // We now serialize the list of all runs directly. 
                // The structure of _allRunsData already contains all metadata and detailed breakdowns, 
                // organized by run.

                // --- 1. Serialize ---

                // Use Newtonsoft.Json for serialization with formatting.
                // We serialize the master list of RunLogData objects.
                string jsonString = JsonConvert.SerializeObject(
                    datacontainer, Formatting.Indented);

                // --- 2. Save ---

                string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                // Use a unique identifier from the first run (if available) or a general identifier.
                // If _seed is only set once for the whole session, it's fine.
                string fileName = $"tddt_v1_rundata_{formattedDateTime}_s{_seed}.json";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);

                File.WriteAllText(filePath, jsonString);

                Debug.Log($"OrderManager: Wrote simulation data ({_allRunsData.Count} runs) to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"OrderManager: Failed to write simulation file! Error: {ex.Message}");
            }

            // NOTE: Section 2 (Complexities by Shift Summary) is removed from the file writing 
            // because it requires querying the combined data, which is now structured per run.
            // If you need it, you must perform the LINQ aggregation *across* all runs 
            // (e.g., _allRunsData.SelectMany(r => r.DetailedOrderBreakdown).GroupBy(...) ).
        }
        
    }
}