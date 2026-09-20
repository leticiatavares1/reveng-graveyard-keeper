using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Add Obj Craft Items To Player", 0)]
[Category("Game Actions")]
public class Flow_AddObjCraftItemsToPlayer : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_craft_id = AddValueInput<string>("Obj Craft");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			ObjectCraftDefinition data = GameBalance.me.GetData<ObjectCraftDefinition>(in_craft_id.value);
			if (data == null)
			{
				Debug.Log("Trying add items to player for obj craft:[" + in_craft_id.value + "] but it is null");
				flow_out.Call(f);
			}
			else
			{
				MainGame.me.player.AddToInventory(data.needs);
				flow_out.Call(f);
			}
		});
	}
}
