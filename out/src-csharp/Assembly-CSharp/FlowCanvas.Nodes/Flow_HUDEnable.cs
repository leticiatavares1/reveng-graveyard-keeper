using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("HUD Enable", 0)]
public class Flow_HUDEnable : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort<bool>("Enable HUD").value)
			{
				return "HUD Disable";
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
		ValueInput<bool> en = AddValueInput<bool>("Enable HUD");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.EnableHUD(en.value);
			flow_out.Call(f);
		});
	}
}
