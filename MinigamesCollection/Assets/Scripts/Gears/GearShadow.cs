using UnityEngine;

public class GearShadow : MonoBehaviour
{
    private Vector3 initialLocalPosition;
    [SerializeField] private Transform parentPosition;
    private bool isNestedPin = false;

    void Start()
    {
        initialLocalPosition = transform.localPosition;
       
        if (GetComponentInParent<PinController>() != null)
        {
            parentPosition = GetComponentInParent<PinController>().transform;
            isNestedPin = true;
        }
        
        if (GetComponentInParent<GearController>() != null)
        {
            parentPosition = GetComponentInParent<GearController>().transform;
        }
    }

    void Update()
    {
        // Get the parent's rotation angle in radians
        float angle = parentPosition.eulerAngles.z * Mathf.Deg2Rad;

        // Calculate the rotated offset
        float offsetX = initialLocalPosition.x * Mathf.Cos(angle) + initialLocalPosition.y * Mathf.Sin(angle);
        float offsetY = -initialLocalPosition.x * Mathf.Sin(angle) + initialLocalPosition.y * Mathf.Cos(angle);

        // Set the Shadow's local position
        transform.localPosition = new Vector3(offsetX, offsetY, parentPosition.position.z);
        if (isNestedPin) transform.localRotation = Quaternion.Euler(0f, 0f, -parentPosition.eulerAngles.z);
    }
}
