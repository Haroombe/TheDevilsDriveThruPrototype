using Assets._scripts.Mods;
using UnityEngine;

public class VipRerollButton : Interactable
{
    private string _name = "Vip Reroll Mod";
    public override bool canBuy => false;

    VipReroll VipRerollMod;

    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {
        AudioManager.Instance.PlaySFX("mod_click", playInstantly: true);

        foreach (ModButtonPress modButton in ModManager.Instance.ClickableModPressButtons)
        {
            if (modButton.buttonModKey == ClickableModname.VIPReroll)
            {
                modButton.OnVIPRerollModButtonClick();
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

