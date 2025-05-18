using System;
using UnityEngine;

namespace UnityEventKit
{
    [AddComponentMenu("")]
    internal sealed class EventBusDriver : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Bootstrap()
        {
            var go = new GameObject("[UnityEventKit] EventBusDriver");
            go.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(go);
            go.AddComponent<EventBusDriver>();
        }

        private void Update()
        {
            EventBus.Global.DrainQueued();
        }
    }
}
