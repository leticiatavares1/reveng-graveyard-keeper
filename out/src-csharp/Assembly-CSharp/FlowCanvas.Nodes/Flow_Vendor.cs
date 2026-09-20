using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Vendor", 0)]
public class Flow_Vendor : MyFlowNode
{
	public override string name
	{
		get
		{
			return "Open vendor gui";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_closed = AddFlowOutput("On Hide");
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject value = par_wgo.value;
			if (value == null)
			{
				value = base.wgo;
			}
			if (!MainGame.me.player_char.control_enabled)
			{
				GS.SetPlayerEnable(player_enabled: true, affect_cinematic: false);
			}
			GUIElements.me.vendor.Open(value, delegate
			{
				flow_closed.Call(f);
			});
			flow_out.Call(f);
		});
	}
}
