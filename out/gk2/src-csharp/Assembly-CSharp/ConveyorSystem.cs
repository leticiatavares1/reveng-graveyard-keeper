using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class ConveyorSystem : ICustomUpdatable
{
	public bool isReconstructLocked;

	private bool soundsArePlayingThisUpdate;

	private bool? lastHasEnoughPower;

	private bool zonePowerChangesSubscribed;

	private static ConveyorSystemData Data => MainGame.Instance.GameSave.conveyorSystemData;

	public WorldZoneData ConveyorWorldZone => MainGame.Instance.GameSave.worldData.GetWorldZoneDataById("conveyor");

	public bool HasEnoughPower
	{
		get
		{
			WorldZoneData conveyorWorldZone = ConveyorWorldZone;
			if (conveyorWorldZone == null)
			{
				return true;
			}
			return conveyorWorldZone.GetTotalQuality(WorldZoneWgoQualityType.ConveyorPowerSource) >= conveyorWorldZone.GetTotalQuality(WorldZoneWgoQualityType.ConveyorCells);
		}
	}

	public bool IsPaused
	{
		get
		{
			return Data.isPaused;
		}
		set
		{
			Debug.Log($"Set ConveyorSystem IsPaused to [{value}] ");
			Data.isPaused = value;
			if (soundsArePlayingThisUpdate && value)
			{
				LazySingleton<ConveyorSoundSystem>.Instance.PauseSounds();
			}
		}
	}

	public float UpdateInterval { get; private set; }

	public event Action OnUpdated;

	public void Init()
	{
		UpdateInterval = ConstDef.Get("conveyor_system_update_interval").FloatValue;
		SubscribeToZonePowerChanges();
	}

	public void CustomUpdate(float deltaTime)
	{
		RefreshNoPowerIconsIfNeeded();
		if (IsPaused)
		{
			return;
		}
		Data.timer += deltaTime;
		if (ConveyorWorldZone == null)
		{
			return;
		}
		for (int num = Data.workbenchElements.Count - 1; num >= 0; num--)
		{
			if (Data.workbenchElements[num].WgoData.CraftComponent.HasPreFinishUpdate)
			{
				Data.workbenchElements[num].WgoData.CraftComponent.PreFinishUpdate(deltaTime);
			}
		}
		if (!HasEnoughPower)
		{
			Data.timer = 0f;
			return;
		}
		for (int num2 = Data.zombieCraftActivities.Count - 1; num2 >= 0; num2--)
		{
			Data.zombieCraftActivities[num2].Update(deltaTime);
		}
		for (int num3 = Data.workbenchElements.Count - 1; num3 >= 0; num3--)
		{
			ConveyorComponent conveyorComponent = Data.workbenchElements[num3];
			if (conveyorComponent.WgoData.Definition.isAutoCrafter && conveyorComponent.WgoData.CraftComponent.HasCraftsInQueue && conveyorComponent.WgoData.CraftComponent.Status != CraftComponentStatus.WaitingForOutputDrop)
			{
				conveyorComponent.WgoData.CraftComponent.Update(deltaTime);
			}
		}
		if (!(Data.timer >= UpdateInterval))
		{
			return;
		}
		if (Data.graphEndElements.Count == 0)
		{
			ReconstructConveyorsCache();
		}
		foreach (ConveyorComponent conveyorComponent2 in Data.conveyorComponents)
		{
			conveyorComponent2.CurrentVisitState = VisitState.NotVisited;
			conveyorComponent2.wasPerformedItemTransfer = false;
			conveyorComponent2.ClearInAndOutItemDatas();
		}
		foreach (ConveyorWorkbenchComponent workbenchElement in Data.workbenchElements)
		{
			workbenchElement.DoJobIn();
		}
		foreach (ConveyorComponent graphEndElement in Data.graphEndElements)
		{
			if (graphEndElement.CurrentVisitState == VisitState.NotVisited)
			{
				graphEndElement.DoJob();
			}
		}
		foreach (ConveyorWorkbenchComponent workbenchElement2 in Data.workbenchElements)
		{
			workbenchElement2.DoJobOut();
		}
		foreach (ConveyorSplitterComponent splitterElement in Data.splitterElements)
		{
			splitterElement.SwitchDirection();
		}
		Data.timer = 0f;
		this.OnUpdated?.Invoke();
		LazySingleton<ConveyorSystemAnimationOrchestrator>.Instance.SetState("Out");
		soundsArePlayingThisUpdate = true;
		LazySingleton<ConveyorSoundSystem>.Instance.PauseSounds();
		LazySingleton<ConveyorSoundSystem>.Instance.PlaySounds();
	}

	public void AddConveyorObject(ConveyorComponent conveyorComponent)
	{
		if (!Data.conveyorComponents.Contains(conveyorComponent))
		{
			Data.conveyorComponents.Add(conveyorComponent);
			ReconstructConveyorsCache();
			RefreshNoPowerIconsIfNeeded();
		}
	}

	public void RemoveConveyorObject(ConveyorComponent conveyorComponent)
	{
		Data.conveyorComponents.Remove(conveyorComponent);
		ReconstructConveyorsCache();
		RefreshNoPowerIconsIfNeeded();
	}

	public void AddWorker(ZombieCraftActivity craftActivity)
	{
		if (TryGetCraftActivity(craftActivity.Zombie) == null)
		{
			Data.zombieCraftActivities.Add(craftActivity);
		}
	}

	public void RemoveWorker(ZombieCraftActivity craftActivity)
	{
		ZombieCraftActivity zombieCraftActivity = TryGetCraftActivity(craftActivity.Zombie);
		if (zombieCraftActivity != null)
		{
			Data.zombieCraftActivities.Remove(zombieCraftActivity);
		}
	}

	public ZombieCraftActivity TryGetCraftActivity(ZombieWgoData zombieWgoData)
	{
		foreach (ZombieCraftActivity zombieCraftActivity in Data.zombieCraftActivities)
		{
			if (zombieCraftActivity.Zombie.UniqueId.Guid == zombieWgoData.UniqueId.Guid)
			{
				return zombieCraftActivity;
			}
		}
		return null;
	}

	public void Clear()
	{
		for (int num = Data.conveyorComponents.Count - 1; num >= 0; num--)
		{
			ConveyorComponent conveyorComponent = Data.conveyorComponents[num];
			if (conveyorComponent.WgoData.GetGameResInt("conveyor_build_is_not_removable") > 0)
			{
				continue;
			}
			for (int num2 = conveyorComponent.WgoData.AttachedWorkbenchExtensions.Count - 1; num2 >= 0; num2--)
			{
				MainGame.WorldData.RemoveWgoDataFromGameScene(conveyorComponent.WgoData.AttachedWorkbenchExtensions[num2]);
			}
			MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(conveyorComponent.WgoData);
			if (conveyorComponent.WgoData.Worker != null)
			{
				if (!(conveyorComponent.WgoData.Worker is ZombieWgoData zombieWgoData))
				{
					continue;
				}
				MainGame.WorldData.RemoveWgoDataFromGameScene(zombieWgoData.UniqueId);
				MainGame.Instance.GameSave.zombieSystemData.zombieOnSceneWgoIds.Remove(zombieWgoData.UniqueId);
				MainGame.Instance.GameSave.zombieSystemData.Cache.Remove(zombieWgoData.UniqueId.Guid);
			}
			if (!(conveyorComponent is ConveyorPowerSourceComponent))
			{
				continue;
			}
			List<DockPointData> dockPoints = conveyorComponent.WgoData.MainWgoPartData.GetDockPoints(DockPointData.Availability.OnlyOccupied, DockPointData.Filter.OnlyZombie);
			if (dockPoints.Count <= 0)
			{
				continue;
			}
			foreach (DockPointData item in dockPoints)
			{
				SGuid occupiedBy = item.OccupiedBy;
				ZombieWgoData zombie = MainGame.Instance.GameSave.zombieSystemData.GetZombie(occupiedBy);
				MainGame.WorldData.RemoveWgoDataFromGameScene(zombie.UniqueId);
				MainGame.Instance.GameSave.zombieSystemData.zombieOnSceneWgoIds.Remove(zombie.UniqueId);
				MainGame.Instance.GameSave.zombieSystemData.Cache.Remove(zombie.UniqueId.Guid);
			}
		}
		Data.conveyorComponents.Clear();
		Data.workbenchElements.Clear();
		Data.splitterElements.Clear();
		Data.zombieCraftActivities.Clear();
		Data.graphStartElements.Clear();
		Data.graphEndElements.Clear();
		LazySingleton<ConveyorSoundSystem>.Instance.PauseSounds();
	}

	private void UpdateStartElements()
	{
		Data.graphStartElements.Clear();
		foreach (ConveyorComponent conveyorComponent in Data.conveyorComponents)
		{
			if (conveyorComponent.ParentsData.Count == 0)
			{
				Data.graphStartElements.Add(conveyorComponent);
			}
		}
	}

	private void UpdateEndElements()
	{
		Data.graphEndElements.Clear();
		foreach (ConveyorComponent conveyorComponent in Data.conveyorComponents)
		{
			conveyorComponent.CurrentVisitState = VisitState.NotVisited;
			conveyorComponent.wasPerformedItemTransfer = false;
		}
		foreach (ConveyorComponent graphStartElement in Data.graphStartElements)
		{
			if (graphStartElement.CurrentVisitState == VisitState.NotVisited)
			{
				graphStartElement.GetEndElement(ref Data.graphEndElements);
			}
		}
		Data.graphEndElements.Reverse();
	}

	private void UpdateElements()
	{
		Data.workbenchElements.Clear();
		Data.splitterElements.Clear();
		foreach (ConveyorComponent conveyorComponent in Data.conveyorComponents)
		{
			if (conveyorComponent is ConveyorWorkbenchComponent item)
			{
				Data.workbenchElements.Add(item);
			}
			else if (conveyorComponent is ConveyorSplitterComponent item2)
			{
				Data.splitterElements.Add(item2);
			}
		}
	}

	private void ReconstructConveyorsCache()
	{
		if (!isReconstructLocked)
		{
			UpdateStartElements();
			UpdateEndElements();
			UpdateElements();
		}
	}

	private void SubscribeToZonePowerChanges()
	{
		if (!zonePowerChangesSubscribed)
		{
			WorldZoneData conveyorWorldZone = ConveyorWorldZone;
			if (conveyorWorldZone != null)
			{
				conveyorWorldZone.OnWgoDataAdded += OnConveyorZoneMembershipChanged;
				conveyorWorldZone.OnWgoDataRemoved += OnConveyorZoneMembershipChanged;
				conveyorWorldZone.OnWgoDataChanged += OnConveyorZonePowerPossiblyChanged;
				conveyorWorldZone.OnWgoDataToCustomQualityAdded += OnConveyorZoneMembershipChanged;
				conveyorWorldZone.OnWgoDataFromCustomQualityRemoved += OnConveyorZoneMembershipChanged;
				zonePowerChangesSubscribed = true;
			}
		}
	}

	private void OnConveyorZoneMembershipChanged(WgoData _)
	{
		RefreshNoPowerIconsIfNeeded();
	}

	private void OnConveyorZonePowerPossiblyChanged()
	{
		RefreshNoPowerIconsIfNeeded();
	}

	private void RefreshNoPowerIconsIfNeeded()
	{
		SubscribeToZonePowerChanges();
		if (ConveyorWorldZone != null)
		{
			bool hasEnoughPower = HasEnoughPower;
			if (lastHasEnoughPower != hasEnoughPower)
			{
				lastHasEnoughPower = hasEnoughPower;
				RedrawConveyorWidgets();
			}
		}
	}

	private static void RedrawConveyorWidgets()
	{
		List<ConveyorComponent> conveyorComponents = Data.conveyorComponents;
		for (int i = 0; i < conveyorComponents.Count; i++)
		{
			ConveyorComponent conveyorComponent = conveyorComponents[i];
			if (conveyorComponent?.WgoData != null)
			{
				GameScene.GetWgoViewGlobal(conveyorComponent.WgoData.UniqueId)?.DrawWidgets();
			}
		}
	}
}
