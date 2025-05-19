using UnityEngine;
using UnityEventKit;

public class TestScriptChannelHolder : MonoBehaviour
{
	[SerializeField] private FloatEventChannelSO floatChannel;
	[SerializeField] private IntEventChannelSO intChannel;
	[SerializeField] private StringEventChannelSO stringChannel;
	[SerializeField] private Vector2EventChannelSO vector2Channel;
	[SerializeField] private Vector3EventChannelSO vector3Channel;
	[SerializeField] private QuaternionEventChannelSO quaternionChannel;
	[SerializeField] private GameObjectEventChannelSO gameObjectChannel;
	[SerializeField] private TransformEventChannelSO transformChannel;
	[SerializeField] private BoolEventChannelSO boolChannel;

	[SerializeField] private float floatValue;
	[SerializeField] private int intValue;
	[SerializeField] private string stringValue;
	[SerializeField] private Vector2 vector2Value;
	[SerializeField] private Vector3 vector3Value;
	[SerializeField] private Quaternion quaternionValue;
	[SerializeField] private GameObject gameObjectValue;
	[SerializeField] private Transform transformValue;
	[SerializeField] private bool boolValue;


	[ContextMenu("Send Bool Event")]
	public void OnBoolEventCalled()
	{
		boolChannel.Raise(new ValueEvent<bool>(boolValue));
	}

	[ContextMenu("Send Float Event")]
	public void OnFloatEventCalled()
	{
		floatChannel.Raise(new ValueEvent<float>(floatValue));
	}

	[ContextMenu("Send Int Event")]
	public void OnIntEventCalled()
	{
		intChannel.Raise(new ValueEvent<int>(intValue));
	}

	[ContextMenu("Send String Event")]
	public void OnStringEventCalled()
	{
		stringChannel.Raise(new ValueEvent<string>(stringValue));
	}

	[ContextMenu("Send Vector2 Event")]
	public void OnVector2EventCalled()
	{
		vector2Channel.Raise(new ValueEvent<Vector2>(vector2Value));
	}

	[ContextMenu("Send Vector3 Event")]
	public void OnVector3EventCalled()
	{
		vector3Channel.Raise(new ValueEvent<Vector3>(vector3Value));
	}

	[ContextMenu("Send Quaternion Event")]
	public void OnQuaternionEventCalled()
	{
		quaternionChannel.Raise(new ValueEvent<Quaternion>(quaternionValue));
	}

	[ContextMenu("Send GameObject Event")]
	public void OnGameObjectEventCalled()
	{
		gameObjectChannel.Raise(new ValueEvent<GameObject>(gameObjectValue));
	}

	[ContextMenu("Send Transform Event")]
	public void OnTransformEventCalled()
	{
		transformChannel.Raise(new ValueEvent<Transform>(transformValue));
	}
}