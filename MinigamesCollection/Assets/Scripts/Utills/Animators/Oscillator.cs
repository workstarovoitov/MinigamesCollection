using UnityEngine;
using DG.Tweening;
using System;

public class Oscillator : MonoBehaviour
{
    [SerializeField] private Vector2 angleMinMax;
    public Vector2 AngleMinMax { get { return angleMinMax; } set { angleMinMax = value; } }

    [SerializeField] private float rotationDuration;
    public float RotationDuration { get { return rotationDuration; } set { rotationDuration = value; } }
    [SerializeField] private Ease easeType = Ease.Linear;
    private Tween rotationTween;

    private void Start()
    {
        RunAnimation();
    }

    public void StopAnimation()
    {
        if (rotationTween != null) rotationTween.Kill();
    }

    public void RunAnimation(Action onAnimationComplete = null)
    {
        // Rotate animation
        float initialRotation = transform.eulerAngles.z;
        float targetRotation = angleMinMax.y;
        rotationTween = DOTween.To(() => initialRotation, x => initialRotation = x, targetRotation, rotationDuration)
            .SetLink(gameObject)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo) // This line makes the rotation animation seamless
            .OnUpdate(() =>
            {
                Vector3 newRotation = new Vector3(0f, 0f, initialRotation);
                transform.eulerAngles = newRotation;
            });
    }
}