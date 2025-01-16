using Architecture;
using System.Collections.Generic;

public class GameEventManager : Singleton<GameEventManager>
{
    private Dictionary<string, List<GameEventsListener>> _listeners = new();

    public void Register(string gameEvent, GameEventsListener listener)
    {
        if (string.IsNullOrEmpty(gameEvent)) return;
        if (!_listeners.ContainsKey(gameEvent))
        {
            _listeners[gameEvent] = new();
        }
        if (!_listeners[gameEvent].Contains(listener))
        {
            _listeners[gameEvent].Add(listener);
        }
    }

    public void Deregister(string gameEvent, GameEventsListener listener)
    {
        if (string.IsNullOrEmpty(gameEvent)) return;
        if (_listeners.ContainsKey(gameEvent))
        {
            _listeners[gameEvent].Remove(listener);
        }
    }

    public void InvokeEvent(string gameEvent)
    {
        if (string.IsNullOrEmpty(gameEvent)) return;

        if (_listeners.ContainsKey(gameEvent))
        {
            var listenersSnapshot2 = new List<GameEventsListener>(_listeners[gameEvent]);
            foreach (var listener in listenersSnapshot2)
            {
                if (listener != null && listener.isActiveAndEnabled)
                {
                    listener.RaiseEvent();
                }
            }
        }
    }
}
