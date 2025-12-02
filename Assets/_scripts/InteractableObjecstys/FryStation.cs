using UnityEngine;

public class FryStation : Interactable
{
    private string _name = "Fries";
    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {
        GameManager.Instance.BuyFries();
    }
}
