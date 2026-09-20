using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TownBuildingWgoComponent : IComponent
{
	[SerializeField]
	private TownBuildingSceneConfiguration sceneConfiguration;

	[SerializeField]
	private int tierIndex;

	[SerializeField]
	private string townBuildingId;

	[SerializeField]
	private bool isActive;

	public TownBuildingSceneConfiguration SceneConfiguration
	{
		get
		{
			return sceneConfiguration;
		}
		set
		{
			sceneConfiguration = value;
		}
	}

	public int TierIndex
	{
		get
		{
			return tierIndex;
		}
		set
		{
			tierIndex = value;
		}
	}

	public string TownBuildingId
	{
		get
		{
			return townBuildingId;
		}
		set
		{
			townBuildingId = value;
			isActive = !string.IsNullOrEmpty(value);
		}
	}

	public TownBuildingDef TownBuildingDef => GameBalance.Me.GetData<TownBuildingDef>(townBuildingId);

	public bool HasLevelUp => !string.IsNullOrEmpty(TownBuildingDef.lvlUpId);

	public bool HasVendor => !string.IsNullOrEmpty(TownBuildingDef.vendorId);

	public bool IsActive => isActive;

	public List<TownBuildingDef> GetAvailableBuildings(WgoData wgoData)
	{
		List<TownBuildingDef> list = new List<TownBuildingDef>();
		foreach (TownBuildingDef townBuildingDef in GameBalance.Me.townBuildingDefs)
		{
			if ((!townBuildingDef.isNeedsUnlock || MainGame.Instance.GameSave.knowledgeSystem.unlockedTownBuildings.Contains(townBuildingDef.id)) && !MainGame.Instance.GameSave.knowledgeSystem.lockedTownBuildings.Contains(townBuildingDef.id) && townBuildingDef.craftsIn.Contains(wgoData.id))
			{
				list.Add(townBuildingDef);
			}
		}
		return list;
	}
}
