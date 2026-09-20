using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Create Town Building", 0)]
[Category("Game/UI")]
public class Flow_CreateTownBuilding : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> townBuildingId;

	private ValueInput<WgoData> buildOnWgoData;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Create);
		@out = AddFlowOutput("out".CapitalizeFirst());
		townBuildingId = AddValueInput<string>("townBuildingId".CapitalizeFirst());
		buildOnWgoData = AddValueInput<WgoData>("buildOnWgoData".CapitalizeFirst());
	}

	private void Create(Flow flow)
	{
		MainGame.Instance.GameSave.townSystem.StartTownBuildingCraftOnWgoFromScript(GameBalance.Me.GetData<TownBuildingDef>(townBuildingId.value), buildOnWgoData.value);
		@out.Call(flow);
	}
}
