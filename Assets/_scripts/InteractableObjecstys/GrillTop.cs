using UnityEngine;

public class GrillTop : Interactable
{
    private string _name = "Burger";
    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {
        GameManager.Instance.BuyBurger();
    }

    public override string getDialogue()
    {
        return "buy burgers";
    }
}
