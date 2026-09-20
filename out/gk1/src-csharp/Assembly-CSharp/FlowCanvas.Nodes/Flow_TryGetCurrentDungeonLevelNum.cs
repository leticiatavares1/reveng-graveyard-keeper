using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Try Get Current Dungeon Level Num")]
[Icon("CubeArrowCube", false, "")]
[Category("Game Actions")]
[Name("Try Get Current Dungeon Level Num", 0)]
public class Flow_TryGetCurrentDungeonLevelNum : MyFlowNode
{
	private int dungeon_level;

	protected override void RegisterPorts()
	{
		AddValueOutput("dungeon level num", () => dungeon_level);
		FlowOutput flow_yes = AddFlowOutput("Dungeon is loaded");
		FlowOutput flow_no = AddFlowOutput("Dungeon is NOT loaded");
		AddFlowInput("In", delegate(Flow f)
		{
			dungeon_level = 0;
			if (!MainGame.me.dungeon_root.dungeon_is_loaded_now)
			{
				flow_no.Call(f);
			}
			else
			{
				dungeon_level = MainGame.me.dungeon_root.cur_dungeon_preset.dungeon_level;
				flow_yes.Call(f);
			}
		});
	}
}
