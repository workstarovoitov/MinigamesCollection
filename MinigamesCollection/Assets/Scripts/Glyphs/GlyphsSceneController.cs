using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using FMODUnity;
using Architecture;
using System;
using Random = UnityEngine.Random;

public enum RingsPlacementPattern
{
    InSector,
    SameAngle,
    RandomAngle
}

[System.Serializable]
public class HintMaterials
{
    public Material glyphMaterial;
    public Material emissionMaterial;
    public Material ringMaterial;
}

public class GlyphsSceneController : Singleton<GlyphsSceneController>
{
    [SerializeField] private EventReference backgroundMusic;
    [SerializeField] private EventReference backgroundAmbience;

    [Header("Hints setup")]
    [SerializeField] private GameObject hintPrefab;
    [SerializeField] private float hintsCircleRadius;
    [SerializeField] private Transform hintsParent;
    [SerializeField] private List<HintMaterials> hintDefaultMaterials = new();
    [SerializeField] private float hintEmissionSwitchDuration = 0.75f;
    [SerializeField] private EventReference onHintOnEventRef;
    [SerializeField] private EventReference onHintOffEventRef;

    [Header("Small rings setup")]
    [SerializeField] private GameObject smallRingPrefab;
    [SerializeField] private float smallRingsCircleRadius;
    [SerializeField] private Transform smallRingsParent;

    [Header("Big rings setup")]
    [SerializeField] private GameObject outerRingPrefab;
    [SerializeField] private GameObject midRingPrefab;
    [SerializeField] private GameObject innerRingPrefab;
    [SerializeField] private Transform bigRingsParent;
   
    [Header("Glyphs setup")]
    [SerializeField] private GameObject glyphPrefab;
    [SerializeField] private Transform glyphsParent;

    [SerializeField] private float glyphsOuterCircleRadius;
    [SerializeField] private float glyphsMidCircleRadius;
    [SerializeField] private float glyphsInnerCircleRadius;

    [SerializeField] private float glyphsOuterAngleOffset;
    [SerializeField] private float glyphsMidAngleOffset;
    [SerializeField] private float glyphsInnerAngleOffset;

    [SerializeField] private float gyphSpawnRandomizedAreaRadius;
    
    [Header("Animations setup")]
    [SerializeField] private float outerSwitchDuration = 2f;
    [SerializeField] private float midSwitchDuration = 1.5f;
    [SerializeField] private float innerSwitchDuration = 1f;
    [SerializeField] private float smallSwitchDuration = .75f;
    [SerializeField] private float shadingDuration = 0.25f;
    [SerializeField] private Ease bigRingEase = Ease.Linear;
    [SerializeField] private Ease smallRingEase = Ease.Linear;
    [SerializeField] private EventReference onRotateEventRef;

    [Header("Win condition")]
    [SerializeField] private float winTransitionDuration = 1f;
    [SerializeField] private EventReference onStartEventRef;
    [SerializeField] private EventReference onWinEventRef;
    [SerializeField] private GameEvent onContentLoadedEvent;
    [SerializeField] private GameEvent onWinEvent;
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private float winLensSize = 4.5f;
    [SerializeField] private Ease cameraEase = Ease.Linear;

    [SerializeField] private float ringSpeedMultiplier = 50f;
    [SerializeField] private float ringScaleDelay = .25f;
    [SerializeField] private float ringScaleDuration = 1f;
    [SerializeField] private Ease scaleRingEase = Ease.Linear;
    [SerializeField] private Ease glyphsMoveEase = Ease.Linear;

    [SerializeField] private SpriteRenderer shadedBG;

    List<int> hintIndexes = new();

    private List<float> angles = new();

    private List<GameObject> hintGlyphs = new();
    private List<GameObject> smallRings = new();

    private List<GameObject> glyphsInner = new();
    private List<GameObject> glyphsMid = new();
    private List<GameObject> glyphsOuter = new();
    private List<GameObject> glyphsSmall = new();
    private List<Material> hintsEmissions = new();
    

    private int smallRingsAmount;
    private bool twoRingSetup;
    [SerializeField] private RingsPlacementPattern hintPlacement = RingsPlacementPattern.SameAngle;
    [SerializeField] private bool firstHintRandomPlaced = true;

