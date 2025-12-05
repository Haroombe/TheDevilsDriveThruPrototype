using UnityEngine;
using System.Collections.Generic;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;
    private void Awake() => Instance = this;

    public GameObject[] customers;
    private List<int> customerPool = new List<int>();
    private int lastSpawnedIndex = -1;

    public Transform spawnPoint;
    public Transform midPoint;
    public Transform endPoint;
    private float smoothSpeed = 3.25f;
    public float rotationOffset = 90f;
    private CustomerMover currentCustomer;
    [SerializeField] public GameObject Player;

    void Start()
    {
        GameManager.Instance.ResetGameAction += ResetCustomers;
        RefillPool();
    }

    private void OnDestroy()
    {
        GameManager.Instance.ResetGameAction -= ResetCustomers;
    }
    // Add these fields to your CustomerManager class

    private float spazzCooldown = 0f;
    private float spazzIntervalMin = 4f;
    private float spazzIntervalMax = 8f;
    private float spazzDuration = 0f;
    private float maxSpazzDuration = 0.6f;
    private bool isSpazzing = false;

    // For jerky snaps during spazz
    private float spazzSnapTimer = 0f;
    private float spazzSnapIntervalMin = 0.05f; // Min hold time
    private float spazzSnapIntervalMax = 0.1f; // Max hold time
    private float currentSpazzRotationX = 0f;
    private float currentSpazzRotationY = 0f;
    private float currentSpazzRotationZ = 0f;
    private float originalRotationX = 0f;
    private float originalRotationZ = 0f;

    private void FixedUpdate()
    {
        if (currentCustomer != null)
        {
            if (Player == null)
            {
                return;
            }

            //// Handle spazz timer
            //spazzCooldown -= Time.fixedDeltaTime;
            //if (spazzCooldown <= 0f)
            //{
            //    // Start spazzing and set next random interval
            //    isSpazzing = true;
            //    spazzDuration = maxSpazzDuration;
            //    spazzCooldown = Random.Range(spazzIntervalMin, spazzIntervalMax);
            //    spazzSnapTimer = 0f; // Reset snap timer
            //                         // Store original rotations
            //    originalRotationX = currentCustomer.transform.eulerAngles.x;
            //    originalRotationZ = currentCustomer.transform.eulerAngles.z;
            //}

            //// Handle spazz rotation
            //if (isSpazzing && spazzDuration > 0f)
            //{
            //    spazzDuration -= Time.fixedDeltaTime;
            //    spazzSnapTimer -= Time.fixedDeltaTime;

            //    // Snap to new random direction at intervals
            //    if (spazzSnapTimer <= 0f)
            //    {
            //        currentSpazzRotationX = Random.Range(-15f, 15f);
            //        currentSpazzRotationY = Random.Range(0f, 360f);
            //        currentSpazzRotationZ = Random.Range(-15f, 15f);
            //        spazzSnapTimer = Random.Range(spazzSnapIntervalMin, spazzSnapIntervalMax);
            //    }

            //    // Hold current direction
            //    currentCustomer.transform.eulerAngles = new Vector3(
            //        originalRotationX + currentSpazzRotationX,
            //        currentSpazzRotationY,
            //        originalRotationZ + currentSpazzRotationZ
            //    );
            //}
            //else if (isSpazzing && spazzDuration <= 0f)
            //{
            //    // Spazz ended, return to normal
            //    isSpazzing = false;
            //    // Return to original X and Z
            //    currentCustomer.transform.eulerAngles = new Vector3(
            //        originalRotationX,
            //        currentCustomer.transform.eulerAngles.y,
            //        originalRotationZ
            //    );
            //}

            // Normal behavior: stare at player
            if (!isSpazzing)
            {
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
    }

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
    }

    public void SpawnToMid()
    {
        if (customers.Length == 0) return;

        // Refill pool if empty
        if (customerPool.Count == 0)
        {
            RefillPool();
        }

        // Get next customer from shuffled pool
        int customerIndex = customerPool[0];
        customerPool.RemoveAt(0);

        GameObject npc = Instantiate(
            customers[customerIndex],
            spawnPoint.position,
            Quaternion.identity
        );

        currentCustomer = npc.GetComponent<CustomerMover>();
        currentCustomer.transform.rotation = Quaternion.Euler(-90f, 0f, 90f);
        currentCustomer.MoveTo(midPoint);

        if (currentCustomer.transform.rotation.x != -90f)
        {
            currentCustomer.transform.rotation = Quaternion.Euler(-90f, 0f, 90f);
        }

        lastSpawnedIndex = customerIndex;
    }

    private void RefillPool()
    {
        customerPool.Clear();

        // Add all customer indices to pool
        for (int i = 0; i < customers.Length; i++)
        {
            customerPool.Add(i);
        }

        // Shuffle using Fisher-Yates algorithm
        for (int i = customerPool.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = customerPool[i];
            customerPool[i] = customerPool[randomIndex];
            customerPool[randomIndex] = temp;
        }

        // Prevent last spawned customer from being first in new pool
        if (lastSpawnedIndex != -1 && customerPool.Count > 1 && customerPool[0] == lastSpawnedIndex)
        {
            // Swap first position with a random other position
            int swapIndex = Random.Range(1, customerPool.Count);
            int temp = customerPool[0];
            customerPool[0] = customerPool[swapIndex];
            customerPool[swapIndex] = temp;
        }
    }

    private void MoveToEnd(CustomerMover ai)
    {
        StartCoroutine(SpinThenLeave(ai));
    }

    private System.Collections.IEnumerator SpinThenLeave(CustomerMover ai)
    {
        // Random spin direction
        float direction = Random.Range(0, 2) == 0 ? 1f : -1f;
        float spinSpeed = 1950f; // degrees per second
        float duration = .75f; // 1 second spin (was 3f)
        float elapsed = 0f;

        // Spin first (blocking)
        while (elapsed < duration && ai != null)
        {
            elapsed += Time.deltaTime;
            ai.transform.Rotate(0, 0, spinSpeed * direction * Time.deltaTime); // Y axis, not Z
            yield return null; // Wait one frame
        }

        // Then move (after spin completes)
        ai.transform.rotation = Quaternion.Euler(-90f, 0f, 90f);

        ai.MoveTo(endPoint);
        Destroy(ai.gameObject, 3f);
    }


    private void ResetCustomers()
    {
        // Destroy existing customers
        CustomerMover[] existingCustomers = FindObjectsByType<CustomerMover>(FindObjectsSortMode.None);
        foreach (var customer in existingCustomers)
        {
            Destroy(customer.gameObject);
        }

        // Reset pool and spawn first customer
        customerPool.Clear();
        lastSpawnedIndex = -1;
        RefillPool();
    }
}