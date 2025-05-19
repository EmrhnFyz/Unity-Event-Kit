using System;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEventKit
{
	public sealed class VoidEventListener : MonoBehaviour
	{
		[SerializeField] private VoidEventChannelSO channel;
		[SerializeField] private UnityEvent response = new();

		private Action<VoidEvent> _handler;

		private void OnEnable()
		{
			if (channel == null)
			{
				return;
			}

			_handler = _ => response.Invoke();
			channel.RegisterListener(_handler);
		}

		private void OnDisable()
		{
			if (channel != null && _handler != null)
			{
				channel.UnregisterListener(_handler);
			}
		}
	}
}