    private bool contentLoaded = false;
    private int glyphsSolved = 0;
    
    private GameObject innerRing;
    private GameObject outerRing;
    private GameObject midRing;
    private float cameraSize;
    Sequence sequence;
    private ScenarioEntityGlyphs currentScenario;

    private void OnDisable()
    {
        RingController.ringClicked -= SwitchGlyphPositions;
    }

    private void Awake()
    {
        RingController.ringClicked += SwitchGlyphPositions;
    }


    void Start()
    {
        DOTween.SetTweensCapacity(5000, 50);
        cameraSize = sceneCamera.orthographicSize;
        SoundManager.Instance.PlayBackgroundMusic(backgroundMusic);
        SoundManager.Instance.PlayBackgroundAmbience(backgroundAmbience);
    }

    private void LoadContent()
    {
        contentLoaded = false;

        StartCoroutine(WaitForContent());
        sceneCamera.orthographicSize = cameraSize;

        angles = GlyphGenerator.GetAngles(smallRingsAmount, hintPlacement, firstHintRandomPlaced);

        ClearHints();
        CreateHints(smallRingsAmount, angles);
        ClearSmallRings();
        CreateSmallRings(smallRingsAmount, angles);

        ClearBigRings();
        CreateBigRings();

        ClearGlyphs();
        CreateGlyphs(smallRingsAmount, angles);

        UpdateLabels();

        shadedBG.color = new Color(0, 0, 0, 1);


        contentLoaded = true;
    }

    private void SwitchGlyphPositions(int ringNum, bool clockwise)
    {
        if (sequence != null && sequence.IsActive()) return;
        SoundManager.Instance.Shoot(onRotateEventRef);
        sequence = DOTween.Sequence();
        sequence.OnComplete(() =>
        {
            CheckSolution();
        });

        bool midAddedToEnd = false;
        if (ringNum >= 10) 
        {
            glyphsSmall.Clear();

            glyphsSmall.Add(glyphsInner[ringNum - 10]);
            glyphsSmall.Add(glyphsOuter[ringNum - 10]);
            if (!twoRingSetup)
            {
                float innerAngle = Mathf.Atan2(glyphsInner[ringNum - 10].transform.position.y - midRing.transform.position.y, glyphsInner[ringNum - 10].transform.position.x - midRing.transform.position.x);
                float midAngle = Mathf.Atan2(glyphsMid[ringNum - 10].transform.position.y - midRing.transform.position.y, glyphsMid[ringNum - 10].transform.position.x - midRing.transform.position.x);

                if ((innerAngle * Mathf.Rad2Deg + 360) % 360 < (midAngle * Mathf.Rad2Deg + 360) % 360)
                {
                    glyphsSmall.Add(glyphsMid[ringNum - 10]);
                    midAddedToEnd = true;
                }
                else 
                {
                    glyphsSmall.Insert(1, glyphsMid[ringNum - 10]);
                }
            }
        }

        switch (ringNum)
        {
            case 0: 
                SwitchGlyphPositions(glyphsOuter, outerRing.transform, clockwise, outerSwitchDuration, bigRingEase);
                ShiftGlyphList(glyphsOuter, clockwise);
                break;
            case 1:
                SwitchGlyphPositions(glyphsMid, midRing.transform, clockwise, midSwitchDuration, bigRingEase);
                ShiftGlyphList(glyphsMid, clockwise);
                break;
            case 2: 
                SwitchGlyphPositions(glyphsInner, innerRing.transform, clockwise, innerSwitchDuration, bigRingEase);
                ShiftGlyphList(glyphsInner, clockwise);
                break;
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
                SwitchGlyphPositions(glyphsSmall, smallRings[ringNum-10].transform, clockwise, smallSwitchDuration, smallRingEase);
                if (twoRingSetup) ShiftGlyphsBetweenTwoLists(ringNum -10);
                else ShiftGlyphsBetweenThreeLists(ringNum - 10, clockwise, midAddedToEnd);
                break;
        }
        CountSolvedGlyphs();
  
    }

    private void UpdateBgShading()
    {
        SetBgShading(1.0f - glyphsSolved * .01f);
    }

