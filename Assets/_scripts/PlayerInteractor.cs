 using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private Camera playerCamera;

    private ui_helper ui;

    private Interactable current;

    private void Start()
    {
        ui = FindAnyObjectByType<ui_helper>();
        if (ui == null)
            Debug.LogError("UIHelper not found in scene.");
    }

    private void Update()
    {
        DetectInteractable();
        HandleInput();

    }

    private void DetectInteractable()
    {
        if (!playerCamera)
        {
            Debug.LogError("PlayerInteractor: playerCamera not assigned.");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            var interactable = hit.collider.GetComponentInParent<Interactable>();

            if (interactable != null)
            {
                if (interactable != current)
                    SwitchFocus(interactable);

                return;
            }
        }

        ClearFocus();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && current != null)
            current.Interact();

    }

    private void SwitchFocus(Interactable newTarget)
    {
        ClearFocus();
        string dialog_prompt = newTarget.getDialogue();
        ui.UpdatePromptUi($"press 'e' to {dialog_prompt}");

        current = newTarget;
    }

    private void ClearFocus()
    {
        if (current != null)
        {
            ui.UpdatePromptUi(string.Empty);
            current = null;
        }
    }
}
