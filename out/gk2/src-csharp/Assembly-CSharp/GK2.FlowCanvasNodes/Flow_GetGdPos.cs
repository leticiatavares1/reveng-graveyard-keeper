using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Get GD Point Data Position", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_GetGdPos : GKCustomFlowNode
{
	public enum InfoType
	{
		Position,
		Direction,
		Id
	}

	[GatherPortsCallback]
	public InfoType infoType;

	private ValueInput<GDPointData> gdPointDataValIn;

	private ValueOutput<Vector3> posValOut;

	private ValueOutput<Direction> direction;

	private ValueOutput<string> gdPointId;

	public override string name => "Get GD Point Data" + ((infoType == InfoType.Id) ? " Id" : ((infoType == InfoType.Direction) ? " Direction" : " Pos"));

	protected override void RegisterPorts()
	{
		gdPointDataValIn = AddValueInput<GDPointData>("GDPointData");
		switch (infoType)
		{
		case InfoType.Position:
			posValOut = AddValueOutput("Pos", () => gdPointDataValIn.value?.Position ?? Vector3.zero);
			break;
		case InfoType.Direction:
			direction = AddValueOutput("Direction", () => gdPointDataValIn.value.Direction);
			break;
		case InfoType.Id:
			gdPointId = AddValueOutput("Id", () => gdPointDataValIn.value.Id);
			break;
		}
	}
}
