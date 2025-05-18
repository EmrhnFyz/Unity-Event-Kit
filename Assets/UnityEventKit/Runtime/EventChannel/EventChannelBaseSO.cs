using System;
using UnityEngine;

namespace UnityEventKit
{
    /// <summary>
    /// Non-generic root class for all event channels. So  the custom editor can talk to all event channels.
    /// </summary>
    public abstract class EventChannelBaseSO : ScriptableObject, IEventChannel
    {
        public abstract Type EventType { get; }
        
        void IEventChannel.RaiseBoxed(object evt) => RaiseBoxed(evt);

        internal abstract void RaiseBoxed(object boxed);
    }
}
