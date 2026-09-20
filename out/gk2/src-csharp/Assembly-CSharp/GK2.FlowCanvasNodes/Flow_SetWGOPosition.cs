using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Teleport WGO", 0)]
[Category("Game/Environment")]
[Color("f47dff")]
public class Flow_SetWGOPosition : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> gdPointId;

	private ValueInput<GDPointData> gdPointData;

	private ValueOutput<GDPointData> gdPointDataOutput;

	[GatherPortsCallback]
	public bool findGdPointById;

	[GatherPortsCallback]
	public bool fxBefore;

	[GatherPortsCallback]
	public bool fxAfter;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), Teleport);
		@out = AddFlowOutput("out".CapitalizeFirst());
		gdPointDataOutput = AddValueOutput("GDPointData", () => GetGDPointData());
		if (findGdPointById)
		{
			gdPointId = AddValueInput<string>("gdPointId");
		}
		else
		{
			gdPointData = AddValueInput<GDPointData>("gdPointData");
		}
	}

	private GDPointData GetGDPointData()
	{
		if (!findGdPointById)
		{
			return gdPointData.value;
		}
		return MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(gdPointId.value);
	}

	private void Teleport(Flow flow)
	{
		GDPointData gDPointData = GetGDPointData();
		WgoData wgoData = GetWgoData();
		if (wgoData != null)
		{
			if (fxBefore)
			{
				WorldFX.Spawn(wgoData.Position, "puff_npc");
			}
			wgoData.Position = gDPointData.Position;
			if (fxAfter)
			{
				WorldFX.Spawn(wgoData.Position, "puff_npc");
			}
		}
		@out.Call(flow);
	}
}
