using UnityEngine;
using TMPro; // Assuming you are using TextMeshPro for modern Unity UI

public class ModPageDisplay : MonoBehaviour
{
    // Assign these TextMeshPro components in the Inspector (drag from your UI panel)
    [Header("UI References")]
    [SerializeField] private TMP_Text modNameText;
    [SerializeField] private TMP_Text modDescriptionText;
    [SerializeField] private TMP_Text modTypeText;
    [SerializeField] private TMP_Text usesRemainingText;

    // The main display method
    public void DisplayMod(Mod mod)
    {
        if (mod == null)
        {
            // Clear the UI if no mod is passed (e.g., if we go past the last page)
            modNameText.text = "No Active Mod";
            modDescriptionText.text = "You currently don't have this many mods active.";
            modTypeText.text = "";
            usesRemainingText.text = "";
            return;
        }

        // 1. Display Core Info
        modNameText.text = mod.ModName;
        modDescriptionText.text = mod.description;
        modTypeText.text = $"Type: {mod.modType}";

        // 2. Display Uses Remaining (Conditional Logic)
        if (mod.modType == ModType.Permanent)
        {
            // Permanent mods don't need a counter
            usesRemainingText.text = "Uses: Permanent";
        }
        else // Finite or Clickable mods need to show a counter
        {
            usesRemainingText.text = $"Uses Left: {mod.UsesRemaining}";

            // Optional: Visually indicate low usage
            if (mod.UsesRemaining <= 1)
            {
                usesRemainingText.color = Color.red;
            }
            else
            {
                usesRemainingText.color = Color.white;
            }
        }
    }
}