    private void SetBgShading(float alpha)
    {
        shadedBG.transform.DOKill();
        shadedBG.DOColor(new Color(0, 0, 0, alpha), shadingDuration);
    }

    private void CountSolvedGlyphs()
    {
        glyphsSolved = 0;
        for (int i = 0; i < hintGlyphs.Count; i++)
        {
            string glyphChar = hintGlyphs[i].GetComponentInChildren<TextMeshPro>().text;
            bool isSolved = glyphChar.Equals(glyphsInner[i].GetComponentInChildren<TextMeshPro>().text) &&
                            glyphChar.Equals(glyphsOuter[i].GetComponentInChildren<TextMeshPro>().text) &&
                            (twoRingSetup || glyphChar.Equals(glyphsMid[i].GetComponentInChildren<TextMeshPro>().text));

            // Update emission based on solved state
            SetHintEmission(hintsEmissions[i], isSolved);

            if (isSolved)
            {
                glyphsSolved++;
            }
        }
    }

    private void CheckSolution()
    {
        if (glyphsSolved < hintGlyphs.Count) return;

        outerRing.GetComponentInChildren<RingController>().enabled = false;
        innerRing.GetComponentInChildren<RingController>().enabled = false;
        if (midRing != null) midRing.GetComponentInChildren<RingController>().enabled = false;

        foreach (var ring in smallRings)
        {
            ring.GetComponentInChildren<RingController>().enabled = false;
        }

        SoundManager.Instance.Shoot(onWinEventRef);
        onWinEvent?.Invoke();

        RunFinalTransition();
    }

    private void RunFinalTransition()
    {
        // Create a Tween to animate the lens size over time
        DOTween.To(() => sceneCamera.orthographicSize, x => sceneCamera.orthographicSize = x, winLensSize, winTransitionDuration)
            .SetLink(sceneCamera.gameObject).SetEase(cameraEase);
        shadedBG.DOColor(new Color(0, 0, 0, 0), winTransitionDuration);
        foreach (GameObject glyph in hintGlyphs)
        {
            Color c = glyph.GetComponentInChildren<TextMeshPro>().color;
            glyph.GetComponentInChildren<TextMeshPro>().DOColor(new Color(c.r, c.g, c.b, 0f), ringScaleDuration);
            MeshRenderer ring = glyph.GetComponentInChildren<MeshRenderer>();
            if (ring != null)
            {
                ring.material.SetFloat("_ThicknessGlow", 0);
                ring.material.SetFloat("_ThicknessRing", 0);
            }
        }

        foreach (GameObject glyph in glyphsInner)
        {
            Color c = glyph.GetComponentInChildren<TextMeshPro>().color;
            glyph.GetComponentInChildren<TextMeshPro>().DOColor(new Color(c.r, c.g, c.b, 0f), ringScaleDuration);
            MeshRenderer ring = glyph.GetComponentInChildren<MeshRenderer>();
            if (ring != null)
            {
                ring.material.SetFloat("_ThicknessGlow", 0);
                ring.material.SetFloat("_ThicknessRing", 0);
            }
        }

        foreach (GameObject glyph in glyphsOuter)
        {
            Color c = glyph.GetComponentInChildren<TextMeshPro>().color;
            glyph.GetComponentInChildren<TextMeshPro>().DOColor(new Color(c.r, c.g, c.b, 0f), ringScaleDuration); MeshRenderer ring = glyph.GetComponentInChildren<MeshRenderer>();
            if (ring != null)
            {
                ring.material.SetFloat("_ThicknessGlow", 0);
                ring.material.SetFloat("_ThicknessRing", 0);
            }
        }

        foreach (GameObject glyph in glyphsMid)
        {
            Color c = glyph.GetComponentInChildren<TextMeshPro>().color;
            glyph.GetComponentInChildren<TextMeshPro>().DOColor(new Color(c.r, c.g, c.b, 0f), ringScaleDuration);
            MeshRenderer ring = glyph.GetComponentInChildren<MeshRenderer>();
            if (ring != null)
            {
                ring.material.SetFloat("_ThicknessGlow", 0);
                ring.material.SetFloat("_ThicknessRing", 0);
            }
        }

        foreach (var ring in smallRings)
        {
            ring.GetComponentInChildren<RingController>().HighlightRing(false);
            ring.GetComponentInChildren<RingController>().RotationSpeed *= ringSpeedMultiplier;

            ring.transform.DOScale(15, ringScaleDuration)
                .SetEase(scaleRingEase)
                .SetLink(gameObject);
        }

        if (twoRingSetup)
        {
            outerRing.GetComponentInChildren<RingController>().HighlightRing(true);
            innerRing.GetComponentInChildren<RingController>().HighlightRing(true);

            outerRing.transform.DOScale(5, ringScaleDuration)
                .SetEase(scaleRingEase)
                .SetDelay(ringScaleDelay)
                .SetLink(gameObject)
                .OnStart(() =>
            {
                outerRing.GetComponentInChildren<RingController>().RotationSpeed *= ringSpeedMultiplier;
            });
            innerRing.transform.DOScale(5, ringScaleDuration)
                .SetEase(scaleRingEase)
                .SetDelay(ringScaleDelay * 2)
                .SetLink(gameObject)
                .OnStart(() =>
            {
                innerRing.GetComponentInChildren<RingController>().RotationSpeed *= ringSpeedMultiplier;
            });
        }
        else
        {
            outerRing.GetComponentInChildren<RingController>().HighlightRing(true);
            midRing.GetComponentInChildren<RingController>().HighlightRing(true);
            innerRing.GetComponentInChildren<RingController>().HighlightRing(true);

            outerRing.transform.DOScale(5, ringScaleDuration)
                .SetEase(scaleRingEase)
                .SetDelay(ringScaleDelay)
                .SetLink(gameObject)
                .OnStart(() =>
            {
                outerRing.GetComponentInChildren<RingController>().RotationSpeed *= ringSpeedMultiplier;
            });
            midRing.transform.DOScale(5, ringScaleDuration)
                .SetEase(scaleRingEase)
                .SetDelay(ringScaleDelay * 2)
                .SetLink(gameObject).
                OnStart(() =>
            {
                midRing.GetComponentInChildren<RingController>().RotationSpeed *= ringSpeedMultiplier;
            });
            innerRing.transform.DOScale(5, ringScaleDuration)
                .SetEase(scaleRingEase)
                .SetDelay(ringScaleDelay * 3)
                .SetLink(gameObject)
                .OnStart(() =>
            {
                innerRing.GetComponentInChildren<RingController>().RotationSpeed *= ringSpeedMultiplier;
            });
        }
    }

