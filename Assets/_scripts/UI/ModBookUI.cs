
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ModBookUI : MonoBehaviour
{
    // --- INSPECTOR REFERENCES ---

    // Assign the root UI Panel GameObject here (the parent of all the book elements)
    [Header("UI Structure")]
    [SerializeField] private GameObject rootPanel;

    // Assign the ModPageDisplay component attached to the single page panel
    [SerializeField] private ModPageDisplay modPageView;

    // Assign the Next and Previous buttons
    [Header("Navigation")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text pageNumberText;

    // --- PRIVATE STATE ---

    private List<Mod> activeModsList;
    private int currentPageIndex = 0;

    // --- UNITY LIFECYCLE ---

    private void Awake()
    {
        // Ensure the UI is hidden when the game starts
        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // 1. Ensure buttons are linked to the navigation methods
        previousButton.onClick.AddListener(() => ChangePage(-1));
        nextButton.onClick.AddListener(() => ChangePage(1));

        // 2. Initial load of the mod list and display update (in case it's shown later)
        RefreshModsList();

    }

    // --- PUBLIC TOGGLE METHOD (Called by GameManager on KeyPress) ---

    public void ToggleUI()
    {
        bool isCurrentlyActive = rootPanel.activeSelf;

        if (isCurrentlyActive)
        {
            // --- CLOSING LOGIC ---

            // 1. Reset the page index to 0, ready for next opening
            currentPageIndex = 0;

            // 2. Hide the entire UI panel
            rootPanel.SetActive(false);

            // Optional: Resume game/input flow if paused
        }
        else
        {
            // --- OPENING LOGIC ---

            // 1. Refresh the list of mods (must be done before displaying)
            RefreshModsList();

            // 2. Show the entire UI panel
            rootPanel.SetActive(true);

            // 3. Display the first page (index 0)
            DisplayCurrentPage();

            // Optional: Pause game/input flow
        }
    }

    // --- DATA REFRESH ---

    // Loads the current list of active mods
    public void RefreshModsList()
    {
        // Assumes ModManager.Instance.activeMods is accessible
        activeModsList = ModManager.Instance.activeMods;
    }


    // --- NAVIGATION LOGIC ---

    public void ChangePage(int direction) // direction should be +1 or -1
    {
        int newIndex = currentPageIndex + direction;

        // Clamp the index to stay within bounds
        if (newIndex >= 0 && newIndex < activeModsList.Count)
        {
            currentPageIndex = newIndex;
            DisplayCurrentPage();
        }
    }

    private void DisplayCurrentPage()
    {
        if (activeModsList == null || activeModsList.Count == 0)
        {
            // Display message for zero mods
            modPageView.DisplayMod(null);
            pageNumberText.text = "0 of 0";
            UpdateNavigationButtons(0, 0);
            return;
        }

        // Get the mod for the current page
        Mod modToDisplay = activeModsList[currentPageIndex];

        // Tell the View component to update its fields
        modPageView.DisplayMod(modToDisplay);

        // Update the page number indicator
        pageNumberText.text = $"{currentPageIndex + 1} of {activeModsList.Count}";

        // Update the button interactability
        UpdateNavigationButtons(currentPageIndex, activeModsList.Count);
    }

    private void UpdateNavigationButtons(int currentIndex, int totalCount)
    {
        // Disable the Previous button if on page 1 (index 0)
        previousButton.interactable = currentIndex > 0;

        // Disable the Next button if on the last page
        nextButton.interactable = currentIndex < totalCount - 1;
    }
}
