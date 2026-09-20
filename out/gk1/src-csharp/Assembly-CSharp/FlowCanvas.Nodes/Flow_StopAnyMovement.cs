using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Stop any movement", 0)]
public class Flow_StopAnyMovement : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("is player").value)
			{
				return "<color=#FFFF50>Stop player's movement </color>";
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
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<bool> in_is_player = AddValueInput<bool>("is player");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_is_player.value)
			{
				MainGame.me.player_char.StopMovement();
				MainGame.me.player_char.player_controlled_by_script = false;
			}
			else
			{
				WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
				if (worldGameObject == null)
				{
					Debug.LogError("WGO is null!");
					flow_out.Call(f);
					return;
				}
				if (!worldGameObject.obj_def.IsCharacter())
				{
					Debug.LogError("WGO is not character!");
					flow_out.Call(f);
					return;
				}
				BaseCharacterComponent character = worldGameObject.components.character;
				if (character == null)
				{
					Debug.LogError("BaseCharacterComponent is null!");
					flow_out.Call(f);
					return;
				}
				character.StopMovement();
				if (worldGameObject.is_player)
				{
					character.player_controlled_by_script = false;
				}
			}
			flow_out.Call(f);
		});
	}
}
