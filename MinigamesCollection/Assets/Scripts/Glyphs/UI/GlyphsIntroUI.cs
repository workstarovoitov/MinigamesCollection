using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class GlyphsIntroUI : MonoBehaviour
{
    [SerializeField] private Toggle shortToggle;
    [SerializeField] private Toggle mediumToggle;
    [SerializeField] private Toggle longToggle;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
   
    [SerializeField] private UIAlphaFader backgroundPanel;
    [SerializeField] private UIAlphaFader uiPanel;

    [SerializeField] private EventReference onButtonClickEventRef;

    private bool isUpdatingToggles = false;
    
    private void Start()
    {
        startButton.interactable = true;
        quitButton.interactable = true;

        mediumToggle.isOn = true;
        shortToggle.onValueChanged.AddListener(OnShortToggle);
        mediumToggle.onValueChanged.AddListener(OnMediumToggle);
        longToggle.onValueChanged.AddListener(OnLongToggle);
        startButton.onClick.AddListener(OnStartButton);
        quitButton.onClick.AddListener(OnQuitButton);
    }
    
    public void OnSceneReady()
    {
        uiPanel.RunAnimationFadeOut(() =>
                                        {
                                            backgroundPanel.RunAnimationFadeOut();
                                        });
    }
    public void OnSceneRestarted()
    {
        backgroundPanel.RunAnimationFadeIn(() =>
        {
            startButton.interactable = true;
            quitButton.interactable = true;
            uiPanel.RunAnimationFadeIn();
        });
    }

    private void ShowPanel()
    {
        //uiPanel.GetComponent<CanvasGroup>().blocksRaycasts = true;
        //GetComponent<UIAlphaFader>().RunAnimationFadeIn();
    }

    private void OnShortToggle(bool isOn)
    {
        if (isUpdatingToggles) return;

        SoundManager.Instance.Shoot(onButtonClickEventRef);

        if (isOn)
        {
            isUpdatingToggles = true;
            mediumToggle.isOn = false;
            longToggle.isOn = false;
            isUpdatingToggles = false;
        }
        else
        {
            shortToggle.isOn = true;
        }
    }

    private void OnMediumToggle(bool isOn)
    {
        if (isUpdatingToggles) return;

        SoundManager.Instance.Shoot(onButtonClickEventRef);

        if (isOn)
        {
            isUpdatingToggles = true;
            shortToggle.isOn = false;
            longToggle.isOn = false;
            isUpdatingToggles = false;
        }
        else
        {
            mediumToggle.isOn = true;
        }
    }

    private void OnLongToggle(bool isOn)
    {
        if (isUpdatingToggles) return;

        SoundManager.Instance.Shoot(onButtonClickEventRef);

        if (isOn)
        {
            isUpdatingToggles = true;
            shortToggle.isOn = false;
            mediumToggle.isOn = false;
            isUpdatingToggles = false;
        }
        else
        {
            longToggle.isOn = true;
        }
    }



    private void OnStartButton()
    {
        SoundManager.Instance.Shoot(onButtonClickEventRef);
        startButton.interactable = false;
        quitButton.interactable = false;
        if (shortToggle.isOn)
        {
            GlyphsSceneController.Instance.StartGame(0);
        }
        else if (mediumToggle.isOn)
        {
            GlyphsSceneController.Instance.StartGame(1);
        }
        else if (longToggle.isOn)
        {
            GlyphsSceneController.Instance.StartGame(2);
        }
    }

    private void OnQuitButton()
    {
        SoundManager.Instance.Shoot(onButtonClickEventRef);
        startButton.interactable = false;
        quitButton.interactable = false;
        GlyphsSceneController.Instance.QuitGame();
    }
}
