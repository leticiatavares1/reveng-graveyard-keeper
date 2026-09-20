using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Enable Show WGO Quality", 0)]
[Category("Game Actions")]
[Description("Enable Show WGO Quality")]
public class Flow_ShowWGOQuality : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("Enable").value)
			{
				return "<color=#FFFF50>Enable Show WGO Quality</color>";
			}
			return "<color=#30FF30>Disable Show WGO Quality</color>";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<bool> par_do_enable = AddValueInput<bool>("Enable");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.player.data.SetParam("do_not_show_wgo_qualities", (!par_do_enable.value) ? 1 : 0);
			MainGame.me.player_component.CheckShowWGOQuality();
			flow_out.Call(f);
		});
	}
}
