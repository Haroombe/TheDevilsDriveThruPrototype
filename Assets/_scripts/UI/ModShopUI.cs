using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ModShopUI : MonoBehaviour
{
    // Assign these TWO fixed UI elements in the Inspector
    [SerializeField] private GameObject cardContainer1;
    [SerializeField] private GameObject cardContainer2;


    // A list to hold our two choice wrappers
    private List<ModButtonWrapper> choiceWrappers = new List<ModButtonWrapper>();

    private void Awake()
    {
        gameObject.SetActive(false);

        
    }

    private void Start()
    {
        // Get the wrapper scripts from the fixed UI cards once
        choiceWrappers.Add(cardContainer1.GetComponent<ModButtonWrapper>());
        choiceWrappers.Add(cardContainer2.GetComponent<ModButtonWrapper>());
    }

    public bool PlayerModChoiceSelection()
    {
        // 1. Get the choices from the Manager
        List<Mod> modChoices = ModManager.Instance.ModChoicesForPlayer();

        // Ensure we have exactly 2 choices (or handle errors)
        if (modChoices.Count < 2)
        {
            Debug.LogError("ModManager did not return enough choices.");
            return false;
        }

        // 2. Loop through the choices and assign them to the fixed UI cards
        for (int i = 0; i < 2; i++)
        {
            Mod modToDisplay = modChoices[i];
            ModButtonWrapper wrapper = choiceWrappers[i];

            // --- A. Pass the dynamic Mod data ---
            wrapper.SetModChoice(modToDisplay);

            // --- B. Update the visuals ---
            // Assuming your wrapper or card has methods to set text
            wrapper.SetModDetails(modToDisplay.ModName, modToDisplay.description, modToDisplay.modType.ToString());

            // --- C. Ensure the UI is visible ---
            wrapper.gameObject.SetActive(true);
        }

        // Show the entire choice panel
        gameObject.SetActive(true);
        return true;

    }

    public void turnOffModCanvas()
    {
        gameObject.SetActive(false);
    }
}