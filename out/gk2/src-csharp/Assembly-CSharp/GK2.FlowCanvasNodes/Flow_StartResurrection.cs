using System.Collections.Generic;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Start Resurrection", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_StartResurrection : GKCustomFlowNode
{
	private FlowInput flowInput;

	private FlowOutput flowOutput;

	protected override void RegisterPorts()
	{
		flowInput = AddFlowInput("In", StartResurrection);
		flowOutput = AddFlowOutput("Out");
	}

	private void StartResurrection(Flow flow)
	{
		List<WgoData> wgoDataList = MainGame.WorldData.GetWgoDataList("resurrection_table_1");
		PlayerData playerData = MainGame.PlayerData;
		foreach (WgoData item in wgoDataList)
		{
			if (item.GetGameResInt("resurrection_prepared") > 0)
			{
				CraftDef craftDef = GameBalance.GetCraftDef("corpse_zombie_transition");
				CraftElement craftElement = new CraftElement(craftDef.id, 1, new List<NeedItemData>(), new CraftParamsData(craftDef.id, new GameRes()));
				if (item.CraftComponent.TryStartCraft(craftElement))
				{
					playerData.AddRes("cur_zombies_count", 1f);
				}
			}
		}
		if (playerData.CurrentWorldZoneData != null && playerData.CurrentWorldZoneData.Definition.id == "resurrection")
		{
			GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
		}
		playerData.SetRes("resurrection_has_power", 0f);
		flowOutput.Call(flow);
	}
}
