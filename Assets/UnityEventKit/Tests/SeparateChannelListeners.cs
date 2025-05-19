using UnityEngine;

public class SeparateChannelListeners : MonoBehaviour
{
	public void OnEventReceived(float val)
	{
		Debug.Log($" {gameObject.name} got: {val}");
	}
}