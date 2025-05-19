namespace UnityEventKit
{
	internal interface IQueuedEvent
	{
		void Dispatch(EventBus bus);
		void Clear();
	}
}