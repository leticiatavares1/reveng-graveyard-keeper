using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class TownBuildingDef : BalanceBaseObject
{
	[AutoParse("crafts_in")]
	public List<string> craftsIn = new List<string>();

	[AutoParse("building_type")]
	public TownBuildingType townBuildingType;

	[AutoParse("is_locked")]
	public bool isNeedsUnlock;

	[AutoParse("building_cost")]
	public List<NeedItemData> needItems = new List<NeedItemData>();

	[AutoParse("execute_on_building_finished")]
	public List<LazyExpression> onCraftEndExpressions = new List<LazyExpression>();

	[AutoParse("variation_id")]
	public string variationId;

	[AutoParse("icon_id")]
	public string iconId;

	[AutoParse("vendor_id")]
	public string vendorId;

	[AutoParse("lvl_up_building_id")]
	public string lvlUpId;

	[AutoParse("character_id")]
	public string characterId;

	[AutoParse("expression_on_char")]
	public List<LazyExpression> expressionOnCharCreate = new List<LazyExpression>();

	[AutoParse("dont_place_building_on_wgo")]
	public bool dontPlaceBuildingOnWgo;

	[AutoParse("upgrade_requirements")]
	public List<ExpressionGameRes> upgradeRequirements = new List<ExpressionGameRes>();

	[AutoParse("drop_items_on_building_finished")]
	public OutputItems dropItemsOnBuildingFinished = new OutputItems();

	public string BuildResultIcon
	{
		get
		{
			if (!string.IsNullOrEmpty(iconId))
			{
				return iconId;
			}
			return "i_b_blueprint_placeholder";
		}
	}

	public string GetHeader()
	{
		return LLBase.L(id);
	}

	public string GetHeaderPrefix()
	{
		return LLBase.L("ui_blueprint");
	}
}
