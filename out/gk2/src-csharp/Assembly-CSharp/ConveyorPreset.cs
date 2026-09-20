using System;
using System.Collections.Generic;
using System.IO;
using LazyBearTechnology;
using UnityEngine;

public class ConveyorPreset : ISerializableData
{
	public const string CONVEYOR_PRESETS_LOCAL_DIRECTORY = "/AddressableAssets/ConveyorPresets/";

	public const string CONVEYOR_PRESETS_ADDRESSABLES_FOLDER = "Assets/AddressableAssets/ConveyorPresets/";

	public const string CONVEYOR_PRESETS_ADDRESSABLES_LABEL = "ConveyorPresets";

	public string presetName;

	public List<ConveyorWgoData> wgoData = new List<ConveyorWgoData>();

	public List<ZombieWgoData> workers = new List<ZombieWgoData>();

	public List<ZombieWgoData> carouselWorkers = new List<ZombieWgoData>();

	public List<WgoData> extensions = new List<WgoData>();

	public static void SaveLayoutToPreset(string presetName)
	{
		if (File.Exists(Application.dataPath + "/AddressableAssets/ConveyorPresets/" + presetName + ".bytes"))
		{
			File.Delete(Application.dataPath + "/AddressableAssets/ConveyorPresets/" + presetName + ".bytes");
		}
		ConveyorPreset conveyorPreset = new ConveyorPreset();
		conveyorPreset.presetName = presetName;
		foreach (ConveyorComponent conveyorComponent in MainGame.Instance.GameSave.conveyorSystemData.conveyorComponents)
		{
			ConveyorWgoData conveyorWgoData = conveyorComponent.WgoData;
			conveyorPreset.wgoData.Add(conveyorWgoData);
			if (conveyorWgoData.Worker != null)
			{
				conveyorPreset.workers.Add(conveyorWgoData.Worker as ZombieWgoData);
			}
			if (conveyorComponent is ConveyorPowerSourceComponent)
			{
				List<DockPointData> dockPoints = conveyorWgoData.MainWgoPartData.GetDockPoints(DockPointData.Availability.OnlyOccupied, DockPointData.Filter.OnlyZombie);
				if (dockPoints.Count > 0)
				{
					foreach (DockPointData item in dockPoints)
					{
						SGuid occupiedBy = item.OccupiedBy;
						ZombieWgoData zombie = MainGame.Instance.GameSave.zombieSystemData.GetZombie(occupiedBy);
						conveyorPreset.carouselWorkers.Add(zombie);
					}
				}
			}
			foreach (SGuid attachedWorkbenchExtension in conveyorComponent.WgoData.AttachedWorkbenchExtensions)
			{
				conveyorPreset.extensions.Add(MainGame.WorldData.GetWgoData(attachedWorkbenchExtension));
			}
		}
		ConveyorSaveSystem.SaveConveyorPreset(conveyorPreset, Application.dataPath + "/AddressableAssets/ConveyorPresets/", presetName);
	}

	public static void LoadPreset(string presetName)
	{
		MainGame.Instance.conveyorSystem.IsPaused = true;
		MainGame.Instance.conveyorSystem.isReconstructLocked = true;
		MainGame.Instance.conveyorSystem.Clear();
		ConveyorSaveSystem.LoadConveyorPreset("Assets/AddressableAssets/ConveyorPresets/", presetName, ApplyPreset);
	}

	private static void ApplyPreset(ConveyorPreset preset)
	{
		Dictionary<Guid, ZombieWgoData> cache = MainGame.Instance.GameSave.zombieSystemData.Cache;
		List<SGuid> zombieOnSceneWgoIds = MainGame.Instance.GameSave.zombieSystemData.zombieOnSceneWgoIds;
		WorldData worldData = MainGame.Instance.GameSave.worldData;
		List<ConveyorComponent> conveyorComponents = MainGame.ConveyorSystemData.conveyorComponents;
		foreach (ZombieWgoData worker in preset.workers)
		{
			zombieOnSceneWgoIds.Add(worker.UniqueId);
			cache.Add(worker.UniqueId.Guid, worker);
			worker.PrepareForGame();
			worldData.AddWgoData(worker, recheckVisibilityOnSpawn: true);
		}
		if (preset.carouselWorkers != null)
		{
			foreach (ZombieWgoData carouselWorker in preset.carouselWorkers)
			{
				zombieOnSceneWgoIds.Add(carouselWorker.UniqueId);
				cache.Add(carouselWorker.UniqueId.Guid, carouselWorker);
				carouselWorker.PrepareForGame();
				worldData.AddWgoData(carouselWorker, recheckVisibilityOnSpawn: true);
			}
		}
		foreach (ConveyorWgoData wgoDatum in preset.wgoData)
		{
			conveyorComponents.Add(wgoDatum.ConveyorComponent);
			wgoDatum.PrepareForGame();
			worldData.AddWgoData(wgoDatum, recheckVisibilityOnSpawn: true);
		}
		if (preset.extensions != null)
		{
			foreach (WgoData extension in preset.extensions)
			{
				extension.PrepareForGame();
				worldData.AddWgoData(extension, recheckVisibilityOnSpawn: true);
			}
		}
		foreach (ZombieWgoData worker2 in preset.workers)
		{
			worker2.PrepareForGame();
		}
		MainGame.Instance.conveyorSystem.isReconstructLocked = false;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterSerialize()
	{
	}
}
