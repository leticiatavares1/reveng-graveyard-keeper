using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UIPrefightWindowData : LazyWidgetDataBase
{
	public FightDef FightDefinition { get; private set; }

	public int BuildingsQuality { get; private set; }

	public List<WgoData> BuildingInZone { get; private set; }

	public List<UIPrefightSquadWidgetData> SquadWidgetDatas { get; private set; }

	public int CurrentSquadCount { get; set; }

	public int CurrentDefencePower { get; set; }

	public Action OnStartButtonPressed { get; set; }

	public Action OnPressedSquad { get; set; }

	public bool AllowClose { get; private set; }

	public bool ShowDefencePowerRequirement { get; private set; }

	public bool HasEnoughDefencePower
	{
		get
		{
			if (ShowDefencePowerRequirement)
			{
				return CurrentDefencePower >= FightDefinition.defencePowerLock;
			}
			return true;
		}
	}

	public UIPrefightWindowData(string fightId, Action onStartButtonPressed, bool allowClose = true)
	{
		AllowClose = allowClose;
		MilitaryBaseData militaryBaseData = MainGame.Instance.GameSave.militaryBaseData;
		FightDefinition = GameBalance.Me.GetData<FightDef>(fightId);
		ShowDefencePowerRequirement = FightDefinition.defencePowerLock > 0 && !IsFollowUpFight(fightId);
		OnStartButtonPressed = onStartButtonPressed;
		BuildingInZone = new List<WgoData>();
		for (int i = 0; i < militaryBaseData.baseBuildings.Count; i++)
		{
			WgoData wgoData = MainGame.WorldData.GetWgoData(militaryBaseData.baseBuildings[i]);
			BuildingDef dataOrNull = GameBalance.Me.GetDataOrNull<BuildingDef>(wgoData.id + "_fb");
			if (dataOrNull != null)
			{
				BuildingInZone.Add(wgoData);
				if ((!FightDefinition.isBarricadesUnavailable || !GameBalance.Me.HasWgoIdByGroup("barricades", dataOrNull.wgoId)) && (!FightDefinition.isTowersUnavailable || !GameBalance.Me.HasWgoIdByGroup("towers", dataOrNull.wgoId)))
				{
					BuildingsQuality += (int)wgoData.Quality;
				}
			}
		}
		CurrentDefencePower = BuildingsQuality;
		SquadWidgetDatas = new List<UIPrefightSquadWidgetData>();
		if (militaryBaseData.IsMercenaryPayed)
		{
			WgoData wgoData2 = MainGame.WorldData.GetWgoData(militaryBaseData.FighterContainerMercenary);
			SquadWidgetDatas.Add(new UIPrefightSquadWidgetData(wgoData2, isMercenary: true, isTurnedOn: true, canBeTurnedOn: false, null, null));
			CurrentSquadCount = 1;
		}
		else
		{
			SquadWidgetDatas.Add(new UIPrefightSquadWidgetData());
		}
		bool canBeTurnedOn = CurrentSquadCount < FightDefinition.squads;
		for (int j = 0; j < 5; j++)
		{
			if (j < militaryBaseData.fighterContainers.Count)
			{
				WgoData wgoData3 = MainGame.WorldData.GetWgoData(militaryBaseData.fighterContainers[j]);
				SquadWidgetDatas.Add(new UIPrefightSquadWidgetData(wgoData3, isMercenary: false, isTurnedOn: false, canBeTurnedOn, OnSquadPressedDefault, OnSquadPressedTurnedOn));
			}
			else
			{
				SquadWidgetDatas.Add(new UIPrefightSquadWidgetData());
			}
		}
		for (int k = 0; k < SquadWidgetDatas.Count; k++)
		{
			if (SquadWidgetDatas[k].IsTurnedOn)
			{
				CurrentDefencePower += SquadWidgetDatas[k].SquadPower;
			}
		}
	}

	private bool IsFollowUpFight(string fightId)
	{
		foreach (FightDef fightDefinition in GameBalance.Me.fightDefinitions)
		{
			if (string.Equals(fightDefinition.onWinNextFightId, fightId, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	private void OnSquadPressedDefault(UIPrefightSquadWidget squadWidget)
	{
		CurrentSquadCount++;
		squadWidget.Data.IsTurnedOn = true;
		squadWidget.Data.CanBeTurnedOn = false;
		squadWidget.Redraw();
		if (CurrentSquadCount >= FightDefinition.squads)
		{
			for (int i = 1; i < SquadWidgetDatas.Count; i++)
			{
				UIPrefightSquadWidgetData uIPrefightSquadWidgetData = SquadWidgetDatas[i];
				if (uIPrefightSquadWidgetData != squadWidget.Data && !uIPrefightSquadWidgetData.IsTurnedOn)
				{
					uIPrefightSquadWidgetData.CanBeTurnedOn = false;
					uIPrefightSquadWidgetData.OnRedraw?.Invoke();
				}
			}
		}
		CurrentDefencePower = BuildingsQuality;
		for (int j = 0; j < SquadWidgetDatas.Count; j++)
		{
			if (SquadWidgetDatas[j].IsTurnedOn)
			{
				CurrentDefencePower += SquadWidgetDatas[j].SquadPower;
			}
		}
		OnPressedSquad?.Invoke();
	}

	private void OnSquadPressedTurnedOn(UIPrefightSquadWidget squadWidget)
	{
		bool num = CurrentSquadCount >= FightDefinition.squads;
		CurrentSquadCount--;
		squadWidget.Data.IsTurnedOn = false;
		squadWidget.Data.CanBeTurnedOn = true;
		squadWidget.Redraw();
		if (num)
		{
			for (int i = 1; i < SquadWidgetDatas.Count; i++)
			{
				UIPrefightSquadWidgetData uIPrefightSquadWidgetData = SquadWidgetDatas[i];
				if (uIPrefightSquadWidgetData != squadWidget.Data && !uIPrefightSquadWidgetData.IsTurnedOn)
				{
					uIPrefightSquadWidgetData.CanBeTurnedOn = true;
					uIPrefightSquadWidgetData.OnRedraw?.Invoke();
				}
			}
		}
		CurrentDefencePower = BuildingsQuality;
		for (int j = 0; j < SquadWidgetDatas.Count; j++)
		{
			if (SquadWidgetDatas[j].IsTurnedOn)
			{
				CurrentDefencePower += SquadWidgetDatas[j].SquadPower;
			}
		}
		OnPressedSquad?.Invoke();
	}
}