    private void SwitchGlyphPositions(List<GameObject> glyphs, Transform center, bool clockwise, float duration, Ease ease)
    {
        // Loop through each glyph
        for (int i = 0; i < glyphs.Count; i++)
        {
            // Get the current and next glyph indices in the list
            int currentIndex = i;
            int nextIndex = clockwise ? (i - 1 + glyphs.Count) % glyphs.Count : (i + 1 + glyphs.Count) % glyphs.Count; ; // Wrap around to the first glyph if at the end of the list

            // Get the current and next glyph GameObjects
            GameObject currentGlyph = glyphs[currentIndex];
            GameObject nextGlyph = glyphs[nextIndex];

            // Get the positions of the current and next glyphs
            Vector3 currentPosition = currentGlyph.transform.position;
            Vector3 nextPosition = nextGlyph.transform.position; 

            float radius1 = Vector2.Distance(currentPosition, center.position);
            float radius2 = Vector2.Distance(nextPosition, center.position);

            float initialAngle = Mathf.Atan2(currentPosition.y - center.position.y, currentPosition.x - center.position.x);
            float targetAngle = Mathf.Atan2(nextPosition.y - center.position.y, nextPosition.x - center.position.x);
            
            // Determine the rotation direction based on clockwise flag
            float angleDifference = targetAngle - initialAngle;

            if (angleDifference < 0 && !clockwise) angleDifference += 2 * Mathf.PI;
            else if (angleDifference > 0 && clockwise) angleDifference -= 2 * Mathf.PI;
            targetAngle = initialAngle + angleDifference;

            sequence.Join(DOTween.To(() => 0f, t => currentGlyph.transform.position = CalcCurve(center.position, initialAngle, targetAngle, radius1, radius2, t), 1f, duration)
                .SetEase(ease).SetLink(gameObject));
        }
    }

