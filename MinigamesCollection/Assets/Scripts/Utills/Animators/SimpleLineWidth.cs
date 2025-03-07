using UnityEngine;
using DG.Tweening;
using System;

public class SimpleLineWidth : MonoBehaviour
{
    private float endWidth;
    private Material material;

    [SerializeField] private float transitionDuration = 0.75f;
    [SerializeField] private Ease transitionCurve = Ease.Linear;

    private Tweener movementTween; // Store reference to the movement tween

    public void StopAnimation()
    {
        // Check if a movement tween exists
        if (movementTween != null)
        {
            movementTween.Kill(); // Kill the movement tween
        }
    }

    public void SetParams(float end, Material mat)
    {
        endWidth = end;
        material = mat;
    }

    public void SetParams(float end)
    {
        endWidth = end;
    }

    public void RunAnimation(Action onAnimationComplete = null)
    {
        StopAnimation();

        movementTween = material
            .DOFloat(endWidth, "_Width", transitionDuration).SetLink(gameObject)
            .SetUpdate(true)
            .SetEase(transitionCurve)
            .SetLink(gameObject)
            .OnComplete(() => onAnimationComplete?.Invoke());
    }
}
