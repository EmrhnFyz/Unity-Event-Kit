using System;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEventKit
{
	public abstract class ValueEventListenerBase<T> : MonoBehaviour
	{
		[SerializeField] private ValueEventChannelSO<T> channel;

		[SerializeField] private UnityEvent<T> response = new();

		private Action<ValueEvent<T>> _handler;

		private void OnEnable()
		{
			if (!channel)
			{
				return;
			}

			_handler = HandleEvent;
			channel.RegisterListener(_handler);
		}

		private void OnDisable()
		{
			if (channel != null && _handler != null)
			{
				channel.UnregisterListener(_handler);
			}
		}

		private void HandleEvent(ValueEvent<T> evt)
		{
			response.Invoke(evt.Value);
		}
	}
}