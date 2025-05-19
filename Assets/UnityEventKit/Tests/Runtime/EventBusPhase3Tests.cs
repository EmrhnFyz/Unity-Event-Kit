using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.Profiling;
using UnityEventKit;

public sealed class EventBusPhase3Tests
{
	private EventBus _bus;

	private readonly struct Ping : IEvent
	{
		public readonly int V;

		public Ping(int v)
		{
			V = v;
		}
	}

	[SetUp]
	public void Setup()
	{
		_bus = new EventBus();

		// Warm‐up: get one wrapper allocated & returned to the pool
		_bus.PublishQueued(new Ping(0));
		_bus.DrainQueued();

		// Clean up any gen-0 survivors
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	[Test]
	public void Queued_NoBoxing()
	{
		const int N = 10_000;
		var before = Profiler.GetMonoUsedSizeLong();

		for (var i = 0; i < N; i++)
		{
			_bus.PublishQueued(new Ping(i));
			_bus.DrainQueued();
		}

		var after = Profiler.GetMonoUsedSizeLong();
		Assert.Less(after - before, 1024, $"Expected <1 KB alloc for {N} single-drain events");
	}

	[Test]
	public void PublishQueued_WithDelay_DoesNotInvokeImmediately()
	{
		var hit = 0;
		_bus.Subscribe<Ping>(_ => hit++);
		_bus.PublishQueued(new Ping(0), 1);

		_bus.DrainQueued();
		Assert.AreEqual(0, hit, "Should not fire on the same DrainQueued call when delayed");
	}

	[Test]
	public void PublishQueued_WithDelay_EnqueuesForLater()
	{
		var hit = 0;
		_bus.Subscribe<Ping>(_ => hit++);
		_bus.PublishQueued(new Ping(0), 2);

		var delayedCount = GetDelayedCount(_bus);
		Assert.AreEqual(1, delayedCount, "The event should be sitting in the delayed list");
	}

	private static int GetDelayedCount(EventBus bus)
	{
		var field = typeof(EventBus).GetField("_delayed", BindingFlags.Instance | BindingFlags.NonPublic);
		var delayed = (IList)field.GetValue(bus);
		return delayed.Count;
	}
}