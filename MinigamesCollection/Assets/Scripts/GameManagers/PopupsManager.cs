using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using Architecture;

[System.Serializable]
public class PopupSpawnEvent
{
    public BasePopupController controller;
    public bool pauseOn;
}

public class PopupsManager : MonoBehaviour, IService
{
    [SerializeField] private List<PopupSpawnEvent> popupMenus = new();

    public void Initialize()
    {
        // Initialization logic here
    }

    public void ClearPopupList()
    {
        popupMenus.Clear();
    }

    public void AddPopup(BasePopupController newPopup)
    {
        if (!newPopup.BlocksPrevious && !newPopup.PausesGame) return;

        if (popupMenus.Count > 0 && newPopup.BlocksPrevious)
        {
            foreach (PopupSpawnEvent popupMenu in popupMenus)
            {
                if (popupMenu == null || popupMenu.controller == null) continue;
                if (newPopup != popupMenu.controller) popupMenu.controller.enabled = false;
            }
        }

        if (newPopup.PausesGame) ServiceLocator.Get<PauseController>()?.PauseScene();
        PopupSpawnEvent popupSpawnEvent = new PopupSpawnEvent();
        popupSpawnEvent.controller = newPopup;
        popupSpawnEvent.pauseOn = newPopup.PausesGame;
        popupMenus.Add(popupSpawnEvent);
        newPopup.enabled = true;
    }

    public void SetPause(bool pauseOn)
    {
        if (popupMenus.Count == 0) return;
        popupMenus[popupMenus.Count - 1].pauseOn = pauseOn;
        if (pauseOn) ServiceLocator.Get<PauseController>()?.PauseScene();
        else ServiceLocator.Get<PauseController>()?.UnpauseScene();
    }

    public void RemovePopup(BasePopupController popup)
    {
        if (popupMenus == null || popupMenus.Count == 0) return;
        if (popupMenus[popupMenus.Count - 1].controller != popup)
        {
            popupMenus.RemoveAll(popupMenu => popupMenu.controller == popup);
            return;
        }

        popupMenus.RemoveAt(popupMenus.Count - 1);

        if (popupMenus.Count > 0)
        {
            foreach (PopupSpawnEvent popupMenu in popupMenus)
            {
                popupMenu.controller.enabled = false;
            }
            if (!popupMenus[popupMenus.Count - 1].pauseOn) ServiceLocator.Get<PauseController>()?.UnpauseScene();
        }
        else
        {
            ServiceLocator.Get<PauseController>()?.UnpauseScene();
        }

        StartCoroutine(EnableLastController());
    }

    public bool IsOpened(BasePopupController popup)
    {
        return popupMenus?.Any(popupMenu => popupMenu.controller == popup) ?? false;
    }

    private IEnumerator EnableLastController()
    {
        yield return new WaitForEndOfFrame();

        if (popupMenus.Count > 0 && popupMenus[popupMenus.Count - 1].controller != null)
        {
            popupMenus[popupMenus.Count - 1].controller.enabled = true;
        }
    }
}
