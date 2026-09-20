using ParadoxNotion.Design;

namespace FlowCanvas.Nodes.Souls;

[Category("Game Actions/Souls")]
[Name("Get Soul Item From Inventory", 0)]
public class Flow_GetSoulItemFromInventory : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private new ValueInput<WorldGameObject> wgo;

	private ValueOutput<Item> soul_item;

	private WorldGameObject _wgo;

	private Item _out_soul_item;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", GetSoulItem);
		@out = AddFlowOutput("Out");
		wgo = AddValueInput<WorldGameObject>("WGO");
		soul_item = AddValueOutput("soul_item", () => _out_soul_item);
	}

	private void GetSoulItem(Flow flow)
	{
		_wgo = WGOParamOrSelf(wgo);
		if (_wgo != null)
		{
			_out_soul_item = _wgo.GetItemOfType(ItemDefinition.ItemType.Soul);
		}
		@out.Call(flow);
	}
}
