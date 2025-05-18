using UnityEngine;

namespace UnityEventKit
{
	public readonly struct VoidEvent : IEvent { }

	[CreateAssetMenu(fileName = "VoidEventChannel", menuName = "Unity Event Kit/Channels/Void Event")]
	public sealed class VoidEventChannelSO : EventChannelSO<VoidEvent> { }
}