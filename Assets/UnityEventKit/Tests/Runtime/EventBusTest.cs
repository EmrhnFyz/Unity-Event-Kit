using NUnit.Framework;
using UnityEventKit;

public sealed class EventBusTests
{
	private EventBus _bus;

	#region Sample Events ----------------------------------------------------

	private readonly struct Ping : IEvent 
	{
		public readonly int Value;
		public Ping(int value) => Value = value;
	}

	private readonly struct Pong : IEvent { }

	#endregion ---------------------------------------------------------------

	[SetUp] public void Setup() => _bus = new EventBus();

	[Test]
	public void Publish_Reaches_Subscriber()
	{
		int received = 0;
		_bus.Subscribe<Ping>(p => received = p.Value);

		_bus.Publish(new Ping(42));

		Assert.AreEqual(42, received);
	}

	[Test]
	public void DisposeToken_Unsubscribes()
	{
		int count = 0;
		var token = _bus.Subscribe<Pong>(_ => count++);

		token.Dispose();
		_bus.Publish(new Pong());

		Assert.AreEqual(0, count);
	}

	[Test]
	public void MultipleHandlers_AllInvoked()
	{
		int a = 0, b = 0;
		_bus.Subscribe<Ping>(_ => a++);
		_bus.Subscribe<Ping>(_ => b++);

		_bus.Publish(new Ping(0));

		Assert.AreEqual(1, a);
		Assert.AreEqual(1, b);
	}

}