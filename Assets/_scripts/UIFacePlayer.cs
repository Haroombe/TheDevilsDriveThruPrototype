using UnityEngine;

public class UIFacePlayer : MonoBehaviour
{
    public Transform playerCamera;


    private void LateUpdate()
    {
        if (playerCamera != null)
        {
            Vector3 directionToCamera = playerCamera.position - transform.position;
            float targetYRotation = Mathf.Atan2(directionToCamera.x, directionToCamera.z) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, targetYRotation + 180, 0);
        }
    }
}