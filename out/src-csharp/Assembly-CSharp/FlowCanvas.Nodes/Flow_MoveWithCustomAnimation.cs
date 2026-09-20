using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Move with custom animation", 0)]
[Category("Game Actions")]
[Description("If WGO is null, then self")]
[Icon("CubeArrowStraight", false, "")]
public class Flow_MoveWithCustomAnimation : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<GameObject> par_target = AddValueInput<GameObject>("target");
		ValueInput<string> par_anim = AddValueInput<string>("animation_trigger");
		ValueInput<float> par_speed = AddValueInput<float>("speed");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_came = AddFlowOutput("Came to dest");
		WorldGameObject _wgo;
		AddFlowInput("In", delegate(Flow f)
		{
			if ((_wgo = WGOParamOrSelf(par_wgo)) == null)
			{
				Debug.LogError("WGO GoTo error: WGO #1 is null");
				flow_out.Call(f);
			}
			else
			{
				float num = par_speed.value;
				if (num <= 0f)
				{
					num = _wgo.data.GetParam("speed");
				}
				_wgo.TriggerSmartAnimation(par_anim.value);
				float num2 = Vector2.Distance(par_target.value.transform.position, _wgo.transform.position) / 96f;
				_wgo.components.timer.Play(num2 / num);
				if (_wgo == MainGame.me.player)
				{
					MainGame.me.player.components.character.player_controlled_by_script = true;
				}
				_wgo.components.character.CurveMoveTo(par_target.value.transform, null, num2, delegate
				{
					if (_wgo == MainGame.me.player)
					{
						MainGame.me.player.components.character.player_controlled_by_script = false;
					}
					flow_came.Call(f);
				}, based_on_anim_timing: false, set_active_now_because_of_movement: true);
				flow_out.Call(f);
			}
		});
	}
}
