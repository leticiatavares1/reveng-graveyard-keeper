using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;

[Category("Game Actions")]
[Name("Set Inventory Size", 0)]
public class Flow_SetInventorySize : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WorldGameObject> wgo_in;

	private ValueInput<int> inventory_size_in;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", SetInventorySize);
		@out = AddFlowOutput("Out");
		wgo_in = AddValueInput<WorldGameObject>("WGO");
		inventory_size_in = AddValueInput<int>("Size");
	}

	private void SetInventorySize(Flow flow)
	{
		WorldGameObject worldGameObject = WGOParamOrSelf(wgo_in);
		if (worldGameObject != null)
		{
			worldGameObject.data.SetInventorySize(inventory_size_in.value);
			Debug.Log(worldGameObject?.ToString() + " changed inventory size to " + inventory_size_in.value);
		}
		else
		{
			Debug.LogError("Flow_SetInventorySize error: WGO is null");
		}
		@out.Call(flow);
	}
}
