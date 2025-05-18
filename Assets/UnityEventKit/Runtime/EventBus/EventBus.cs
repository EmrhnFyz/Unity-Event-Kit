using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEventKit
{
	/// <summary>
	/// Disposable token that automatically removes the registered callback
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
	/// Default singleton implementation of the event bus.
	/// </summary>
	public sealed class EventBus : IEventBus
	{
		private static readonly Lazy<EventBus> _global = new(() => new EventBus());
		public static EventBus Global => _global.Value;

		private readonly Dictionary<Type, ISubscriberList> _routes = new();
		private readonly ConcurrentQueue<(Type, object)> _queue = new();
		
		#region IEventBus Implementation

		#region  Subscribe
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
			if(_routes.TryGetValue(typeof(T), out var list))
			{
				((SubscriberList<T>)list).Invoke(message);
			}
		}
		#endregion

		#region PublishQueued / DrainQueued
		public void PublishQueued<T>(in T message) where T : struct, IEvent
		{
			_queue.Enqueue((typeof(T), message));
		}

		public void DrainQueued()
		{
			while (_queue.TryDequeue(out var item))
			{
				if (_routes.TryGetValue(item.Item1, out var list))
				{
					list.InvokeBoxed(item.Item2);
				}
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