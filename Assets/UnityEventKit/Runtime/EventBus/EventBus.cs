using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEventKit
{
	/// <summary>
	///     Disposable token that automatically removes the registered callback
	/// </summary>
	public readonly struct SubscriptionToken : IDisposable
	{
		private readonly IEventBus _eventBus;
		private readonly Type _eventType;
		private readonly Delegate _handler;
		private readonly bool _isValid;

		internal SubscriptionToken(IEventBus eventBus, Type eventType, Delegate handler)
		{
			_eventBus = eventBus;
			_eventType = eventType;
			_handler = handler;
			_isValid = true;
		}

		public void Dispose()
		{
			if (!_isValid)
			{
				return;
			}

			_eventBus.UnsubscribeInternal(_eventType, _handler);
		}
	}

	/// <summary>
	///     Default singleton implementation of the event bus.
	/// </summary>
	public sealed class EventBus : IEventBus
	{
		private static readonly Lazy<EventBus> _global = new(() => new EventBus());
		public static EventBus Global => _global.Value;

		private readonly Dictionary<Type, ISubscriberList> _routes = new();
		private readonly ConcurrentQueue<IQueuedEvent> _queue = new();
		private readonly List<(int frame, IQueuedEvent queue)> _delayed = new();

		#region IEventBus Implementation

		#region Subscribe

		public SubscriptionToken Subscribe<T>(Action<T> handler) where T : struct, IEvent
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}

			var type = typeof(T);
			if (!_routes.TryGetValue(type, out var list))
			{
				list = new SubscriberList<T>();
				_routes[type] = list;
			}

			list.Add(handler);

			return new SubscriptionToken(this, type, handler);
		}

		#endregion

		#region Publish (Immediate)

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Publish<T>(in T message) where T : struct, IEvent
		{
			if (_routes.TryGetValue(typeof(T), out var list))
			{
				((SubscriberList<T>)list).Invoke(message);
			}

#if UNITY_EDITOR
			EventDebuggerRuntime.RecordEvent(typeof(T), message);
#endif
		}

		#endregion

		#region PublishQueued / DrainQueued

		public void PublishQueued<T>(in T evt, int delayFrames = 0) where T : struct, IEvent
		{
			var wrapper = QueuedEvent<T>.Get(evt);

			if (delayFrames <= 0)
			{
				_queue.Enqueue(wrapper);
			}
			else
			{
				lock (_delayed)
				{
					_delayed.Add((Time.frameCount + delayFrames, wrapper));
				}
			}
		}

		public void DrainQueued()
		{
			var now = Time.frameCount;

			lock (_delayed)
			{
				for (var i = _delayed.Count - 1; i >= 0; --i)
				{
					if (_delayed[i].frame > now)
					{
						continue;
					}

					_queue.Enqueue(_delayed[i].queue);

					_delayed.RemoveAt(i);
				}
			}

			while (_queue.TryDequeue(out var queue))
			{
				queue.Dispatch(this);
				queue.Clear();
			}
		}

		#endregion


		void IEventBus.UnsubscribeInternal(Type eventType, Delegate handler)
		{
			if (!_routes.TryGetValue(eventType, out var list))
			{
				return;
			}

			list.Remove(handler);

			if (list.Count == 0)
			{
				_routes.Remove(eventType);
			}
		}

		#endregion
	}
}