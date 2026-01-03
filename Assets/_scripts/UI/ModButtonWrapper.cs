using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class ModButtonWrapper : MonoBehaviour
{
    // The specific Mod object for this card
    private Mod assignedMod;

    // UI elements to update (assign in Inspector)
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button selectButton;
    [SerializeField]  private TextMeshProUGUI ModType;
    [SerializeField] private Canvas ShopCanvas;

    private void Awake()
    {
        // Crucial: Programmatically attach the listener and capture the 'assignedMod' reference.
        // This ensures the button passes the correct unique mod instance on click.
        selectButton.onClick.AddListener(HandleSelectionClick);
    }

    public void SetModChoice(Mod mod)
    {
        assignedMod = mod;
    }

    public void SetModDetails(string name, string description, string modType)
    {
        nameText.text = name;
        descriptionText.text = description;
        ModType.text = modType;
    }

    // This is the function called when the player clicks the button
    private void HandleSelectionClick()
    {
        if (assignedMod != null)
        {
            // The single, clean call to the manager
            ModManager.Instance.PlayerChoosesMod(assignedMod);

            // Hide the Mod Shop UI after selection
            FadingMessage.Instance.ShowMessage($"You got the '{assignedMod.ModName}' modifier!", isGreen:true);


        }
        else
        {

            FadingMessage.Instance.ShowMessage($"Error:Failed to apply mod!!!");
        }
        ShopCanvas.gameObject.SetActive(false);
        GameManager.Instance.ModShopUnPause();
        GameManager.Instance.GameLoop(GameState.OrderStart);

    }
}