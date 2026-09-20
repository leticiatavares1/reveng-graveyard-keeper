using System;
using System.Collections.Generic;
using LazyBearTechnology;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class MilitaryBaseData
{
	[Serializable]
	public class MilitaryBaseFightBuilding
	{
		public Vector3 basePosition;

		public SGuid uniqueId;

		public string variationId;

		public int rotationIndex = -1;

		public MilitaryBaseFightBuilding(Vector3 position, SGuid uniqueId, string variationId = null, int rotationIndex = -1)
		{
			basePosition = position;
			this.uniqueId = uniqueId;
			this.variationId = variationId;
			this.rotationIndex = rotationIndex;
		}
	}

	[Serializable]
	public class MilitaryBaseFighter
	{
		public SGuid containerParent;

		public SGuid uniqueId;

		public int dockPointIndex;

		public Vector2 baseDirection;

		public MilitaryBaseFighter(SGuid containerParent, SGuid uniqueId, int dockPointIndex, Vector2 baseDirection)
		{
			this.containerParent = containerParent;
			this.uniqueId = uniqueId;
			this.dockPointIndex = dockPointIndex;
			this.baseDirection = baseDirection;
		}
	}

	private const string MILITARY_BASE_SCENE_ID = "RuinedTemple";

	private const string BASE_POSTFIX = "_pre";

	private const string FIGHT_POSTFIX = "_fight";

	private const string FIGHTER_CONTAINER_PLACE_POSTFIX = "_place";

	public List<SGuid> baseBuildings = new List<SGuid>();

	public List<MilitaryBaseFightBuilding> fightBuildings = new List<MilitaryBaseFightBuilding>();

	public List<SGuid> fighterContainers = new List<SGuid>();

	public List<SGuid> fighterContainersSelectedForFight = new List<SGuid>();

	public List<MilitaryBaseFighter> fighters = new List<MilitaryBaseFighter>();

	[SerializeField]
	private bool isMercenaryPayed;

	[SerializeField]
	private SGuid fighterContainerMercenary;

	[OdinSerialize]
	private HashSet<SGuid> fightingBuildingsSpawnedNotFromBase = new HashSet<SGuid>();

	[SerializeField]
	private string mercenariesPaymentId;

	public bool IsMaxFighterContainers => fighterContainers.Count >= 4;

	public SGuid FighterContainerMercenary
	{
		get
		{
			if (isMercenaryPayed)
			{
				return fighterContainerMercenary;
			}
			return null;
		}
	}

	public bool IsMercenaryPayed
	{
		get
		{
			return isMercenaryPayed;
		}
		set
		{
			isMercenaryPayed = value;
			if (isMercenaryPayed)
			{
				MainGame.Instance.GameSave.WorldData.GetWorldZoneDataById("town_guard_barracks")?.NotifyWgoDataChanged();
			}
		}
	}

	public string MercenariesPaymentId
	{
		get
		{
			return mercenariesPaymentId;
		}
		set
		{
			mercenariesPaymentId = value;
		}
	}

	public void PrepareForGame()
	{
		if (fighterContainerMercenary != null && !fighterContainerMercenary.IsEmpty)
		{
			return;
		}
		WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData("fighter_container_mercenary");
		if (string.IsNullOrEmpty(mercenariesPaymentId))
		{
			mercenariesPaymentId = GameBalance.Me.mercenariesDefs[0].id;
		}
		if (wgoData == null)
		{
			Debug.LogError("Military Base Data: can not find mercenary fighter container");
			return;
		}
		WgoData wgoData2 = null;
		for (int i = 1; i <= 4; i++)
		{
			string text = "npc_town_barracks_mercenary" + $"_{i}";
			wgoData2 = MainGame.Instance.GameSave.worldData.GetWgoData(text);
			if (wgoData2 == null)
			{
				Debug.LogError("Military Base Data: can not find mercenary fighter with id " + text);
				return;
			}
			DockPointData nearestDockPointData = wgoData.GetNearestDockPointData(wgoData2.Position);
			if (nearestDockPointData == null)
			{
				Debug.LogError("Military Base Data: no available dock point for mercenary fighter [" + text + "] in container [" + wgoData.id + "]");
			}
			else
			{
				nearestDockPointData.Occupy(wgoData2.UniqueId);
				wgoData2.takenDockPointsParentSGuid = wgoData.UniqueId;
			}
		}
		fighterContainerMercenary = wgoData.UniqueId;
	}

	public void AddFightBuilding(WgoData building)
	{
		LazySingleton<FightingGameController>.Instance.BaseDefenseAgentsController.AddWgoAsAgent(GameScene.GetWgoViewGlobal(building.UniqueId), null, snapToNavmesh: false);
		string text = building.id.Replace("_fight", "_pre");
		foreach (SGuid baseBuilding in baseBuildings)
		{
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(baseBuilding);
			if (wgoData.id == text)
			{
				baseBuildings.Remove(baseBuilding);
				fightBuildings.Add(new MilitaryBaseFightBuilding(wgoData.Position, building.UniqueId, wgoData.MainWgoPartData?.variationId, wgoData.MainWgoPartData?.rotationIndex ?? (-1)));
				MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(baseBuilding);
				return;
			}
		}
		fightBuildings.Add(new MilitaryBaseFightBuilding(building.Position, building.UniqueId));
		if (fightingBuildingsSpawnedNotFromBase == null)
		{
			fightingBuildingsSpawnedNotFromBase = new HashSet<SGuid>();
		}
		fightingBuildingsSpawnedNotFromBase.Add(building.UniqueId);
	}

	public void RemoveFightBuilding(WgoData building)
	{
		LazySingleton<FightingGameController>.Instance.BaseDefenseAgentsController.RemoveAgent(building.UniqueId);
		if (fightingBuildingsSpawnedNotFromBase != null && fightingBuildingsSpawnedNotFromBase.Remove(building.UniqueId))
		{
			return;
		}
		MilitaryBaseFightBuilding militaryBaseFightBuilding = fightBuildings.Find((MilitaryBaseFightBuilding x) => x.uniqueId == building.UniqueId);
		if (militaryBaseFightBuilding == null)
		{
			Debug.LogError($"Military Base Data: can not find fight building [{building.UniqueId}] to return to base");
			return;
		}
		WgoData wgoData = new WgoData(building.id.Replace("_fight", "_pre"), militaryBaseFightBuilding.basePosition, "RuinedTemple");
		if (!string.IsNullOrEmpty(militaryBaseFightBuilding.variationId) || militaryBaseFightBuilding.rotationIndex != -1)
		{
			wgoData.MainWgoPartData.variationId = militaryBaseFightBuilding.variationId;
			wgoData.MainWgoPartData.rotationIndex = militaryBaseFightBuilding.rotationIndex;
		}
		MainGame.Instance.GameSave.worldData.AddWgoData(wgoData);
		baseBuildings.Add(wgoData.UniqueId);
	}

	public List<Inventory> CreateBaseBuildingsInventory()
	{
		Inventory inventory = Inventory.Create(99);
		foreach (SGuid baseBuilding in baseBuildings)
		{
			string id = MainGame.Instance.GameSave.worldData.GetWgoData(baseBuilding).id;
			inventory.AddItemToInventory(new Item(id));
		}
		return new List<Inventory> { inventory };
	}

	public void RemoveFighterFromContainer(WgoData fighter)
	{
		WgoData wgoData = MainGame.WorldData.GetWgoData(fighter.takenDockPointsParentSGuid);
		int occupiedDockPointIndex = wgoData.MainWgoPartData.GetOccupiedDockPointIndex(fighter.UniqueId);
		DockPointData occupiedDockPointBy = wgoData.MainWgoPartData.GetOccupiedDockPointBy(fighter.UniqueId);
		MilitaryBaseFighter item = new MilitaryBaseFighter(wgoData.UniqueId, fighter.UniqueId, occupiedDockPointIndex, fighter.direction.Value);
		fighter.takenDockPointsParentSGuid = null;
		occupiedDockPointBy.UnOccupy();
		fighters.Add(item);
		MainGame.WorldData.RemoveWgoDataFromGameScene(fighter);
		fighter.IsInteractable = false;
	}

	public void ReturnFighterToContainer(WgoData fighter)
	{
		MainGame.WorldData.RemoveWgoDataFromGameScene(fighter);
		MilitaryBaseFighter militaryBaseFighter = fighters.Find((MilitaryBaseFighter x) => x.uniqueId == fighter.UniqueId);
		WgoData wgoData = MainGame.WorldData.GetWgoData(militaryBaseFighter.containerParent);
		DockPointData dockPointByIndex = wgoData.MainWgoPartData.GetDockPointByIndex(militaryBaseFighter.dockPointIndex);
		dockPointByIndex.Occupy(fighter.UniqueId);
		fighter.WorldId = "RuinedTemple";
		fighter.HpComponent.RestoreFullHp();
		fighter.Position = wgoData.GetDockPointDataWorldPosition(dockPointByIndex);
		fighter.takenDockPointsParentSGuid = militaryBaseFighter.containerParent;
		fighter.direction.Value = militaryBaseFighter.baseDirection;
		MainGame.WorldData.AddWgoData(fighter);
		fighter.PrepareForGame();
		fighters.Remove(militaryBaseFighter);
		fighter.IsInteractable = true;
	}

	public void AddBaseBuilding(WgoData building)
	{
		if (!baseBuildings.Contains(building.UniqueId))
		{
			baseBuildings.Add(building.UniqueId);
		}
	}

	public void RemoveBaseBuilding(WgoData building)
	{
		if (baseBuildings.Contains(building.UniqueId))
		{
			baseBuildings.Remove(building.UniqueId);
		}
	}

	public void AddFighterContainer(SGuid fightingPlace)
	{
		if (!fighterContainers.Contains(fightingPlace))
		{
			fighterContainers.Add(fightingPlace);
		}
	}

	public void UpgradeFighterContainers()
	{
		if (!IsMaxFighterContainers)
		{
			int num = fighterContainers.Count + 1;
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData("fighter_container" + string.Format("{0}_{1}", "_place", num));
			if (wgoData == null)
			{
				Debug.LogError("Military Base Data: can not find upgradable fighter container");
				return;
			}
			MainGame.WorldData.ChangeWgoData(wgoData, "fighter_container");
			AddFighterContainer(wgoData.UniqueId);
			MainGame.WorldData.GetWgoData(SGuid.Parse(wgoData.GameResStr.Get("fighters_flag"))).IsHidden = false;
		}
	}

	public void ReturnFightBuildingsToBase()
	{
		foreach (MilitaryBaseFightBuilding fightBuilding in fightBuildings)
		{
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(fightBuilding.uniqueId);
			if (wgoData != null)
			{
				RemoveFightBuilding(wgoData);
				MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(wgoData);
			}
		}
		fightBuildings.Clear();
	}

	public int GetSquadSlotIndex(SGuid containerId)
	{
		if (SGuid.IsNullOrEmpty(containerId))
		{
			return -1;
		}
		if (containerId == fighterContainerMercenary)
		{
			return 0;
		}
		int num = fighterContainers.IndexOf(containerId);
		if (num < 0)
		{
			return -1;
		}
		return num + 1;
	}

	public bool ContainsFighter(WgoData wgoData, bool checkMercenaries = false)
	{
		if (SGuid.IsNullOrEmpty(wgoData.takenDockPointsParentSGuid))
		{
			return false;
		}
		foreach (SGuid fighterContainer in fighterContainers)
		{
			if (wgoData.takenDockPointsParentSGuid == fighterContainer)
			{
				return true;
			}
		}
		if (checkMercenaries && wgoData.takenDockPointsParentSGuid == fighterContainerMercenary)
		{
			return true;
		}
		return false;
	}
}
