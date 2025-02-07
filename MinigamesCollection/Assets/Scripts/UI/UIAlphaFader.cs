using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System;
using Architecture;

public enum FadeState
{
    Disabled,
    FadeIn,
    FadeOut,
    FadeInOut,
    FadeOutIn
}

public class UIAlphaFader : MonoBehaviour
{
    [SerializeField] private float startDelay = 0f;
    public float StartDelay { get => startDelay; set => startDelay = value; }
    [SerializeField] private float fadeInDuration = 1f;
    public float FadeInDuration { set => fadeInDuration = value; }
    [SerializeField] private Ease fadeInEase = Ease.Linear;

    [SerializeField] private float fadeOutDuration = 1f;
    public float FadeOutDuration { set => fadeOutDuration = value; }
    [SerializeField] private Ease fadeOutEase = Ease.Linear;

    [SerializeField] private float showDuration = 1f;

    [SerializeField] private float levelOnStart = 0f;
    public float LevelOnStart { set => levelOnStart = value; }
    [SerializeField] private FadeState stateOnStart = FadeState.FadeIn;
    public FadeState CurrentState { get; set; }

    [SerializeField] private UnityEvent onFadeInEnd;
    [SerializeField] private UnityEvent onDelayEnd;
    [SerializeField] private UnityEvent onFadeOutEnd;

    private Sequence currentSequence;
    private CanvasGroup canvasGroup;
    private bool isPlaying;
    public bool IsPlaying { get => isPlaying; }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogWarning("UIAlphaFader requires a CanvasGroup component.");
        }
        CurrentState = stateOnStart;
    }

    private void OnEnable()
    {
        canvasGroup.alpha = levelOnStart;
        RunAnimation();
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    public void RunAnimation(Action onAnimationComplete = null)
    {
        switch (CurrentState)
        {
            case FadeState.FadeIn:
                RunAnimationFadeIn(onAnimationComplete);
                break;
            case FadeState.FadeOut:
                RunAnimationFadeOut(onAnimationComplete);
                break;
            case FadeState.FadeInOut:
                RunAnimationFadeInOut(onAnimationComplete);
                break;
            case FadeState.FadeOutIn:
                RunAnimationFadeOutIn(onAnimationComplete);
                break;
        }
    }

    public void StopAnimation()
    {
        if (currentSequence != null) currentSequence.Kill();
        isPlaying = false;
    }

    public void RunFadeIn()
    {
        RunAnimationFadeIn();
    }

    public void RunAnimationFadeIn(Action onAnimationComplete = null)
    {
        StopAnimation();

        float currentAlpha = canvasGroup.alpha;
        float animationDuration = fadeInDuration * (1f - currentAlpha);

        currentSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true).SetDelay(startDelay);
        isPlaying = true;

        currentSequence.Append(canvasGroup.DOFade(1f, animationDuration)
            .SetEase(fadeInEase)
            .OnComplete(() =>
            {
                isPlaying = false;
                onFadeInEnd?.Invoke();
                OnAnimationComplete(onAnimationComplete);
            }));
    }

    public void RunFadeOut()
    {
        RunAnimationFadeOut();
    }

    public void RunAnimationFadeOut(Action onAnimationComplete = null)
    {
        StopAnimation();

        float currentAlpha = canvasGroup.alpha;
        float animationDuration = fadeOutDuration * currentAlpha;

        currentSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true).SetDelay(startDelay);
        isPlaying = true;

        currentSequence.Append(canvasGroup.DOFade(0f, animationDuration)
            .SetEase(fadeOutEase)
            .OnComplete(() =>
            {
                isPlaying = false;
                onFadeOutEnd?.Invoke();
                OnAnimationComplete(onAnimationComplete);
            }));
    }

    public void RunAnimationFadeInOut(Action onAnimationComplete = null)
    {
        StopAnimation();

        float currentAlpha = canvasGroup.alpha;
        float animationDuration = fadeInDuration * (1f - currentAlpha);

        currentSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true).SetDelay(startDelay);
        isPlaying = true;

        currentSequence.Append(canvasGroup.DOFade(1f, animationDuration)
            .SetEase(fadeInEase)
            .OnComplete(() => onFadeInEnd?.Invoke()));

        currentSequence.AppendInterval(showDuration)
            .OnComplete(() => onDelayEnd?.Invoke());

        currentSequence.Append(canvasGroup.DOFade(0f, fadeOutDuration)
            .SetEase(fadeOutEase)
            .OnComplete(() =>
            {
                isPlaying = false;
                onFadeOutEnd?.Invoke();
                OnAnimationComplete(onAnimationComplete);
            }));
    }

    public void RunAnimationFadeOutIn(Action onAnimationComplete = null)
    {
        StopAnimation();

        float currentAlpha = canvasGroup.alpha;
        float animationDuration = fadeOutDuration * currentAlpha;

        currentSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true).SetDelay(startDelay);
        isPlaying = true;

        currentSequence.Append(canvasGroup.DOFade(0f, animationDuration)
            .SetEase(fadeOutEase)
            .OnComplete(() => onFadeOutEnd?.Invoke()));

        currentSequence.AppendInterval(showDuration)
            .OnComplete(() => onDelayEnd?.Invoke());

        currentSequence.Append(canvasGroup.DOFade(1f, fadeInDuration)
            .SetEase(fadeInEase)
            .OnComplete(() =>
            {
                isPlaying = false;
                onFadeInEnd?.Invoke();
                OnAnimationComplete(onAnimationComplete);
            }));
    }

    private void OnAnimationComplete(Action onAnimationComplete)
    {
        onAnimationComplete?.Invoke();
    }

    public void SetToDefault()
    {
        canvasGroup.alpha = levelOnStart;
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }

    public void FireEvent(GameEvent gameEvent) => gameEvent?.Invoke();
}
