using UnityEngine;
using DG.Tweening;
using System;

public class SimpleColorIntensity : MonoBehaviour
{
    private Color endColor;
    private Material material;

    [SerializeField] private float transitionDuration = 0.75f;
    private Tweener movementTween; // Store reference to the movement tween

    public void StopAnimation()
    {
        // Check if a movement tween exists
        if (movementTween != null)
        {
            movementTween.Kill(); // Kill the movement tween
        }
    }

    public void SetParams(Color end, Material mat)
    {
        endColor = end;
        material = mat;
    }

    public void SetParams(Color end)
    {
        endColor = end;
    }

    public void RunAnimation(Action onAnimationComplete = null)
    {
        movementTween = material.DOColor(endColor, "_Color", transitionDuration) // "_Color" is the name of the color property in the material
            .OnComplete(() => onAnimationComplete?.Invoke());
    }
}
