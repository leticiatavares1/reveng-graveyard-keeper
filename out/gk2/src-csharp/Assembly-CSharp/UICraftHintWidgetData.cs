using LazyBearTechnology;

public class UICraftHintWidgetData : LazyWidgetDataBase
{
	public SGuid wgoUniqueId;

	public CraftComponent CraftComponent { get; private set; }

	public CraftElementBase CraftElement => CraftComponent.CurrentCraftElement;

	public IWorker Worker => CraftComponent.CraftableObject.CraftableAttachedWorker;

	public bool IsPlantingCraft { get; private set; }

	public bool IsWgoUnderInteraction
	{
		get
		{
			if (!(MainGame.PlayerController.PlayerInteractionComponent.WgoUnderInteraction?.Data.UniqueId == wgoUniqueId))
			{
				return MainGame.PlayerController.PlayerWorkComponent.Wgo?.Data?.UniqueId == wgoUniqueId;
			}
			return true;
		}
	}

	public UICraftHintWidgetData(CraftComponent craftComponent)
	{
		CraftComponent = craftComponent;
		IsPlantingCraft = CraftElement.ParamsData.craftParamsType == CraftParamsData.CraftParamsType.GardenPlanting;
	}
}
