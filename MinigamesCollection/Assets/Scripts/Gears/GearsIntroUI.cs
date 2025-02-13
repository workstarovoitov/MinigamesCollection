using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using TMPro;
using System.Collections.Generic;

public class GearsIntroUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private List<ScenarioEntity> scenarios;

    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
   
    [SerializeField] private UIMoverSimple uiPanel;

    [SerializeField] private EventReference onButtonClickEventRef;
    [SerializeField] private EventReference onSlideOut;
    [SerializeField] private EventReference onSlideIn;
    private int currentScenarioIndex = 0;

    private void Start()
    {
        levelLabel.text = (currentScenarioIndex + 1).ToString();

        previousButton.interactable = true;
        nextButton.interactable = true;
        startButton.interactable = true;
        quitButton.interactable = true;

        previousButton.onClick.AddListener(() => OnLevelChangeButton(-1));
        nextButton.onClick.AddListener(() => OnLevelChangeButton(1));
        startButton.onClick.AddListener(OnStartButton);
        quitButton.onClick.AddListener(OnQuitButton);
    }
    
    public void OnSceneReady()
    {
    }
   
    public void OnSceneRestarted()
    {
        SoundManager.Instance.Shoot(onButtonClickEventRef);
        SoundManager.Instance.Shoot(onSlideIn);

        uiPanel.SetStartAsTarget();
        uiPanel.RunAnimation(() =>
        {
            GearsSceneController.Instance.ClearContent();
            previousButton.interactable = true;
            nextButton.interactable = true;
            startButton.interactable = true;
            quitButton.interactable = true;
        });
    }

    private void OnLevelChangeButton(int num)
    {
        SoundManager.Instance.Shoot(onButtonClickEventRef);
        currentScenarioIndex = (currentScenarioIndex + num + scenarios.Count) % scenarios.Count;
        levelLabel.text = (currentScenarioIndex + 1).ToString();
    }

    private void OnStartButton()
    {
        SoundManager.Instance.Shoot(onButtonClickEventRef);
        SoundManager.Instance.Shoot(onSlideOut);
       
        previousButton.interactable = false;
        nextButton.interactable = false;
        startButton.interactable = false;
        quitButton.interactable = false;

        uiPanel.SetEndAsTarget();
        uiPanel.RunAnimation(() =>
        {
            
        });

        GearsSceneController.Instance.StartGame(scenarios[currentScenarioIndex]);
    }

    private void OnQuitButton()
    {
        SoundManager.Instance.Shoot(onButtonClickEventRef);
        startButton.interactable = false;
        quitButton.interactable = false;
        Application.Quit();
    }
}
