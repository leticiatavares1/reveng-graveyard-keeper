using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Enable World Zone", 0)]
public class Flow_EnableWorldZone : MyFlowNode
{
	private ValueInput<bool> enable;

	public override string name
	{
		get
		{
			if (!enable.value)
			{
				return "Disable World Zone";
			}
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<string> zone_id = AddValueInput<string>("World Zone id");
		enable = AddValueInput<bool>("Enable World Zone");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldZone zoneByID = WorldZone.GetZoneByID(zone_id.value);
			if (zoneByID != null)
			{
				if (enable.value)
				{
					zoneByID.EnableWorldZone();
				}
				else
				{
					zoneByID.DisableWorldZone();
				}
			}
			flow_out.Call(f);
		});
	}
}
