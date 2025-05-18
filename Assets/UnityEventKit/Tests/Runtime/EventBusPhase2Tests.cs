using System.Threading;
using NUnit.Framework;
using UnityEventKit;

public sealed class EventBusPhase2Tests
{
	private EventBus _bus;

	private readonly struct Ping : IEvent 
	{
		public readonly int Value;
		public Ping(int value) => Value = value;
	}

	[SetUp] public void Setup() => _bus = new EventBus();

	[Test]
	public void PublishQueued_HitsSubscriber_WhenDrained()
	{
		int count = 0;
		_bus.Subscribe<Ping>(_ => count++);

		_bus.PublishQueued(new Ping(1));
		Assert.AreEqual(0, count, "Should not fire until DrainQueued");

		_bus.DrainQueued();
		Assert.AreEqual(1, count);
	}

	[Test]
	public void PublishQueued_FromBackgroundThread_IsSafe()
	{
		int received = 0;
		_bus.Subscribe<Ping>(p => received = p.Value);

		var thread = new Thread(() => _bus.PublishQueued(new Ping(99)));
		thread.Start(); thread.Join();

		_bus.DrainQueued();
		Assert.AreEqual(99, received);
	}
}