using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Enable Global Craft Control", 0)]
[Category("Game Actions")]
public class Flow_EnableGlobalCraftControl : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort<bool>("Enabled").value)
			{
				return "Disable Global Craft Control";
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
		ValueInput<bool> is_enabled = AddValueInput<bool>("Enabled");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.save.has_global_craft_control = is_enabled.value;
			flow_out.Call(f);
		});
	}
}
