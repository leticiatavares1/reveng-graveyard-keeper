using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;

[Description("Trying to use item from character's inventory. Happens nothing, if item not found")]
[Name("Use Item From Inventory", 0)]
[Category("Game Actions")]
public class Flow_UseItemFromInventory : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> item_id;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", UseItemFromInventory);
		@out = AddFlowOutput("Out");
		item_id = AddValueInput<string>("Item ID");
	}

	private void UseItemFromInventory(Flow flow)
	{
		Item itemWithID = MainGame.me.player.data.GetItemWithID(item_id.value);
		if (itemWithID != null)
		{
			MainGame.me.player.UseItemFromInventory(itemWithID);
		}
		else
		{
			Debug.LogError("Found not found by ID: " + item_id.value);
		}
		@out.Call(flow);
	}
}
