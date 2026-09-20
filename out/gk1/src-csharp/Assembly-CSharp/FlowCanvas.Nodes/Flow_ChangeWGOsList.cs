using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Icon("CubeArrowCube", false, "")]
[Description("If WGO is null, then self")]
[Category("Game Actions")]
[Name("Change WGOs List", 0)]
public class Flow_ChangeWGOsList : MyFlowNode
{
	public override string name
	{
		get
		{
			string text = base.name;
			if (IsEmptyStringInputPort("obj_id"))
			{
				return text + "\n<color=red>obj_id is empty</color>";
			}
			return text;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> par_wgo = AddValueInput<List<WorldGameObject>>("WGOs List");
		ValueInput<string> par_obj_id = AddValueInput<string>("obj_id");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			List<WorldGameObject> value = par_wgo.value;
			if (value != null)
			{
				foreach (WorldGameObject item in value)
				{
					if (item == null)
					{
						Debug.LogError("Null WGO found!");
					}
					else
					{
						item.ReplaceWithObject(par_obj_id.value);
						item.Redraw();
					}
				}
				flow_out.Call(f);
			}
		});
	}
}
