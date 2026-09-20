using System;
using System.Collections.Generic;
using FlowCanvas.Macros;
using FlowCanvas.Nodes;
using NodeCanvas.Framework;
using ParadoxNotion;
using UnityEngine;

namespace FlowCanvas;

[Serializable]
[GraphInfo(packageName = "FlowCanvas", docsURL = "http://flowcanvas.paradoxnotion.com/documentation/", resourcesURL = "http://flowcanvas.paradoxnotion.com/downloads/", forumsURL = "http://flowcanvas.paradoxnotion.com/forums-page/")]
public abstract class FlowGraph : Graph
{
	private bool hasInitialized;

	private List<IUpdatable> updatableNodes;

	private Dictionary<string, CustomFunctionEvent> functions;

	private Dictionary<Type, Component> cachedAgentComponents = new Dictionary<Type, Component>();

	public override Type baseNodeType => typeof(FlowNode);

	public override bool useLocalBlackboard => false;

	public sealed override bool requiresAgent => false;

	public sealed override bool requiresPrimeNode => false;

	public sealed override bool autoSort => false;

	public T CallFunction<T>(string name, params object[] args)
	{
		return (T)CallFunction(name, args);
	}

	public object CallFunction(string name, params object[] args)
	{
		CustomFunctionEvent value = null;
		if (functions.TryGetValue(name, out value))
		{
			return value.Invoke(default(Flow), args);
		}
		return null;
	}

	public UnityEngine.Object GetAgentComponent(Type type)
	{
		if (base.agent == null)
		{
			return null;
		}
		if (type == typeof(GameObject))
		{
			return base.agent.gameObject;
		}
		if (type == typeof(Transform))
		{
			return base.agent.transform;
		}
		if (type == typeof(Component))
		{
			return base.agent;
		}
		Component value = null;
		if (cachedAgentComponents.TryGetValue(type, out value))
		{
			return value;
		}
		if (typeof(Component).RTIsAssignableFrom(type))
		{
			value = base.agent.GetComponent(type);
		}
		return cachedAgentComponents[type] = value;
	}

	protected override void OnGraphStarted()
	{
		if (!hasInitialized)
		{
			updatableNodes = new List<IUpdatable>();
			functions = new Dictionary<string, CustomFunctionEvent>(StringComparer.Ordinal);
		}
		for (int i = 0; i < base.allNodes.Count; i++)
		{
			Node node = base.allNodes[i];
			if (node is MacroNodeWrapper)
			{
				MacroNodeWrapper macroNodeWrapper = (MacroNodeWrapper)node;
				if (macroNodeWrapper.macro != null)
				{
					macroNodeWrapper.CheckInstance();
					macroNodeWrapper.macro.StartGraph(base.agent, base.blackboard, autoUpdate: false);
				}
			}
			if (!hasInitialized)
			{
				if (node is IUpdatable)
				{
					updatableNodes.Add((IUpdatable)node);
				}
				if (node is CustomFunctionEvent)
				{
					CustomFunctionEvent customFunctionEvent = (CustomFunctionEvent)node;
					functions[customFunctionEvent.identifier] = customFunctionEvent;
				}
			}
		}
		if (!hasInitialized)
		{
			for (int j = 0; j < base.allNodes.Count; j++)
			{
				if (base.allNodes[j] is FlowNode)
				{
					FlowNode obj = (FlowNode)base.allNodes[j];
					obj.AssignSelfInstancePort();
					obj.BindPorts();
				}
			}
		}
		hasInitialized = true;
	}

	protected override void OnGraphUpdate()
	{
		if (updatableNodes != null && updatableNodes.Count > 0)
		{
			for (int i = 0; i < updatableNodes.Count; i++)
			{
				updatableNodes[i].Update();
			}
		}
	}

	protected override void OnGraphStoped()
	{
		for (int i = 0; i < base.allNodes.Count; i++)
		{
			Node node = base.allNodes[i];
			if (node is MacroNodeWrapper)
			{
				MacroNodeWrapper macroNodeWrapper = (MacroNodeWrapper)node;
				if (macroNodeWrapper.macro != null)
				{
					macroNodeWrapper.macro.Stop();
				}
			}
		}
	}

	private static Type[] FindCustomObjectWrappers(Type targetType)
	{
		List<Type> list = new List<Type>();
		Type[] allTypes = ReflectionTools.GetAllTypes();
		foreach (Type type in allTypes)
		{
			if (type.IsSubclassOf(typeof(CustomObjectWrapper)))
			{
				Type[] genericArguments = type.BaseType.GetGenericArguments();
				if (genericArguments.Length == 1 && genericArguments[0] == targetType)
				{
					list.Add(type);
				}
			}
		}
		return list.ToArray();
	}
}
