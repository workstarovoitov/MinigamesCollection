using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace Architecture
{
    public class GameEventManager : Singleton<GameEventManager>
    {
        private Dictionary<AssetReference, List<GameEventsListener>> _listeners = new();

        public void Register(AssetReference gameEvent, GameEventsListener listener)
        {
            if (gameEvent == null || !gameEvent.RuntimeKeyIsValid()) return;
            if (!_listeners.ContainsKey(gameEvent))
            {
                _listeners[gameEvent] = new();
            }
            if (!_listeners[gameEvent].Contains(listener))
            {
                _listeners[gameEvent].Add(listener);
            }
        }

        public void Deregister(AssetReference gameEvent, GameEventsListener listener)
        {
            if (gameEvent == null || !gameEvent.RuntimeKeyIsValid()) return;
            if (_listeners.ContainsKey(gameEvent))
            {
                _listeners[gameEvent].Remove(listener);
            }
        }

        public void InvokeEvent(AssetReference gameEvent)
        {
            if (gameEvent == null || !gameEvent.RuntimeKeyIsValid()) return;

            if (_listeners.ContainsKey(gameEvent))
            {
                var listenersSnapshot = new List<GameEventsListener>(_listeners[gameEvent]);
                foreach (var listener in listenersSnapshot)
                {
                    if (listener != null && listener.isActiveAndEnabled)
                    {
                        listener.RaiseEvent();
                    }
                }
            }
        }
    }
}
