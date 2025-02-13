using TMPro;
using UnityEngine;

public class GlyphsOutroUI : MonoBehaviour
{
    [SerializeField] private UIAlphaFader uiPanel;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private PredictionEntity predictionEntity;

    public void OnSceneFinished()
    {
        text.text = predictionEntity.predictions[Random.Range(0, predictionEntity.predictions.Count)];
        uiPanel.RunAnimationFadeIn();
    }

    public void OnSceneRestarted()
    {
        uiPanel.RunAnimationFadeOut();
    }
}
