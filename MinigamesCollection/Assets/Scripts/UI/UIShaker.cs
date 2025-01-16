using DG.Tweening;
using UnityEngine;
using System;

public class UIShaker : MonoBehaviour
{
    [SerializeField] private float startDelay;
    [SerializeField] private float shakeDuration;
    [SerializeField] private int shakeVibrato;
    [SerializeField] private float shakeStrength;
    public float ShakeStrength { set => shakeStrength = value; }

    [SerializeField] private float shakeRandomness;
    [SerializeField] private int rotationVibrato;
    [SerializeField] private float rotationStrength;
    public float RotationStrength { set => rotationStrength = value; }

    private RectTransform rectTransform;
    private Sequence shakeSequence;
    private Sequence rotationSequence;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogWarning("UIShaker requires a RectTransform component.");
        }
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    public void RunAnimation(Action onAnimationComplete = null)
    {
        ResetSequences();

        shakeSequence = CreateShakeSequence();
        rotationSequence = CreateRotationSequence();

        shakeSequence.Join(rotationSequence)
            .OnComplete(() =>
            {
                rectTransform.localRotation = Quaternion.identity; // Reset rotation at the end of the animation
                OnAnimationComplete(onAnimationComplete);
            });
    }

    public void StopAnimation()
    {
        ResetSequences();
        rectTransform.localRotation = Quaternion.identity; // Reset rotation when killing tween
    }

    private void ResetSequences()
    {
        shakeSequence?.Kill();
        rotationSequence?.Kill();
    }

    private Sequence CreateShakeSequence()
    {
        return DOTween.Sequence()
            .SetLink(gameObject)
            .Append(rectTransform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness, false, true)
            .SetDelay(startDelay)
            .SetUpdate(true));
    }

    private Sequence CreateRotationSequence()
    {
        if (rotationVibrato < 2) rotationVibrato = 2;
        float halfDuration = shakeDuration / rotationVibrato * 0.5f;

        return DOTween.Sequence()
            .SetLink(gameObject)
            .Append(rectTransform.DORotate(new Vector3(0, 0, rotationStrength), halfDuration).SetEase(Ease.InOutQuad))
            .Append(rectTransform.DORotate(new Vector3(0, 0, -rotationStrength), halfDuration).SetEase(Ease.InOutQuad))
            .SetLoops(rotationVibrato, LoopType.Restart);
    }
    private void OnAnimationComplete(Action onAnimationComplete)
    {
        onAnimationComplete?.Invoke();
    }
    public void FireEvent(GameEvent gameEvent) => gameEvent?.Invoke();
}
