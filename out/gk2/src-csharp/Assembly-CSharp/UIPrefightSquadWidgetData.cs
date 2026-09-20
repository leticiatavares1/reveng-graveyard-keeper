using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UIPrefightSquadWidgetData : LazyWidgetDataBase
{
	public WgoData WgoData { get; private set; }

	public int SquadPower { get; private set; }

	public List<ItemType> FightersWeapons { get; private set; }

	public List<ItemType> FightersArmors { get; private set; }

	public Action<UIPrefightSquadWidget> OnPressedDefault { get; private set; }

	public Action<UIPrefightSquadWidget> OnPressedTurnedOn { get; private set; }

	public bool IsMercenary { get; private set; }

	public bool HasAnyFighterInSquad { get; private set; }

	public bool IsTurnedOn { get; set; }

	public bool CanBeTurnedOn { get; set; }

	public Action OnRedraw { get; set; }

	public UIPrefightSquadWidgetData()
	{
		WgoData = null;
		SquadPower = 0;
		FightersWeapons = new List<ItemType>();
		FightersArmors = new List<ItemType>();
		OnPressedDefault = null;
		OnPressedTurnedOn = null;
		IsTurnedOn = false;
		CanBeTurnedOn = false;
	}

	public UIPrefightSquadWidgetData(WgoData wgoData, bool isMercenary, bool isTurnedOn, bool canBeTurnedOn, Action<UIPrefightSquadWidget> onPressedDefault, Action<UIPrefightSquadWidget> onPressedTurnedOn)
	{
		WgoData = wgoData;
		OnPressedDefault = onPressedDefault;
		OnPressedTurnedOn = onPressedTurnedOn;
		IsTurnedOn = isTurnedOn;
		IsMercenary = isMercenary;
		CanBeTurnedOn = canBeTurnedOn;
		FightersWeapons = new List<ItemType>();
		FightersArmors = new List<ItemType>();
		for (int i = 0; i < WgoData.MainWgoPartData.DockPointsCount; i++)
		{
			DockPointData dockPointData = WgoData.MainWgoPartData.DockPointDataList[i];
			if (!dockPointData.IsOccupied)
			{
				continue;
			}
			bool flag = false;
			bool flag2 = false;
			if (isMercenary)
			{
				WgoData wgoData2 = MainGame.WorldData.GetWgoData(dockPointData.OccupiedBy);
				SquadPower += (int)wgoData2.Quality;
				Item itemByType = wgoData2.Inventory.GetItemByType(ItemType.Pike);
				Item itemByType2 = wgoData2.Inventory.GetItemByType(ItemType.Bow);
				if (!itemByType.IsEmpty)
				{
					FightersWeapons.Add(ItemType.Pike);
					flag2 = true;
				}
				else if (!itemByType2.IsEmpty)
				{
					FightersWeapons.Add(ItemType.Bow);
					flag2 = true;
				}
				else
				{
					FightersWeapons.Add(ItemType.None);
				}
				Item itemByType3 = wgoData2.Inventory.GetItemByType(ItemType.BodyArmor);
				if (!itemByType3.IsEmpty)
				{
					FightersArmors.Add(itemByType3.Definition.type);
					flag = true;
				}
				else
				{
					FightersArmors.Add(ItemType.None);
				}
			}
			else
			{
				ZombieWgoData zombie = MainGame.ZombieSystemData.GetZombie(dockPointData.OccupiedBy);
				Item hand = zombie.Hand;
				Item armor = zombie.Armor;
				if ((!hand.IsEmpty && hand.Definition.type == ItemType.Pike) || hand.Definition.type == ItemType.Bow)
				{
					flag2 = true;
				}
				if (!armor.IsEmpty && armor.Definition.type == ItemType.BodyArmor)
				{
					flag = true;
				}
				if (flag && flag2)
				{
					SquadPower += hand.Definition.quality + armor.Definition.quality;
					FightersWeapons.Add(hand.Definition.type);
					FightersArmors.Add(armor.Definition.type);
				}
				else
				{
					FightersWeapons.Add(ItemType.None);
					FightersArmors.Add(ItemType.None);
				}
			}
			if (flag2 && flag)
			{
				HasAnyFighterInSquad = true;
			}
		}
		if (isMercenary)
		{
			IsTurnedOn = true;
			HasAnyFighterInSquad = true;
		}
	}
}
