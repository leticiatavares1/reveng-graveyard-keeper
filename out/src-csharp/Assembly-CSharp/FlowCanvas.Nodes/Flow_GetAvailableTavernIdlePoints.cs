using System;
using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Get Available Tavern Idle Points", 0)]
[Category("Game Functions")]
[Color("eed9a7")]
[ContextDefinedOutputs(new Type[] { typeof(List<GDPoint>) })]
public class Flow_GetAvailableTavernIdlePoints : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_event = AddValueInput<string>("event");
		AddValueOutput("List", "GD point", delegate
		{
			TavernEventDefinition dataOrNull = GameBalance.me.GetDataOrNull<TavernEventDefinition>(in_event.value);
			List<GDPoint> list = new List<GDPoint>();
			foreach (string availableIdlePoint in MainGame.me.save.players_tavern_engine.GetAvailableIdlePoints(dataOrNull))
			{
				GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(availableIdlePoint);
				if (gDPointByGDTag != null)
				{
					list.Add(gDPointByGDTag);
				}
			}
			return list;
		});
	}
}
