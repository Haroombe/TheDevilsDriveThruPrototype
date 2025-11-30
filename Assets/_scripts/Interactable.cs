using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract string InteractableName { get; }
    public abstract void Interact();
}
