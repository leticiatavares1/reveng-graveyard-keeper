using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Set WGO Scale", 0)]
[Category("Game Actions")]
public class Flow_SetWGOScale : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<Vector3> in_scale = AddValueInput<Vector3>("Scale Vector");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("Flow_SetWGOScale error: WGO is null");
			}
			else
			{
				Transform transform = null;
				try
				{
					transform = worldGameObject.GetComponent<Transform>();
				}
				catch (Exception ex)
				{
					Debug.LogError("Flow_SetWGOScale exception: " + ex);
				}
				if (transform != null)
				{
					transform.localScale = in_scale.value;
					Debug.Log($"Flow_SetWGOScale: set scale {in_scale.value} to WGO name={worldGameObject.name}, obj_id={worldGameObject.obj_id}");
				}
			}
			flow_out.Call(f);
		});
	}
}
