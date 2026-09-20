using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Filter WGOs List", 0)]
[Category("Game Actions")]
[Description("Filter WGOs List")]
public class Flow_FilterWGOsList : MyFlowNode
{
	private List<WorldGameObject> filtered_wgos;

	public override string name
	{
		get
		{
			string text = base.name;
			if (!GetInputValuePort<string>("wgo_name").isDefaultValue)
			{
				return text + " by name";
			}
			if (!GetInputValuePort<string>("custom_tag").isDefaultValue)
			{
				return text + " by tag";
			}
			return text + " by ??????";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> par_wgo = AddValueInput<List<WorldGameObject>>("WGOs List");
		ValueInput<string> par_obj_id = AddValueInput<string>("Obj ID", "wgo_name");
		ValueInput<string> par_custom_tag = AddValueInput<string>("Custom Tag", "custom_tag");
		AddValueOutput("WGOs List", () => filtered_wgos);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			filtered_wgos = new List<WorldGameObject>();
			bool flag = false;
			bool flag2 = false;
			List<WorldGameObject> value = par_wgo.value;
			if (value == null || value.Count == 0)
			{
				flow_out.Call(f);
			}
			else
			{
				if (!string.IsNullOrEmpty(par_obj_id.value))
				{
					flag = true;
				}
				if (!string.IsNullOrEmpty(par_custom_tag.value))
				{
					flag2 = true;
				}
				if (flag || flag2)
				{
					foreach (WorldGameObject item in value)
					{
						if (!(item == null) && (!flag || !(item.obj_id != par_obj_id.value)) && (!flag2 || !(item.custom_tag != par_custom_tag.value)))
						{
							filtered_wgos.Add(item);
						}
					}
				}
				flow_out.Call(f);
			}
		});
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		MakeStringNullIfEmpty("wgo_name");
		MakeStringNullIfEmpty("custom_tag");
	}
}
