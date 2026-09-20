using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Try Add To Conveyor System", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_TryAddToConveyorSystem : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WgoData> wgoData;

	private WgoData spawnedData;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), TryAddToConveyorSystem);
		@out = AddFlowOutput("out".CapitalizeFirst());
		wgoData = AddValueInput<WgoData>("wgoData");
	}

	private void TryAddToConveyorSystem(Flow flow)
	{
		if (wgoData.value != null)
		{
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(wgoData.value.UniqueId);
			if (wgoViewGlobal != null)
			{
				ConveyorBuildPointer.TryMakeConnectionsOutsideBuildSystem(wgoViewGlobal);
			}
			else if (wgoData.value is ConveyorWgoData conveyorWgoData)
			{
				MainGame.Instance.conveyorSystem.AddConveyorObject(conveyorWgoData.ConveyorComponent);
			}
		}
		@out.Call(flow);
	}
}
