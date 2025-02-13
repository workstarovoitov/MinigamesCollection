using UnityEngine;
using UnityEngine.UI;
using Architecture;

public class GlyphsGameUI : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private GameEvent backGameEvent;
    [SerializeField] private UIAlphaFader uiPanel;
    [SerializeField] private float startDelay = 3f;

    private void Start()
    {
        backButton.onClick.AddListener(OnBackButton);
        backButton.interactable = false;
    }

    private void OnBackButton()
    {
        backButton.interactable = false;
        uiPanel.StartDelay = 0;
        uiPanel.RunAnimationFadeOut();
        backGameEvent?.Invoke();
    }

    public void OnSceneReady()
    {
        uiPanel.StartDelay = startDelay;
        uiPanel.RunAnimationFadeIn(() => backButton.interactable = true);
    }
}
