namespace UnityEventKit
{
	/// <summary>
	///     Generic value wrapper for simple payloads.
	/// </summary>
	public readonly struct ValueEvent<T> : IEvent
	{
		public readonly T Value;
		public ValueEvent(T value) => Value = value;

		public override string ToString()
		{
			return Value != null ? Value.ToString() : "null";
		}
	}

	/// <summary>
	///     Basic class every primitive event channel should inherit from.
	/// </summary>
	/// <typeparam name="T"> Type of the value to be passed in the event.</typeparam>
	public abstract class ValueEventChannelSO<T> : EventChannelSO<ValueEvent<T>>
	{
	}
}