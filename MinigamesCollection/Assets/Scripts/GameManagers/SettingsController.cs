using UnityEngine;
using FMOD.Studio;
using UnityEngine.Audio;

public class SettingsController : MonoBehaviour
{
    [Header("Language")]
    [SerializeField] private string[] languages;

    private int currentLangIndex = 0;
    public int CurrentLangIndex
    {
        get => currentLangIndex;
        set
        {
            currentLangIndex = value;
            currentLangIndex = (currentLangIndex + languages.Length) % languages.Length;
            PlayerPrefs.SetInt("langIndex", currentLangIndex);
            PlayerPrefs.Save();
        }
    }

    [Header("Video")]
    [SerializeField] private Vector2Int[] resolution;
    private int fullscreenMode;
    public bool FullscreenMode
    {
        get
        {
            return fullscreenMode > 0 ? true : false;
        }
        set
        {
            fullscreenMode = value ? 1 : 0;
            PlayerPrefs.SetInt("fullscreenMode", fullscreenMode);
            PlayerPrefs.Save();
            UpdateVideoSettings();
        }
    }

    private int currentResIndex = 0;
    public int CurrentResIndex
    {
        get
        {
            return currentResIndex;
        }
        set
        {
            currentResIndex = value;
            currentResIndex = (currentResIndex + resolution.Length) % resolution.Length;
            PlayerPrefs.SetInt("resIndex", currentResIndex);
            PlayerPrefs.Save();
            UpdateVideoSettings();
        }
    }

    [Header("Sound")]
    [SerializeField] private string ambBusPath;
    [SerializeField] private string sfxBusPath;
    [SerializeField] private string voBusPath;

    private Bus sfxBus;
    private Bus ambBus;
    private Bus voBus;
    private float currentMusicLevel = 100;
    public float CurrentMusicLevel
    {
        get => currentMusicLevel;
        set
        {
            currentMusicLevel = Mathf.Clamp(value, 0f, 100f);
            PlayerPrefs.SetFloat("MusicLevel", currentMusicLevel);
            PlayerPrefs.Save();

            ambBus.setVolume(currentMusicLevel / 100f);
        }
    }

    private float currentSoundLevel = 100;
    public float CurrentSoundLevel
    {
        get => currentSoundLevel;
        set
        {
            currentSoundLevel = Mathf.Clamp(value, 0f, 100f);
            PlayerPrefs.SetFloat("SFXLevel", currentSoundLevel);
            PlayerPrefs.Save();

            sfxBus.setVolume(currentSoundLevel / 100f);
        }
    }

    private float currentVoiceLevel = 100;
    public float CurrentVoiceLevel
    {
        get => currentVoiceLevel;
        set
        {
            currentVoiceLevel = Mathf.Clamp(value, 0f, 100f);
            PlayerPrefs.SetFloat("VoiceLevel", currentVoiceLevel);
            PlayerPrefs.Save();

            voBus.setVolume(currentSoundLevel / 100f);
        }
    }

    private string actionPathPrefix = "key";


    // Start is called before the first frame update
    void Start()
    {
        // Load sound settings
        currentMusicLevel = PlayerPrefs.HasKey("MusicLevel") ? PlayerPrefs.GetFloat("MusicLevel") : 100;
        currentSoundLevel = PlayerPrefs.HasKey("SFXLevel") ? PlayerPrefs.GetFloat("SFXLevel") : 100;
        currentVoiceLevel = PlayerPrefs.HasKey("VoiceLevel") ? PlayerPrefs.GetFloat("VoiceLevel") : 100;

        sfxBus = FMODUnity.RuntimeManager.GetBus(sfxBusPath);
        ambBus = FMODUnity.RuntimeManager.GetBus(ambBusPath);
        voBus = FMODUnity.RuntimeManager.GetBus(voBusPath);

        sfxBus.setVolume(currentSoundLevel / 100f);
        ambBus.setVolume(currentMusicLevel / 100f);
        voBus.setVolume(currentVoiceLevel / 100f);

        // Load lng settings
        currentLangIndex = PlayerPrefs.HasKey("langIndex") ? PlayerPrefs.GetInt("langIndex") : 0;

        //Load video settings
        fullscreenMode = PlayerPrefs.HasKey("fullscreenMode") ? PlayerPrefs.GetInt("fullscreenMode") : 1;
        currentResIndex = PlayerPrefs.HasKey("resIndex") ? PlayerPrefs.GetInt("resIndex") : 0;
        UpdateVideoSettings();
    }

    public void UpdateVideoSettings()
    {
        Screen.SetResolution(resolution[currentResIndex].x, resolution[currentResIndex].y, fullscreenMode > 0 ? true : false);
    }

    public string GetLanguage()
    {
        return languages[currentLangIndex];
    }

    public string GetResolution()
    {
        return resolution[currentResIndex].x.ToString() + "x" + resolution[currentResIndex].y.ToString();
    }

    public void SaveKeyBinding(string actionName, string path)
    {
        PlayerPrefs.SetString(actionPathPrefix + actionName, path);
        PlayerPrefs.Save();
    }

    public string LoadKeyBinding(string actionName)
    {
        string path = PlayerPrefs.HasKey(actionPathPrefix + actionName) ? PlayerPrefs.GetString(actionPathPrefix + actionName) : string.Empty;
        return path;
    }

    public float RemapVolumeLevel(float value)
    {
        float normalizedValue = Mathf.Clamp01(value / 100);
        if (value <= 5f) normalizedValue = -80f;
        float mappedValueDecibels = Mathf.Lerp(-30, 0, Mathf.Pow(normalizedValue, 0.5f)); // Applying a square root for smoother adjustment
        return mappedValueDecibels;
    }
}
