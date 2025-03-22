using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class TarotIntroUI : MonoBehaviour
{
    [SerializeField] private List<ScenarioEntity> scenarios;

    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private UIAlphaFader backgroundPanel;
    [SerializeField] private UIAlphaFader uiPanel;
    [SerializeField] private GameObject selector;

    [SerializeField] private EventReference onButtonClickEventRef;
    [SerializeField] private EventReference onRotateEventRef;

    [SerializeField] private GameObject glyphPrefab;
    [SerializeField] private GameObject selectorRing;
    [SerializeField] private float switchDuration;
    [SerializeField] private Ease selectorEase;
    [SerializeField] private float selectorRadius;
    [SerializeField] private Transform glyphsHolder;

    [SerializeField] private string[] symbols;


    private int currentScenarioIndex = 0;
    Sequence sequence;
    private List<GameObject> glyphs = new();
    private List<float> angles = new();

    private void OnDisable()
    {
        RingController.ringClicked -= SwitchGlyphPositions;
    }

    private void Awake()
    {
        RingController.ringClicked += SwitchGlyphPositions;
    }

    private void Start()
    {
        startButton.interactable = true;
        quitButton.interactable = true;

        startButton.onClick.AddListener(OnStartButton);
        quitButton.onClick.AddListener(OnQuitButton);

        CreateGlyphs();
        UpdateLabels();
    }

    public void OnSceneReady()
    {
        backgroundPanel.RunAnimationFadeIn(() =>
        {
            uiPanel.gameObject.SetActive(false);
            selector.SetActive(false);
            backgroundPanel.RunAnimationFadeOut();
        });
    }

    public void OnSceneRestarted()
    {
        backgroundPanel.RunAnimationFadeIn(() =>
        {
            uiPanel.gameObject.SetActive(true);
            selector.SetActive(true);
            backgroundPanel.RunAnimationFadeOut();

            startButton.interactable = true;
            quitButton.interactable = true;
        });
    }

    private void OnLevelChangeButton(int num)
    {
        SoundManager.Instance.Shoot(onButtonClickEventRef);
        currentScenarioIndex = (currentScenarioIndex + num + scenarios.Count) % scenarios.Count;
    }

    private void OnStartButton()
    {
        SoundManager.Instance.Shoot(onButtonClickEventRef);
       
        startButton.interactable = false;
        quitButton.interactable = false;

        TarotSceneController.Instance.StartGame(scenarios[currentScenarioIndex]);
    }

    private void OnQuitButton()
    {
        SoundManager.Instance.Shoot(onButtonClickEventRef);
        startButton.interactable = false;
        quitButton.interactable = false;
        Application.Quit();
    }

    private void SwitchGlyphPositions(int ringNum, bool clockwise)
    {
        if (sequence != null && sequence.IsActive()) return;

        if (clockwise) currentScenarioIndex = (currentScenarioIndex + 1) % scenarios.Count;
        else currentScenarioIndex = (currentScenarioIndex - 1 + scenarios.Count) % scenarios.Count;

        SoundManager.Instance.Shoot(onRotateEventRef);
        sequence = DOTween.Sequence();

        SwitchGlyphPositions(glyphs, selectorRing.transform, !clockwise, switchDuration, selectorEase);
    }

    private void SwitchGlyphPositions(List<GameObject> glyphs, Transform center, bool clockwise, float duration, Ease ease)
    {
        // Loop through each glyph
        for (int i = 0; i < glyphs.Count; i++)
        {
            // Get the current and next glyph indices in the list
            int currentIndex = i;
            int nextIndex = clockwise ? (i - 1 + glyphs.Count) % glyphs.Count : (i + 1 + glyphs.Count) % glyphs.Count; // Wrap around to the first glyph if at the end of the list

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

    private void CreateGlyphs()
    {
        angles = GlyphGenerator.GetAngles(12, RingsPlacementPattern.SameAngle, false);
        for (int i = 0; i < 12; i++)
        {
            float angleRad = (angles[i]) * Mathf.Deg2Rad;
            Vector3 glyphPosition = new Vector3(
                Mathf.Cos(angleRad) * selectorRadius + selectorRing.transform.position.x,
                Mathf.Sin(angleRad) * selectorRadius + selectorRing.transform.position.y,
                0f
            );

            GameObject glyph = Instantiate(glyphPrefab, glyphPosition, Quaternion.identity, glyphsHolder);
            glyphs.Add(glyph);
        }
    }

    private void UpdateLabels()
    {
        for(int i = 0; i < 12; i++)
        {
            glyphs[i].GetComponentInChildren<TextMeshPro>().text = symbols[i];
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
}
