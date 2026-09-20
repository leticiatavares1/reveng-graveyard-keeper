using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("True if wgo has equipped item of type")]
[Name("Has equiped item", 0)]
[Category("Game Actions")]
public class Flow_HasEquippedItem : MyFlowNode
{
	private Item _item;

	protected override void RegisterPorts()
	{
		AddValueInput<ItemDefinition.EquipmentType>("Equip type");
		AddValueOutput("item", () => _item);
		FlowOutput flow_yes = AddFlowOutput("Yes");
		FlowOutput flow_no = AddFlowOutput("No");
		AddFlowInput("In", delegate(Flow f)
		{
			_item = MainGame.me.player.GetEquippedItem(ItemDefinition.EquipmentType.FishingRod);
			if (_item == null || _item.IsEmpty())
			{
				flow_no.Call(f);
			}
			else
			{
				flow_yes.Call(f);
			}
		});
	}
}
