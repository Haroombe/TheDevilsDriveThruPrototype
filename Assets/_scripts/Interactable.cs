using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract string InteractableName { get; }

    public abstract bool canBuy { get; }
    public abstract void Interact();
    public abstract void BulkInteract();

    public abstract string getDialogue();
}
