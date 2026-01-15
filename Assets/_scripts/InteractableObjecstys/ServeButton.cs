using UnityEngine;
using System.Collections;
public class ServeButton : Interactable
{
    private string _name = "Drive Thru Window";
    public override bool canBuy => true;
    private bool hasFulfilledOrder = false;
    [SerializeField] private float cooldownTime = 2f;
    public override string InteractableName => _name;
    public override void Interact()
    {
        if (hasFulfilledOrder) return;

        if (GameManager.Instance.FulfillOrder())
        {
            StartCoroutine(Cooldown());
        }
    }
    // prevent spam bugs
    private IEnumerator Cooldown()
    {
        hasFulfilledOrder = true;
        yield return new WaitForSeconds(cooldownTime);
        hasFulfilledOrder = false;
    }
    public override void BulkInteract()
    {
    }
    public override string getDialogue()
    {
        return "serve customer";
    }
}
