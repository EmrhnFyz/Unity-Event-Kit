using System;
using UnityEngine;

namespace UnityEventKit
{
    /// <summary>
    /// Generic channel wrapper – derive concrete classes from this.
    /// Fires through the global EventBus, so code-only subscribers still work.
    /// </summary>
    public abstract class EventChannelSO<T> : EventChannelBaseSO where T : struct, IEvent
    {
        public override Type EventType => typeof(T);

        public void Raise(T evnt) => EventBus.Global.Publish(evnt);

        internal override void RaiseBoxed(object boxed) => Raise((T)boxed);
    }
}
