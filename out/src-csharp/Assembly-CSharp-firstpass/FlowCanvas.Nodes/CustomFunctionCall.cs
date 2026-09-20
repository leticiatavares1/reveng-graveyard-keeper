using System.Collections.Generic;
using LinqTools;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Function Call", 0)]
[DeserializeFrom(new string[] { "FlowCanvas.Nodes.RelayFlowInput" })]
[Color("d86b13")]
[Category("Functions/Custom")]
[Description("Calls an existing Custom Function")]
[DoNotList]
public class CustomFunctionCall : FlowControlNode
{
	[SerializeField]
	private string _sourceOutputUID;

	private ValueInput[] portArgs;

	private object[] objectArgs;

	private FlowOutput fOut;

	private object _sourceFunction;

	private string sourceFunctionUID
	{
		get
		{
			return _sourceOutputUID;
		}
		set
		{
			_sourceOutputUID = value;
		}
	}

	public CustomFunctionEvent sourceFunction
	{
		get
		{
			if (_sourceFunction == null)
			{
				_sourceFunction = base.graph.GetAllNodesOfType<CustomFunctionEvent>().FirstOrDefault((CustomFunctionEvent i) => i.UID == sourceFunctionUID);
				if (_sourceFunction == null)
				{
					_sourceFunction = new object();
				}
			}
			return _sourceFunction as CustomFunctionEvent;
		}
		set
		{
			_sourceFunction = value;
		}
	}

	public override string name => string.Format("Call {0} ()", (sourceFunction != null) ? sourceFunction.identifier : "NONE");

	public override string description
	{
		get
		{
			if (sourceFunction == null || string.IsNullOrEmpty(sourceFunction.nodeComment))
			{
				return base.description;
			}
			return sourceFunction.nodeComment;
		}
	}

	public void SetFunction(CustomFunctionEvent func)
	{
		sourceFunctionUID = func?.UID;
		sourceFunction = ((func != null) ? func : null);
		GatherPorts();
	}

	protected override void RegisterPorts()
	{
		AddFlowInput(" ", Invoke);
		if (sourceFunction != null)
		{
			List<DynamicPortDefinition> parameters = sourceFunction.parameters;
			portArgs = new ValueInput[parameters.Count];
			for (int i = 0; i < parameters.Count; i++)
			{
				int num = i;
				DynamicPortDefinition dynamicPortDefinition = parameters[num];
				portArgs[num] = AddValueInput(dynamicPortDefinition.name, dynamicPortDefinition.type, dynamicPortDefinition.ID);
			}
			if (sourceFunction.returns.type != null)
			{
				AddValueOutput(sourceFunction.returns.name, sourceFunction.returns.ID, sourceFunction.returns.type, sourceFunction.GetReturnValue);
			}
			fOut = AddFlowOutput(" ");
		}
	}

	private void Invoke(Flow f)
	{
		if (sourceFunction != null)
		{
			if (objectArgs == null)
			{
				objectArgs = new object[portArgs.Length];
			}
			for (int i = 0; i < portArgs.Length; i++)
			{
				objectArgs[i] = portArgs[i].value;
			}
			sourceFunction.InvokeAsync(f, fOut.Call, objectArgs);
		}
	}
}
