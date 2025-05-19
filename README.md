# Unity Event Kit

A **lightweight**, **type-safe**, **zero-GC**, and **Editor-friendly** event system for Unity.  
Unity Event Kit provides:

- A **strongly-typed** `EventBus` for global publish/subscribe
- **Zero allocations** on hot paths 
- **Thread-safe** `PublishQueued` with optional frame delays
- **ScriptableObject Event Channels** for designer-friendly, per-asset dispatch
- **ValueEventListener<T>** MonoBehaviours exposing `UnityEvent<T>` in the Inspector
- An in-Editor **Event Debugger** with virtualized TreeView, filtering, coloring, and clickable entries

---

## 📖 Table of Contents

1. [Features](#features)
2. [Getting Started](#getting-started)
    - [Installation](#installation)
    - [Folder & Assembly Layout](#folder--assembly-layout)
3. [Quick Start Examples](#quick-start-examples)
    - [Core EventBus](#core-eventbus)
    - [Queued & Thread-Safe Dispatch](#queued--thread-safe-dispatch)
    - [ScriptableObject Channels & Primitives](#scriptableobject-channels--primitives)
    - [Defining Custom Struct Events & Channels](#defining-custom-struct-events--channels)
4. [Usage Guide](#usage-guide)
    - [Publishing Events](#publishing-events)
    - [Listening in Code](#listening-in-code)
    - [Listening in the Inspector](#listening-in-the-inspector)
    - [Debugging with the Event Debugger](#debugging-with-the-event-debugger)
5. [Advanced](#advanced)
    - [Disabling Global Bus per Channel](#disabling-global-bus-per-channel)
    - [Delayed & Frame-Deferred Dispatch](#delayed--frame-deferred-dispatch)

---

## Features

- **Type-safe** APIs (`struct T : IEvent`) prevent runtime mismatches
- **Zero-GC** in Publish/Subscribe hot paths via pooled delegates
- **Thread-safe** `PublishQueued` and automatic main-thread flush (via `EventBusDriver`)
- **Optional frame delays** on queued publishes (e.g. `delayFrames: 2`)
- **ScriptableObject Event Channels** for drag-and-drop designer workflows
- **Primitive channels** built-in for `bool`, `int`, `float`, `string`, `Vector2`, `Vector3`, `Quaternion`, `GameObject`, `Transform`
- **ValueEventListener<T>** MonoBehaviours exposing `UnityEvent<T>` fields
- **“Also Publish Global”** toggle on each channel asset
- **Live Event Debugger**: virtualized TreeView, multi-column layout, filter/persistent/orphans modes, clickable rows


---

## Getting Started

### Installation

You have two easy options to get up and running:

**Import the UnityPackage**
   - **Download** the latest `UnityEventKit.unitypackage` from the [Releases](https://github.com/EmrhnFyz/Unity-Event-Kit/releases) page.
   - In Unity, go to **Assets ▶ Import Package ▶ Custom Package…**, select the downloaded package, and click **Import**.

Once imported, you’ll find the **UnityEventKit** folder under **Packages** (or **Assets**, for the `.unitypackage` method), ready to use.
### Folder & Assembly Layout
```
UnityEventKit/
 ├─ Runtime/
 │   ├─ EventBus/           ← core IEventBus & EventBus
 │   ├─ EventChannel/       ← ScriptableObject channels 
 │   ├─ EventListener/      ← MonoBehaviour listeners
 │   ├─ Debug/              ← runtime history recorder
 │   └─ Interfaces/         ← IEvent, IEventBus, IEventChannel
 ├─ Editor/
 │   └─ EventDebuggerWindow.cs
 └─ UnityEventKit.asmdef

```

## Quick Start Examples

### Core EventBus

```csharp
// Define an event:
public readonly struct PlayerDied : IEvent { public readonly int Id; }

// Subscribe & Publish immediately:
var token = EventBus.Global.Subscribe<PlayerDied>(e => Debug.Log($"Dead: {e.Id}"));
EventBus.Global.Publish(new PlayerDied { Id = 42 });
token.Dispose();
```

### Queued & Thread-Safe Dispatch

```csharp
// Thread-safe enqueue, auto-flushed on the main thread each frame:
EventBus.Global.PublishQueued(new PlayerDied(7));

// You can also schedule for N frames later:
EventBus.Global.PublishQueued(new PlayerDied(99), delayFrames: 2);

// Or use the handy extension to fire next frame:
public static class EventExtensions 
{
    public static void PublishNextFrame<T>(in this T e) where T : struct, IEvent =>
        EventBus.Global.PublishQueued(e, 1);
}

// → Usage:
new PlayerDied(5).PublishNextFrame();
```

### ScriptableObject Event Channels
The package already includes primitive channels for common types:
- `BoolEventChannelSO`
- `IntEventChannelSO`
- `FloatEventChannelSO`
- `StringEventChannelSO`
- `Vector2EventChannelSO`
- `Vector3EventChannelSO`
- `QuaternionEventChannelSO`
- `GameObjectEventChannelSO`
- `TransformEventChannelSO`
no need to write your own for simple payloads.


```csharp
// Example: use the built-in Int channel
[SerializeField] private IntEventChannelSO scoreChannel;

void AddScore(int pts) 
{
    scoreChannel.Raise(new ValueEvent<int>(pts));
}
```

#### Defining Custom Struct Events & Channels
When you need a bespoke payload, define your own struct event:
```csharp
// 1) Define your payload as a readonly struct
public readonly struct EnemySpawned : IEvent 
{
    public readonly Vector3 Position;
    public EnemySpawned(Vector3 pos) => Position = pos;
}

// 2) Create a channel asset so designers can hook it up
[CreateAssetMenu(menuName="Unity Event Kit/Channels/Enemy Spawned")]
public sealed class EnemySpawnedChannelSO : EventChannelSO<EnemySpawned> { }

// 3) Raise it in code
_enemySpawnedChannel.Raise(new EnemySpawned(spawnPoint));

// 4) Listen in code or via the Inspector:
//    • Code: EventBus.Global.Subscribe<EnemySpawned>(e => SpawnAt(e.Position));
//    • Inspector: add an EnemySpawnedListener & hook UnityEvent<Vector3>.
```

## Usage Guide
### Publishing Events
- **Immediate (zero-GC)**:
```csharp
channel.Raise(new MyEvent(...));
EventBus.Global.Publish(new MyEvent(...));
```
- **Queued (thread-safe & optional delay):**:
```csharp
channel.Raise(new MyEvent(...), delayFrames: 0);
EventBus.Global.PublishQueued(new MyEvent(...), delayFrames: 3);
```

### Listening in Code
```csharp
// Global bus:
var token = EventBus.Global.Subscribe<MyEvent>(e => Handle(e));

// Per-channel asset:
channel.RegisterListener(e => Handle(e));
```

### Listening in the Inspector
1. Add a **ChannelSO** asset
2. On a GameObject, add the corresponding **XXXEventListener** component (e.g., `FloatEventListener`).
3. Drag the channel asset into the field and hook the `UnityEvent<T>` callback.

### Debugging with the Event Debugger
1. Window ▶ Event Debugger
2. Toolbar: filter by name, toggle Persistent, Orphans Only
3. Columns: Frame | Time | EventType | Payload | Source | Subscribers | Listeners

##  Advanced
### Disabling Global Bus per Channel
Uncheck Also Publish Global Bus on any `EventChannelSO<T>` asset to make it purely local.

### Delayed & Frame-Deferred Dispatch
Use the overload:

```csharp
EventBus.Global.PublishQueued<MyEvent>(evt, delayFrames: n);
```
to dispatch `n` frames later.
