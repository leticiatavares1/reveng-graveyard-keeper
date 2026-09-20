using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Destroy WGOData", 0)]
[Category("Game/Wgo")]
[Color("f47dff")]
public class Flow_DestroyWgoData : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool destroyList;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<List<WgoData>> wgoDataList;

	public override string name => "Destroy WgoData" + (destroyList ? " List" : "");

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), DestroyWgoData);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (destroyList)
		{
			wgoDataList = AddValueInput<List<WgoData>>("wgoDataList");
		}
		else
		{
			base.RegisterPorts();
		}
	}

	private void DestroyWgoData(Flow flow)
	{
		if (destroyList && wgoDataList != null)
		{
			int count = wgoDataList.value.Count;
			for (int i = 0; i < count; i++)
			{
				MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(wgoDataList.value[0]);
			}
		}
		else
		{
			MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(GetWgoData());
		}
		@out.Call(flow);
	}
}
