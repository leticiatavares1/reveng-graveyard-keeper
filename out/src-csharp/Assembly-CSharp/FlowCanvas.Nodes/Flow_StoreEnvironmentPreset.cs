using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Store Environment Preset", 0)]
[Category("Game Actions")]
[Description("Stores previously set preset, restore and apply it")]
public class Flow_StoreEnvironmentPreset : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort<bool>("Store preset?").value)
			{
				return "Restore Environment Preset";
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
		ValueInput<bool> to_store_flag = AddValueInput<bool>("Store preset?");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (!to_store_flag.value)
			{
				MainGame.me.save.RestoreEnvironmentPreset();
			}
			else
			{
				MainGame.me.save.StoreEnvironmentPreset();
			}
			flow_out.Call(f);
		});
	}
}
