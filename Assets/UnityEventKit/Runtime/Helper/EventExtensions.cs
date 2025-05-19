namespace UnityEventKit
{
	public static class EventExtensions
	{
        /// <summary>
        ///     Enqueue <paramref name="evt" /> so it’s delivered on the NEXT Unity frame.
        /// </summary>
        public static void PublishNextFrame<T>(this T evnt) where T : struct, IEvent
		{
			EventBus.Global.PublishQueued(evnt, 1);
		}
	}
}