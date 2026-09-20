using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Pre-refresh zone", 0)]
[Category("Game Actions")]
public class Flow_PreRefreshZone : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> par_zone = AddValueInput<string>("zone id");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldZone zoneByID = WorldZone.GetZoneByID(par_zone.value);
			if (zoneByID != null)
			{
				zoneByID.PreRefreshZone();
			}
			flow_out.Call(f);
		});
	}
}
