using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEventKit
{
	/// <summary>
	///     Records global-bus events for the Editor debug window.
	/// </summary>
	public static class EventDebuggerRuntime
	{
		public struct RecordedEvent
		{
			public int Frame;
			public DateTime TimeStamp;
			public Type EventType;
			public object Payload;
			public Object Source;
		}

		private static readonly List<RecordedEvent> History = new();
		private const int MaxHistory = 2000;

		public static void RecordEvent(Type eventType, object payload, Object source = null)
		{
			var frame = Time.frameCount;
			History.Add(new RecordedEvent
			            {
				            Frame = frame,
				            TimeStamp = DateTime.Now,
				            EventType = eventType,
				            Payload = payload,
				            Source = source
			            });

			if (History.Count > MaxHistory)
			{
				History.RemoveRange(0, History.Count - MaxHistory);
			}
		}

		public static RecordedEvent[] GetHistory()
		{
			return History.ToArray();
		}

		public static void ClearHistory()
		{
			History.Clear();
		}
	}
}