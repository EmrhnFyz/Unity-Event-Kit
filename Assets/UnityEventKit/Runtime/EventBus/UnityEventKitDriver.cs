using UnityEngine;

namespace UnityEventKit
{
	[AddComponentMenu("")]
	internal sealed class UnityEventKitDriver : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Bootstrap()
		{
			var go = new GameObject("[UnityEventKit] UnityEventKitDriver");
			go.hideFlags = HideFlags.HideAndDontSave;
			DontDestroyOnLoad(go);
			go.AddComponent<UnityEventKitDriver>();
		}

		private void Update()
		{
			EventBus.Global.DrainQueued();
		}
	}
}