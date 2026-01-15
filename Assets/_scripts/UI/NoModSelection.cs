using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class NoModSelection : MonoBehaviour
{
    // The specific Mod object for this card

    // UI elements to update (assign in Inspector)
    [SerializeField] public Button selectButton;
    [SerializeField] private Canvas ShopCanvas;

    private void Awake()
    {
        // Crucial: Programmatically attach the listener and capture the 'assignedMod' reference.
        // This ensures the button passes the correct unique mod instance on click.
        selectButton.onClick.AddListener(HandleSelectionClick);
    }



    // This is the function called when the player clicks the button
    private void HandleSelectionClick()
    {
        FadingMessage.Instance.ShowMessage($"No modifier was chosen!", isGreen: true, customFadeDuration: 1.7f);



        ShopCanvas.enabled = false;
        GameManager.Instance.ModShopUnPause();
        GameManager.Instance.GameLoop(GameState.OrderStart);

    }
}