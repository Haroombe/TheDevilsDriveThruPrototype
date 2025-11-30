using UnityEngine;

public class torquerotate : MonoBehaviour
{
    public float torqueStrength = 5f;
    public float returnSpeed = 2f;

    private Rigidbody rb;
    private Vector3 defaultRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        defaultRotation = transform.eulerAngles;
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        ReturnToDefault();
    }

    void HandleInput()
    {
        if (Input.GetMouseButton(0))
        {
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");

            // IMPORTANT: apply torque on physics step
            rb.AddTorque(-y * torqueStrength, x * torqueStrength, 0f, ForceMode.Acceleration);
        }
    }

    void ReturnToDefault()
    {
        if (!Input.GetMouseButton(0))
        {
            Quaternion target = Quaternion.Euler(defaultRotation);
            Quaternion current = rb.rotation;

            Quaternion delta = Quaternion.Slerp(current, target, returnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(delta);
        }
    }
}
