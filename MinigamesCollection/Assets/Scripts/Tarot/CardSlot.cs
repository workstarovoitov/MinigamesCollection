using UnityEngine;
using FMODUnity;
using DG.Tweening;

public class CardSlot : MonoBehaviour
{
    private CardController card;
    public CardController Card { get => card; set => card = value; }
    public bool IsFree { get => card == null; }
    
    [SerializeField] private SpriteRenderer glow;
    [SerializeField] private EventReference getEventRef;
    [SerializeField] private float transitionDuration;

    private Tweener outlineTween;

    public void ShowParticle() => GetComponentInChildren<ParticleSystem>().Play();
    
    public void ShowOutline(bool show = true)
    {
        if (show) SoundManager.Instance.Shoot(getEventRef);

        float targetAlpha = show ? 1f : 0f;
        Color glowColor = glow.color;
        glowColor.a = targetAlpha;

        outlineTween?.Kill();

        outlineTween = glow.DOFade(targetAlpha, transitionDuration)
            .SetLink(gameObject)
            .SetUpdate(true)
            .OnComplete(() => glow.color = glowColor);
    }
}
