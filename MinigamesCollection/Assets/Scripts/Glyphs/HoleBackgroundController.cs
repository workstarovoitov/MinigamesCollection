using UnityEngine;
using UnityEngine.InputSystem;

public class HoleBackgroundController : MonoBehaviour
{
    [SerializeField] private Vector2 offsetPosition;
    [SerializeField] private Vector2 maxDeltaPosition;
    [SerializeField] private Vector2 offsetMultiplier = Vector2.one;
    [SerializeField] private float dampingFactor = 0.9f; // Adjust this value for desired damping effect

    private Vector2 screenCenter = Vector2.zero;
    private Vector2 targetPosition;
    private Vector2 velocity = Vector2.zero;
    private Camera mainCamera;

    private void Start()
    {
        screenCenter = transform.position;
    }

    void Update()
    {
        if (Time.timeScale < 1f) return;
        Vector2 currentMousePosition;
        if (mainCamera != null) currentMousePosition = (Vector2)mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        else currentMousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Calculate the target position based on mouse movement from the screen center
        targetPosition = screenCenter;
        targetPosition.x += (currentMousePosition.x - screenCenter.x) * offsetMultiplier.x + offsetPosition.x;
        targetPosition.x = Mathf.Clamp(targetPosition.x, screenCenter.x - maxDeltaPosition.x + offsetPosition.x, screenCenter.x + maxDeltaPosition.x + offsetPosition.x);
        targetPosition.y += (currentMousePosition.y - screenCenter.y) * offsetMultiplier.y + offsetPosition.y;
        targetPosition.y = Mathf.Clamp(targetPosition.y, screenCenter.y - maxDeltaPosition.y + offsetPosition.y, screenCenter.y + maxDeltaPosition.y + offsetPosition.y);

        // Apply inertia effect with damping
        float deltaTime = Time.deltaTime;
        velocity = (targetPosition - (Vector2)transform.position) * (1f - dampingFactor) / deltaTime;

        // Apply the new position to the object
        transform.position += new Vector3 (velocity.x * deltaTime, velocity.y * deltaTime, 0);
    }
}