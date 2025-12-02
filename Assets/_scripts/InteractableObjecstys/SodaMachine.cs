using UnityEngine;

public class SodaMachine : Interactable
{
    private string _name = "Soda";
    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {
        GameManager.Instance.BuySoda();
    }
    public override string getDialogue()
    {
        return "buy soda";
    }
}

