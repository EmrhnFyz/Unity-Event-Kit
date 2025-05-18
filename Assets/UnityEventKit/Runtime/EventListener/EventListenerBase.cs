using UnityEngine;
using UnityEngine.Events;

namespace UnityEventKit
{
    public abstract class EventListenerBase<T> : MonoBehaviour where T : struct, IEvent
    {
        [SerializeField] private EventChannelSO<T> channel;
        [SerializeField] private UnityEvent response;

        private SubscriptionToken _token;

        private void OnEnable()
        {
            if (channel == null)
            {
                return;
            }
            _token = EventBus.Global.Subscribe<T>(_ => response.Invoke());
        }

        private void OnDisable() => _token.Dispose();
    }
}
