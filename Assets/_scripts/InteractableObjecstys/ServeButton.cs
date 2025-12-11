using UnityEngine;

using System;
public class ServeButton : Interactable
{
    private string _name = "Drive Thru Window";
    public override bool canBuy => true;

    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {
        GameManager.Instance.FulfillOrder();

    }
    public override void BulkInteract()
    {
    }
    public override string getDialogue()
    {
        return "serve customer";
    }
}
