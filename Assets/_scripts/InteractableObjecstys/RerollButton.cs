using Assets._scripts.Mods;
using UnityEngine;

public class RerollButton : Interactable
{
    private string _name = "Reroll Mod";
    public override bool canBuy => false;

    OrderRerollNormal RerollMod;

    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {
        AudioManager.Instance.PlaySFX("mod_click", playInstantly: true);

        foreach (ModButtonPress modButton in ModManager.Instance.ClickableModPressButtons)
        {
            if (modButton.buttonModKey == ClickableModname.OrderRerollNormal)
            {
                modButton.OnRerollModButtonClick();
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

