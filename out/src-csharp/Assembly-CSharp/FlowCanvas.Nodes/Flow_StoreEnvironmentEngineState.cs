using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Store Environment Engine State", 0)]
[Category("Game Actions")]
[Description("State can be 'Inside' xor 'Realtime'")]
public class Flow_StoreEnvironmentEngineState : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort<bool>("Store state?").value)
			{
				return "Restore Environment Engine State";
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
		ValueInput<bool> to_store_flag = AddValueInput<bool>("Store state?");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (!to_store_flag.value)
			{
				EnvironmentEngine.me.RestoreEnvironmentEngineState();
			}
			else
			{
				EnvironmentEngine.me.StoreEnvironmentEngineState();
			}
			flow_out.Call(f);
		});
	}
}
