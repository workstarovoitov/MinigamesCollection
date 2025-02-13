using UnityEngine;
using DG.Tweening;
using System;
using Architecture;

public class UIMoverSimple : MonoBehaviour
{
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool useSpawnPoint = false;
    [SerializeField] private Ease curve = Ease.Linear;
    public Ease Curve { set => curve = value; }
    [SerializeField] private Vector2 startPoint;
    public Vector2 StartPoint { get => startPoint; set => startPoint = value; }
    [SerializeField] private Vector2 endPoint;
    public Vector2 EndPoint { get => endPoint; set => endPoint = value; }
    
    [SerializeField] private float startDelay;
    public float StartDelay { get => startDelay; set => startDelay = value; } 

    [SerializeField] private bool moveOnStart = true;
    [SerializeField] private float transitionDuration;
    [SerializeField] private float transitionSpeed;
    public float TransitionSpeed { get => transitionSpeed; set => transitionSpeed = value; }

    private Vector2 targetPoint;

    void Start()
    {
        if (!useSpawnPoint) GetComponent<RectTransform>().anchoredPosition = startPoint;
        SetEndAsTarget();
        if (moveOnStart)
        {
            RunAnimation();
        }
    }

    public void StopAnimation()
    {
        DOTween.Kill(GetComponent<RectTransform>(), false);
    }

    public void RunAnimation()
    {
        RunAnimation(null);
    }

    public void RunAnimation(Action onAnimationComplete = null)
    {
        StopAnimation();

        if (transitionSpeed > 0)
        {
            float distance = Vector2.Distance(GetComponent<RectTransform>().anchoredPosition, targetPoint);
            transitionDuration = distance / transitionSpeed;
        }

        GetComponent<RectTransform>()
            .DOAnchorPos(targetPoint, transitionDuration)
            .SetLink(gameObject)
            .SetDelay(startDelay)
            .SetUpdate(useUnscaledTime)
            .SetEase(curve)
            .OnComplete(() => OnAnimationComplete(onAnimationComplete));
    }

    private void OnAnimationComplete(Action onAnimationComplete)
    {
        // Animation has completed, execute the desired actions
        onAnimationComplete?.Invoke();
    }

    public void SetStartAsTarget()
    {
        targetPoint = startPoint;
    }

    public void SetEndAsTarget()
    {
        targetPoint = endPoint;
    }
   
    public void SetTarget(Vector2 target)
    {
        targetPoint = target;
    }

    public void FireEvent(GameEvent gameEvent) => gameEvent?.Invoke();
}
