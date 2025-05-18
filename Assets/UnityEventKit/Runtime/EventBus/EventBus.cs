using System;
using System.Collections.Generic;

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

		private readonly Dictionary<Type, Delegate> _routes = new();

		#region IEventBus Implementation

		public SubscriptionToken Subscribe<T>(Action<T> handler) where T : struct, IEvent
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}
			
			var type = typeof(T);
			_routes.TryGetValue(type, out var existingDelegate);

			_routes[type] = (existingDelegate as Action<T>) + handler;
			
			return new SubscriptionToken(this, type, handler);
		}

		public void Publish<T>(in T message) where T : struct, IEvent
		{
			if(_routes.TryGetValue(typeof(T), out var existingDelegate))
			{
				// Safe cast
				((Action<T>)existingDelegate)?.Invoke(message);
			}
		}

		void IEventBus.UnsubscribeInternal(Type eventType, Delegate handler)
		{
			if (!_routes.TryGetValue(eventType, out var existingDelegate))
			{
				return;
			}
			
			var newDelegate = Delegate.Remove(existingDelegate, handler);

			if (newDelegate is null)
			{
				_routes.Remove(eventType);
			}
			else
			{
				_routes[eventType] = newDelegate;
			}
		}

		#endregion
	}
}