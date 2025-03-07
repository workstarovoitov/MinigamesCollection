using UnityEngine;
using DG.Tweening;
using System;

public class SimpleScaler : MonoBehaviour
{
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool useSpawnScale = false;

    [SerializeField] private float startScale;
    public float StartScale { get => startScale; set => startScale = value; }

    [SerializeField] private float startDelay;
    public float StartDelay { get => startDelay; set => startDelay = value; }

    [SerializeField] private float transitionDuration;
    [SerializeField] private bool scaleOnStart = true;

    private float targetScale;
    public float TargetScale { get => targetScale; set => targetScale = value; }

    private Tweener movementTween; // Store reference to the movement tween

    void Start()
    {
        if (useSpawnScale)
        {
            startScale = transform.localScale.x;
        }

        if (scaleOnStart)
        {
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

        movementTween = transform
            .DOScale(targetScale, transitionDuration)
            .SetLink(gameObject)
            .SetDelay(startDelay)
            .SetUpdate(useUnscaledTime) // Use the useUnscaledTime variable
            .OnComplete(() => onAnimationComplete?.Invoke());
    }
}
