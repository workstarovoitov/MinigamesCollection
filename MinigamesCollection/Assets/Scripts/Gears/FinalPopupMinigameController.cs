using System;
using TMPro;
using UnityEngine;

public class FinalPopupMinigameController : BasePopupController
{
    [SerializeField] private TextMeshProUGUI timeLabel;
    [SerializeField] private TextMeshProUGUI frictionsLabel;

    [SerializeField] private TextMeshProUGUI popupTitle;
    [SerializeField] private TextMeshProUGUI popupText;

    protected override void OnEnable()
    {
        base.OnEnable();
        ShowStatistics();
        DisplayFinalPanel();
    }

    public void ShowStatistics()
    {
        //float time = (GameManager.Instance.StatisticsController.CurrentScenarioData as MinigameStatistics).playedTime;
        //TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        //timeLabel.text = string.Format("{0}: {1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        //frictionsLabel.text = (GameManager.Instance.StatisticsController.CurrentScenarioData as MinigameStatistics).movesNum.ToString();
    }

    public void DisplayFinalPanel()
    {
        //MinigameScenario currentMinigameScenario = MasterMachine.Instance.GetCurrentScenario() as MinigameScenario;

        //if (currentMinigameScenario == null) return;

        //if (!string.IsNullOrEmpty(currentMinigameScenario.OutroTitle)) popupTitle.text = currentMinigameScenario.OutroTitle;
        //if (!string.IsNullOrEmpty(currentMinigameScenario.OutroText)) popupText.text = currentMinigameScenario.OutroText;
    }
}
