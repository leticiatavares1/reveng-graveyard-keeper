using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Save and Exit Dungeon", 0)]
[Category("Game Actions")]
[Icon("CubeArrowCube", false, "")]
[Description("Save Dungeon And Exit.")]
public class Flow_SaveAndExitDungeon : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_target_wgo = AddValueInput<WorldGameObject>("Destination WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_target_wgo.value == null)
			{
				Debug.LogError("Destination WGO is null!!!");
			}
			else
			{
				GUIElements.ChangeBubblesVisibility(show: false);
				GJTimer.AddTimer(0.01f, delegate
				{
					MainGame.me.player.components.character.TeleportWithFade(in_target_wgo.value, delegate
					{
						if (MainGame.me.dungeon_root.TrySaveDungeon())
						{
							Debug.Log("Successfully saved dungeon.");
						}
						MainGame.me.dungeon_root.DestroyTiles();
						MainGame.me.OnExitDungeon();
					}, delegate
					{
						GUIElements.ChangeBubblesVisibility(MainGame.me.player_char.control_enabled);
						MainGame.me.save.quests.CheckKeyQuests("dungeon_exit");
					});
				});
				flow_out.Call(f);
			}
		});
	}
}
