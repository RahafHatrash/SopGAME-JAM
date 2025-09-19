using UnityEngine;

public class CloudPlatform : MonoBehaviour
{
    [Header("Platform Movement Settings")]
    public float moveDistance = 2f;      // المسافة التي تتحرك فيها المنصة
    public float moveSpeed = 3f;         // سرعة حركة المنصة

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private bool moving = false;

    void Start()
    {
        initialPosition = transform.position;
        targetPosition = initialPosition;
    }

    void Update()
    {
        if(moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if(Vector3.Distance(transform.position, targetPosition) < 0.01f)
                moving = false;
        }
    }

    public void RaisePlatform()
    {
        targetPosition = initialPosition + Vector3.up * moveDistance;
        moving = true;
        Debug.Log("RaisePlatform triggered!");
    }

    public void LowerPlatform()
    {
        targetPosition = initialPosition + Vector3.down * moveDistance;
        moving = true;
        Debug.Log("LowerPlatform triggered!");
    }
}
