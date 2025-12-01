using UnityEngine;

using System;
public class DriveThruWindow : Interactable
{
    private string _name = "Drive Thru Window";

    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {
        CustomerManager.Instance.ServeCustomer();
    }
}
