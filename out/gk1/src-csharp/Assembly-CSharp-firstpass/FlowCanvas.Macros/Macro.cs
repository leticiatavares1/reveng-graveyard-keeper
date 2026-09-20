using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

namespace FlowCanvas.Macros;

public class Macro : FlowGraph
{
	[Serializable]
	private struct DerivedSerializationData
	{
		public List<DynamicPortDefinition> inputDefinitions;

		public List<DynamicPortDefinition> outputDefinitions;
	}

	[SerializeField]
	public List<DynamicPortDefinition> inputDefinitions = new List<DynamicPortDefinition>();

	[SerializeField]
	public List<DynamicPortDefinition> outputDefinitions = new List<DynamicPortDefinition>();

	[NonSerialized]
	public Dictionary<string, FlowHandler> entryActionMap = new Dictionary<string, FlowHandler>(StringComparer.Ordinal);

	[NonSerialized]
	public Dictionary<string, FlowHandler> exitActionMap = new Dictionary<string, FlowHandler>(StringComparer.Ordinal);

	[NonSerialized]
	public Dictionary<string, ValueHandlerObject> entryFunctionMap = new Dictionary<string, ValueHandlerObject>(StringComparer.Ordinal);

	[NonSerialized]
	public Dictionary<string, ValueHandlerObject> exitFunctionMap = new Dictionary<string, ValueHandlerObject>(StringComparer.Ordinal);

	private MacroInputNode _entry;

	private MacroOutputNode _exit;

	public override bool useLocalBlackboard => true;

	public MacroInputNode entry
	{
		get
		{
			if (_entry == null)
			{
				_entry = base.allNodes.OfType<MacroInputNode>().FirstOrDefault();
				if (_entry == null)
				{
					_entry = AddNode<MacroInputNode>(new Vector2(0f - base.translation.x + 200f, 0f - base.translation.y + 200f));
				}
			}
			return _entry;
		}
	}

	public MacroOutputNode exit
	{
		get
		{
			if (_exit == null)
			{
				_exit = base.allNodes.OfType<MacroOutputNode>().FirstOrDefault();
				if (_exit == null)
				{
					_exit = AddNode<MacroOutputNode>(new Vector2(0f - base.translation.x + 600f, 0f - base.translation.y + 200f));
				}
			}
			return _exit;
		}
	}

	public override object OnDerivedDataSerialization()
	{
		DerivedSerializationData derivedSerializationData = default(DerivedSerializationData);
		derivedSerializationData.inputDefinitions = inputDefinitions;
		derivedSerializationData.outputDefinitions = outputDefinitions;
		return derivedSerializationData;
	}

	public override void OnDerivedDataDeserialization(object data)
	{
		if (data is DerivedSerializationData)
		{
			inputDefinitions = ((DerivedSerializationData)data).inputDefinitions;
			outputDefinitions = ((DerivedSerializationData)data).outputDefinitions;
		}
	}

	protected override void OnGraphValidate()
	{
		base.OnGraphValidate();
		_entry = null;
		_exit = null;
		_entry = entry;
		_exit = exit;
		if (inputDefinitions.Count == 0 && outputDefinitions.Count == 0)
		{
			DynamicPortDefinition dynamicPortDefinition = new DynamicPortDefinition("In", typeof(Flow));
			DynamicPortDefinition dynamicPortDefinition2 = new DynamicPortDefinition("Out", typeof(Flow));
			inputDefinitions.Add(dynamicPortDefinition);
			outputDefinitions.Add(dynamicPortDefinition2);
			entry.GatherPorts();
			exit.GatherPorts();
			Port outputPort = entry.GetOutputPort(dynamicPortDefinition.ID);
			Port inputPort = exit.GetInputPort(dynamicPortDefinition2.ID);
			BinderConnection.Create(outputPort, inputPort);
		}
	}

	public bool AddInputDefinition(DynamicPortDefinition def)
	{
		if (inputDefinitions.Find((DynamicPortDefinition d) => d.ID == def.ID) == null)
		{
			inputDefinitions.Add(def);
			return true;
		}
		return false;
	}

	public bool AddOutputDefinition(DynamicPortDefinition def)
	{
		if (outputDefinitions.Find((DynamicPortDefinition d) => d.ID == def.ID) == null)
		{
			outputDefinitions.Add(def);
			return true;
		}
		return false;
	}
}
