using Assets._scripts.Mods;
using TMPro;
using UnityEngine;

public class FireSaleButton : Interactable
{
    private string _name = "Fire Sale Mod";
    public override bool canBuy => false;

    FireSale FiresaleMod;
    public TextMeshPro UsesRemainingTextField;

    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {
        AudioManager.Instance.PlaySFX("mod_click", playInstantly: true);
        foreach (ModButtonPress modButton in ModManager.Instance.ClickableModPressButtons)
        {
            if (modButton.buttonModKey == ClickableModname.FireSale)
            {
                modButton.OnFireSaleModButtonClick();
            }
        }
    }

    public override void BulkInteract()
    {
       

    }
    public override string getDialogue()
    {
        
        return $"Activate {_name}";
    }

 
}

