using UnityEngine;

public class VeggieAssembly : Interactable
{
    private string _name = "Veggies";
    public override string InteractableName
    {
        get { return _name; }
    }
    public override void Interact()
    {
    }
}
