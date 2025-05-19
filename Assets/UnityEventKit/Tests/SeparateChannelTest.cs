using UnityEngine;
using UnityEventKit;

public class SeparateChannelTest : MonoBehaviour
{
	[SerializeField] private FloatEventChannelSO floatChannel;
	[SerializeField] private float floatValue;

	[ContextMenu("Send Float Event")]
	public void OnFloatEventCalled()
	{
		floatChannel.Raise(new ValueEvent<float>(floatValue));
	}
}