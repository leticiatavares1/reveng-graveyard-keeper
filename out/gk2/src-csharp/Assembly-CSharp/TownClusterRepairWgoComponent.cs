using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TownClusterRepairWgoComponent : IComponent
{
	[SerializeField]
	private int id;

	[SerializeField]
	private List<TownClusterData.TownClusterDataDto> configurations = new List<TownClusterData.TownClusterDataDto>();

	public int Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public List<TownClusterData.TownClusterDataDto> Configurations
	{
		get
		{
			return configurations;
		}
		set
		{
			configurations = value;
		}
	}

	public void DoRepairLogic()
	{
		string currentGameSceneId = MainGame.PlayerData.currentGameSceneId;
		if (string.IsNullOrEmpty(currentGameSceneId))
		{
			Debug.LogWarning("[TownClusterRepairWgoComponent] Current game scene id is empty.");
			return;
		}
		foreach (TownClusterData.TownClusterDataDto configuration in configurations)
		{
			foreach (SGuid destroyedWgoUniqueId in configuration.destroyedWgoUniqueIds)
			{
				MainGame.WorldData.RemoveWgoDataFromGameScene(destroyedWgoUniqueId);
			}
			switch (configuration.handleDestroyedWsoMode)
			{
			case TownCluster.HandleDestroyedWsoMode.Destroy:
				foreach (SGuid destroyedWsoUniqueId in configuration.destroyedWsoUniqueIds)
				{
					MainGame.WorldData.RemoveWsoDataFromGameScene(destroyedWsoUniqueId);
				}
				break;
			case TownCluster.HandleDestroyedWsoMode.HouseRepair:
				foreach (SGuid destroyedWsoUniqueId2 in configuration.destroyedWsoUniqueIds)
				{
					TownUtils.RepairHouse(MainGame.WorldData.GetWsoData(destroyedWsoUniqueId2));
				}
				break;
			}
		}
		foreach (TownClusterData.TownClusterDataDto configuration2 in configurations)
		{
			foreach (TownClusterData.ClusterWgoData repairedWgoDatum in configuration2.repairedWgoData)
			{
				string wgoId = repairedWgoDatum.wgoId;
				if (!string.IsNullOrEmpty(wgoId))
				{
					MainGame.WorldData.AddWgoData(new WgoData(wgoId, repairedWgoDatum.position, currentGameSceneId));
				}
			}
			foreach (TownClusterData.ClusterWgoData repairedWsoDatum in configuration2.repairedWsoData)
			{
				string wgoId2 = repairedWsoDatum.wgoId;
				if (!string.IsNullOrEmpty(wgoId2))
				{
					MainGame.WorldData.AddWsoData(wgoId2, repairedWsoDatum.position, currentGameSceneId, out var _);
				}
			}
		}
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.RepairTownCluster);
	}
}
