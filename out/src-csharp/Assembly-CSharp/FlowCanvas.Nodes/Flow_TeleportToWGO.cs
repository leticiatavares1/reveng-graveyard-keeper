using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Teleport With Fade", 0)]
public class Flow_TeleportToWGO : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<WorldGameObject>("Who").value == null)
			{
				return "Player Teleport To WGO";
			}
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_who = AddValueInput<WorldGameObject>("Who");
		ValueInput<string> in_destination = AddValueInput<string>("Destination Tag");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_middle = AddFlowOutput("Middle");
		FlowOutput flow_finished = AddFlowOutput("On Finished");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = in_who.value ?? MainGame.me.player;
			if (worldGameObject == null)
			{
				Debug.LogError("TeleportToWGO error: WGO is null");
			}
			else
			{
				if (!worldGameObject.is_player)
				{
					worldGameObject.RedrawBubble();
				}
				WorldGameObject worldGameObjectByCustomTag = WorldMap.GetWorldGameObjectByCustomTag(in_destination.value);
				if (worldGameObjectByCustomTag == null)
				{
					Debug.LogError("Can't find destination tag: " + in_destination.value);
				}
				else
				{
					Debug.Log("Teleporting to target: " + worldGameObjectByCustomTag.name, worldGameObjectByCustomTag.gameObject);
					string[] array = in_destination.value.Split('_');
					if (array.Length >= 2 && array[0] == "tp")
					{
						MainGame.me.save.quests.CheckKeyQuests("tp_" + array[1]);
					}
					worldGameObject.cur_gd_point = string.Empty;
					worldGameObject.components.character.TeleportWithFade(worldGameObjectByCustomTag, delegate
					{
						flow_middle.Call(f);
					}, delegate
					{
						GUIElements.ChangeBubblesVisibility(MainGame.me.player_char.control_enabled);
						flow_finished.Call(f);
					});
					flow_out.Call(f);
				}
			}
		});
	}
}
