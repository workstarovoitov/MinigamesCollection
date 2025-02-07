using Architecture;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class GameManager : Singleton<GameManager>, IService
{
    private ScenarioEntity currentScenario;
    public ScenarioEntity CurrentScenario { get => currentScenario; }
    ScenarioEntity nextScenario;
    private GameObject content;
    private bool forcedTransition;
    public bool DebugEnabled { get => ServiceLocator.Get<DebugController>()?.DebugEnabled ?? false; }

    [SerializeField] private ScenarioEntity transitionToMenu;

    //[SerializeField] private GameEvent contentReady;
    [SerializeField] private GameEvent sceneReady;

    void Awake()
    {
        SceneManager.activeSceneChanged += OnSceneWasLoaded;
        RegisterServices();
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneWasLoaded;
    }

    public void Initialize()
    {
        // Initialization logic here
    }

    private void OnSceneWasLoaded(Scene oldScene, Scene newScene)
    {
        //contentReady?.Invoke();
    }

    private void SetNextScenario(ScenarioEntity scenario)
    {
        nextScenario = scenario;
    }

    private void Start()
    {
        SetScenario(currentScenario);
        LoadScene();
    }

    private void RegisterServices()
    {
        ServiceLocator.Register(this);
    }

    private void SetScenario(ScenarioEntity newScenario)
    {
        if (newScenario == null)
        {
            Debug.LogError("ScenarioEntity not defined");
            return;
        }

        currentScenario = newScenario;
        nextScenario = newScenario.NextScenario != null ? newScenario.NextScenario : transitionToMenu;
    }

    public void LoadScene()
    {
        if (currentScenario == null)
        {
            Debug.LogError("ScenarioEntity not defined");
            return;
        }

        if (GetSceneIndex(currentScenario.SceneName) < 0)
        {
            Debug.LogError("Game scene not defined " + currentScenario.SceneName);
            return;
        }

        SceneManager.LoadScene(currentScenario.SceneName, LoadSceneMode.Single);

        Log("Scenario <b>" + currentScenario.ScenarioPartTitle + "</b> set. Scene: " + currentScenario.SceneName, MessageCategory.State);
    }

    private int GetSceneIndex(string sceneName)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            if (SceneUtility.GetScenePathByBuildIndex(i).Contains(sceneName + ".unity"))
                return i;
        }
        return -1;
    }

    public void LoadDefaultSave()
    {
    }

    public void LoadAutoSave()
    {
    }

    public void EndScene()
    {
        EndScene(null);
    }

    public void EndScene(ScenarioEntity newScenario)
    {
        Log("Scenario <b>" + currentScenario.ScenarioPartTitle + "</b> ended", MessageCategory.State);

        if (newScenario != null) nextScenario = newScenario;

        StartNextScenario(nextScenario);
    }

    public void RestartScene()
    {
        Log("Scenario <b>" + currentScenario.ScenarioPartTitle + "</b> restarted", MessageCategory.State);

        forcedTransition = true;
        StartNextScenario(currentScenario);
    }

    public void SwitchToScene(ScenarioEntity newScenario)
    {
        if (newScenario == null) return;

        nextScenario = newScenario;
        Log("Scenario <b>" + currentScenario.ScenarioPartTitle + "</b> switched to <b>" + nextScenario.ScenarioPartTitle + "</b>", MessageCategory.State);

        forcedTransition = true;
        StartNextScenario(nextScenario);
    }

    public void StartNextScenario(ScenarioEntity newScenario)
    {
        SoundManager.Instance.StopMusic();
        bool showEndTransition = currentScenario.ShowEndTransition;
        SetScenario(newScenario);
        if (forcedTransition || showEndTransition) TransitionManager.Instance.RunPostProcessSceneEnd();
        else TransitionManager.Instance.RunFadelessPostProcessSceneEnd(forcedTransition || currentScenario.ShowStartTransition);
    }

    public void SceneReady()
    {
        Log("Content loaded", MessageCategory.State);
        sceneReady?.Invoke();

        if (forcedTransition || currentScenario.ShowStartTransition) TransitionManager.Instance.RunPostProcessSceneStart();

        forcedTransition = false;
    }

    public void QuitGame()
    {
        Log("Game Quit!", MessageCategory.State);
        Application.Quit();
    }

    public void Log(string message, MessageCategory entry = MessageCategory.Undefined)
    {
        var debugController = ServiceLocator.Get<DebugController>();
        debugController?.Log(message, entry);
    }
}
