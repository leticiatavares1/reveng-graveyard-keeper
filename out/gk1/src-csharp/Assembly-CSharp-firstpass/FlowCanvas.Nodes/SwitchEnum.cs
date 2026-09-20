using System;
using System.Collections.Generic;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace FlowCanvas.Nodes;

[ContextDefinedInputs(new Type[] { typeof(Enum) })]
[Description("Branch the Flow based on an enum value.\nPlease connect an Enum first for the options to show.")]
[Category("Flow Controllers/Switchers")]
public class SwitchEnum : FlowControlNode
{
	[SerializeField]
	private SerializedTypeInfo _type;

	private Type type
	{
		get
		{
			if (_type == null)
			{
				return null;
			}
			return _type.Get();
		}
		set
		{
			if (_type == null || _type.Get() != value)
			{
				_type = new SerializedTypeInfo(value);
			}
		}
	}

	protected override void RegisterPorts()
	{
		if (type == null)
		{
			type = typeof(Enum);
		}
		ValueInput e = AddValueInput(type.Name, type, "Enum");
		if (type != typeof(Enum))
		{
			List<FlowOutput> outs = new List<FlowOutput>();
			string[] names = Enum.GetNames(type);
			foreach (string text in names)
			{
				outs.Add(AddFlowOutput(text));
			}
			AddFlowInput("In", delegate(Flow f)
			{
				int index = (int)Enum.Parse(e.value.GetType(), e.value.ToString());
				outs[index].Call(f);
			});
		}
	}

	public override Type GetNodeWildDefinitionType()
	{
		return typeof(Enum);
	}

	public override void OnPortConnected(Port port, Port otherPort)
	{
		if (type == typeof(Enum) && typeof(Enum).RTIsAssignableFrom(otherPort.type))
		{
			type = otherPort.type;
			GatherPorts();
		}
	}
}
