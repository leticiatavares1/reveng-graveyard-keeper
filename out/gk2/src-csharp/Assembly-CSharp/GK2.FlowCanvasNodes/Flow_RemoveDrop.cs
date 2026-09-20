using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Remove Drop", 0)]
[Category("Game/Item")]
[Description("Finds and removes an item dropped in the world by its reference.")]
[Color("FFFFFF")]
public class Flow_RemoveDrop : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<Item> item;

	private ValueInput<string> worldId;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Remove);
		@out = AddFlowOutput("out".CapitalizeFirst());
		item = AddValueInput<Item>("item");
		worldId = AddValueInput<string>("worldId");
	}

	private void Remove(Flow flow)
	{
		Item value = item.value;
		if (value != null && !value.IsEmpty)
		{
			string value2 = worldId.value;
			DropData dropData = null;
			foreach (GameSceneData gameSceneData in MainGame.Instance.GameSave.worldData.gameSceneDataList)
			{
				if (!string.IsNullOrEmpty(value2) && gameSceneData.id != value2)
				{
					continue;
				}
				foreach (DropData droppedItem in gameSceneData.droppedItems)
				{
					if (droppedItem.UniqueId == value.UniqueId)
					{
						dropData = droppedItem;
						break;
					}
				}
				if (dropData != null)
				{
					break;
				}
			}
			if (dropData != null)
			{
				MainGame.Instance.dropSystem.RemoveDrop(dropData, dropData.WorldId);
			}
		}
		@out.Call(flow);
	}
}
