using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class BuildingDef : BalanceBaseObject
{
	public enum BuildingMode
	{
		None,
		Place,
		Remove,
		ConveyorPlace,
		FightingPlace,
		FightBuilding,
		Script,
		Upgrade
	}

	public enum BuildAreaChoosingType
	{
		None = -1,
		Soft,
		Strict,
		FullCoverWithCount,
		FullCoverSoft
	}

	public string wgoId;

	public BuildingMode buildingMode;

	[AutoParse("instant_destroy")]
	public bool deleteInstantly;

	[AutoParse("tab")]
	public string tab;

	[AutoParse("start_craft")]
	public string startCraft = "destroy_basic";

	[AutoParse("is_needs_unlock")]
	public bool isNeedsUnlock;

	[AutoParse("custom_grid_step")]
	public int customGridStep = 1;

	public BuildAreaChoosingType chooseCustomBuildAreaType;

	[AutoParse("custom_build_area_id")]
	public string customBuildAreaId;

	[AutoParse("exclude_wgo_groups")]
	public List<string> excludeWgoGroups = new List<string>();

	[SerializeField]
	[AutoParse("icon_id")]
	private string buildResultIcon;

	[AutoParse("custom_wgo_p_preview")]
	public string customWgoPlacePreview;

	[AutoParse("builds_in")]
	public List<string> buildsIn = new List<string>();

	[AutoParse("need_item")]
	public List<NeedItemData> needItems = new List<NeedItemData>();

	[AutoParse("out_item")]
	public OutputItems outputItems = new OutputItems();

	[AutoParse("exec_expressions_after_building")]
	public List<LazyExpression> expressionAfterBuilding = new List<LazyExpression>();

	[AutoParse("current_limit_expression")]
	[LazyExpressionPureValueType(PureValueType.Float)]
	public LazyExpression currentLimitExpression = new LazyExpression();

	[AutoParse("limit_max")]
	public int limitMax;

	[AutoParse("always_blocked_by_wgo_groups")]
	public List<string> alwaysBlockedByWgoGroups = new List<string>();

	public int fullCoveringCount;

	public bool HasLimits => limitMax > 0;

	public string BuildResultIcon
	{
		get
		{
			if (!string.IsNullOrEmpty(buildResultIcon))
			{
				return buildResultIcon;
			}
			return "i_b_blueprint_placeholder";
		}
	}

	public bool IsAlwaysBlockedByWgoGroup(string wgoGroup)
	{
		if (!string.IsNullOrEmpty(wgoGroup) && alwaysBlockedByWgoGroups.Count > 0)
		{
			return alwaysBlockedByWgoGroups.Contains(wgoGroup);
		}
		return false;
	}

	public bool ShouldIgnoreWgoGroupAsObstacle(string wgoGroup)
	{
		if (IsAlwaysBlockedByWgoGroup(wgoGroup))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(wgoGroup))
		{
			return excludeWgoGroups.Contains(wgoGroup);
		}
		return false;
	}

	public string GetHeader()
	{
		return LLBase.L(id);
	}

	public string GetHeaderPrefix()
	{
		return LLBase.L("ui_blueprint");
	}

	public static List<BuildData> GetBuildingsInBuilder(Wgo builder)
	{
		bool flag = false;
		bool flag2 = false;
		if (builder != null && builder.Data.Definition.interactionType == WGODef.InteractionType.FightBuilder && LazySingleton<FightingGameController>.Instance.CurrentFightState != 0)
		{
			FightDef data = GameBalance.Me.GetData<FightDef>(LazySingleton<FightingGameController>.Instance.CurrentLevel.id);
			flag = data.isBarricadesUnavailable;
			flag2 = data.isTowersUnavailable;
		}
		List<BuildData> list = new List<BuildData>();
		foreach (BuildingDef item in GameBalance.Me.buildDefsInBuilder[builder.Id])
		{
			BuildingMode buildingMode = item.buildingMode;
			if (buildingMode != 0 && buildingMode != BuildingMode.Remove && (!item.isNeedsUnlock || MainGame.Instance.GameSave.knowledgeSystem.unlockedBuildings.Contains(item.id)) && !MainGame.Instance.GameSave.knowledgeSystem.lockedBuildings.Contains(item.id) && (!flag || !GameBalance.Me.HasWgoIdByGroup("barricades", item.wgoId)) && (!flag2 || !GameBalance.Me.HasWgoIdByGroup("towers", item.wgoId)))
			{
				list.Add(BuildData.GetDataForBuild(item));
			}
		}
		return list;
	}

	public string GetLimitsString()
	{
		return $"{currentLimitExpression.EvaluateInt()}/{limitMax}";
	}
}
