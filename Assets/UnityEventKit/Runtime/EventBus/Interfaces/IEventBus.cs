using System;

namespace UnityEventKit
{
	/// <summary>
	/// Public surface for any bus implementation.
	/// </summary>
	public interface IEventBus
	{
		SubscriptionToken Subscribe<T>(Action<T> handler) where T : struct, IEvent;
		void Publish<T>(in T message) where T : struct, IEvent;

		#region Internal Field
		void UnsubscribeInternal(Type eventType, Delegate handler);
		#endregion
	}
}