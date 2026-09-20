using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Spawn WSO", 0)]
[Category("Game Actions")]
[Description("If WSO is null, place nothing")]
[Icon("CubePlus", false, "")]
[Color("#2a2070")]
public class Flow_SpawnWSO : MyFlowNode
{
	private WorldSimpleObject o_wso;

	protected override void RegisterPorts()
	{
		ValueInput<GameObject> par_go = AddValueInput<GameObject>("Point");
		ValueInput<string> par_obj_id = AddValueInput<string>("WSO id");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			o_wso = Resources.Load<WorldSimpleObject>("objects/WorldSimpleObjects/" + par_obj_id.value);
			if (o_wso == null)
			{
				Debug.LogError("Couldn't spawn: " + par_obj_id.value + " at " + par_go.value);
			}
			else
			{
				WorldSimpleObject worldSimpleObject = null;
				try
				{
					worldSimpleObject = UnityEngine.Object.Instantiate(o_wso, MainGame.me.world_root, worldPositionStays: false);
				}
				catch (Exception ex)
				{
					Debug.LogError("Flow_SpawnWSO exception[" + par_obj_id.value + "]: " + ex);
				}
				if (worldSimpleObject != null)
				{
					worldSimpleObject.transform.position = par_go.value.transform.position;
				}
				else
				{
					Debug.LogError("Flow_SpawnWSO error: new_wso_obj is NULL! Call Artur!");
				}
			}
			flow_out.Call(f);
		});
	}
}
