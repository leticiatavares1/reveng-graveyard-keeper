using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Get World Zone Quality", 0)]
[Category("Game/World Zones")]
public class Flow_GetWorldZoneQuality : GKCustomFlowNode
{
	private ValueInput<string> worldZoneId;

	private ValueOutput<int> quality;

	protected override void RegisterPorts()
	{
		worldZoneId = AddValueInput<string>("worldZoneId".CapitalizeFirst());
		quality = AddValueOutput("quality", delegate
		{
			WorldZoneData worldZoneDataById = MainGame.WorldData.GetWorldZoneDataById(worldZoneId.value);
			if (worldZoneDataById == null)
			{
				Debug.LogError("Can't find world zone with id " + worldZoneId.value);
				return 0;
			}
			return (int)worldZoneDataById.GetTotalQuality();
		});
	}
}
