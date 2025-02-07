using UnityEngine;
using FMODUnity;
using System;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using Random = UnityEngine.Random;

public class RingController : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private int ringNum;
    [SerializeField] private EventReference onEnterEventRef;
    [SerializeField] private EventReference onExitEventRef;

    [SerializeField] private float scaleTime = 0.25f;

    [SerializeField] private Material ringMaterial;
    [SerializeField] private Transform ringOuter;
    [SerializeField] private Transform ringInner;
    [SerializeField] private float ringOuterRingScale = 1.2f;
    [SerializeField] private float ringInnerRingScale = 0.8f;
    [SerializeField] private float ringColorMultiplier = 1.5f;

    [SerializeField] private GameObject glyphPrefab;
    [SerializeField] private int glyphsNum;
    [SerializeField] private float radiusDefault;
    [SerializeField] private float radiusHighlighted;

    [SerializeField] private float fontSizeDefault;
    [SerializeField] private float fontSizeHighlighted;
    [SerializeField] private float glyphAlphaDefault = 0.5f;
   
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private bool randomDirection = false;
    [SerializeField] private bool alignVertically = false;
    #endregion

    #region Public Properties
    public int RingNum { get => ringNum; set => ringNum = value; }
    public float RotationSpeed
    {
        get => rotationSpeed;
        set
        {
            rotationSpeed = value;
            RotateEndlessly();
        }
    }
    public bool Clockwise { get => clockwise; }
    #endregion

    #region Private Fields
    private List<TextMeshPro> glyphs = new();
    private bool clockwise;
    private GameObject generatedGlyphs;
    private int lastGlyphNum = -1;
    private float ringOuterDefaultScale;
    private float ringInnerDefaultScale;
    private bool hovered = false;

    Sequence sequenceFontSize;
    Sequence sequenceFontColor;
    Sequence sequenceGlyphsRadius;
    Sequence sequenceRingsSize;
    Sequence sequenceRingsColor;
    #endregion

    #region Static Actions
    public static Action<RingController> ringHovered;
    public static Action<int, bool> ringClicked;
    #endregion

    #region Unity Methods

    private void Start()
    {
        BackgroundHoverController.bgHovered += UnhoverRing;
        RingController.ringHovered += UnhoverRing;

        if (sequenceFontSize != null && sequenceFontSize.IsPlaying()) sequenceFontSize.Kill();
        if (sequenceFontColor != null && sequenceFontColor.IsPlaying()) sequenceFontColor.Kill();
        if (sequenceGlyphsRadius != null && sequenceGlyphsRadius.IsPlaying()) sequenceGlyphsRadius.Kill();
        if (sequenceRingsSize != null && sequenceRingsSize.IsPlaying()) sequenceRingsSize.Kill();
        if (sequenceRingsColor != null && sequenceRingsColor.IsPlaying()) sequenceRingsColor.Kill();

        GenerateGlyphs();
        InitializeRing();
    }

    private void OnDisable()
    {
        UnhoverRing();
  
        BackgroundHoverController.bgHovered -= UnhoverRing;
        RingController.ringHovered -= UnhoverRing;
    }
    #endregion

    #region Public Methods
    public void HoverRing()
    {
        if (!enabled || hovered) return;
        hovered = true;
        ringHovered?.Invoke(this);

        SoundManager.Instance.Shoot(onEnterEventRef);
        HighlightRing(true);
    }

    public void ClickRing()
    {
        ringClicked?.Invoke(ringNum, Clockwise);
    }

    public void ClickRingClockwise()
    {
        ringClicked?.Invoke(ringNum, true);
    }

    public void ClickRingCounterClockwise()
    {
        ringClicked?.Invoke(ringNum, false);
    }

    public void UnhoverRing(RingController ring)
    {
        if (ring == this) return;
        UnhoverRing();
    }

    public void UnhoverRing()
    {
        if (!hovered) return;
        hovered = false;

        SoundManager.Instance.Shoot(onExitEventRef);
        HighlightRing(false);
    }

    public void HighlightRing(bool highlight)
    {
        if (highlight)
        {
            SetGlyphsHighlighted(1f);
            SetFontSize(fontSizeHighlighted);
            SetGlyphsRadius(radiusHighlighted);

            ScaleRings(ringOuterRingScale, ringInnerRingScale);
            ScaleRingsHDR(ringColorMultiplier);
        }
        else
        {
            SetGlyphsHighlighted(glyphAlphaDefault);
            SetFontSize(fontSizeDefault);
            SetGlyphsRadius(radiusDefault);

            ScaleRings(1f, 1f);
            ScaleRingsHDR(1f);
        }
    }

    private void InitializeRing()
    {
        ringOuterDefaultScale = ringOuter.localScale.x;
        ringInnerDefaultScale = ringInner.localScale.x;

        clockwise = randomDirection ? Mathf.Sign(Random.Range(-1f, 1f)) >= 0 : rotationSpeed >= 0;

        // Create instances of the material and assign them to the MeshRenderer components
        if (ringMaterial != null)
        {
            MeshRenderer outerRenderer = ringOuter.GetComponent<MeshRenderer>();
            if (outerRenderer != null)
            {
                outerRenderer.material = Instantiate(ringMaterial);
            }

            MeshRenderer innerRenderer = ringInner.GetComponent<MeshRenderer>();
            if (innerRenderer != null)
            {
                innerRenderer.material = Instantiate(ringMaterial);
            }
        }

        HighlightRing(false);
        RotateEndlessly();
    }

    private void RotateEndlessly()
    {
        transform.DOKill();
        float angle = clockwise ? -360f : 360f;

        transform.DOLocalRotate(new Vector3(0f, 0f, angle), 360f / Mathf.Abs(rotationSpeed), RotateMode.FastBeyond360)
                 .SetLink(gameObject)
                 .SetLoops(-1)
                 .SetEase(Ease.Linear);
    }

    private void GenerateGlyphs()
    {
        transform.DOKill();
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        glyphs.Clear();

        Transform glyphsHolder = transform.Find("Glyphs");
        DestroyImmediate(glyphsHolder?.gameObject);

        generatedGlyphs = new GameObject("Glyphs");
        generatedGlyphs.transform.SetParent(transform);
        generatedGlyphs.transform.localPosition = Vector3.zero;

        float angleIncrement = 360f / glyphsNum;
        for (int i = 0; i < glyphsNum; i++)
        {
            CreateSingleGlyph(i, angleIncrement);
        }
    }

    private void CreateSingleGlyph(int index, float angleIncrement)
    {
        float angle = index * angleIncrement;
        float radAngle = Mathf.Deg2Rad * angle;
        float x = radiusDefault * Mathf.Cos(radAngle);
        float y = radiusDefault * Mathf.Sin(radAngle);

        GameObject newRune = Instantiate(glyphPrefab, generatedGlyphs.transform);
        RectTransform runeTransform = newRune.GetComponent<RectTransform>();
        runeTransform.localPosition = new Vector3(x, y, 0f);

        if (!alignVertically) newRune.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);

        TextMeshPro runeText = newRune.GetComponentInChildren<TextMeshPro>();
        if (runeText != null)
        {
            runeText.fontSize = fontSizeDefault;
            runeText.text = GetRandomSymbol();
            glyphs.Add(runeText);
        }
    }

    private string GetRandomSymbol()
    {
        int glyphNum = lastGlyphNum;
        while (glyphNum == lastGlyphNum)
        {
            lastGlyphNum = Random.Range(0, GlyphGenerator.SymbolsAll.Length);
        }
        return GlyphGenerator.SymbolsAll[lastGlyphNum];
    }

    private void SetGlyphsHighlighted(float alpha)
    {
        if (generatedGlyphs == null || glyphs.Count == 0) return;

        if (sequenceFontColor != null && sequenceFontColor.IsActive()) sequenceFontColor.Kill();
        sequenceFontColor = DOTween.Sequence();

        foreach (TextMeshPro glyph in glyphs)
        {
            sequenceFontColor.Join(glyph.DOColor(new Color(glyph.color.r, glyph.color.g, glyph.color.b, alpha), scaleTime).SetLink(gameObject));
        }
        sequenceFontColor.Play();
    }

    private void SetFontSize(float fontSize)
    {
        if (generatedGlyphs == null || glyphs.Count == 0) return;

        if (sequenceFontSize != null && sequenceFontSize.IsActive()) sequenceFontSize.Kill();
        sequenceFontSize = DOTween.Sequence();

        foreach (TextMeshPro glyph in glyphs)
        {
            sequenceFontSize.Join(DOTween.To(() => glyph.fontSize, x => glyph.fontSize = x, fontSize, scaleTime).SetLink(gameObject));
        }
        sequenceFontSize.Play();
    }
   
    private void SetGlyphsRadius(float raduis)
    {
        float adjustedRadius =  raduis;
        float angleIncrement = 360f / glyphsNum;

        if (sequenceGlyphsRadius != null && sequenceGlyphsRadius.IsActive()) sequenceGlyphsRadius.Kill();
        sequenceGlyphsRadius = DOTween.Sequence();

        for (int i = 0; i < glyphs.Count; i++)
        {
            float angle = i * angleIncrement;
            float radAngle = Mathf.Deg2Rad * angle;
            float x = adjustedRadius * Mathf.Cos(radAngle);
            float y = adjustedRadius * Mathf.Sin(radAngle);

            RectTransform runeTransform = glyphs[i].GetComponent<RectTransform>();
            if (runeTransform != null)
            {
                sequenceGlyphsRadius.Join(runeTransform.DOLocalMove(new Vector3(x, y, 0f), scaleTime).SetLink(gameObject));
            }
        }
        sequenceGlyphsRadius.Play();
    }
  
    private void ScaleRings(float outerScale, float innerScale)
    {
        if (sequenceRingsSize != null && sequenceRingsSize.IsActive()) sequenceRingsSize.Kill();
        sequenceRingsSize = DOTween.Sequence();

        sequenceRingsSize.Join(ringOuter.DOScale(Vector3.one * ringOuterDefaultScale * outerScale, scaleTime).SetLink(ringOuter.gameObject));
        sequenceRingsSize.Join(ringInner.DOScale(Vector3.one * ringInnerDefaultScale * innerScale, scaleTime).SetLink(ringInner.gameObject));
        sequenceRingsSize.Play();
    }

    public void ScaleRingsHDR(float value)
    {
        if (sequenceRingsColor != null && sequenceRingsColor.IsActive()) sequenceRingsColor.Kill();
        sequenceRingsColor = DOTween.Sequence();

        if (ringOuter != null)
        {
            MeshRenderer outerRenderer = ringOuter.GetComponent<MeshRenderer>();
            if (outerRenderer != null && outerRenderer.material != null)
            {
                sequenceRingsColor.Join(outerRenderer.material.DOFloat(value, "_HDRMultiplier", scaleTime).SetLink(ringOuter.gameObject));
            }
        }

        if (ringInner != null)
        {
            MeshRenderer innerRenderer = ringInner.GetComponent<MeshRenderer>();
            if (innerRenderer != null && innerRenderer.material != null)
            {
                sequenceRingsColor.Join(innerRenderer.material.DOFloat(value, "_HDRMultiplier", scaleTime).SetLink(ringInner.gameObject));
            }
        }
        sequenceRingsColor.Play();
    }
    #endregion
}
