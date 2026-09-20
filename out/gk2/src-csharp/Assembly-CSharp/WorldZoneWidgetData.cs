using System.Collections.Generic;
using LazyBearTechnology;

public class WorldZoneWidgetData : LazyWidgetDataBase
{
	public WorldZoneData WorldZoneData { get; private set; }

	public bool InsideTown { get; private set; }

	public TownSubZone TownSubZone { get; private set; }

	public WorldZoneWidgetData()
	{
		WorldZoneData = MainGame.PlayerData.CurrentWorldZoneData;
		InsideTown = MainGame.PlayerData.insideTownZones.Count > 0;
		object townSubZone;
		if (MainGame.PlayerData.insideTownSubZones.Count <= 0)
		{
			townSubZone = null;
		}
		else
		{
			List<TownSubZone> insideTownSubZones = MainGame.PlayerData.insideTownSubZones;
			townSubZone = insideTownSubZones[insideTownSubZones.Count - 1];
		}
		TownSubZone = (TownSubZone)townSubZone;
	}

	public WorldZoneWidgetData(WorldZoneData worldZoneData)
		: this()
	{
		WorldZoneData = worldZoneData;
	}
}
