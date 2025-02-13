using UnityEngine;
using DG.Tweening;
using System;

public class SimpleMover : MonoBehaviour
{
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool useSpawnPoint = false;

    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 endPoint;
    public Vector3 StartPoint { get => startPoint; set => startPoint = value; }
    public Vector3 EndPoint { get => endPoint; set => endPoint = value; }

    [SerializeField] private float startDelay;
    public float StartDelay { get => startDelay; set => startDelay = value; }

    [SerializeField] private float transitionDuration;
    [SerializeField] private bool moveOnStart = true;
    [SerializeField] private float transitionSpeed;
    public float TransitionSpeed { get => transitionSpeed; set => transitionSpeed = value; }

    private Vector3 targetPoint;
    public Vector3 TargetPoint { get => targetPoint; set => targetPoint = value; }

    private Tweener movementTween; // Store reference to the movement tween

    void Start()
    {
        if (useSpawnPoint)
        {
            startPoint = transform.position;
        }

        if (moveOnStart)
        {
            SetEndAsTarget();
            RunAnimation();
        }
    }

    public void StopAnimation()
    {
        movementTween?.Kill(); // Use null-conditional operator
    }

    public void RunAnimation(Action onAnimationComplete = null)
    {
        StopAnimation();

        float distance = Vector3.Distance(transform.position, targetPoint);
        transitionDuration = distance / transitionSpeed;

        movementTween = transform
            .DOMove(targetPoint, transitionDuration)
            .SetLink(gameObject)
            .SetDelay(startDelay)
            .SetUpdate(useUnscaledTime) // Use the useUnscaledTime variable
            .OnComplete(() => onAnimationComplete?.Invoke());
    }

    public void SetStartAsTarget()
    {
        targetPoint = startPoint;
    }

    public void SetEndAsTarget()
    {
        targetPoint = endPoint;
    }
}
