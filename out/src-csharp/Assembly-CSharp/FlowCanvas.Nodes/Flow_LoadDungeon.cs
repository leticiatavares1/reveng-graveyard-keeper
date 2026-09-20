using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Load Dungeon", 0)]
[Description("Load Dungeon")]
public class Flow_LoadDungeon : MyFlowNode
{
	private Texture lut;

	protected override void RegisterPorts()
	{
		ValueInput<int> in_dungeon_level = AddValueInput<int>("Dunge level");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.TeleportToDungeonLevel(in_dungeon_level.value);
			flow_out.Call(f);
		});
	}
}
