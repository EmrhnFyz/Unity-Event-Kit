using System;
using UnityEngine;

namespace UnityEventKit
{
	/// <summary>
	///     Generic channel wrapper – derive concrete classes from this.
	///     Fires through the global EventBus, so code-only subscribers still work.
	/// </summary>
	public abstract class EventChannelSO<T> : EventChannelBaseSO where T : struct, IEvent
	{
		public override Type EventType => typeof(T);

		// Per-asset listener list
		private event Action<T> _localListeners;

		[Header("Global Bus (optional)")]
		[Tooltip("If enabled, this channel will also publish to EventBus.Global.")]
		[SerializeField] private bool _alsoPublishGlobal = true;

		public void RegisterListener(Action<T> callback)
		{
			_localListeners += callback;
		}

		public void UnregisterListener(Action<T> callback)
		{
			_localListeners -= callback;
		}

		public void Raise(T evnt)
		{
			_localListeners?.Invoke(evnt);

#if UNITY_EDITOR
			EventDebuggerRuntime.RecordEvent(typeof(T), evnt, this);
#endif

			if (_alsoPublishGlobal)
			{
				EventBus.Global.Publish(evnt);
			}
		}

		internal override void RaiseBoxed(object boxed)
		{
			Raise((T)boxed);
		}
	}
}