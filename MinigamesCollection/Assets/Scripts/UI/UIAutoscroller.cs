using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using System;
using System.Collections;
using UnityEngine.InputSystem;
using Architecture;

public enum ScrollState
{
    Disabled,
    ScrollToEnd,
    ScrollToStart,
}

public class UIAutoscroller : MonoBehaviour
{
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float scrollSpeed = .25f;
    public float ScrollSpeed { set => scrollSpeed = value; }

    [SerializeField] private ScrollState stateOnStart = ScrollState.ScrollToEnd;

    [SerializeField] private UnityEvent onScrollEnd;
    [SerializeField] private UnityEvent onScrollStart;

    private ScrollRect scrollRect;
    private bool isPlaying;
    private Sequence currentSequence;
    public bool IsPlaying { get => isPlaying; }
    private Coroutine animationCoroutine;
    private InputAction scrollAction;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            Debug.LogWarning("UIAutoscroller requires a ScrollRect component.");
        }
    }

    private void OnEnable()
    {
        InitializeInputAction();
        SetScrollPosition(Vector2.one);
        animationCoroutine = StartCoroutine(AnimationRoutine());
    }

    private void OnDisable()
    {
        StopAnimation();
        if (scrollAction != null)
        {
            scrollAction.performed -= OnScrollPerformed;
            scrollAction.canceled -= OnScrollCanceled;
        }
    }

    private void InitializeInputAction()
    {
        var inputController = ServiceLocator.Get<InputController>();
        if (inputController != null)
        {
            scrollAction = inputController.Actions.UI.ScrollSlider;
            scrollAction.Enable();
            scrollAction.performed += OnScrollPerformed;
            scrollAction.canceled += OnScrollCanceled;
        }
    }

    private void OnScrollPerformed(InputAction.CallbackContext context)
    {
        StopAnimation();
    }

    private void OnScrollCanceled(InputAction.CallbackContext context)
    {
        animationCoroutine = StartCoroutine(AnimationRoutine());
    }

    public void RunAnimation(Action onAnimationComplete = null)
    {
        switch (stateOnStart)
        {
            case ScrollState.ScrollToEnd:
                RunScrollToEnd(onAnimationComplete);
                break;
            case ScrollState.ScrollToStart:
                RunScrollToStart(onAnimationComplete);
                break;
        }
    }

    public void StopAnimation()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = null;
        if (currentSequence != null) currentSequence.Kill();
        isPlaying = false;
    }

    public void RunScrollToEnd(Action onAnimationComplete = null)
    {
        StopAnimation();

        isPlaying = true;
        onScrollStart?.Invoke();

        float duration = Vector2.Distance(scrollRect.normalizedPosition, Vector2.zero) / scrollSpeed;

        currentSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
        currentSequence.Append(scrollRect.DONormalizedPos(Vector2.zero, duration).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isPlaying = false;
                onScrollEnd?.Invoke();
                OnAnimationComplete(onAnimationComplete);
            }));
    }

    public void RunScrollToStart(Action onAnimationComplete = null)
    {
        StopAnimation();

        isPlaying = true;
        onScrollStart?.Invoke();

        float duration = Vector2.Distance(scrollRect.normalizedPosition, Vector2.one) / scrollSpeed;

        currentSequence = DOTween.Sequence().SetLink(gameObject).SetUpdate(true);
        currentSequence.Append(scrollRect.DONormalizedPos(Vector2.one, duration).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isPlaying = false;
                onScrollEnd?.Invoke();
                OnAnimationComplete(onAnimationComplete);
            }));
    }

    private void OnAnimationComplete(Action onAnimationComplete)
    {
        onAnimationComplete?.Invoke();
    }

    public void SetScrollPosition(Vector2 position)
    {
        scrollRect.normalizedPosition = position;
    }

    public void SetToDefault()
    {
        SetScrollPosition(Vector2.one);
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }

    public void FireEvent(GameEvent gameEvent) => gameEvent?.Invoke();

    IEnumerator AnimationRoutine()
    {
        yield return new WaitForSecondsRealtime(startDelay);
        RunAnimation();
    }
}
