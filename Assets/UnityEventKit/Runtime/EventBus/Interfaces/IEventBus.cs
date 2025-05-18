using System;

namespace UnityEventKit
{
	/// <summary>
	/// Public surface for any bus implementation.
	/// </summary>
	public interface IEventBus
	{
		SubscriptionToken Subscribe<T>(Action<T> handler) where T : struct, IEvent;
		
		/// <summary>
		/// Immediate, main-thread only.
		/// </summary>
		/// <param name="message"> The message to publish.</param>
		/// <typeparam name="T"> The type of the message.</typeparam>
		void Publish<T>(in T message) where T : struct, IEvent;
		
		/// <summary>
		/// Thread-safe, Enqueued adn flushed on the main thread.
		/// </summary>
		/// <param name="message"> The message to publish. </param>
		/// <typeparam name="T"> The type of the message. </typeparam>
		void PublishQueued<T>(in T message) where T : struct, IEvent;
		
		void UnsubscribeInternal(Type eventType, Delegate handler);

		void DrainQueued();
	}
}