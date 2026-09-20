using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UIBuildingWindowData : LazyWidgetDataBase
{
	public Wgo AssignedWgo { get; private set; }

	public Action<BuildData, List<NeedItemData>> OnBuildPressed { get; private set; }

	public Func<BuildData, List<NeedItemData>, bool> CanBuild { get; private set; }

	public PlayerData PlayerData { get; private set; }

	public List<Inventory> AdditionalInventories { get; private set; }

	public Dictionary<string, List<BuildData>> TabSortedBuilds { get; private set; }

	public UIBuildingWindowData(Wgo wgo, PlayerData playerData, List<BuildData> buildsData, Action<BuildData, List<NeedItemData>> onBuildPressed, Func<BuildData, List<NeedItemData>, bool> canBuild, List<Inventory> additionalInventories = null)
	{
		AssignedWgo = wgo;
		PlayerData = playerData;
		OnBuildPressed = onBuildPressed;
		CanBuild = canBuild;
		AdditionalInventories = additionalInventories;
		TabSortedBuilds = new Dictionary<string, List<BuildData>>();
		for (int i = 0; i < buildsData.Count; i++)
		{
			BuildData buildData = buildsData[i];
			if (buildData != null)
			{
				string key = (string.IsNullOrEmpty(buildData.Definition.tab) ? "tab_building_default" : buildData.Definition.tab);
				if (TabSortedBuilds.TryGetValue(key, out var value))
				{
					value.Add(buildData);
					continue;
				}
				TabSortedBuilds.Add(key, new List<BuildData> { buildData });
			}
		}
		if (AssignedWgo != null && AssignedWgo.Data.Definition.interactionType == WGODef.InteractionType.FightBuilder && LazySingleton<FightingGameController>.Instance.CurrentFightState == FightState.ActiveFight)
		{
			return;
		}
		foreach (KeyValuePair<string, List<BuildData>> tabSortedBuild in TabSortedBuilds)
		{
			tabSortedBuild.Value.Add(BuildData.GetDataForRemove());
		}
	}
}
