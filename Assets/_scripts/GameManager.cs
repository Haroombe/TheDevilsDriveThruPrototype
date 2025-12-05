using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    //imports

    // init
    private int lastDisplayedSecond = -1;
    public enum GameState
    {
        Null,
        Initializing, // initialize setup, start first shift, first order, etc.
        ShiftOver, // all end shift stuff and start next shift or end game
        OrderStart, // create order, display, wait for fulfillment
        OrderFilled, // payout, apply modifiers, next order or end shift, display feedback
        Reset, // trigger game reset
        GameOverRestart, // give up case (button pressed to giveup)
    }
    [Header("UI Elements")]
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private Canvas pauseCanvas;
    [SerializeField] private Canvas gameOverCanvas;


    [SerializeField] private OrderHistoryDisplay curOrderDisplayDebug;

    [Header("Inventory UI Elements")]
    [SerializeField] private TextMeshProUGUI MoneyAmountText;
    [SerializeField] private TextMeshProUGUI SodaAmountText;
    [SerializeField] private TextMeshProUGUI BurgerAmountText;
    [SerializeField] private TextMeshProUGUI FriesAmountText;
    [SerializeField] private TextMeshProUGUI TimerText;

    [Header("Items Text")]
    [SerializeField] private TextMeshPro BurgerCostText;
    [SerializeField] private TextMeshPro SodaCostText;
    [SerializeField] private TextMeshPro FriesCostText;
    [SerializeField] private TextMeshPro BurgerProfitText;
    [SerializeField] private TextMeshPro SodaProfitText;
    [SerializeField] private TextMeshPro FriesProfitText;

    [Header("Round Tracking")]
    [SerializeField] private TextMeshProUGUI ShiftUITextField;
    [SerializeField] private TextMeshProUGUI OrderUITextField;

    [Header("Order UI")]
    [SerializeField] private TextMeshPro BurgerOrderText;
    [SerializeField] private TextMeshPro FriesOrderText;
    [SerializeField] private TextMeshPro SodaOrderText;
    [SerializeField] private TextMeshPro OrderPayout;
    [SerializeField] private TextMeshPro OrderNumText;

    [Header("PromptPanels")]
    [SerializeField] public GameObject GiveUpPanel;
    [SerializeField] public GameObject GameOverPanel;
    private bool isGiveUpPanelActive = false;
    private bool isGameOverPanelActive = false;
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

    public void UpdateOrderValuesUI(Order order)
    {
        if (BurgerOrderText != null)
            BurgerOrderText.text = $"{order.burgerOrderAmount.ToString()}";
        if (FriesOrderText != null)
            FriesOrderText.text = $"{order.friesOrderAmount.ToString()}";
        if (SodaOrderText != null)
            SodaOrderText.text = $"{order.sodaOrderAmount.ToString()}";
        if (OrderPayout != null)
            OrderPayout.text = $"${order.getPayout():F2}";
        if (OrderNumText != null)
            OrderNumText.text = $"Order #{OrderNum.ToString()}";

    }

    // starting values
    [Header("Starting Values")]
    public float startingMoney = 25.00f;
    public int startingSoda = 0;
    public int startingBurgers = 0;
    public int startingFries = 0;
    private int shiftNum = 1;
    public int OrdersPerShift = 7;
    private int OrderNum = 1;
    private GameState curGameState;

    // game variables
    [Header("Game Variables")]
    public int playerSoda;
    public int playerBurgers;
    public int playerFries;
    public float playerMoney;

    public Order currentOrder;





    // cost
    [Header("Item Costs")]
    [SerializeField] public float startingburgerCost = 2.50f;
    [SerializeField] public float startingfriesCost = 1.00f;
    [SerializeField] public float startingSodaCost = 0.25f;

    // profit
    [Header("Item Profits")]
    [SerializeField] public float startingBurgerProfit = 3.99f;
    [SerializeField] public float startingFriesProfit = 2.22f;
    [SerializeField] public float startingSodaProfit = 1.20f;

    [SerializeField] public float startingflatProfitPerOrder = 0f;
    [SerializeField] public float startingflatProfitPerShift = 0f;

    // round trackers
    private int totalOrdersCompleted = 0;
    private int totalShiftsCompleted= 0;
    // multipliers
    [Header("Multipliers")]

    [SerializeField] public int NumOrdersToPlayerEvent = 6; // modulo total orders to check
    [Header("player multiplier affectors")]

    public float burgerCost;
    public float friesCost;
    public float SodaCost;
    public float BurgerProfit;
    public float FriesProfit;
    public float SodaProfit;
    public float flatProfitPerOrder;
    public float flatProfitPerShift;




    private void initRates()
    {
        burgerCost = startingburgerCost;
        friesCost = startingfriesCost;
        SodaCost = startingSodaCost;
        BurgerProfit = startingBurgerProfit;
        FriesProfit = startingFriesProfit;
        SodaProfit = startingSodaProfit;
        flatProfitPerOrder = startingflatProfitPerOrder;
        flatProfitPerShift = startingflatProfitPerShift;
        UpdateRateUI();
    }
    private void UpdateRateUI()
    {
        // Update item cost and profit texts
        BurgerCostText.text = $"{burgerCost:F2}";
        FriesCostText.text = $"{friesCost:F2}";
        SodaCostText.text = $"{SodaCost:F2}";
        BurgerProfitText.text = $"{BurgerProfit:F2}";
        FriesProfitText.text = $"{FriesProfit:F2}";
        SodaProfitText.text = $"{SodaProfit:F2}";
    
    }

    // burger price multiplier
    // soda price multiplier
    // fries price multiplier
    // burger profit multiplier
    // soda profit multiplier
    // fries profit multiplier
    // per order price multiplier 
    // per shift price multiplier
    // per order profit multiplier
    // per shift profit multiplier
    // order volume increase multiplier

    // chance
    // order reroll randomizer
    // shift randomizer
    // order randomizer


    // timer
    private float elapsedTime = 0f;

    // Actions
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
        AudioManager.Instance.PlayBGM(0); // Play main menu BGM

        GameLoop(GameState.Initializing);
    }


    private void Update()
    {
        HandleInput();
        elapsedTime += Time.deltaTime;
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            FadingMessage.Instance.ShowMessage("This is a fading message ex!");
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            FadingMessage.Instance.ShowMessage("This is a fading message ex!", true);

        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            curOrderDisplayDebug.TogglePanel();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            SetPanelVisibility(GiveUpPanel,true);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            SetPanelVisibility(GiveUpPanel, false);

        }


    }


    // game loop

    public void GameLoop(GameState state)
    {
        if (curGameState == state) return;
        curGameState = state;

        switch (state)
        {
            case GameState.Initializing:
                // initialize game
                Debug.Log("initializing");
                currentOrder=null;

                initRates();
                initializePlayerItems();
                UpdateAllInventoryUI();
                InitializeFirstShiftOrder();
                ResumeGame();
                GameLoop(GameState.OrderStart);



                break;
            case GameState.OrderStart:
                                // start order
                Debug.Log("order number "+ OrderNum.ToString() +" start");
                CustomerManager.Instance.SpawnToMid();
                currentOrder = CreateOrder();
                UpdateOrderValuesUI(currentOrder);


                // create order and display
                break;
            case GameState.OrderFilled:
                Debug.Log("order number " + OrderNum.ToString() + " filled during shift " + shiftNum.ToString());

                OrderNum++;
                totalOrdersCompleted++;
                if (OrderNum >= OrdersPerShift+1)
                {
                    Debug.Log("shift number " + shiftNum.ToString() + " over");
                    shiftNum++;
                    OrderNum = 1;
                    addAmountF(ref playerMoney, flatProfitPerShift);

                    UpdateOrderUI(OrderNum);
                    UpdateShiftUI(shiftNum);
                    FadingMessage.Instance.ShowCallout($"Starting Shift {shiftNum}", .9f, 1.9f);

                }
                else
                {
                    addAmountF(ref playerMoney, flatProfitPerOrder);

                    Debug.Log("starting next order number " + OrderNum.ToString());
                    UpdateOrderUI(OrderNum);
                }
                

                GameLoop(GameState.OrderStart);

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
        totalOrdersCompleted = 0;
        totalShiftsCompleted = 0;
        shiftNum = 1;
        OrderNum = 1;
        FadingMessage.Instance.ShowCallout($"Starting Shift {shiftNum}",.9f, 2f);

        UpdateShiftUI(shiftNum);
        UpdateOrderUI(OrderNum);

    }

    private Order CreateOrder()
    { // add modifiers, shift, order randomizers, volume increases
        return new Order(burgers: 1, sodas: 1, fries: 1);
    }
    

    // --- UI Update Methods ---
    // ------------------------------
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

    private void addAmount(ref int baseAmount, int amount)
    {
        baseAmount += amount;
    }

    private void minusAmountF(ref float baseAmount, float amount)
    {
        baseAmount -= amount;
    }

    private void minusAmount(ref int baseAmount, int amount)
    {
        baseAmount -= amount;
    }

    private void initializePlayerItems()
    {
        Debug.Log("Initializing player items");

        setAmountF(ref playerMoney, startingMoney);
        setAmount(ref playerSoda, startingSoda);
        setAmount(ref playerBurgers, startingBurgers);
        setAmount(ref playerFries, startingFries);
    }
    private void UpdateAllInventoryUI()
    {
        UpdateUIF(MoneyAmountText, playerMoney, "F2");
        UpdateUI(SodaAmountText, playerSoda);
        UpdateUI(BurgerAmountText, playerBurgers);
        UpdateUI(FriesAmountText, playerFries);
    }


    private void updateTimerUI()
    {
        int minutes = (int)(elapsedTime / 60f);
        int seconds = (int)(elapsedTime % 60f);
        string text = $"{minutes:00}:{seconds:00}";
        TimerText.text = text;

    }
    // ------------------------------

    // Game data




    private bool isPaused = false;

    // --- Public Methods ---

    public void PauseGame()
    {
        Cursor.visible = true;
        isPaused = true;
        Time.timeScale = 0f;
        gameCanvas.enabled = false;
        pauseCanvas.enabled = true;
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
        //Debug.Log("GameManager ResetGame called");
        curGameState = GameState.Null;
        ResetGameAction?.Invoke();
        isPaused = false;
        Time.timeScale = 1f;

        elapsedTime = 0f;
        GameLoop(GameState.Initializing);


    }


    // checks
    // has enough money to purchase x
    public void FulfillOrder()
    {
        if (!canFulfillOrder(currentOrder))
        {
            FadingMessage.Instance.ShowMessage("Can't fill order!"); // ui
            AudioManager.Instance.PlaySFX("No", playInstantly: true);

            return;
        }

        // Deduct inventory
        minusAmount(ref playerBurgers, currentOrder.burgerOrderAmount);
        minusAmount(ref playerFries, currentOrder.friesOrderAmount);
        minusAmount(ref playerSoda, currentOrder.sodaOrderAmount);

        // Add money
        addAmountF(ref playerMoney, currentOrder.getPayout());

        //fulfilled timestamp
        currentOrder.FulfilledTime = elapsedTime;
        currentOrder.CalculateFulfilledTimeString();
        // Add to history

        orderHistory.Add(currentOrder);

        // Update UI
        UpdateUI(BurgerAmountText, playerBurgers);
        UpdateUI(FriesAmountText, playerFries);
        UpdateUI(SodaAmountText, playerSoda);
        UpdateUIF(MoneyAmountText, playerMoney, "F2");

        FadingMessage.Instance.ShowMessage($"Order #{OrderNum} filled! Earned +${currentOrder.getPayout():F2}", true,.6f ,2f); // ui

        CustomerManager.Instance.ServeCustomer();
        GameLoop(GameState.OrderFilled);


    }
    // current player inventory can fulfill order
    public bool canFulfillOrder(Order curOrder) {
        bool hasEnoughBurgers = playerBurgers >= curOrder.burgerOrderAmount;
        bool hasEnoughFries = playerFries >= curOrder.friesOrderAmount;
        bool hasEnoughSoda = playerSoda >= curOrder.sodaOrderAmount;

        return hasEnoughBurgers && hasEnoughFries && hasEnoughSoda;

    }

    // Game actions

    // Generic buy function
    private void BuyFood(ref int playerInventory, float cost, TextMeshProUGUI inventoryText, string foodName)
    {
        if (playerMoney >= cost)
        {
            minusAmountF(ref playerMoney, cost);
            addAmount(ref playerInventory, 1);
            UpdateUIF(MoneyAmountText, playerMoney, "F2");
            UpdateUI(inventoryText, playerInventory);
            FadingMessage.Instance.ShowMessage($"Bought 1 {foodName} for ${cost:F2}!", true);

        }
        else
        {
            FadingMessage.Instance.ShowMessage($"Not enough money to buy {foodName}!");
        }
    }

    // Simplified buy methods
    public void BuyBurger()
    {
        BuyFood(ref playerBurgers, burgerCost, BurgerAmountText, "burger");
    } 

    public void BuyFries()
    {
        BuyFood(ref playerFries, friesCost, FriesAmountText, "fries");
    }

    public void BuySoda()
    {
        BuyFood(ref playerSoda, SodaCost, SodaAmountText, "soda");
    }

}