    private void ShiftGlyphList(List<GameObject> glyphs, bool clockwise)
    {
        if (clockwise)
        {
            // Shift the list backward
            GameObject firstGlyph = glyphs[0];
            glyphs.RemoveAt(0);
            glyphs.Add(firstGlyph);
        }
        else
        {
            // Shift the list forward
            GameObject lastGlyph = glyphs[glyphs.Count - 1];
            glyphs.RemoveAt(glyphs.Count - 1);
            glyphs.Insert(0, lastGlyph);
        }
    }
 
    private void ShiftGlyphsBetweenTwoLists(int ringNum)
    {
        // Shift the list forward
        GameObject lastGlyph = glyphsInner[ringNum];
        glyphsInner[ringNum] = glyphsOuter[ringNum];
        glyphsOuter[ringNum] = lastGlyph; 
    }

    private void ShiftGlyphsBetweenThreeLists(int ringNum, bool clockwise, bool midAddedToEnd)
    {
        // Shift the list forward
        GameObject lastGlyph = glyphsInner[ringNum];
        if ((clockwise && midAddedToEnd) || (!clockwise && !midAddedToEnd)) 
        { 
            glyphsInner[ringNum] = glyphsOuter[ringNum];
            glyphsOuter[ringNum] = glyphsMid[ringNum]; 
            glyphsMid[ringNum] = lastGlyph; 
        }
        else
        {
            glyphsInner[ringNum] = glyphsMid[ringNum];
            glyphsMid[ringNum] = glyphsOuter[ringNum];
            glyphsOuter[ringNum] = lastGlyph;
        }
    }

    private static Vector3 CalcCurve(Vector3 centerPosition, float initialAngle, float targetAngle, float radius1, float radius2, float t)
    {
        float angle = Mathf.Lerp(initialAngle, targetAngle, t);
        float radius = Mathf.Lerp(radius1, radius2, t);
        // Calculate the position along the circle using the angle and radius
        float x = centerPosition.x + Mathf.Cos(angle) * radius;
        float y = centerPosition.y + Mathf.Sin(angle) * radius;

        return new Vector3(x, y, centerPosition.z);
    }

    IEnumerator WaitForContent()
    {
        while (!contentLoaded)
        {
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        onContentLoadedEvent?.Invoke();
    }

    private void ClearHints()
    {
        for (int i = hintsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(hintsParent.GetChild(i).gameObject);
        }
        hintGlyphs.Clear();
        hintsEmissions.Clear();
    }

    private void CreateHints(int amount, List<float> angles)
    {
        for (int i = 0; i < amount; i++)
        {
            float angleRad = angles[i] * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(angleRad) * hintsCircleRadius,
                Mathf.Sin(angleRad) * hintsCircleRadius,
                0f
            );

            GameObject hint = Instantiate(hintPrefab, position, Quaternion.identity, hintsParent);
            hintGlyphs.Add(hint);
        }
    }

    private void ClearSmallRings()
    {
        for (int i = smallRingsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(smallRingsParent.GetChild(i).gameObject);
        }
        smallRings.Clear();
    }

