using LazyBearTechnology;
using UnityEngine;

public class UISquadHudWidgetData : LazyWidgetDataBase
{
	public AlliesSpawn AlliesSpawn { get; private set; }

	public Sprite Banner { get; private set; }

	public UISquadHudWidgetData(AlliesSpawn alliesSpawn, Sprite banner)
	{
		AlliesSpawn = alliesSpawn;
		Banner = banner;
	}
}
