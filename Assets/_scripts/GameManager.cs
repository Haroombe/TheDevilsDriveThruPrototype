using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private Canvas pauseCanvas;

    // Game data
    private float startingMoney = 25.00f;
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResumeGame();
    }

    private void Update()
    {
        HandleInput();
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
    }

    // Example game state

    private bool isPaused = false;

    // --- Public Methods ---

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        gameCanvas.enabled = false;
        pauseCanvas.enabled = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        gameCanvas.enabled = true;
        pauseCanvas.enabled = false;
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public void ResetGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }
}