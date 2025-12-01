using UnityEngine;
public class CustomerMover : MonoBehaviour
{
    public float speed = 2f;
    private Transform target;
    private bool moving = false;

    public void MoveTo(Transform t)
    {
        target = t;
        moving = true;
    }

    private void Update()
    {
        if (!moving || target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            moving = false;
        }
    }
}