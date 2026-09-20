using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("Try Load Next Dungeon Level. CALL ANLY FROM ALREADY LOADED DUNGEON!!!")]
[Icon("CubeArrowCube", false, "")]
[Name("Try Load Next Dungeon Level", 0)]
public class Flow_TryLoadNextDungeonLevel : MyFlowNode
{
	private Texture lut;

	protected override void RegisterPorts()
	{
		FlowOutput flow_yes = AddFlowOutput("Yes");
		FlowOutput flow_no = AddFlowOutput("No");
		AddFlowInput("In", delegate(Flow f)
		{
			if (!MainGame.me.dungeon_root.dungeon_is_loaded_now)
			{
				Debug.LogError("Calling \"Flow_TryLoadNextDungeonLevel\" not from dungeon!");
			}
			else
			{
				MainGame.me.dungeon_root.UpdateDungeonState();
				if (!MainGame.me.dungeon_root.cur_saved_dungeon.is_completed)
				{
					Debug.Log("Can not load next dungeon level: current is not completed.");
					flow_no.Call(f);
				}
				else
				{
					GJTimer.AddTimer(0.01f, delegate
					{
						GUIElements.ChangeBubblesVisibility(show: false);
						CameraTools.Fade(delegate
						{
							MainGame.me.TeleportToDungeonLevel(MainGame.me.dungeon_root.cur_dungeon_preset.dungeon_level + 1);
							CameraTools.UnFade(delegate
							{
								GUIElements.ChangeBubblesVisibility(MainGame.me.player_char.control_enabled);
							}, 0.5f);
						}, 0.5f);
					});
					flow_yes.Call(f);
				}
			}
		});
	}
}
