using UnityEngine;

using System;
public class GiveUpButton : Interactable
{
    private string _name = "Give Up Button";

    public override bool canBuy => true;

    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {// show give up panel
        GameManager.Instance.SetPanelVisibility(GameManager.Instance.GiveUpPanel, true);
        GameManager.Instance.SetPanelVisibility(GameManager.Instance.GameOverPanel, false);

        GameManager.Instance.PauseGame(true);

    }
    public override void BulkInteract()
    {
    }
    public override string getDialogue()
    {
        return "Give Up";
    }
}
