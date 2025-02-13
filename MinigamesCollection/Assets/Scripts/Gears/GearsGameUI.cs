using UnityEngine;
using UnityEngine.UI;
using Architecture;

public class GearsGameUI : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private GameEvent backGameEvent;

    private void Start()
    {
        backButton.onClick.AddListener(OnBackButton);
    }

    private void OnBackButton()
    {
        backButton.interactable = false;
        backGameEvent?.Invoke();
    }
}
