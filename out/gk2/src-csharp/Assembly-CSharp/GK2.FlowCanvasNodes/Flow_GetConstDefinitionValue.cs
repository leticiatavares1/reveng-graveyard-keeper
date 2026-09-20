using System;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Get Const Definition Value", 0)]
[Category("Player")]
public class Flow_GetConstDefinitionValue : GKCustomFlowNode
{
	[GatherPortsCallback]
	public ConstDef.ConstType constType;

	private ValueInput<string> definitionId;

	private ValueOutput<bool> boolValue;

	private ValueOutput<int> intValue;

	private ValueOutput<float> floatValue;

	private ValueOutput<string> stringValue;

	protected override void RegisterPorts()
	{
		definitionId = AddValueInput<string>("definitionId".CapitalizeFirst());
		switch (constType)
		{
		case ConstDef.ConstType.@bool:
			boolValue = AddValueOutput("boolValue".CapitalizeFirst(), () => ConstDef.Get(definitionId.value).BoolValue);
			break;
		case ConstDef.ConstType.@int:
			intValue = AddValueOutput("intValue".CapitalizeFirst(), () => ConstDef.Get(definitionId.value).IntValue);
			break;
		case ConstDef.ConstType.@float:
			floatValue = AddValueOutput("floatValue".CapitalizeFirst(), () => ConstDef.Get(definitionId.value).FloatValue);
			break;
		case ConstDef.ConstType.@string:
			stringValue = AddValueOutput("stringValue".CapitalizeFirst(), () => ConstDef.Get(definitionId.value).StringValue);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
