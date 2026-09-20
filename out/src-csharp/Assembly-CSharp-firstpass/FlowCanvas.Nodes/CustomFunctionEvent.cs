using System;
using System.Collections.Generic;
using LinqTools;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace FlowCanvas.Nodes;

[DeserializeFrom(new string[] { "FlowCanvas.Nodes.RelayFlowOutput" })]
[Category("Functions/Custom")]
[Description("A custom function, defined by any number of parameters and an optional return value. It can be called using the 'Call Custom Function' node. To return a value, the 'Return' node should be used.")]
[Name("New Custom Function", 10)]
public class CustomFunctionEvent : EventNode, IEditorMenuCallbackReceiver
{
	[Tooltip("The identifier name of the function")]
	public string identifier = "MyFunction";

	[SerializeField]
	private List<DynamicPortDefinition> _parameters = new List<DynamicPortDefinition>();

	[SerializeField]
	private DynamicPortDefinition _returns = new DynamicPortDefinition("Value", null);

	private object[] args;

	private object returnValue;

	private FlowOutput onInvoke;

	public List<DynamicPortDefinition> parameters
	{
		get
		{
			return _parameters;
		}
		private set
		{
			_parameters = value;
		}
	}

	public DynamicPortDefinition returns
	{
		get
		{
			return _returns;
		}
		private set
		{
			_returns = value;
		}
	}

	private Type returnType => returns.type;

	private Type[] parameterTypes => parameters.Select((DynamicPortDefinition p) => p.type).ToArray();

	public override string name => "➥ " + identifier;

	protected override void RegisterPorts()
	{
		onInvoke = AddFlowOutput(" ");
		for (int j = 0; j < parameters.Count; j++)
		{
			int i = j;
			DynamicPortDefinition dynamicPortDefinition = parameters[i];
			AddValueOutput(dynamicPortDefinition.name, dynamicPortDefinition.ID, dynamicPortDefinition.type, () => args[i]);
		}
	}

	public object Invoke(Flow f, params object[] args)
	{
		this.args = args;
		f.ReturnType = returns.type;
		f.Return = delegate(object o)
		{
			returnValue = o;
		};
		onInvoke.Call(f);
		return returnValue;
	}

	public void InvokeAsync(Flow f, FlowHandler Callback, params object[] args)
	{
		this.args = args;
		f.ReturnType = returns.type;
		f.Return = delegate(object o)
		{
			returnValue = o;
			Callback(f);
		};
		onInvoke.Call(f);
	}

	public object GetReturnValue()
	{
		return returnValue;
	}

	private void AddParameter(Type type)
	{
		parameters.Add(new DynamicPortDefinition(type.FriendlyName(), type));
		GatherPortsUpdateRefs();
	}

	private void GatherPortsUpdateRefs()
	{
		GatherPorts();
		foreach (CustomFunctionCall item in from n in base.flowGraph.GetAllNodesOfType<CustomFunctionCall>()
			where n.sourceFunction == this
			select n)
		{
			item.GatherPorts();
		}
	}
}
