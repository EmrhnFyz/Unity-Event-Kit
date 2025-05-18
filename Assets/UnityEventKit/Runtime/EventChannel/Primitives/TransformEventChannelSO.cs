using UnityEngine;
namespace UnityEventKit
{
	[CreateAssetMenu(fileName = "TransformEventChannel", menuName = "Unity Event Kit/Channels/Transform")]
	public sealed class TransformEventChannelSO : ValueEventChannelSO<Transform> { }
}