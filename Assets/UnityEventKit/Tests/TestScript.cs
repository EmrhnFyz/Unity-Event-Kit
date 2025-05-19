using System;
using UnityEngine;
using UnityEventKit;

public class TestScript : MonoBehaviour
{
	[Header("Channel Assets")]
	[SerializeField] private BoolEventChannelSO boolChannel;

	[SerializeField] private FloatEventChannelSO floatChannel;
	[SerializeField] private IntEventChannelSO intChannel;
	[SerializeField] private StringEventChannelSO stringChannel;
	[SerializeField] private Vector2EventChannelSO vec2Channel;
	[SerializeField] private Vector3EventChannelSO vec3Channel;
	[SerializeField] private QuaternionEventChannelSO quatChannel;
	[SerializeField] private GameObjectEventChannelSO goChannel;
	[SerializeField] private TransformEventChannelSO transformChannel;

	// Keep these so we can unregister exactly the same handler
	private Action<ValueEvent<bool>> _boolHandler;
	private Action<ValueEvent<float>> _floatHandler;
	private Action<ValueEvent<int>> _intHandler;
	private Action<ValueEvent<string>> _stringHandler;
	private Action<ValueEvent<Vector2>> _vec2Handler;
	private Action<ValueEvent<Vector3>> _vec3Handler;
	private Action<ValueEvent<Quaternion>> _quatHandler;
	private Action<ValueEvent<GameObject>> _goHandler;
	private Action<ValueEvent<Transform>> _trHandler;

	private void OnEnable()
	{
		if (boolChannel != null)
		{
			_boolHandler = BoolEventHandler;
			boolChannel.RegisterListener(_boolHandler);
		}

		if (floatChannel != null)
		{
			_floatHandler = FloatEventHandler;
			floatChannel.RegisterListener(_floatHandler);
		}

		if (intChannel != null)
		{
			_intHandler = IntEventHandler;
			intChannel.RegisterListener(_intHandler);
		}

		if (stringChannel != null)
		{
			_stringHandler = StringEventHandler;
			stringChannel.RegisterListener(_stringHandler);
		}

		if (vec2Channel != null)
		{
			_vec2Handler = Vec2EventHandler;
			vec2Channel.RegisterListener(_vec2Handler);
		}

		if (vec3Channel != null)
		{
			_vec3Handler = Vec3EventHandler;
			vec3Channel.RegisterListener(_vec3Handler);
		}

		if (quatChannel != null)
		{
			_quatHandler = QuatEventHandler;
			quatChannel.RegisterListener(_quatHandler);
		}

		if (goChannel != null)
		{
			_goHandler = GoEventHandler;
			goChannel.RegisterListener(_goHandler);
		}

		if (transformChannel != null)
		{
			_trHandler = TREventHandler;
			transformChannel.RegisterListener(_trHandler);
		}
	}

	private void TREventHandler(ValueEvent<Transform> e)
	{
		OnTransformEvent(e.Value);
	}

	private void GoEventHandler(ValueEvent<GameObject> e)
	{
		OnGameObjectEvent(e.Value);
	}

	private void QuatEventHandler(ValueEvent<Quaternion> e)
	{
		OnQuaternionEvent(e.Value);
	}

	private void Vec3EventHandler(ValueEvent<Vector3> e)
	{
		OnVector3Event(e.Value);
	}

	private void Vec2EventHandler(ValueEvent<Vector2> e)
	{
		OnVector2Event(e.Value);
	}

	private void StringEventHandler(ValueEvent<string> e)
	{
		OnStringEvent(e.Value);
	}

	private void IntEventHandler(ValueEvent<int> e)
	{
		OnIntEvent(e.Value);
	}

	private void FloatEventHandler(ValueEvent<float> e)
	{
		OnFloatEvent(e.Value);
	}

	private void BoolEventHandler(ValueEvent<bool> e)
	{
		OnBoolEventCalled(e.Value);
	}

	private void OnDisable()
	{
		if (boolChannel != null)
		{
			boolChannel.UnregisterListener(_boolHandler);
		}

		if (floatChannel != null)
		{
			floatChannel.UnregisterListener(_floatHandler);
		}

		if (intChannel != null)
		{
			intChannel.UnregisterListener(_intHandler);
		}

		if (stringChannel != null)
		{
			stringChannel.UnregisterListener(_stringHandler);
		}

		if (vec2Channel != null)
		{
			vec2Channel.UnregisterListener(_vec2Handler);
		}

		if (vec3Channel != null)
		{
			vec3Channel.UnregisterListener(_vec3Handler);
		}

		if (quatChannel != null)
		{
			quatChannel.UnregisterListener(_quatHandler);
		}

		if (goChannel != null)
		{
			goChannel.UnregisterListener(_goHandler);
		}

		if (transformChannel != null)
		{
			transformChannel.UnregisterListener(_trHandler);
		}
	}

	public void OnBoolEventCalled(bool val)
	{
		Debug.Log($" got: {val}");
	}

	public void OnFloatEvent(float val)
	{
		Debug.Log($"  got: {val}");
	}

	public void OnIntEvent(int val)
	{
		Debug.Log($"  got: {val}");
	}

	public void OnStringEvent(string val)
	{
		Debug.Log($"  got: {val}");
	}

	public void OnVector2Event(Vector2 val)
	{
		Debug.Log($"  got: {val}");
	}

	public void OnVector3Event(Vector3 val)
	{
		Debug.Log($"  got: {val}");
	}

	public void OnQuaternionEvent(Quaternion val)
	{
		Debug.Log($"  got: {val}");
	}

	public void OnGameObjectEvent(GameObject val)
	{
		Debug.Log($"  got: {val.name}");
	}

	public void OnTransformEvent(Transform val)
	{
		Debug.Log($"  got: {val.position}");
	}
}