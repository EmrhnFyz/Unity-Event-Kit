using System;

namespace UnityEventKit
{
    public interface IEventChannel
    {
        Type EventType { get; }
        void RaiseBoxed(object evt);
    }
}