    private void CreateSmallRings(int amount, List<float> angles)
    {
        for (int i = 0; i < amount; i++)
        {
            float angleRad = angles[i] * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Cos(angleRad) * smallRingsCircleRadius,
                Mathf.Sin(angleRad) * smallRingsCircleRadius,
                0f
            );

            GameObject smallRing = Instantiate(smallRingPrefab, position, Quaternion.identity, smallRingsParent);
            smallRing.GetComponentInChildren<RingController>().RingNum = i + 10;
            smallRings.Add(smallRing);
        }
    }

    private void ClearBigRings()
    {
        for (int i = bigRingsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(bigRingsParent.GetChild(i).gameObject);
        }

        outerRing = null;
        midRing = null;     
        innerRing = null;
    }

    private void CreateBigRings()
    {
        outerRing = Instantiate(outerRingPrefab, bigRingsParent);
        outerRing.GetComponentInChildren<RingController>().RingNum = 0;
        if (!twoRingSetup)
        {
            midRing = Instantiate(midRingPrefab, bigRingsParent);
            midRing.GetComponentInChildren<RingController>().RingNum = 1;
        }
        innerRing = Instantiate(innerRingPrefab, bigRingsParent);
        innerRing.GetComponentInChildren<RingController>().RingNum = 2;
    }

    private void ClearGlyphs()
    {
        for (int i = glyphsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(glyphsParent.GetChild(i).gameObject);
        }

        glyphsInner.Clear();
        glyphsMid.Clear();
        glyphsOuter.Clear();
        glyphsSmall.Clear();
    }

    private void CreateGlyphs(int amount, List<float> angles)
    {
        for (int i = 0; i < amount; i++)
        {
            int offsetDirection = Random.Range(0, 2) == 0 ? 1 : -1;

            if (!twoRingSetup)
            {
                float angleMidRad = (angles[i] + offsetDirection * glyphsMidAngleOffset) * Mathf.Deg2Rad;
                Vector3 initialMidPosition = new Vector3(
                    Mathf.Cos(angleMidRad) * glyphsMidCircleRadius,
                    Mathf.Sin(angleMidRad) * glyphsMidCircleRadius,
                    0f
                );
                Vector3 finalMidPosition = GetRandomPointInCircle(initialMidPosition, gyphSpawnRandomizedAreaRadius);

                GameObject glyphMid = Instantiate(glyphPrefab, finalMidPosition, Quaternion.identity, glyphsParent);
                glyphsMid.Add(glyphMid);
            }

            float angleOuterRad = twoRingSetup ? (angles[i] + offsetDirection * glyphsOuterAngleOffset) * Mathf.Deg2Rad : (angles[i] - offsetDirection * glyphsOuterAngleOffset) * Mathf.Deg2Rad;
            Vector3 initialOuterPosition = new Vector3(
                Mathf.Cos(angleOuterRad) * glyphsOuterCircleRadius,
                Mathf.Sin(angleOuterRad) * glyphsOuterCircleRadius,
                0f
            );
            Vector3 finalOuterPosition = GetRandomPointInCircle(initialOuterPosition, gyphSpawnRandomizedAreaRadius);

            GameObject glyphOuter = Instantiate(glyphPrefab, finalOuterPosition, Quaternion.identity, glyphsParent);
            glyphsOuter.Add(glyphOuter);

            float angleInnerRad = (angles[i] - offsetDirection * glyphsInnerAngleOffset) * Mathf.Deg2Rad;
            Vector3 initialInnerPosition = new Vector3(
                Mathf.Cos(angleInnerRad) * glyphsInnerCircleRadius,
                Mathf.Sin(angleInnerRad) * glyphsInnerCircleRadius,
                0f
            );
            Vector3 finalInnerPosition = GetRandomPointInCircle(initialInnerPosition, gyphSpawnRandomizedAreaRadius);

            GameObject glyphInner = Instantiate(glyphPrefab, finalInnerPosition, Quaternion.identity, glyphsParent);
            glyphsInner.Add(glyphInner);
        }
    }

    private void UpdateLabels()
    {
        hintIndexes.Clear();
        for (int hintIndex = 0; hintIndex < 8; hintIndex++)
        {
            hintIndexes.Add(hintIndex);
        }

        GlyphGenerator.Shuffle(hintIndexes);

        List<string> symbols = GlyphGenerator.GetRandomUniqueSymbols(smallRingsAmount, 26);
        Dictionary<string, HintMaterials> hintMaterials = new Dictionary<string, HintMaterials>();
       
        int i = 0;
        foreach (var symbol in symbols)
        {
            hintMaterials.Add(symbol, hintDefaultMaterials[hintIndexes[i]]);
            i++;
        }

        i = 0;
        List<string> strings = new List<string>();
        foreach (var hint in hintGlyphs)
        {
            hint.GetComponentInChildren<TextMeshPro>().text = symbols[i];
            hint.GetComponentInChildren<TextMeshPro>().fontMaterial = Instantiate(hintMaterials[symbols[i]].glyphMaterial);

            strings.Add(symbols[i]);
            strings.Add(symbols[i]);
            if (!twoRingSetup) strings.Add(symbols[i]);

            // Find the "Emission" child object and set its material
            Transform emissionTransform = hint.transform.Find("Emission");
            if (emissionTransform != null)
            {
                MeshRenderer emissionRenderer = emissionTransform.GetComponent<MeshRenderer>();
                if (emissionRenderer != null)
                {
                    // Instantiate a new material for the emission
                    Material emissionMaterialInstance = Instantiate(hintDefaultMaterials[hintIndexes[i]].emissionMaterial);
                    emissionRenderer.material = emissionMaterialInstance;
                    hintsEmissions.Add(emissionMaterialInstance);
                }
            }

            // Find the "Ring" child object and set its material
            Transform ringTransform = hint.transform.Find("Ring");
            if (ringTransform != null)
            {
                MeshRenderer ringRenderer = ringTransform.GetComponent<MeshRenderer>();
                if (ringRenderer != null)
                {
                    // Instantiate a new material for the ring
                    Material ringMaterialInstance = Instantiate(hintDefaultMaterials[hintIndexes[i]].ringMaterial);
                    ringRenderer.material = ringMaterialInstance;
                }
            }
            i++;
        }

        GlyphGenerator.Shuffle(strings);

        i = 0;
        foreach (GameObject glyph in glyphsInner)
        {
            glyph.GetComponentInChildren<TextMeshPro>().text = strings[i];
            glyph.GetComponentInChildren<TextMeshPro>().fontMaterial = Instantiate(hintMaterials[strings[i]].glyphMaterial);
            i++;
        }
        foreach (GameObject glyph in glyphsOuter)
        {
            glyph.GetComponentInChildren<TextMeshPro>().text = strings[i];
            glyph.GetComponentInChildren<TextMeshPro>().fontMaterial = Instantiate(hintMaterials[strings[i]].glyphMaterial);
            i++;
        }
        foreach (GameObject glyph in glyphsMid)
        {
            glyph.GetComponentInChildren<TextMeshPro>().text = strings[i];
            glyph.GetComponentInChildren<TextMeshPro>().fontMaterial = Instantiate(hintMaterials[strings[i]].glyphMaterial);
            i++;
        }
    }

    private void SetHintEmission(Material material, bool enable)
    {
        float targetIntensity = enable ? 1.0f : 0.0f;
        float currentIntensity = material.GetFloat("_MaskSize");

        // Only change the intensity if the new intensity is different from the current intensity
        if (Mathf.Approximately(currentIntensity, targetIntensity)) return;
       
        if (enable) SoundManager.Instance.Shoot(onHintOnEventRef);
        else SoundManager.Instance.Shoot(onHintOffEventRef);
        
        // Stop any ongoing DOFloat tween on the material
        material.DOKill();

        // Start a new DOFloat tween to change the intensity
        material.DOFloat(targetIntensity, "_MaskSize", hintEmissionSwitchDuration);
    }

    private Vector3 GetRandomPointInCircle(Vector3 center, float radius)
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomRadius = Random.Range(0f, radius);
        float x = center.x + Mathf.Cos(randomAngle) * randomRadius;
        float y = center.y + Mathf.Sin(randomAngle) * randomRadius;
        return new Vector3(x, y, center.z);
    }

    public void StartGame(int difficulty)
    {
        SoundManager.Instance.Shoot(onStartEventRef);
        switch (difficulty)
        {
            case 0:
                twoRingSetup = true;
                smallRingsAmount = Random.Range(3, 5);
                break;
            case 1:
                twoRingSetup = Random.Range(0, 2) == 0 ? true : false;
                if (twoRingSetup) smallRingsAmount = Random.Range(5, 8);
                else smallRingsAmount = Random.Range(3, 5);
                break;
            case 2:
                twoRingSetup = false;
                smallRingsAmount = Random.Range(5, 8);
                break;
        }

        LoadContent();
    }

    public void StartGame()
    {
        SoundManager.Instance.Shoot(onStartEventRef);
        currentScenario = GameManager.Instance.CurrentScenario as ScenarioEntityGlyphs;

        if (currentScenario == null)
        {
            Debug.LogError("Current scenario is not set");
        }

        twoRingSetup = currentScenario.TwoRingSetup;
        smallRingsAmount = currentScenario.SmallRingsAmount;
        LoadContent();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
