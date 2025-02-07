using Architecture;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SoundClip
{
    public EventReference eventRef;
    public EventInstance eventInstance;
}

public class SoundManager : Singleton<SoundManager>
{
    private EventInstance bgMusic;
    private EventInstance bgAmbience;

    private List<SoundClip> soundList = new();

    private void Awake()
    {
        SceneManager.activeSceneChanged += OnSceneWasLoaded;
    }

    protected virtual void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneWasLoaded;
        StopMusic();
    }

    private void OnSceneWasLoaded(Scene oldScene, Scene newScene)
    {
        if (GameManager.Instance == null) return;
        ScenarioEntity cs = GameManager.Instance.CurrentScenario;
        if (cs == null) return;
        if (!cs.BackgroundMusic.IsNull)
            PlayBackgroundMusic(cs.BackgroundMusic);

        if (!cs.BackgroundAmbience.IsNull)
            PlayBackgroundAmbience(cs.BackgroundAmbience);
    }

    public void Shoot(EventReference eventRef)
    {
        if (!eventRef.IsNull) RuntimeManager.PlayOneShot(eventRef);
    }

    public void Shoot(string sfxPath)
    {
        if (string.IsNullOrEmpty(sfxPath)) return;
        if (RuntimeManager.PathToEventReference(sfxPath).IsNull) return;
        RuntimeManager.PlayOneShot(sfxPath);
    }

    public void Shoot(EventReference eventRef, string[] parameters = null, float[] values = null)
    {
        if (eventRef.IsNull) return;
        EventInstance instance = RuntimeManager.CreateInstance(eventRef);
        if (parameters != null && parameters.Length > 0)
        {
            for (int paramNum = 0; paramNum < parameters.Length; paramNum++)
            {
                instance.setParameterByName(parameters[paramNum], values[paramNum], false);
            }
        }

        instance.start();
        instance.release();
    }

    public void PlaySFX(EventReference eventRef, string[] parameters = null, float[] values = null)
    {
        if (eventRef.IsNull) return;
        foreach (var soundClip in soundList)
        {
            if (soundClip.eventRef.Equals(eventRef))
            {
                if (parameters == null || parameters.Length == 0) return;

                for (int paramNum = 0; paramNum < parameters.Length; paramNum++)
                {
                    soundClip.eventInstance.setParameterByName(parameters[paramNum], values[paramNum], false);
                }
                return;
            }
        }

        EventInstance instance = RuntimeManager.CreateInstance(eventRef);
        if (parameters != null && parameters.Length > 0)
        {
            for (int paramNum = 0; paramNum < parameters.Length; paramNum++)
            {
                instance.setParameterByName(parameters[paramNum], values[paramNum], false);
            }
        }
        instance.start();
        instance.release();

        SoundClip sc = new();
        sc.eventRef = eventRef;
        sc.eventInstance = instance;
        soundList.Add(sc);
    }

    public void StopSFX(EventReference eventRef)
    {
        if (eventRef.IsNull) return;
        foreach (var soundClip in soundList)
        {
            if (soundClip.eventRef.Equals(eventRef))
            {
                if (soundClip.eventInstance.isValid())
                {
                    soundClip.eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    soundClip.eventInstance.release();
                }
                soundList.Remove(soundClip);
                return;
            }
        }
    }

    public virtual void PlayBackgroundMusic(EventReference eventRefMusic)
    {
        if (!eventRefMusic.IsNull)
        {
            bgMusic.stop(0);
            bgMusic = RuntimeManager.CreateInstance(eventRefMusic);
            bgMusic.start();
            bgMusic.release();
        }
    }

    public virtual void PlayBackgroundAmbience(EventReference eventRefAmbience)
    {
        if (!eventRefAmbience.IsNull)
        {
            bgAmbience.stop(0);
            bgAmbience = RuntimeManager.CreateInstance(eventRefAmbience);
            bgAmbience.start();
            bgAmbience.release();
        }
    }

    public void StopMusic()
    {
        bgMusic.stop(0);
        bgAmbience.stop(0);

        foreach (var soundClip in soundList)
        {
            if (soundClip.eventInstance.isValid())
            {
                soundClip.eventInstance.stop(0);
                soundClip.eventInstance.release();
            }
        }
        soundList.Clear();
    }
}
