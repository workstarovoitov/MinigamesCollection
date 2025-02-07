using UnityEngine;
using UnityEngine.Events;

namespace Architecture
{
    public class GameEventsListener : MonoBehaviour
    {
        [SerializeField] protected GameEvent[] _gameEvents;
        [SerializeField] protected UnityEvent _unityEvent;

        void OnEnable()
        {
            foreach (var gameEvent in _gameEvents)
            {
                gameEvent.Register(this);
            }
        }

        void OnDisable()
        {
            foreach (var gameEvent in _gameEvents)
            {
                gameEvent.Deregister(this);
            }
        }

        public void RaiseEvent()
        {
            if (gameObject.activeSelf)
            {
                _unityEvent.Invoke();
            }
        }
    }
}