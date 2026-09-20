using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Get Idle Points Stock WGO")]
[Category("Game Actions")]
[Name("Get Idle Points Stock WGO", 0)]
public class Flow_GetStockWGO : MyFlowNode
{
	private WorldGameObject stock_wgo;

	private string prefix_string = string.Empty;

	protected override void RegisterPorts()
	{
		ValueInput<GDPoint.IdlePointPrefix> in_idle_point_prefix = AddValueInput<GDPoint.IdlePointPrefix>("Prefix");
		AddValueOutput("Stock WGO", () => stock_wgo);
		AddValueOutput("Prefix String", () => prefix_string);
		FlowOutput flow_yes = AddFlowOutput("Found");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_idle_point_prefix.value == GDPoint.IdlePointPrefix.None)
			{
				Debug.LogError("Prefix is NULL!");
			}
			else
			{
				prefix_string = GDPoint.GetIdlePrefix(in_idle_point_prefix.value);
				string text = prefix_string + "stock";
				stock_wgo = WorldMap.GetWorldGameObjectByCustomTag(text);
				if (stock_wgo == null)
				{
					Debug.LogError("Stock WGO with custom_tag=\"" + text + "\" not found!");
				}
				else
				{
					flow_yes.Call(f);
				}
			}
		});
	}
}
