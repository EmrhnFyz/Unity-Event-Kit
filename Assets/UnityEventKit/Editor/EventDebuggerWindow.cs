#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEventKit.Editor
{
	public class EventDebuggerWindow : EditorWindow
	{
		private enum Columns
		{
			Frame,
			Time,
			EventType,
			Payload,
			Source,
			Listeners
		}

		private static MultiColumnHeaderState CreateHeaderState()
		{
			var cols = new[]
			           {
				           new MultiColumnHeaderState.Column
				           {
					           headerContent = new GUIContent("Frame"),
					           width = 60, minWidth = 40, autoResize = false
				           },
				           new MultiColumnHeaderState.Column
				           {
					           headerContent = new GUIContent("Time"),
					           width = 100, minWidth = 80
				           },
				           new MultiColumnHeaderState.Column
				           {
					           headerContent = new GUIContent("EventType"),
					           width = 150, minWidth = 100
				           },
				           new MultiColumnHeaderState.Column
				           {
					           headerContent = new GUIContent("Payload"),
					           width = 100, minWidth = 80
				           },
				           new MultiColumnHeaderState.Column
				           {
					           headerContent = new GUIContent("Source"),
					           width = 100, minWidth = 80
				           },
				           new MultiColumnHeaderState.Column
				           {
					           headerContent = new GUIContent("Listeners"),
					           width = 200, minWidth = 100
				           }
			           };
			var state = new MultiColumnHeaderState(cols);
			return state;
		}

		private class EventHistoryTreeView : TreeView
		{
			private List<EventDebuggerRuntime.RecordedEvent> _data;

			public EventHistoryTreeView(TreeViewState state, MultiColumnHeader header) : base(state, header)
			{
				showAlternatingRowBackgrounds = true;
				showBorder = true;
				header.sortingChanged += OnSortingChanged;
			}

			public void SetData(List<EventDebuggerRuntime.RecordedEvent> data)
			{
				_data = data;
				Reload();
			}

			protected override TreeViewItem BuildRoot()
			{
				// one top‐level item, depth = -1
				var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
				var allItems = new List<TreeViewItem>(_data.Count);
				for (var i = 0; i < _data.Count; i++)
				{
					var evt = _data[i];
					// pack both index and type to find on click
					var item = new TreeViewItem(
						i + 1,
						0,
						GetFriendlyTypeName(evt.EventType)
					);
					allItems.Add(item);
				}

				SetupParentsAndChildrenFromDepths(root, allItems);
				return root;
			}

			protected override void RowGUI(RowGUIArgs args)
			{
				var evt = _data[args.item.id - 1];
				var cols = args.GetNumVisibleColumns();

				for (var ci = 0; ci < cols; ci++)
				{
					var col = (Columns)args.GetColumn(ci);
					var cell = args.GetCellRect(ci);
					switch (col)
					{
						case Columns.Frame:
							EditorGUI.LabelField(cell, evt.Frame.ToString());
							break;
						case Columns.Time:
							EditorGUI.LabelField(cell, evt.TimeStamp.ToString("HH:mm:ss.fff"));
							break;
						case Columns.EventType:
							EditorGUI.LabelField(cell, GetFriendlyTypeName(evt.EventType));
							break;
						case Columns.Payload:
							EditorGUI.LabelField(cell, evt.Payload?.ToString() ?? "");
							break;
						case Columns.Source:
							var srcName = evt.Source != null
								              ? evt.Source.name
								              : "Global";
							EditorGUI.LabelField(cell, srcName);
							break;
						case Columns.Listeners:
							var listText = "";
							if (evt.Source is EventChannelBaseSO channel)
							{
								var field = FindPrivateField(channel.GetType(), "_localListeners");
								var dlg = field?.GetValue(channel) as Delegate;
								if (dlg != null)
								{
									var parts = new List<string>();
									foreach (var sub in dlg.GetInvocationList())
									{
										// if the target is a Component (MonoBehaviour), get its GameObject name
										string goName;
										if (sub.Target is Component comp)
										{
											goName = comp.gameObject.name;
										}
										else if (sub.Target is Object uo)
										{
											goName = uo.name;
										}
										else
										{
											goName = sub.Target?.GetType().Name ?? "static";
										}

										parts.Add($"{goName}.{sub.Method.Name}");
									}

									listText = string.Join(", ", parts);
								}
							}
							else
							{
								// fallback: global subscribers
								var c = GetGlobalCount(evt.EventType);
								listText = $"{c} global";
							}

							EditorGUI.LabelField(cell, listText);
							break;
					}
				}
			}

			// allow clicking a row to ping the asset or subscriber
			protected override void SingleClickedItem(int id)
			{
				var evt = _data[id - 1];
				// try pinging the channel asset first
				// look up the channel asset by EventType name
				var guids = AssetDatabase.FindAssets($"t:EventChannelSO`1 {GetFriendlyTypeName(evt.EventType)}");
				if (guids.Length > 0)
				{
					var path = AssetDatabase.GUIDToAssetPath(guids[0]);
					var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
					if (asset != null)
					{
						EditorGUIUtility.PingObject(asset);
					}
				}
			}

			private void OnSortingChanged(MultiColumnHeader header)
			{
				// optional: implement sorting logic here
			}
		}


		private Vector2 _scrollPos;
		private string _filter = "";
		private int _historyFrames = 100;
		private bool _persistentMode;
		private bool _orphansOnly;

		private TreeViewState _tvState;
		private MultiColumnHeader _header;
		private EventHistoryTreeView _tree;

		private readonly Dictionary<Type, Color> _typeColors = new();

		private static readonly Dictionary<Type, string> _typeAliases = new()
		                                                                {
			                                                                { typeof(bool), "bool" },
			                                                                { typeof(byte), "byte" },
			                                                                { typeof(sbyte), "sbyte" },
			                                                                { typeof(char), "char" },
			                                                                { typeof(decimal), "decimal" },
			                                                                { typeof(double), "double" },
			                                                                { typeof(float), "float" },
			                                                                { typeof(int), "int" },
			                                                                { typeof(uint), "uint" },
			                                                                { typeof(long), "long" },
			                                                                { typeof(ulong), "ulong" },
			                                                                { typeof(object), "object" },
			                                                                { typeof(short), "short" },
			                                                                { typeof(ushort), "ushort" },
			                                                                { typeof(string), "string" }
		                                                                };

		[MenuItem("Window/Event Debugger")]
		public static void ShowWindow()
		{
			GetWindow<EventDebuggerWindow>("Event Debugger");
		}

		private void OnEnable()
		{
			EditorApplication.update += Repaint;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private void OnDisable()
		{
			EditorApplication.update -= Repaint;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		private void EnsureTree()
		{
			if (_tvState == null)
			{
				_tvState = new TreeViewState();
			}

			if (_header == null)
			{
				var hs = CreateHeaderState();
				_header = new MultiColumnHeader(hs);
				_header.ResizeToFit();
			}

			if (_tree == null)
			{
				_tree = new EventHistoryTreeView(_tvState, _header);
			}
		}

		private static string GetFriendlyTypeName(Type type)
		{
			// Primitive alias?
			if (_typeAliases.TryGetValue(type, out var alias))
			{
				return alias;
			}

			if (!type.IsGenericType)
			{
				return type.Name;
			}

			// Generic: strip the `1 from the name
			var name = type.Name;
			var idx = name.IndexOf('`');
			if (idx > 0)
			{
				name = name.Substring(0, idx);
			}

			// Format each generic argument
			var args = type.GetGenericArguments();
			var argNames = new string[args.Length];
			for (var i = 0; i < args.Length; i++)
			{
				argNames[i] = GetFriendlyTypeName(args[i]);
			}

			return $"{name}<{string.Join(", ", argNames)}>";
		}

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingPlayMode)
			{
				EventDebuggerRuntime.ClearHistory();
			}
		}

		private void OnGUI()
		{
			DrawToolbar();

			if (!Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Enter Play Mode to see events.", MessageType.Info);
				return;
			}

			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

			DrawEventHistory();
			EditorGUILayout.Space();
			DrawGlobalSubscriberCounts();
			EditorGUILayout.Space();
			DrawChannelSubscriberCounts();

			EditorGUILayout.EndScrollView();
		}

		private void DrawToolbar()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);

			_filter = GUILayout.TextField(_filter, EditorStyles.toolbarTextField, GUILayout.MinWidth(100));
			GUILayout.Label("Filter", GUILayout.Width(40));

			_persistentMode = GUILayout.Toggle(_persistentMode, "Persistent", EditorStyles.toolbarButton, GUILayout.Width(80));

			_orphansOnly = GUILayout.Toggle(_orphansOnly, "Orphans Only", EditorStyles.toolbarButton, GUILayout.Width(100));

			GUILayout.FlexibleSpace();
			EditorGUI.BeginDisabledGroup(_persistentMode);
			_historyFrames = EditorGUILayout.IntField(new GUIContent("Frames"), _historyFrames, GUILayout.Width(80));
			EditorGUI.EndDisabledGroup();

			GUILayout.EndHorizontal();
		}

		private void DrawEventHistory()
		{
			EnsureTree();

			// prepare filtered history list
			var raw = EventDebuggerRuntime.GetHistory();
			var currentFrame = Time.frameCount;
			var list = new List<EventDebuggerRuntime.RecordedEvent>();
			foreach (var evt in raw)
			{
				if (!_persistentMode && evt.Frame < currentFrame - _historyFrames)
				{
					continue;
				}

				if (!string.IsNullOrEmpty(_filter) &&
				    GetFriendlyTypeName(evt.EventType).IndexOf(_filter, StringComparison.OrdinalIgnoreCase) < 0)
				{
					continue;
				}

				if (_orphansOnly && GetGlobalCount(evt.EventType) > 0)
				{
					continue;
				}

				list.Add(evt);
			}

			_tree.SetData(list);

			// render the tree view
			var rect = GUILayoutUtility.GetRect(0, position.width,
			                                    200, position.height * 0.6f);
			_tree.OnGUI(rect);
		}

		private void DrawGlobalSubscriberCounts()
		{
			EditorGUILayout.LabelField("Global Bus Subscribers", EditorStyles.boldLabel);
			var routesField = typeof(EventBus)
				.GetField("_routes", BindingFlags.Instance | BindingFlags.NonPublic);
			var routes = (IDictionary)routesField.GetValue(EventBus.Global);

			foreach (Type evtType in routes.Keys)
			{
				var count = GetGlobalCount(evtType);
				var typeName = GetFriendlyTypeName(evtType);
				if (!string.IsNullOrEmpty(_filter) &&
				    typeName.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) < 0)
				{
					continue;
				}

				if (_orphansOnly && count > 0)
				{
					continue;
				}

				// color-code by type
				var old = GUI.contentColor;
				GUI.contentColor = GetTypeColor(evtType);
				EditorGUILayout.LabelField($"• {typeName}: {count}");
				GUI.contentColor = old;
			}
		}

		private static int GetGlobalCount(Type evtType)
		{
			var routesField = typeof(EventBus)
				.GetField("_routes", BindingFlags.Instance | BindingFlags.NonPublic);
			var routes = (IDictionary)routesField.GetValue(EventBus.Global);
			if (!routes.Contains(evtType))
			{
				return 0;
			}

			var list = routes[evtType];
			var prop = list.GetType().GetProperty("Count");
			return (int)prop.GetValue(list);
		}

		private Color GetTypeColor(Type t)
		{
			if (!_typeColors.TryGetValue(t, out var c))
			{
				// generate a pastel color from the hashcode
				var h = (t.FullName.GetHashCode() & 0xffff) / (float)0xffff;
				c = Color.HSVToRGB(h, 0.3f, 0.9f);
				_typeColors[t] = c;
			}

			return c;
		}

		private static FieldInfo FindPrivateField(Type t, string name)
		{
			while (t != null)
			{
				var fi = t.GetField(name,
				                    BindingFlags.Instance | BindingFlags.NonPublic);
				if (fi != null)
				{
					return fi;
				}

				t = t.BaseType;
			}

			return null;
		}

		private void DrawChannelSubscriberCounts()
		{
			EditorGUILayout.LabelField("Channel Asset Subscribers", EditorStyles.boldLabel);

			// Locate all channel assets
			var guids = AssetDatabase.FindAssets("t:EventChannelBaseSO");
			foreach (var guid in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var channel = AssetDatabase.LoadAssetAtPath<EventChannelBaseSO>(path);
				if (channel == null)
				{
					continue;
				}

				// 1) find the '_localListeners' event on the base generic class
				var field = FindPrivateField(channel.GetType(), "_localListeners");
				Delegate dlg = null;
				var count = 0;
				if (field != null)
				{
					dlg = field.GetValue(channel) as Delegate;
					count = dlg?.GetInvocationList().Length ?? 0;
				}

				// 2) print the channel name + subscriber count
				EditorGUILayout.LabelField($"• {channel.name}: {count}");

				// 3) list each subscriber underneath, if any
				if (count > 0 && dlg != null)
				{
					EditorGUI.indentLevel++;

					foreach (var sub in dlg.GetInvocationList())
					{
						string targetName;
						if (sub.Target is Object uo)
						{
							targetName = uo.name;
						}
						else if (sub.Target != null)
						{
							targetName = sub.Target.GetType().Name;
						}
						else
						{
							targetName = "static";
						}

						EditorGUILayout.LabelField($"- {targetName}.{sub.Method.Name}");
					}

					EditorGUI.indentLevel--;
				}
			}
		}
	}
}
#endif