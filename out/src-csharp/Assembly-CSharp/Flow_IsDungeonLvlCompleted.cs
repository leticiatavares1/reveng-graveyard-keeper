using DungeonGenerator;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;

[Category("Game Actions")]
[Name("Is Dungeon LVL Completed", 0)]
public class Flow_IsDungeonLvlCompleted : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput yes;

	private FlowOutput no;

	private ValueInput<int> dungeon_lvl_in;

	private ValueOutput<bool> dungeon_lvl_completed_out;

	private bool dungeon_lvl_completed_out_value;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", IsDungeonLvlCompleted);
		yes = AddFlowOutput("Yes");
		no = AddFlowOutput("No");
		dungeon_lvl_in = AddValueInput<int>("lvl_number");
		dungeon_lvl_completed_out = AddValueOutput("is_completed", () => dungeon_lvl_completed_out_value);
	}

	private void IsDungeonLvlCompleted(Flow flow)
	{
		SavedDungeon savedDungeon = MainGame.me.save.dungeons.GetSavedDungeon(dungeon_lvl_in.value);
		MainGame.me.dungeon_root.UpdateDungeonState();
		if (savedDungeon != null)
		{
			dungeon_lvl_completed_out_value = savedDungeon.is_completed;
			if (savedDungeon.is_completed)
			{
				yes.Call(flow);
			}
			else
			{
				no.Call(flow);
			}
		}
		else
		{
			Debug.LogError("No found saved dungeon for number " + dungeon_lvl_in.value);
			no.Call(flow);
		}
	}
}
