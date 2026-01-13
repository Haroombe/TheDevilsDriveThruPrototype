using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class OrderHistoryDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject historyPanel;

    [Tooltip("The Content GameObject inside the Scroll View.")]
    [SerializeField] private Transform contentParent;

    [Tooltip("The TMP Prefab for a single order entry (from Project folder).")]
    [SerializeField] private GameObject orderItemPrefab;


    // NEW: Define the scale factor as a serialized field for easy adjustment
    [Header("Styling")]
    [Tooltip("Scale factor for history text size (e.g., 0.75 for 75%).")]
    [SerializeField] private float historyTextScale = 0.92f;

    private bool isPanelActive = false;

    private void Start()
    {
        // Initial state is hidden
        if (historyPanel != null)
        {
            historyPanel.SetActive(false);
        }
    }


    public void TogglePanel()
    {
        isPanelActive = !isPanelActive;
        historyPanel.SetActive(isPanelActive);

        if (isPanelActive)
        {
            DisplayHistory();
        }
    }

    public void DisplayHistory()
    {
        // 1. Clear all previously generated entries
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        // Get the historical data list
        List<Order> history = GameManager.Instance.orderHistory;

        // 2. Iterate through history and generate items
        for (int i = 0; i < history.Count; i++)
        {
            Order record = history[i];

            // Instantiate the prefab as a child of the Content area
            GameObject itemObject = Instantiate(orderItemPrefab, contentParent);

            // Get the TMP component
            TMP_Text itemText = itemObject.GetComponent<TMP_Text>();

            if (itemText != null)
            {
                // Format the string using the data from the struct
                string formattedText = $"Order #{i + 1} | Time: {record.getFulfilledTimeString()} | Payout: ${record.getPayout().ToString("N2")} (B:{record.burgerOrderAmount} | F:{record.friesOrderAmount} | S:{record.sodaOrderAmount})";
                itemText.text = formattedText;

                // --- NEW SIZE ADJUSTMENT LOGIC ---
                // Reduce the font size by 75%
                itemText.fontSize *= historyTextScale;
                // --- END NEW LOGIC ---
            }
        }
    }
}