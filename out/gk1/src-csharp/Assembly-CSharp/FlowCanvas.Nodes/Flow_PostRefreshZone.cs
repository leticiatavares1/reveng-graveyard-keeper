using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Post-refresh zone", 0)]
[Category("Game Actions")]
public class Flow_PostRefreshZone : MyFlowNode
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
				zoneByID.PostRefreshZone();
				GUIElements.ChangeBubblesVisibility(MainGame.me.player_char.control_enabled);
			}
			flow_out.Call(f);
		});
	}
}
