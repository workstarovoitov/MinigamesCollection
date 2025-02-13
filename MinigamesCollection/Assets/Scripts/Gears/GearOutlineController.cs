using UnityEngine;
using FMODUnity;
using DG.Tweening;

public class GearOutlineController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer srOutline;
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private EventReference getEventRef;
    [SerializeField] private float transitionDuration;

    private Tweener outlineTween;
    
    private Material currentMaterial;
    private Material CurrentMaterial
    {
        get
        {
            if (currentMaterial == null)
            {
                currentMaterial = Instantiate(outlineMaterial);
            }
            return currentMaterial;
        }
    }

    private void Start()
    {
        srOutline.material = CurrentMaterial;
    }

    private void OnValidate()
    {
        srOutline.material = CurrentMaterial;
    }

    public void SetOutlineColor(Color baseColor, Color color)
    {
        CurrentMaterial.SetColor("_Color", color); // Directly set the color without animation
        CurrentMaterial.SetColor("_ColorBase", baseColor); // Directly set the color without animation
    }

    public void SetOutlineThickness(float thickness, float innerRadius)
    {
        CurrentMaterial.SetFloat("_Thickness", thickness);
        CurrentMaterial.SetFloat("_InnerRadius", innerRadius);
    }

    public void ShowOutline(bool show = true, bool shootSound = true)
    {
        if (show && shootSound) SoundManager.Instance?.Shoot(getEventRef);

        float targetAlpha = show ? 1f : 0f;
        Color glowColor = srOutline.color;
        glowColor.a = targetAlpha;

        outlineTween?.Kill();

        outlineTween = srOutline.DOFade(targetAlpha, transitionDuration)
            .SetLink(gameObject)
            .SetUpdate(true)
            .OnComplete(() => srOutline.color = glowColor);
    }
}
