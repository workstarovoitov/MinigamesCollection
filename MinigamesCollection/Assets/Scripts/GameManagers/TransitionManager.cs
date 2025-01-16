using Architecture;
using DG.Tweening;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Rendering;

public class TransitionManager : Singleton<TransitionManager>
{
    private float currentBlur = -0.1f;
    private float Blur
    {
        get => currentBlur;
        set
        {
            currentBlur = value;
            //if (blur != null) blur.intensity.value = value;
            //if (blurUI != null) blurUI.intensity.value = value;
        }
    }

    [SerializeField] Volume maskPPEVolume;
    [SerializeField] Volume maskPPEUIVolume;
    //[SerializeField] private Mask_PostProcessEffect blur;
    //[SerializeField] private Mask_PostProcessEffect blurUI;

    [SerializeField] float sceneStartAnimationDuration = 2f;
    [SerializeField] Ease  fadeInEase = Ease.InQuad;
    [SerializeField] float sceneEndAnimationDuration = 2f;
    [SerializeField] Ease fadeOutEase = Ease.OutQuad;

    [SerializeField] float fadeOutUIDuration = 1f;

    [SerializeField] private GameEvent sceneStartRoutineFinished;

    [SerializeField] private GameEvent sceneEndRoutineStarted;
    [SerializeField] private GameEvent sceneEndRoutineFinished;

    private Tweener intensityTween;

    private void Start()
    {
        //Mask_PostProcessEffect tmp;

        //if (maskPPEVolume.profile.TryGet<Mask_PostProcessEffect>(out tmp))
        //{
        //    blur = tmp;
        //}
        //if (maskPPEUIVolume.profile.TryGet<Mask_PostProcessEffect>(out tmp))
        //{
        //    blurUI = tmp;
        //}
    }

    private void OnDestroy()
    {
        Blur = -0.1f;
    }

    public void RunPostProcessSceneStart()
    {
        FadeIn();
    }

    public void RunPostProcessSceneEnd()
    {
        sceneEndRoutineStarted?.Invoke();

        StartCoroutine(FadeOutUIWaiter(FadeOut));
    }

    public void RunFadelessPostProcessSceneEnd(bool showStartTransition)
    {
        if (showStartTransition) Blur = 1.0f;
        sceneEndRoutineFinished?.Invoke();
    }

    private void FadeOut()
    {
        // Stop any running intensity tween
        intensityTween?.Kill();

        // Start a new intensity tween from 0 to 1
        intensityTween = DOTween.To(
                () => Blur,
                value => Blur = value,
                1f,
                sceneEndAnimationDuration)
            .SetLink(gameObject)
            .SetUpdate(true)
            .SetEase(fadeOutEase)
            .OnComplete(() =>
            {
                sceneEndRoutineFinished?.Invoke();
            });
    }

    private void FadeIn()
    {
        // Stop any running intensity tween
        if (intensityTween != null) intensityTween?.Kill();

        // Start a new intensity tween from 1 to 0
        intensityTween = DOTween.To(
                () => Blur,
                value => Blur = value,
                -0.1f,
                sceneStartAnimationDuration)
            .SetLink(gameObject)
            .SetEase(fadeInEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                sceneStartRoutineFinished?.Invoke();
            });
    }

    IEnumerator FadeOutUIWaiter(Action callback)
    {
        yield return new WaitForSecondsRealtime(fadeOutUIDuration);
        yield return new WaitForEndOfFrame();
        callback?.Invoke();
    }

    IEnumerator WaitForSaveFile(Action callback)
    {
        yield return new WaitForEndOfFrame();
        //while (GameManager.Instance.SaveController.LoadRoutineInProgress)
        //{
        //    yield return null;
        //}
        yield return new WaitForEndOfFrame();

        callback?.Invoke();
    }
}
