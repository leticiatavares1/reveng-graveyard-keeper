using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Create Item", 0)]
[Icon("CubeArrow", false, "")]
[Category("Game Functions")]
public class Flow_CreateItem : PureFunctionNode<Item, string, int>
{
	public override Item Invoke(string item_id, int value)
	{
		return new Item(item_id, value);
	}
}
