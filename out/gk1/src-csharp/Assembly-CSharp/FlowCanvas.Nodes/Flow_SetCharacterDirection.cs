using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Set Character Direction", 0)]
[Category("Game Actions")]
[Description("If WGO is null, then self")]
public class Flow_SetCharacterDirection : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_me_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<WorldGameObject> par_direction_WGO = AddValueInput<WorldGameObject>("Direction to WGO");
		ValueInput<Direction> par_direction_enum = AddValueInput<Direction>("Direction Enum");
		ValueInput<Vector2> par_direction_vector2 = AddValueInput<Vector2>("Direction Vector2");
		ValueInput<GameObject> par_direction_game_object = AddValueInput<GameObject>("Direction to GameObject");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_me_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null !");
				flow_out.Call(f);
			}
			else
			{
				BaseCharacterComponent character = worldGameObject.components.character;
				if (character == null)
				{
					Debug.LogError("Character Component is null!");
					flow_out.Call(f);
				}
				else
				{
					if (!par_direction_WGO.isDefaultValue || par_direction_WGO.value != null)
					{
						character.LookAt(par_direction_WGO.value);
					}
					else if (par_direction_enum.value != 0)
					{
						character.LookAt(par_direction_enum.value);
					}
					else if (!par_direction_vector2.isDefaultValue)
					{
						character.LookAt(par_direction_vector2.value);
					}
					else if (!par_direction_game_object.isDefaultValue)
					{
						character.LookAt(par_direction_game_object.value);
					}
					flow_out.Call(f);
				}
			}
		});
	}
}
