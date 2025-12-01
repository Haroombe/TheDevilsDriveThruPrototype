using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;
    private void Awake() => Instance = this;

    public GameObject[] customers;
    private int index = 0;

    public Transform spawnPoint;
    public Transform midPoint;
    public Transform endPoint;
    private float smoothSpeed = 3.25f;
    private float rotationOffset = 90f;
    private CustomerMover currentCustomer;
    [SerializeField] public GameObject Player;

    // --- Step 1: Start Button Spawns First Customer ---
    void Start()
    {
        SpawnToMid();
    }

    private void FixedUpdate()
    {
        if (currentCustomer != null)
        {


                if (Player == null)
            {
                return;
            }
            // stare at player
            Vector3 directionToPlayer = (Player.transform.position - currentCustomer.transform.position).normalized;

            float targetYRotation = Mathf.Atan2(directionToPlayer.x, directionToPlayer.z) * Mathf.Rad2Deg;

            float currentYRotation = currentCustomer.transform.eulerAngles.y;
            float smoothedYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation + rotationOffset, Time.fixedDeltaTime * smoothSpeed);

            currentCustomer.transform.eulerAngles = new Vector3(
                currentCustomer.transform.eulerAngles.x,
                smoothedYRotation,
                currentCustomer.transform.eulerAngles.z
            );
        }
    }

    // --- Step 2: Interact Moves Current + Spawns Next ---
    public void ServeCustomer()
    {
        if (currentCustomer == null)
        {
            Debug.Log("No customer to serve.");
            return;
        }

        // current goes to end + will be destroyed on arrival
        MoveToEnd(currentCustomer);

        // spawn next immediately to mid
        SpawnToMid();
    }

    private void SpawnToMid()
    {
        if (index >= customers.Length)
            return;  // cycle or stop, your choice

        GameObject npc = Instantiate(
            customers[index],
            spawnPoint.position,
            Quaternion.identity // Start with no rotation
        );

        currentCustomer = npc.GetComponent<CustomerMover>();
        currentCustomer.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // Set AFTER getting component
        currentCustomer.MoveTo(midPoint);
        if (currentCustomer.transform.rotation.x != -90f)
        {
            currentCustomer.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        }

        index++;
    }

    private void MoveToEnd(CustomerMover ai)
    {
        ai.MoveTo(endPoint);
        Destroy(ai.gameObject, 3f); // give it time to walk before despawn
    }
}
