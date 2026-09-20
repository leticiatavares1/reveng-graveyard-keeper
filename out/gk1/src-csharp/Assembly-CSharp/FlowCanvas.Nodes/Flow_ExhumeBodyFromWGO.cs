using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("Remove body from inventory and drop it")]
[Name("Exhume body from WGO", 0)]
public class Flow_ExhumeBodyFromWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		Item item = new Item();
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		AddValueOutput("Item", () => item);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
			}
			else
			{
				Item item2 = (item = worldGameObject.GetBodyFromInventory());
				if (item2 == null)
				{
					Debug.LogError("Body not found in WGO inventory", worldGameObject);
				}
				else
				{
					worldGameObject.DropItem(item2);
					worldGameObject.data.RemoveItem(item2);
					flow_out.Call(f);
				}
			}
		});
	}
}
