using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Functions")]
[Name("Is Known World Zone", 0)]
public class Flow_IsKnownWorldZone : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> zone_id = AddValueInput<string>("zone_id");
		FlowOutput flow_yes = AddFlowOutput("Is known");
		FlowOutput flow_no = AddFlowOutput("Is NOT known");
		bool is_known = false;
		AddValueOutput("is_known (bool)", () => is_known);
		AddFlowInput("In", delegate(Flow f)
		{
			if (string.IsNullOrEmpty(zone_id.value))
			{
				flow_no.Call(f);
			}
			else if (MainGame.me?.save?.known_world_zones != null)
			{
				is_known = MainGame.me.save.known_world_zones.Contains(zone_id.value);
				if (is_known)
				{
					flow_yes.Call(f);
				}
				else
				{
					flow_no.Call(f);
				}
			}
		});
	}
}
