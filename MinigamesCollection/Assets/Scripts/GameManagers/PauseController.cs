using UnityEngine;
using System;

public class PauseController : MonoBehaviour
{
    public Action OnPause;
    public Action OnUnpause;

    public bool IsActive { get; set; }

    private void Start()
    {
        Time.timeScale = 1f;
    }

    public void PauseScene()
    {
        Time.timeScale = 0.0001f;
        OnPause?.Invoke();
    }

    public void UnpauseScene()
    {
        Time.timeScale = 1f;
        OnUnpause?.Invoke();
    }

    public void DisablePause()
    {
        IsActive = false;
    }

    public void EnablePause()
    {
        IsActive = true;
    }
}
