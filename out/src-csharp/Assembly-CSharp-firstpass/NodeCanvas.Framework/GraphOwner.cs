using System;
using System.Collections.Generic;
using ParadoxNotion;
using UnityEngine;

namespace NodeCanvas.Framework;

public abstract class GraphOwner : MonoBehaviour
{
	public enum EnableAction
	{
		EnableBehaviour,
		DoNothing
	}

	public enum DisableAction
	{
		DisableBehaviour,
		PauseBehaviour,
		DoNothing
	}

	[HideInInspector]
	[SerializeField]
	private string boundGraphSerialization;

	[SerializeField]
	[HideInInspector]
	private List<UnityEngine.Object> boundGraphObjectReferences;

	[HideInInspector]
	public EnableAction enableAction;

	[HideInInspector]
	public DisableAction disableAction;

	public static Action<GraphOwner> onOwnerBehaviourStateChange;

	private Dictionary<Graph, Graph> instances = new Dictionary<Graph, Graph>();

	private bool awakeCalled;

	private bool startCalled;

	private static bool isQuiting;

	public abstract Graph graph { get; set; }

	public abstract IBlackboard blackboard { get; set; }

	public abstract Type graphType { get; }

	public bool isRunning
	{
		get
		{
			if (!(graph != null))
			{
				return false;
			}
			return graph.isRunning;
		}
	}

	public bool isPaused
	{
		get
		{
			if (!(graph != null))
			{
				return false;
			}
			return graph.isPaused;
		}
	}

	public float elapsedTime
	{
		get
		{
			if (!(graph != null))
			{
				return 0f;
			}
			return graph.elapsedTime;
		}
	}

	protected Graph GetInstance(Graph originalGraph)
	{
		if (originalGraph == null)
		{
			return null;
		}
		if (instances.ContainsValue(originalGraph))
		{
			return originalGraph;
		}
		Graph value = null;
		if (!instances.TryGetValue(originalGraph, out value))
		{
			value = Graph.Clone(originalGraph);
			instances[originalGraph] = value;
		}
		value.agent = this;
		value.blackboard = blackboard;
		return value;
	}

	public void StartBehaviour()
	{
		graph = GetInstance(graph);
		if (graph != null)
		{
			graph.StartGraph(this, blackboard, autoUpdate: true);
			if (onOwnerBehaviourStateChange != null)
			{
				onOwnerBehaviourStateChange(this);
			}
		}
	}

	public void StartBehaviour(Action<bool> callback)
	{
		graph = GetInstance(graph);
		if (graph != null)
		{
			graph.StartGraph(this, blackboard, autoUpdate: true, callback);
			if (onOwnerBehaviourStateChange != null)
			{
				onOwnerBehaviourStateChange(this);
			}
		}
	}

	public void PauseBehaviour()
	{
		if (graph != null)
		{
			graph.Pause();
			if (onOwnerBehaviourStateChange != null)
			{
				onOwnerBehaviourStateChange(this);
			}
		}
	}

	public void StopBehaviour()
	{
		if (graph != null)
		{
			graph.Stop();
			if (onOwnerBehaviourStateChange != null)
			{
				onOwnerBehaviourStateChange(this);
			}
		}
	}

	public void UpdateBehaviour()
	{
		if (graph != null)
		{
			graph.UpdateGraph();
		}
	}

	public void SendEvent(string eventName)
	{
		SendEvent(new EventData(eventName));
	}

	public void SendEvent<T>(string eventName, T eventValue)
	{
		SendEvent(new EventData<T>(eventName, eventValue));
	}

	public void SendEvent(EventData eventData)
	{
		if (graph != null)
		{
			graph.SendEvent(eventData);
		}
	}

	public static void SendGlobalEvent(string eventName)
	{
		Graph.SendGlobalEvent(new EventData(eventName));
	}

	public static void SendGlobalEvent<T>(string eventName, T eventValue)
	{
		Graph.SendGlobalEvent(new EventData<T>(eventName, eventValue));
	}

	public void Awake()
	{
		if (awakeCalled)
		{
			return;
		}
		awakeCalled = true;
		if (!string.IsNullOrEmpty(boundGraphSerialization))
		{
			if (graph == null)
			{
				graph = (Graph)ScriptableObject.CreateInstance(graphType);
				graph.Deserialize(boundGraphSerialization, validate: true, boundGraphObjectReferences);
				instances[graph] = graph;
				return;
			}
			graph.SetSerializationObjectReferences(boundGraphObjectReferences);
		}
		graph = GetInstance(graph);
	}

	protected void Start()
	{
		startCalled = true;
		if (enableAction == EnableAction.EnableBehaviour)
		{
			StartBehaviour();
		}
	}

	protected void OnEnable()
	{
		if (startCalled && enableAction == EnableAction.EnableBehaviour)
		{
			StartBehaviour();
		}
	}

	protected void OnDisable()
	{
		if (!isQuiting)
		{
			if (disableAction == DisableAction.DisableBehaviour)
			{
				StopBehaviour();
			}
			if (disableAction == DisableAction.PauseBehaviour)
			{
				PauseBehaviour();
			}
		}
	}

	protected void OnDestroy()
	{
		if (isQuiting)
		{
			return;
		}
		StopBehaviour();
		foreach (Graph value in instances.Values)
		{
			foreach (Graph allInstancedNestedGraph in value.GetAllInstancedNestedGraphs())
			{
				UnityEngine.Object.Destroy(allInstancedNestedGraph);
			}
			UnityEngine.Object.Destroy(value);
		}
	}

	protected void OnApplicationQuit()
	{
		isQuiting = true;
	}
}
public abstract class GraphOwner<T> : GraphOwner where T : Graph
{
	[SerializeField]
	private T _graph;

	[SerializeField]
	private UnityEngine.Object _blackboard;

	public sealed override Graph graph
	{
		get
		{
			return _graph;
		}
		set
		{
			_graph = (T)value;
		}
	}

	public T behaviour
	{
		get
		{
			return _graph;
		}
		set
		{
			_graph = value;
		}
	}

	public sealed override IBlackboard blackboard
	{
		get
		{
			if (graph != null && graph.useLocalBlackboard)
			{
				return graph.localBlackboard;
			}
			if (_blackboard == null)
			{
				_blackboard = GetComponent<Blackboard>();
			}
			return _blackboard as IBlackboard;
		}
		set
		{
			if (_blackboard != value)
			{
				_blackboard = (Blackboard)value;
				if (graph != null && !graph.useLocalBlackboard)
				{
					graph.blackboard = value;
				}
			}
		}
	}

	public sealed override Type graphType => typeof(T);

	public void StartBehaviour(T newGraph)
	{
		SwitchBehaviour(newGraph);
	}

	public void StartBehaviour(T newGraph, Action<bool> callback)
	{
		SwitchBehaviour(newGraph, callback);
	}

	public void SwitchBehaviour(T newGraph)
	{
		SwitchBehaviour(newGraph, null);
	}

	public void SwitchBehaviour(T newGraph, Action<bool> callback)
	{
		StopBehaviour();
		graph = newGraph;
		StartBehaviour(callback);
	}
}
