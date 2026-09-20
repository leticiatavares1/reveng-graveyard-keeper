using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Lock Craft List", 0)]
[Category("Game Actions")]
public class Flow_LockCraftList : MyFlowNode
{
	public List<string> craft_ids = new List<string>();

	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			foreach (string craft_id in craft_ids)
			{
				MainGame.me.save.LockCraftForever(craft_id);
			}
			flow_out.Call(f);
		});
	}
}
