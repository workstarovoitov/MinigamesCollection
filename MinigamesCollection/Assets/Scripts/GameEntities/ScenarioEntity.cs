using UnityEngine;
using FMODUnity;
using UnityEngine.AddressableAssets;

public enum SceneType { Default, Cutscene, Conversation, Minigame, ActionScene, Transition, Map, MainMenu };

[CreateAssetMenu(menuName = "Scenario Entity", fileName = "New Scenario Entity")]

public class ScenarioEntity : ScriptableObject
{
    [SerializeField] private AssetReference selfReference;

    [Header("ScenarioPart description")]
    [field: SerializeField] private string scenarioPartTitle;
    public string ScenarioPartTitle { get => scenarioPartTitle; }

    [TextArea]
    [field: SerializeField] private string scenarioPartDescription;
    public string ScenarioPartDescription { get => scenarioPartDescription; }

    [Header("Sprites settings")]
    [SerializeField] private Sprite backgroundSprite;
    public Sprite BackgroundSprite { get => backgroundSprite; }

    [SerializeField] private GameObject extraContent;
    public GameObject ExtraContent { get => extraContent; }

    [Header("Transitions settings")]
    [field: SerializeField] private bool showStartTransition = true;
    public bool ShowStartTransition { get => showStartTransition; }
    
    [field: SerializeField] private bool showEndTransition = true;
    public bool ShowEndTransition { get => showEndTransition; }

    [field: SerializeField] private bool autosaveEnabled = true;
    public bool AutosaveEnabled { get => autosaveEnabled; }

    [Header("Music settings")]
    [field: SerializeField] private EventReference backgroundMusic;
    public EventReference BackgroundMusic { get => backgroundMusic; }

    [field: SerializeField] private EventReference backgroundAmbience;
    public EventReference BackgroundAmbience { get => backgroundAmbience; }

    [Header("Scene setup", order = 1)]
    [field: SerializeField] private string sceneName;
    public string SceneName { get => sceneName; }

    [field: SerializeField] private SceneType type;
    public SceneType Type { get => type; }

    [field: SerializeField] private ScenarioEntity nextScenario;
    public ScenarioEntity NextScenario { get => nextScenario; }

    [Header("UI copies")]
    [field: SerializeField] private string introTitle;
    public string IntroTitle { get => introTitle; }

    [TextArea]
    [field: SerializeField] private string introText;
    public string IntroText { get => introText; }

    [field: SerializeField] private string outroTitle;
    public string OutroTitle { get => outroTitle; }

    [TextArea]
    [field: SerializeField] private string outroText;
    public string OutroText { get => outroText; }

    [field: SerializeField] private GameObject contentPrefab;
    public GameObject ContentPrefab { get => contentPrefab; }

    public AssetReference GetSelfReference()
    {
        return selfReference;
    }
}
