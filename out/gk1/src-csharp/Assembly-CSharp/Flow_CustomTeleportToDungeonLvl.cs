using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;

[Category("Game Actions")]
[Name("Custom Teleport To Dungeon LVL", 0)]
public class Flow_CustomTeleportToDungeonLvl : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<int> level_in;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", TeleportToLvl);
		@out = AddFlowOutput("Out");
		level_in = AddValueInput<int>("lvl_number");
	}

	private void TeleportToLvl(Flow flow)
	{
		GJTimer.AddTimer(0.01f, delegate
		{
			MainGame.me.TeleportToDungeonLevelCustom(level_in.value);
			@out.Call(flow);
		});
	}
}
