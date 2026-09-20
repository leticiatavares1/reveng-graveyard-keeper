using LazyBearTechnology;
using UnityEngine;

public static class CraftStatusIconHelper
{
	private const string NOT_ENOUGH_RESOURCES_ICON_NAME = "craft_status_not_enough_items";

	private const string NOT_ENOUGH_SPACE_IN_WGO_ICON_NAME = "craft_status_not_enough_space";

	private const string NO_EXTENSION_ICON_NAME = "craft_status_no_extension";

	private const string PLANTING_DIG_ICON_NAME = "icon_shovel_equipped";

	public static Sprite GetCraftStatusIcon(CraftStatus craftStatus, ItemType itemType = ItemType.None, string talentId = "")
	{
		Sprite sprite = null;
		return craftStatus switch
		{
			CraftStatus.NotEnoughResources => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("craft_status_not_enough_items"), 
			CraftStatus.NotEnoughSpaceInWgo => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("craft_status_not_enough_space"), 
			CraftStatus.DoesntHaveRequiredTool => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("icon_" + itemType.ToString().ToLower() + "_not_equipped"), 
			CraftStatus.NotEnoughMastery => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("icon_" + talentId + "_not_enough"), 
			CraftStatus.NotEnoughSpaceInMultiInventory => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("craft_status_not_enough_space"), 
			CraftStatus.NoExtension => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("craft_status_no_extension"), 
			_ => LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("craft_status_not_enough_items"), 
		};
	}

	public static Sprite GetPlantingDigStatusIcon()
	{
		return LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("icon_shovel_equipped");
	}
}
