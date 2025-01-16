using UnityEngine;

[CreateAssetMenu(menuName = "Game Event", fileName = "New Game Event")]
public class GameEvent : ScriptableObject
{
    [SerializeField] private string key;

    public void Invoke()
    {
        GameEventManager.Instance.InvokeEvent(key);
    }

    public void Register(GameEventsListener gameEventListener)
    {
        GameEventManager.Instance.Register(key, gameEventListener);
    }

    public void Deregister(GameEventsListener gameEventListener)
    {
        GameEventManager.Instance.Deregister(key, gameEventListener);
    }
}
