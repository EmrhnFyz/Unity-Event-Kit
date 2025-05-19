using System.Collections.Generic;

namespace UnityEventKit
{
    /// <summary>
    ///     Generic pooled wrapper that stores the payload by value
    ///     so we never allocate or box when calling PublishQueued.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class QueuedEvent<T> : IQueuedEvent where T : struct, IEvent
	{
		private T _payload;

		private static readonly Stack<QueuedEvent<T>> _pool = new();

		public static QueuedEvent<T> Get(in T payload)
		{
			var queue = _pool.Count > 0 ? _pool.Pop() : new QueuedEvent<T>();

			queue._payload = payload;
			return queue;
		}

		public void Dispatch(EventBus bus)
		{
			bus.Publish(_payload);
		}

		public void Clear()
		{
			_payload = default;
			_pool.Push(this);
		}
	}
}