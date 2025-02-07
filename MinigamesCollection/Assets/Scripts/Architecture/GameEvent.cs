using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Architecture
{
    [CreateAssetMenu(fileName = "GameEvent", menuName = "Scriptable Objects/GameEvent")]
    public class GameEvent : ScriptableObject
    {
        [SerializeField] private AssetReference assetReference;
        public AssetReference AssetReference => assetReference;

        public void Invoke()
        {
            GameEventManager.Instance.InvokeEvent(assetReference);
        }

        public void Register(GameEventsListener gameEventListener)
        {
            GameEventManager.Instance.Register(assetReference, gameEventListener);
        }

        public void Deregister(GameEventsListener gameEventListener)
        {
            GameEventManager.Instance.Deregister(assetReference, gameEventListener);
        }
    }
}