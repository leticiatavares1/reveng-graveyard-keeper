using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Icon("CubePlus", false, "")]
[Description("If WGO is null, then self")]
[Category("Game Actions")]
[Name("Spawn NPC", 0)]
public class Flow_SpawnNPC : MyFlowNode
{
	private new WorldGameObject wgo;

	protected override void RegisterPorts()
	{
		ValueInput<GameObject> par_go = AddValueInput<GameObject>("Point");
		ValueInput<string> par_obj_id = AddValueInput<string>("Object id");
		ValueInput<string> par_custom_tag = AddValueInput<string>("Custom tag");
		ValueInput<WorldGameObject> par_direction_WGO = AddValueInput<WorldGameObject>("Direction to WGO");
		ValueInput<Direction> par_direction_enum = AddValueInput<Direction>("Direction Enum");
		ValueInput<Vector2> par_direction_vector2 = AddValueInput<Vector2>("Direction Vector2");
		ValueInput<CharAnimState> par_anim_state = AddValueInput<CharAnimState>("Animation State");
		ValueInput<string> par_skin_id = AddValueInput<string>("Skin id");
		ValueInput<bool> par_is_a_copy = AddValueInput<bool>(" Is it a copy?");
		AddValueOutput("WGO", () => wgo);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			wgo = GS.Spawn(par_obj_id.value, par_go.value.transform, par_custom_tag.value);
			if (wgo == null)
			{
				Debug.LogError("Couldn't spawn: " + par_obj_id.value + " at " + par_go.value);
				flow_out.Call(f);
			}
			else
			{
				wgo.components.StartComponents();
				BaseCharacterComponent char_comp = wgo.components.character;
				if (char_comp == null)
				{
					Debug.LogError("Character Component is null!");
					flow_out.Call(f);
				}
				else
				{
					if (!par_skin_id.isDefaultValue)
					{
						wgo.ApplySkin(par_skin_id.value);
					}
					if (par_is_a_copy.value)
					{
						wgo.SetParam("it_is_a_copy", 1f);
					}
					GJTimer.AddTimer(0.1f, delegate
					{
						if (!par_direction_WGO.isDefaultValue)
						{
							char_comp.LookAt(par_direction_WGO.value);
						}
						else if (par_direction_enum.value != 0)
						{
							char_comp.LookAt(par_direction_enum.value);
						}
						else if (!par_direction_vector2.isDefaultValue)
						{
							char_comp.LookAt(par_direction_vector2.value);
						}
						char_comp.SetAnimationState((!par_anim_state.isDefaultValue) ? par_anim_state.value : CharAnimState.Idle);
					});
					flow_out.Call(f);
				}
			}
		});
	}
}
