using LinqTools;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Set Animator Param", 0)]
[Category("Game Actions")]
[Color("989BA4")]
[Description("If WGO is null, then self")]
public class Flow_SetAnimatorParam : MyFlowNode
{
	protected override void RegisterPorts()
	{
		WorldGameObject out_wgo = null;
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> par_state_name = AddValueInput<string>("Parameter name");
		ValueInput<AnimatorStateOverriderAtom.AnimatorStates> param_type = AddValueInput<AnimatorStateOverriderAtom.AnimatorStates>("Param type");
		ValueInput<float> param_value = AddValueInput<float>("Value");
		AddValueOutput("WGO", () => out_wgo);
		FlowOutput flow_output = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			out_wgo = WGOParamOrSelf(par_wgo);
			string value = par_state_name.value;
			float value2 = param_value.value;
			if (out_wgo != null)
			{
				foreach (Animator item in out_wgo.GetComponentsInChildren<Animator>(includeInactive: true).ToList())
				{
					AnimatorControllerParameter[] parameters = item.parameters;
					for (int i = 0; i < parameters.Length; i++)
					{
						if (parameters[i].name == value)
						{
							switch (param_type.value)
							{
							case AnimatorStateOverriderAtom.AnimatorStates.BOOL:
								item.SetBool(value, !Mathf.Approximately(value2, 0f));
								break;
							case AnimatorStateOverriderAtom.AnimatorStates.FLOAT:
								item.SetFloat(value, value2);
								break;
							case AnimatorStateOverriderAtom.AnimatorStates.INT:
								item.SetInteger(value, (int)value2);
								break;
							}
						}
					}
				}
			}
			else
			{
				Debug.LogError("Flow_SetAnimatorParam, WGO is null");
			}
			flow_output.Call(f);
		});
	}
}
