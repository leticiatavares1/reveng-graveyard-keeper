using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Add Item", 0)]
[Category("Game/Item")]
[Color("FFFFFF")]
public class Flow_AddItem : GKCustomFlowNodeWithWgoData
{
	public enum ItemGiveType
	{
		Add,
		Remove
	}

	[GatherPortsCallback]
	public bool addToPlayer;

	[GatherPortsCallback]
	public ItemGiveType giveType;

	[GatherPortsCallback]
	public bool instanceAsInput;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WgoData> wgo;

	private ValueInput<string> itemId;

	private ValueInput<Item> item;

	private ValueInput<int> itemCount;

	private ValueOutput<string> itemNameOut;

	public override string name => giveType.ToString() + " Item " + ((giveType == ItemGiveType.Add) ? "to " : "from ") + (addToPlayer ? "Player" : "WgoData");

	protected override void RegisterPorts()
	{
		if (!addToPlayer)
		{
			base.RegisterPorts();
		}
		@in = AddFlowInput("in".CapitalizeFirst(), ChangeGameRes);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (!instanceAsInput)
		{
			itemId = AddValueInput<string>("itemId".CapitalizeFirst());
			itemCount = AddValueInput<int>("itemCount".CapitalizeFirst());
		}
		else
		{
			item = AddValueInput<Item>("item".CapitalizeFirst());
		}
		itemNameOut = AddValueOutput("itemId", () => itemId.value);
	}

	private void ChangeGameRes(Flow flow)
	{
		Item item = (instanceAsInput ? this.item.value : new Item(itemId.value, itemCount.value));
		if (addToPlayer)
		{
			switch (giveType)
			{
			case ItemGiveType.Add:
				MainGame.PlayerData.inventory.AddItemToInventory(item);
				break;
			case ItemGiveType.Remove:
				MainGame.PlayerData.inventory.RemoveItemById(item.id, item.Count);
				break;
			}
		}
		else
		{
			WgoData wgoData = GetWgoData();
			if (wgoData != null)
			{
				switch (giveType)
				{
				case ItemGiveType.Add:
					wgoData.Inventory.AddItemToInventory(item);
					break;
				case ItemGiveType.Remove:
					wgoData.Inventory.RemoveItemById(item.id, item.Count);
					break;
				}
			}
			else
			{
				Debug.LogError(string.Format("{0}: Tried to {1} Item to null WGO", "Flow_AddItem", giveType));
			}
		}
		@out.Call(flow);
	}
}
