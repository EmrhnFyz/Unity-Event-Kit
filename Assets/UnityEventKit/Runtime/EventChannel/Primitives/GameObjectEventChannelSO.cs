using UnityEngine;

namespace UnityEventKit
{
	[CreateAssetMenu(fileName = "GameObjectEventChannel", menuName = "Unity Event Kit/Channels/GameObject")]
	public sealed class GameObjectEventChannelSO : ValueEventChannelSO<GameObject> { }
}