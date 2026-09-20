using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Lock teleports")]
[Category("Game Actions")]
[Name("Lock teleports", 0)]
public class Flow_LockTeleports : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("Lock").value)
			{
				return "<color=#FFFF50>Lock Teleports</color>";
			}
			return "<color=#30FF30>Unlock Teleports</color>";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<bool> par_do_lock = AddValueInput<bool>("Lock");
		ValueInput<int> par_lock_param = AddValueInput<int>("Lock Param");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.player.data.SetParam("lock_tp", par_do_lock.value ? 1 : 0);
			MainGame.me.player.data.SetParam("lock_tp_param", par_lock_param.value);
			flow_out.Call(f);
		});
	}
}
