#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEventKit.Editor
{
	[CustomEditor(typeof(EventChannelBaseSO), true)]
	public class EventChannelEditor : UnityEditor.Editor
	{
		private static readonly MethodInfo _raiseBoxed = typeof(IEventChannel).GetMethod("RaiseBoxed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

		private EventChannelBaseSO Channel => (EventChannelBaseSO)target;
		private Type PayloadType => GetPayloadType();

		// field cache
		private bool _boolVal;
		private int _intVal;
		private float _floatVal;
		private string _stringVal;
		private Vector2 _vec2Val;
		private Vector3 _vec3Val;
		private Quaternion _quatVal;
		private GameObject _goVal;
		private Transform _trVal;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space(8);
			EditorGUILayout.LabelField("Event Type", PrettyName(Channel.EventType), EditorStyles.boldLabel);

			// Runtime subscriber count

			if (Application.isPlaying && EventBus.Global != null)
			{
				var count = EventBusInspectorUtility.GetSubscriberCount(Channel.EventType);
				EditorGUILayout.LabelField("Subscribers (runtime)", count.ToString());
			}

			EditorGUILayout.Space(4);

			if (DrawPayloadField())
			{
				EditorGUILayout.Space(2);
			}

			GUI.enabled = !EditorApplication.isCompiling;
			if (GUILayout.Button("Raise"))
			{
				var payload = CreatePayloadInstance();
				_raiseBoxed.Invoke(Channel, new[]
				                            {
					                            payload
				                            });
			}

			GUI.enabled = true;
		}

		private Type GetPayloadType()
		{
			var evt = Channel.EventType;
			if (evt.IsGenericType && evt.GetGenericTypeDefinition() == typeof(ValueEvent<>))
			{
				return evt.GetGenericArguments()[0];
			}

			return null; // not supported for custom structs
		}

		private static string PrettyName(Type t)
		{
			if (!t.IsGenericType)
			{
				return t.Name;
			}

			var main = t.Name.Substring(0, t.Name.IndexOf('`')); // ValueEvent
			var args = t.GetGenericArguments();
			var argNames = string.Join(", ", Array.ConvertAll(args, PrettyName));
			return $"{main}<{argNames}>";
		}

		private bool DrawPayloadField()
		{
			if (PayloadType == null)
			{
				return false;
			}

			EditorGUI.BeginChangeCheck();
			if (PayloadType == typeof(bool))
			{
				_boolVal = EditorGUILayout.Toggle("Value", _boolVal);
			}
			else if (PayloadType == typeof(int))
			{
				_intVal = EditorGUILayout.IntField("Value", _intVal);
			}
			else if (PayloadType == typeof(float))
			{
				_floatVal = EditorGUILayout.FloatField("Value", _floatVal);
			}
			else if (PayloadType == typeof(string))
			{
				_stringVal = EditorGUILayout.TextField("Value", _stringVal);
			}
			else if (PayloadType == typeof(Vector2))
			{
				_vec2Val = EditorGUILayout.Vector2Field("Value", _vec2Val);
			}
			else if (PayloadType == typeof(Vector3))
			{
				_vec3Val = EditorGUILayout.Vector3Field("Value", _vec3Val);
			}
			else if (PayloadType == typeof(Quaternion))
			{
				_quatVal = QuaternionField("Value", _quatVal);
			}
			else if (PayloadType == typeof(GameObject))
			{
				_goVal = (GameObject)EditorGUILayout.ObjectField("Value", _goVal, typeof(GameObject), true);
			}
			else if (PayloadType == typeof(Transform))
			{
				_trVal = (Transform)EditorGUILayout.ObjectField("Value", _trVal, typeof(Transform), true);
			}
			else
			{
				return false; // unsupported
			}

			return EditorGUI.EndChangeCheck();
		}

		private object CreatePayloadInstance()
		{
			var evtType = Channel.EventType;

			// Custom payloads => default(T)
			if (PayloadType == null)
			{
				return Activator.CreateInstance(evtType);
			}

			// Create ValueEvent<T>(value)
			if (PayloadType == typeof(bool))
			{
				return Activator.CreateInstance(evtType, _boolVal);
			}

			if (PayloadType == typeof(int))
			{
				return Activator.CreateInstance(evtType, _intVal);
			}

			if (PayloadType == typeof(float))
			{
				return Activator.CreateInstance(evtType, _floatVal);
			}

			if (PayloadType == typeof(string))
			{
				return Activator.CreateInstance(evtType, _stringVal);
			}

			if (PayloadType == typeof(Vector2))
			{
				return Activator.CreateInstance(evtType, _vec2Val);
			}

			if (PayloadType == typeof(Vector3))
			{
				return Activator.CreateInstance(evtType, _vec3Val);
			}

			if (PayloadType == typeof(Quaternion))
			{
				return Activator.CreateInstance(evtType, _quatVal);
			}

			if (PayloadType == typeof(GameObject))
			{
				return Activator.CreateInstance(evtType, _goVal);
			}

			if (PayloadType == typeof(Transform))
			{
				return Activator.CreateInstance(evtType, _trVal);
			}

			return Activator.CreateInstance(evtType);
		}

		private static Quaternion QuaternionField(string label, Quaternion value)
		{
			var v = EditorGUILayout.Vector4Field(label, new Vector4(value.x, value.y, value.z, value.w));
			return new Quaternion(v.x, v.y, v.z, v.w);
		}

		private static class EventBusInspectorUtility
		{
			public static int GetSubscriberCount(Type evtType)
			{
				// Friend API via reflection (keeps runtime surface clean)
				var field = typeof(EventBus)
					.GetField("_routes",
					          BindingFlags.Instance | BindingFlags.NonPublic);
				var routes = (IDictionary)field.GetValue(EventBus.Global);
				if (routes == null || !routes.Contains(evtType))
				{
					return 0;
				}

				var list = routes[evtType]; // ISubscriberList
				var prop = list.GetType().GetProperty("Count");
				return (int)prop.GetValue(list);
			}
		}
	}
}

#endif