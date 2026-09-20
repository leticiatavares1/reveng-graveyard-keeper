using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class AlliesSpawn : MonoBehaviour
{
	public Transform flagStandPosition;

	public List<Transform> alliesSpawnPositions;

	private Wgo flagStand;

	private List<WgoData> fighters = new List<WgoData>();

	private Wgo flag;

	private AgentsGroupFlagController flagController;

	private bool isMercenarySpawn;

	private int squadSlotIndex = -1;

	public AgentsGroupFlagController FlagController => flagController;

	public IReadOnlyList<WgoData> Fighters => fighters;

	public int SquadSlotIndex => squadSlotIndex;

	public void SpawnFromContainer(WgoData fighterContainer)
	{
		isMercenarySpawn = fighterContainer.id == "fighter_container_mercenary";
		squadSlotIndex = MainGame.Instance.GameSave.militaryBaseData.GetSquadSlotIndex(fighterContainer.UniqueId);
		MainGame.Instance.GameSave.worldData.AddWgoData("flag_stand", flagStandPosition.position, MainGame.PlayerData.currentGameSceneId, "", out var wgoData);
		LazySingleton<FightingGameController>.Instance.AddTemporaryWgoData(wgoData);
		flagStand = GameScene.GetWgoViewGlobal(wgoData.UniqueId);
		flagStand.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
		FlagStandComponent componentInChildren = flagStand.GetComponentInChildren<FlagStandComponent>();
		MainGame.Instance.GameSave.worldData.AddWgoData("test_flag", componentInChildren.FlagPlacementPoint.transform.position, MainGame.PlayerData.currentGameSceneId, "", out var wgoData2);
		LazySingleton<FightingGameController>.Instance.AddTemporaryWgoData(wgoData2);
		wgoData2.ApplyWgoPartState(GameScene.GetWgoViewGlobal(SGuid.Parse(fighterContainer.GameResStr.Get("fighters_flag"))).Data.MainWgoPartData.variationId);
		flag = GameScene.GetWgoViewGlobal(wgoData2.UniqueId);
		flag.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
		flagController = flag.GetComponentInChildren<AgentsGroupFlagController>();
		AgentsGroupBehaviourController componentInChildren2 = flag.GetComponentInChildren<AgentsGroupBehaviourController>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.TargetTeam = LazyConsts.Fighting.TeamType.WildZombie;
		}
		flagController.Init();
		flagController.SetEnabled(isEnabled: true);
		List<DockPointData> dockPoints = fighterContainer.MainWgoPartData.GetDockPoints(DockPointData.Availability.OnlyOccupied);
		for (int i = 0; i < dockPoints.Count; i++)
		{
			if (isMercenarySpawn)
			{
				SpawnMercenary(dockPoints[i], alliesSpawnPositions[i]);
			}
			else
			{
				TrySpawnZombie(dockPoints[i], alliesSpawnPositions[i]);
			}
		}
		componentInChildren.AttachFlag(flag, bindCapturePoint: false);
		flagStand.Data.GameResStr.Set("flag_stand_sguid", flag.Data.UniqueId.ToString());
	}

	public void Clear()
	{
		foreach (WgoData fighter in fighters)
		{
			if (fighter != null)
			{
				if (isMercenarySpawn)
				{
					MainGame.Instance.GameSave.militaryBaseData.ReturnFighterToContainer(fighter);
				}
				else
				{
					MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(fighter);
				}
			}
		}
		fighters.Clear();
		if ((bool)flagController)
		{
			flagController.DeInit();
		}
		if ((bool)flag)
		{
			flag.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: false);
			MainGame.WorldData.RemoveWgoDataFromGameScene(flag.Data);
		}
		if ((bool)flagStand)
		{
			flagStand.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: false);
			MainGame.WorldData.RemoveWgoDataFromGameScene(flagStand.Data);
		}
		MainGame.Instance.GameSave.militaryBaseData.fighters.Clear();
		flagController = null;
		flag = null;
		flagStand = null;
		squadSlotIndex = -1;
	}

	private void SpawnMercenary(DockPointData occupiedDockPoint, Transform spawPosition)
	{
		WgoData wgoData = MainGame.WorldData.GetWgoData(occupiedDockPoint.OccupiedBy);
		MainGame.Instance.GameSave.militaryBaseData.RemoveFighterFromContainer(wgoData);
		wgoData.WorldId = MainGame.PlayerData.currentGameSceneId;
		wgoData.Position = spawPosition.position;
		MainGame.Instance.GameSave.worldData.AddWgoData(wgoData);
		wgoData.PrepareForGame();
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(wgoData.UniqueId);
		wgoViewGlobal.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
		FightingAgent fightingAgent = flagController.AgentsController.AddWgoAsAgent(wgoViewGlobal);
		if (!fightingAgent)
		{
			Debug.LogError($"Failed to initialize fighting agent for mercenary [{wgoViewGlobal.Data.UniqueId}]");
			return;
		}
		AssignCommonFighterWeapon(fightingAgent, wgoViewGlobal.Data);
		fightingAgent.FlagController = flagController;
		fighters.Add(wgoViewGlobal.Data);
	}

	private void TrySpawnZombie(DockPointData occupiedDockPoint, Transform spawPosition)
	{
		ZombieWgoData zombieWgoData = MainGame.WorldData.GetWgoData(occupiedDockPoint.OccupiedBy) as ZombieWgoData;
		if (IsSpawnableZombie(zombieWgoData))
		{
			ZombieWgoData zombieWgoData2 = zombieWgoData.CreateFighterFromThis(spawPosition.position, MainGame.PlayerData.currentGameSceneId);
			MainGame.Instance.GameSave.worldData.AddWgoData(zombieWgoData2);
			zombieWgoData2.PrepareForGame();
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(zombieWgoData2.UniqueId);
			wgoViewGlobal.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
			wgoViewGlobal.InitZombieFighter();
			FightingAgent fightingAgent = flagController.AgentsController.AddWgoAsAgent(wgoViewGlobal);
			if (!fightingAgent)
			{
				Debug.LogError($"Failed to initialize fighting agent for zombie [{wgoViewGlobal.Data.UniqueId}]");
				return;
			}
			AssignZombieWeapon(fightingAgent, zombieWgoData);
			fightingAgent.FlagController = flagController;
			fighters.Add(wgoViewGlobal.Data);
		}
	}

	private bool IsSpawnableZombie(ZombieWgoData zombieData)
	{
		Inventory inventory = new Inventory(zombieData.ZombieItem);
		Item itemByGroupId = inventory.GetItemByGroupId("weapon");
		Item itemByType = inventory.GetItemByType(ItemType.BodyArmor);
		if (!itemByGroupId.IsEmpty)
		{
			return !itemByType.IsEmpty;
		}
		return false;
	}

	private void AssignZombieWeapon(FightingAgent fightingAgent, ZombieWgoData zombieData)
	{
		Item itemByGroupId = new Inventory(zombieData.ZombieItem).GetItemByGroupId("weapon");
		ItemType type = itemByGroupId.Definition.type;
		if (type == ItemType.Bow || type == ItemType.Pike)
		{
			fightingAgent.AssignWeapon(itemByGroupId.Definition);
		}
	}

	private void AssignCommonFighterWeapon(FightingAgent fightingAgent, WgoData fighter)
	{
		Item itemByGroupId = fighter.Inventory.GetItemByGroupId("weapon");
		ItemType type = itemByGroupId.Definition.type;
		if (type == ItemType.Bow || type == ItemType.Pike)
		{
			fightingAgent.AssignWeapon(itemByGroupId.Definition);
		}
	}
}
