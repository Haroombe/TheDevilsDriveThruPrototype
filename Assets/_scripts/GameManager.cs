using Assets._scripts.OrderAlgorithm;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // --- Init ---
    private int lastDisplayedSecond = -1;
    [SerializeField] private bool useRandomSeedInit = false;
    [SerializeField] private bool newSeedOnReset = false;
    [SerializeField] private int fixedSeed = 1338;
    private int gameSeed;
    private bool isFirstRun = true;
    public int runCount = 1;
    public float largestpayout = 0f;

    // --- Constants ---
    private const string POSITIVE_COLOR = "#53C04E"; // Green
    private const string NEGATIVE_COLOR = "#FF0811"; // Red

    // Helper function for color-coded text
    private string FormatProfitText(float amount)
    {
        string sign = amount >= 0 ? "+" : "-";
        string color = amount >= 0 ? POSITIVE_COLOR : NEGATIVE_COLOR;
        float absAmount = Mathf.Abs(amount);

        return $"<color={color}>{sign}${absAmount:F2}</color>";
    }

    public void SetGameSeed()
    {
        bool shouldGenerateNewSeed;

        if (isFirstRun)
        {
            runCount = 1;
            shouldGenerateNewSeed = useRandomSeedInit;
            isFirstRun = false;
        }
        else if (!isFirstRun && newSeedOnReset)
        {
            runCount++;
            shouldGenerateNewSeed = true;
        }
        else if (!isFirstRun && !newSeedOnReset)
        {
            runCount++;
            return;
        }
        else
        {
            runCount++;
            Debug.LogWarning("Game seed edge case hit");
            shouldGenerateNewSeed = false;
        }

        if (shouldGenerateNewSeed)
        {
            int newSeed = (int)DateTime.Now.Ticks;
            if (newSeed < 0) newSeed = -newSeed;
            gameSeed = newSeed;
            Debug.Log($"GameManager: Generated new random seed: {gameSeed}");
        }
        else
        {
            gameSeed = fixedSeed;
            Debug.Log($"GameManager: Using fixed seed for run: {gameSeed}");
        }
        UpdateSeedNum(gameSeed);
    }

    public enum GameState
    {
        Null,
        Initializing,
        ShiftOver,
        OrderStart,
        OrderFilled,
        Reset,
        GameOverRestart,
    }

    [Header("UI Elements")]
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private Canvas pauseCanvas;
    [SerializeField] private Canvas gameOverCanvas;
    public ModShopUI modShopUI;
    public ModBookUI bookUI;

    [SerializeField] private OrderHistoryDisplay curOrderDisplayDebug;

    [Header("Inventory UI Elements")]
    [SerializeField] private TextMeshProUGUI MoneyAmountText;
    [SerializeField] private TextMeshProUGUI SodaAmountText;
    [SerializeField] private TextMeshProUGUI BurgerAmountText;
    [SerializeField] private TextMeshProUGUI FriesAmountText;
    [SerializeField] private TextMeshProUGUI TimerText;
    [SerializeField] private TextMeshProUGUI BurgerMultText;
    [SerializeField] private TextMeshProUGUI FriesMultText;
    [SerializeField] private TextMeshProUGUI SodaMultText;



    // --- ITEM RATES UI (3 columns: Cost, Price, Net Profit) ---
    [Header("Items Text")]
    [SerializeField] private TextMeshPro BurgerCostText;
    [SerializeField] private TextMeshPro SodaCostText;
    [SerializeField] private TextMeshPro FriesCostText;

    [SerializeField] private TextMeshPro BurgerSellText; // This is now SELL PRICE
    [SerializeField] private TextMeshPro SodaSellText;   // This is now SELL PRICE
    [SerializeField] private TextMeshPro FriesSellText;  // This is now SELL PRICE

    [SerializeField] private TextMeshPro BurgerNetProfitText; // This is the new Net Profit (Price - Cost)
    [SerializeField] private TextMeshPro FriesNetProfitText;
    [SerializeField] private TextMeshPro SodaNetProfitText;

    [Header("Round Tracking")]
    [SerializeField] private TextMeshProUGUI ShiftUITextField;
    [SerializeField] private TextMeshProUGUI OrderUITextField;

    [Header("Metadata")]
    [SerializeField] private TextMeshProUGUI SeedNumTextField;

    // --- ORDER SUMMARY UI (Items + Payout, Cost, Net Profit) ---
    [Header("Order UI")]
    [SerializeField] private TextMeshPro BurgerOrderText;
    [SerializeField] private TextMeshPro FriesOrderText;
    [SerializeField] private TextMeshPro SodaOrderText;
    [SerializeField] private TextMeshPro OrderPayout; // TotalPayout (Revenue)
    [SerializeField] private TextMeshPro OrderPayoutMultiplier; 

    [SerializeField] private TextMeshPro OrderNumText;
    [SerializeField] private TextMeshPro OrderPriceText; // TotalCost
    [SerializeField] private TextMeshPro OrderNetRevenueText; // NetProfit
    [SerializeField] private TextMeshPro OrderBonusText;
    [SerializeField] private TextMeshPro ShiftBonusText;


    [Header("PromptPanels")]
    [SerializeField] public GameObject GiveUpPanel;
    [SerializeField] public GameObject GameOverPanel;

    public void SetPanelVisibility(GameObject panel, bool setActive)
    {
        gameOverCanvas.enabled = true;
        if (setActive)
        {
            panel.SetActive(true);
        }
        else
        {
            panel.SetActive(false);
        }
    }
    public void TogglePanel(GameObject panel)
    {
        SetPanelVisibility(panel, !panel.activeSelf);
    }

    public List<Order> orderHistory = new List<Order>();

    public void UpdateShiftUI(int shift)
    {
        if (ShiftUITextField != null)
            ShiftUITextField.text = $"Shift: {shift.ToString()}";
    }
    public void UpdateOrderUI(int order)
    {
        if (OrderUITextField != null)
            OrderUITextField.text = $"Order: {order.ToString()}";
    }
    public void UpdateSeedNum(int seed)
    {
        if (SeedNumTextField != null)
            SeedNumTextField.text = $"Seed: {seed.ToString()}";
    }

    /// <summary>
    /// Updates the Order Summary UI using the financial data from the current order.
    /// </summary>
    public void UpdateOrderValuesUI(Order order)
    {
        if (BurgerOrderText != null)
            BurgerOrderText.text = $"{order.burgerOrderAmount.ToString()}";
        if (FriesOrderText != null)
            FriesOrderText.text = $"{order.friesOrderAmount.ToString()}";
        if (SodaOrderText != null)
            SodaOrderText.text = $"{order.sodaOrderAmount.ToString()}";

        if (OrderNumText != null)
            OrderNumText.text = $"Order #{OrderNum.ToString()} Summary";

        // Display Total Payout (Revenue)
        if (OrderPayout != null)
            OrderPayout.text = $"+ ${order.TotalPayoutNonMod:F2}";

        if (OrderPayoutMultiplier != null && order.orderPayoutMultiplier != 1f)
        { OrderPayoutMultiplier.text = $"x{order.orderPayoutMultiplier:F2}"; }
        else
        {
            OrderPayoutMultiplier.text = "";
        }


        // Display Total Cost (as a negative)
        if (OrderPriceText != null)
            OrderPriceText.text = $"- ${order.TotalCost:F2}";

        // Display Net Profit (Color-coded)
        if (OrderNetRevenueText != null)
            OrderNetRevenueText.text = FormatProfitText(order.NetProfit);

        if (OrderBonusText != null)
            OrderBonusText.text = $"+ ${flatProfitPerOrder*shiftNum:F2}";

        if (ShiftBonusText != null)
            ShiftBonusText.text = $"+ ${flatProfitPerShift*shiftNum:F2}";
    }

    // starting values
    [Header("Starting Values")]
    public float startingMoney = 25.00f;
    public int startingSoda = 0;
    public int startingBurgers = 0;
    public int startingFries = 0;
    public int shiftNum = 1;
    public int OrdersPerShift = 5;
    private int OrderNum = 1;
    private int TotalOrderNum = 1;

    public int getOrderNum()
    {
        return OrderNum;
    }
    private GameState curGameState;

    // game variables
    [Header("Game Variables")]
    public int playerSoda;
    public int playerBurgers;
    public int playerFries;
    public float playerMoney;

    public Order currentOrder;

    // --- REPLACED: Removed old cost/profit variables. EconomyManager handles these.
    // --- ONLY KEEPING FLAT BONUSES ---
    [Header("player multiplier affectors")]
    [SerializeField] public float startingflatProfitPerOrder = 0f;
    [SerializeField] public float startingflatProfitPerShift = 0f;
    public float flatProfitPerOrder;
    public float flatProfitPerShift;

    // --- REFACTORED: Simplified initRates() and UpdateRateUI() ---
    private void initRates()
    {
        // Only initialize flat profits here, real rates come from EconomyManager
        flatProfitPerOrder = startingflatProfitPerOrder;
        flatProfitPerShift = startingflatProfitPerShift;

        UpdateRateUI();
    }

    /// <summary>
    /// Updates the Item Cost/Price/Profit section of the UI using EconomyManager data.
    /// </summary>
    public void UpdateRateUI()
    {
        var eco = EconomyManager.Instance;

        // --- BURGER UI ---
        BurgerCostText.text = $"${eco.CurBurgerCost:F2}";
        BurgerSellText.text = $"${eco.CurBurgerPrice:F2}"; // Selling Price (Renamed from Profit)
        float burgerNetProfit = eco.CurBurgerPrice - eco.CurBurgerCost;
        BurgerNetProfitText.text = FormatProfitText(burgerNetProfit);

        // --- FRIES UI ---
        FriesCostText.text = $"${eco.CurFriesCost:F2}";
        FriesSellText.text = $"${eco.CurFriesPrice:F2}"; // Selling Price
        float friesNetProfit = eco.CurFriesPrice - eco.CurFriesCost;
        FriesNetProfitText.text = FormatProfitText(friesNetProfit);

        // --- SODA UI ---
        SodaCostText.text = $"${eco.CurSodaCost:F2}";
        SodaSellText.text = $"${eco.CurSodaPrice:F2}"; // Selling Price
        float sodaNetProfit = eco.CurSodaPrice - eco.CurSodaCost;
        SodaNetProfitText.text = FormatProfitText(sodaNetProfit);
    }

    // timer
    private float elapsedTime = 0f;
    public float TrueElapsedTime = 0f;


    // Actionse
    public event Action ResetGameAction;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Debug.Log("GameManager Awake");
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResumeGame();
    }

    private void Start()
    {
        // Ensure OrderManager is set up before the GameLoop starts
        SetGameSeed();
        ModManager.Instance.seed = gameSeed;


        OrderManager.Instance.SetSeed(gameSeed);
        AudioManager.Instance.PlayBGM(0);

        GameLoop(GameState.Initializing);
    }

    private void Update()
    {
        HandleInput();
        elapsedTime += Time.deltaTime;
        TrueElapsedTime += Time.deltaTime;
        int currentSecond = (int)elapsedTime;
        if (currentSecond != lastDisplayedSecond)
        {
            updateTimerUI();
            lastDisplayedSecond = currentSecond;
        }
        updateTimerUI();

    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (GameManager.Instance.IsPaused())
            {
                GameManager.Instance.ResumeGame();
            }
            else
            {
                GameManager.Instance.PauseGame();
            }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ResetGame();
        }
        // ... (other input handling)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bookUI.ToggleUI();
            if (!isPaused)
            {
                ModBookPause();
            } else
            {
                ModBookUnPause();
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentOrder != null)
            {

                BuyBurger(currentOrder.burgerOrderAmount,true);
                BuyFries(currentOrder.friesOrderAmount, true);
                BuySoda(currentOrder.sodaOrderAmount, true);
                FulfillOrder();
            }

        }
        //{
        //    SetPanelVisibility(GiveUpPanel, true);
        //}
        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    SetPanelVisibility(GiveUpPanel, false);
        //}
    }

    // game loop
    public void GameLoop(GameState state)
    {
        if (curGameState == state) return;
        curGameState = state;

        switch (state)
        {
            case GameState.Initializing:
                Debug.Log("initializing");
                currentOrder = null;

                // --- INTEGRATION: Initialize Economy ---
                EconomyManager.Instance.InitializeEconomy(runCount);

                initRates();
                initializePlayerItems();
                UpdateAllInventoryUI();
                InitializeFirstShiftOrder();
                ResumeGame();
                GameLoop(GameState.OrderStart);
                break;

            case GameState.OrderStart:

                ModManager.Instance.OnOrderStartInitializeActiveMods();
                Debug.Log($"{ModManager.Instance.activeMods.Count} Mods initialized for Order #{OrderNum}");

                if (shiftNum == 2 && OrderNum == 1)
                {
                    FadingMessage.Instance.ShowMessageAfterDelay(5f, "Press 'space' to show active mods", true, .5f, 2f);
                }


                Debug.Log("order number " + OrderNum.ToString() + " start");
                CustomerManager.Instance.SpawnToMid();

                // Creates order, which uses EconomyManager prices to calculate Payout, Cost, and NetProfit
                currentOrder = CreateOrder();

                UpdateOrderValuesUI(currentOrder);
                break;

            case GameState.OrderFilled:
                Debug.Log("order number " + OrderNum.ToString() + " filled during shift " + shiftNum.ToString());
                AudioManager.Instance.PlaySFX("ThankYou", playInstantly: false, minDelay: 0.15f, maxDelay: 0.35f);

                OrderNum++;
                TotalOrderNum++;
                if (ModManager.Instance.activeMods.Count > 0)
                {
                    ModManager.Instance.OnOrderFilledActiveMods();
                    Debug.Log($"Mods Usage Consumption triggered");
                }

                if (OrderNum >= OrdersPerShift + 1)
                {
                    Debug.Log("shift number " + shiftNum.ToString() + " over");
                    shiftNum++;
                    OrderNum = 1;
                    addAmountF(ref playerMoney, flatProfitPerShift*shiftNum);

                    // --- INTEGRATION: Update Economy for Next Shift ---
                    EconomyManager.Instance.UpdateShiftEconomy(shiftNum);

                    //UpdateRateUI(); // Refresh UI with the new Shift prices

                    UpdateOrderUI(OrderNum);
                    UpdateShiftUI(shiftNum);
                    FadingMessage.Instance.ShowCallout($"Starting Shift {shiftNum}", .9f, 1.9f);
                    GameLoop(GameState.ShiftOver);

                }
                else
                {
                    addAmountF(ref playerMoney, flatProfitPerOrder);

                    Debug.Log("starting next order number " + OrderNum.ToString());
                    UpdateOrderUI(OrderNum);
                    GameLoop(GameState.OrderStart);

                }

                break;
            case GameState.ShiftOver:
                ModShopPause();
                bool modsDisplayed = modShopUI.PlayerModChoiceSelection();
                // unpause and calling gameloop order start handled on mouse click
                if (!modsDisplayed)
                {
                    modShopUI.turnOffModCanvas();
                    ModShopUnPause();
                    FadingMessage.Instance.ShowMessage("Not enough Mods to display, skipping mod selection...");
                    GameLoop(GameState.OrderStart);
                }
                break;

            case GameState.GameOverRestart:
                ResetGame();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }


    private void InitializeFirstShiftOrder()
    {
        TotalOrderNum = 1;
        shiftNum = 1;
        OrderNum = 1;
        FadingMessage.Instance.ShowCallout($"Starting Shift {shiftNum}", .9f, 2f);

        UpdateShiftUI(shiftNum);
        UpdateOrderUI(OrderNum);
    }

    public Order CreateOrder()
    {
        return OrderManager.Instance.generateOrder(TotalOrderNum, OrdersPerShift);
    }


    // --- Helper Math Methods ---
    private void UpdateUI<T>(TextMeshProUGUI field, T value)
    {
        field.text = value.ToString();
    }

    private void UpdateUIF(TextMeshProUGUI field, float value, string format)
    {
        field.text = value.ToString(format);
    }

    private void setAmountF(ref float baseAmount, float amount)
    {
        baseAmount = amount;
    }

    private void setAmount(ref int baseAmount, int amount)
    {
        baseAmount = amount;
    }

    private void addAmountF(ref float baseAmount, float amount)
    {
        baseAmount += amount;
    }

    public void addAmount(ref int baseAmount, int amount)
    {
        baseAmount += amount;
    }

    private void minusAmountF(ref float baseAmount, float amount, float mult = 1)
    {
        baseAmount = (baseAmount*mult) - amount;
        baseAmount = Mathf.Max(0, baseAmount);
    }

    private void minusAmount(ref int baseAmount, int amount, int mult = 1)
    {
        baseAmount = (baseAmount * mult) - amount;
        baseAmount = Mathf.Max(0, baseAmount);

    }
    // ---------------------------


    private void initializePlayerItems()
    {
        Debug.Log("Initializing player items");

        setAmountF(ref playerMoney, startingMoney);
        setAmount(ref playerSoda, startingSoda);
        setAmount(ref playerBurgers, startingBurgers);
        setAmount(ref playerFries, startingFries);
    }
    public void UpdateAllInventoryUI()
    {
        UpdateUIF(MoneyAmountText, playerMoney, "F2");
        UpdateUI(SodaAmountText, playerSoda);
        UpdateUI(BurgerAmountText, playerBurgers);
        UpdateUI(FriesAmountText, playerFries);
        int burgerMult = ModManager.Instance.GetTotalBurgerWeightMultiplier();
        int friesMult = ModManager.Instance.GetTotalFriesWeightMultiplier();
        int sodaMult = ModManager.Instance.GetTotalSodaWeightMultiplier();

        if (burgerMult != 1)
        {
            UpdateUI(BurgerMultText, $"x{burgerMult}");
        } else { UpdateUI(BurgerMultText, $""); }
        if (friesMult != 1)
        {
            UpdateUI(FriesMultText, $"x{friesMult}");
        } else { UpdateUI(FriesMultText, $""); }
        if (sodaMult != 1)
        {
            UpdateUI(SodaMultText, $"x{sodaMult}");
        } else { UpdateUI(SodaMultText, $""); }

    }

    private void updateTimerUI()
    {
        int minutes = (int)(elapsedTime / 60f);
        int seconds = (int)(elapsedTime % 60f);
        string text = $"{minutes:00}:{seconds:00}";
        TimerText.text = text;
    }

    // Game data
    private bool isPaused = false;

    // --- Public Methods ---
    public void ModShopPause()
    {
        Cursor.visible = true;
        isPaused = true;
        Time.timeScale = 0f;
        gameCanvas.enabled = false;
        pauseCanvas.enabled = false;


    }
    public void ModBookPause()
    {
        Cursor.visible = true;
        isPaused = true;
        Time.timeScale = 0f;
        gameCanvas.enabled = false;
        pauseCanvas.enabled = false;
        SetPanelVisibility(GiveUpPanel, false);
        SetPanelVisibility(GameOverPanel, false);

    }
    public void ModBookUnPause()
    {
        Cursor.visible = false;
        isPaused = false;
        Time.timeScale = 1f;
        gameCanvas.enabled = true;
        pauseCanvas.enabled = false;
        SetPanelVisibility(GiveUpPanel, false);
        SetPanelVisibility(GameOverPanel, false);

    }

    public void ModShopUnPause()
    {
        Cursor.visible = false;
        isPaused = false;
        Time.timeScale = 1f;
        
        gameCanvas.enabled = true;
        pauseCanvas.enabled = false;
        gameOverCanvas.enabled = false;
        SetPanelVisibility(GiveUpPanel, false);
        SetPanelVisibility(GameOverPanel, false);

    }
    public void PauseGame(bool skip_canvas = false)
    {
        Cursor.visible = true;
        isPaused = true;
        Time.timeScale = 0f;
        gameCanvas.enabled = false;
        if (skip_canvas)
        { 
            pauseCanvas.enabled = false; }
        else
        {
            pauseCanvas.enabled = true;
        }
    }

    public void ResumeGame()
    {
        Cursor.visible = false;
        isPaused = false;
        Time.timeScale = 1f;
        gameCanvas.enabled = true;
        pauseCanvas.enabled = false;
        gameOverCanvas.enabled = false;
        SetPanelVisibility(GiveUpPanel, false);
        SetPanelVisibility(GameOverPanel, false);
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public void ResetGame()
    {
        SetGameSeed();
        OrderManager.Instance.ResetGenerator(gameSeed);
        ModManager.Instance.seed = gameSeed;
        ModManager.Instance.ResetMods();

        curGameState = GameState.Null;
        ResetGameAction?.Invoke();
        isPaused = false;
        Time.timeScale = 1f;

        elapsedTime = 0f;
        GameLoop(GameState.Initializing);
    }


    // checks
    public void FulfillOrder()
    {
        if (!canFulfillOrder(currentOrder))
        {
            FadingMessage.Instance.ShowMessage("Can't fill order!");
            AudioManager.Instance.PlaySFX("No", playInstantly: true);
            return;
        }

        // Deduct inventory
        AudioManager.Instance.PlaySFX("Pay", playInstantly: true);

        minusAmount(ref playerBurgers, currentOrder.burgerOrderAmount);
        minusAmount(ref playerFries, currentOrder.friesOrderAmount);
        minusAmount(ref playerSoda, currentOrder.sodaOrderAmount);

        // Add money (TotalPayout is used)
        addAmountF(ref playerMoney, currentOrder.TotalPayout);
        largestpayout = Mathf.Max(largestpayout, currentOrder.TotalPayout);

        //fulfilled timestamp
        currentOrder.FulfilledTime = elapsedTime;
        currentOrder.CalculateFulfilledTimeString();

        // Add to history
        orderHistory.Add(currentOrder);

        // Update UI
        UpdateAllInventoryUI();

        FadingMessage.Instance.ShowMessage($"Order #{OrderNum} filled!", true, .6f, 2f);

        CustomerManager.Instance.ServeCustomer();
        GameLoop(GameState.OrderFilled);
    }

    // current player inventory can fulfill order
    public bool canFulfillOrder(Order curOrder)
    {
        bool hasEnoughBurgers = playerBurgers * ModManager.Instance.GetTotalBurgerWeightMultiplier() >= curOrder.burgerOrderAmount;
        bool hasEnoughFries = playerFries * ModManager.Instance.GetTotalFriesWeightMultiplier() >= curOrder.friesOrderAmount;
        bool hasEnoughSoda = playerSoda * ModManager.Instance.GetTotalSodaWeightMultiplier() >= curOrder.sodaOrderAmount;

        return hasEnoughBurgers && hasEnoughFries && hasEnoughSoda;
    }


    /// Replaces the current order with a new one based on external factors (Reroll or Modifier).
    /// </summary>
    public void ReplaceCurrentOrder(int burgers, int fries, int sodas)
    {

        // Create the new Order object, which calculates all financials and bulk amounts from scratch.
        int newVolume = burgers + fries + sodas;
        currentOrder = new Order(burgers, fries, sodas, newVolume, shiftNum);
        Debug.Log($"Order replaced: B={burgers}, F={fries}, S={sodas}.  BulkB={currentOrder.BurgerBulkBuyAmount} BulkF={currentOrder.FriesBulkBuyAmount} BulkS={currentOrder.SodaBulkBuyAmount} Creating new Order object.");

        // Update the UI to show the new order amounts and financials.
        UpdateOrderValuesUI(currentOrder);
    }



    // 1. Generic Buy Function Overload (accepts amount)
    private void BuyFood(ref int playerInventory, float costPerItem, TextMeshProUGUI inventoryText, string foodName, int amount, bool _isBulk=false)
    {
        float totalCost = costPerItem * amount;
        if (_isBulk)
        {
            totalCost *= ModManager.Instance.GetTotalBulkPriceMultiplier(); // MODS
        }

        if (playerMoney >= totalCost)
        {
            AudioManager.Instance.PlaySFX("Buy", playInstantly: true);

            minusAmountF(ref playerMoney, totalCost);
            addAmount(ref playerInventory, amount);

            UpdateAllInventoryUI(); // Update all UI for safety

            FadingMessage.Instance.ShowMessage($"Bought {amount} {foodName}(s)!", true);
        }
        else
        {
            AudioManager.Instance.PlaySFX("No", playInstantly: true);

            FadingMessage.Instance.ShowMessage($"Need ${totalCost:F2} for {amount} {foodName}(s)!");
        }
    }

    // 2. Modified Simplified buy methods to call the BULK overload:

    // Modify the existing BuyBurger to use the new bulk method (defaulting to 1)
    public void BuyBurger()
    {
        BuyBurger(1); // Default to buying 1
    }
    public void BuyBurger(int amount, bool isBulk = false)
    {
        float unitCost = EconomyManager.Instance.CurBurgerCost;
        BuyFood(ref playerBurgers, unitCost, BurgerAmountText, "burger", amount, isBulk);
    }

    // Modify the existing BuyFries to use the new bulk method (defaulting to 1)
    public void BuyFries()
    {
        BuyFries(1); // Default to buying 1
    }
    public void BuyFries(int amount, bool isBulk= false)
    {
        float unitCost = EconomyManager.Instance.CurFriesCost;
        BuyFood(ref playerFries, unitCost, FriesAmountText, "fries", amount, isBulk);
    }

    // Modify the existing BuySoda to use the new bulk method (defaulting to 1)
    public void BuySoda()
    {
        BuySoda(1); // Default to buying 1
    }
    public void BuySoda(int amount, bool isBulk = false)
    {
        float unitCost = EconomyManager.Instance.CurSodaCost;
        BuyFood(ref playerSoda, unitCost, SodaAmountText, "soda", amount, isBulk);
    }



    public void OnPlayerModChoiceClick(Mod mod)
    {
        ModManager.Instance.PlayerChoosesMod(mod);
        //ui 
        // sfx
        GameLoop(GameState.OrderStart);
    }
}