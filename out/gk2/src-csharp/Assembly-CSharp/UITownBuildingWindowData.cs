using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UITownBuildingWindowData : LazyWidgetDataBase
{
	public Wgo AssignedWgo { get; private set; }

	public List<TownBuildingDef> BuildsToDisplay { get; private set; }

	public Action<TownBuildingDef, List<NeedItemData>> OnBuildPressed { get; private set; }

	public PlayerData PlayerData { get; private set; }

	public UITownBuildingWindowData(Wgo wgo, PlayerData playerData, List<TownBuildingDef> buildsData, Action<TownBuildingDef, List<NeedItemData>> onBuildPressed)
	{
		AssignedWgo = wgo;
		PlayerData = playerData;
		BuildsToDisplay = buildsData;
		OnBuildPressed = onBuildPressed;
	}
}
