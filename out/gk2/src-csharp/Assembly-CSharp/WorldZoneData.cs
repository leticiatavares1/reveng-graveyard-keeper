using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class WorldZoneData : ObjectLinkedToDefinition<WorldZoneDef>
{
	public enum WorldZoneType
	{
		Default,
		SimpleNotContainer
	}

	public string gameSceneId;

	public string contentPartName;

	public Vector3 pos;

	public List<SGuid> wgoDataList = new List<SGuid>();

	public List<SGuid> customQualityWgoDataList = new List<SGuid>();

	[NonSerialized]
	public List<PlayerData> playerDataList = new List<PlayerData>();

	public LazyConsts.Navigation.Graph navigationGraph = LazyConsts.Navigation.Graph.None;

	public List<LazyConsts.Navigation.Graph> additionalMovementGraphs = new List<LazyConsts.Navigation.Graph>();

	public Rect wholeZoneRect;

	public List<WorldZoneElevationAreaBakedData> elevationAreas = new List<WorldZoneElevationAreaBakedData>();

	public List<WorldZoneNavigationHoleBakedData> navigationHoles = new List<WorldZoneNavigationHoleBakedData>();

	public int additionalQuality;

	public WorldZoneType worldZoneType;

	public int processingPriority;

	[NonSerialized]
	private bool isActive = true;

	[SerializeField]
	private int maxReachedQuality;

	[SerializeField]
	private List<Rect> customQualityZonesRectList = new List<Rect>();

	[SerializeField]
	private Rect customQualityZonesRoughRect;

	[SerializeField]
	private List<OrderBase> orders = new List<OrderBase>();

	private List<LazyConsts.Navigation.Graph> movementGraphs = new List<LazyConsts.Navigation.Graph>();

	private List<WgoData> multiInventoryWgoDatas;

	public List<WgoData> MultiInventoryWgoDatas => multiInventoryWgoDatas;

	public List<Rect> CustomQualityZonesRectList => customQualityZonesRectList;

	public Vector3 Center => new Vector3(wholeZoneRect.center.x, pos.y, wholeZoneRect.center.y);

	public bool IsContainer => worldZoneType == WorldZoneType.Default;

	public List<LazyConsts.Navigation.Graph> MovementGraphs => movementGraphs;

	public bool IsActive
	{
		get
		{
			return isActive;
		}
		set
		{
			if (isActive != value)
			{
				isActive = value;
				this.OnActiveStateChanged?.Invoke(isActive);
			}
		}
	}

	public int AdditionalQuality
	{
		get
		{
			return additionalQuality;
		}
		set
		{
			if (additionalQuality != value)
			{
				additionalQuality = value;
				PlayerData playerData = MainGame.PlayerData;
				if (playerData.CurrentWorldZoneData != null && playerData.CurrentWorldZoneData.Definition.id == id)
				{
					GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
				}
				else
				{
					GetTotalQuality();
				}
			}
			TryCallOnMaxQualityChangedExpressions();
		}
	}

	public event Action OnWgoDataChanged;

	public event Action<WgoData> OnWgoDataAdded;

	public event Action<WgoData> OnWgoDataRemoved;

	public event Action<WgoData> OnWgoDataToCustomQualityAdded;

	public event Action<WgoData> OnWgoDataFromCustomQualityRemoved;

	public event Action<OrderBase> OnOrderAdded;

	public event Action<OrderBase> OnOrderRemoved;

	public event Action<bool> OnActiveStateChanged;

	public WorldZoneData()
	{
	}

	public WorldZoneData(string id, string gameSceneId, Vector3 pos, Rect wholeZoneRect)
		: base(id)
	{
		this.gameSceneId = gameSceneId;
		this.pos = pos;
		this.wholeZoneRect = wholeZoneRect;
	}

	public WorldZoneData(WorldZoneData other)
		: base(other.id)
	{
		gameSceneId = other.gameSceneId;
		contentPartName = other.contentPartName;
		pos = other.pos;
		worldZoneType = other.worldZoneType;
		navigationGraph = other.navigationGraph;
		additionalMovementGraphs = other.additionalMovementGraphs;
		wholeZoneRect = other.wholeZoneRect;
		elevationAreas = ((other.elevationAreas != null) ? new List<WorldZoneElevationAreaBakedData>(other.elevationAreas) : new List<WorldZoneElevationAreaBakedData>());
		navigationHoles = ((other.navigationHoles != null) ? new List<WorldZoneNavigationHoleBakedData>(other.navigationHoles) : new List<WorldZoneNavigationHoleBakedData>());
		isActive = other.isActive;
	}

	public static WorldZoneData CreateFromBaked(WorldZoneBakedData baked, string gameSceneId, Vector3 offset)
	{
		if (baked == null)
		{
			return null;
		}
		Rect rect = baked.wholeZoneRect;
		rect.center += offset.XZ2();
		WorldZoneData worldZoneData = new WorldZoneData(baked.id, gameSceneId, baked.pos + offset, rect)
		{
			contentPartName = baked.contentPartName,
			worldZoneType = baked.worldZoneType,
			navigationGraph = baked.navigationGraph,
			additionalMovementGraphs = baked.additionalMovementGraphs,
			processingPriority = baked.processingPriority
		};
		if (baked.elevationAreas != null && baked.elevationAreas.Count > 0)
		{
			worldZoneData.elevationAreas = new List<WorldZoneElevationAreaBakedData>(baked.elevationAreas.Count);
			Vector2 vector = offset.XZ2();
			for (int i = 0; i < baked.elevationAreas.Count; i++)
			{
				WorldZoneElevationAreaBakedData worldZoneElevationAreaBakedData = baked.elevationAreas[i];
				Rect xzRect = worldZoneElevationAreaBakedData.xzRect;
				xzRect.center += vector;
				worldZoneData.elevationAreas.Add(new WorldZoneElevationAreaBakedData
				{
					xzRect = xzRect,
					elevationY = worldZoneElevationAreaBakedData.elevationY
				});
			}
		}
		if (baked.navigationHoles != null && baked.navigationHoles.Count > 0)
		{
			worldZoneData.navigationHoles = new List<WorldZoneNavigationHoleBakedData>(baked.navigationHoles.Count);
			for (int j = 0; j < baked.navigationHoles.Count; j++)
			{
				worldZoneData.navigationHoles.Add(baked.navigationHoles[j].WithOffset(offset));
			}
		}
		return worldZoneData;
	}

	public bool TryGetBuildElevationY(float x, float z, out float elevationY)
	{
		Vector2 xz = new Vector2(x, z);
		float num = float.MinValue;
		bool flag = false;
		if (elevationAreas != null)
		{
			for (int i = 0; i < elevationAreas.Count; i++)
			{
				WorldZoneElevationAreaBakedData worldZoneElevationAreaBakedData = elevationAreas[i];
				if (worldZoneElevationAreaBakedData.ContainsXZ(xz) && (!flag || worldZoneElevationAreaBakedData.elevationY > num))
				{
					num = worldZoneElevationAreaBakedData.elevationY;
					flag = true;
				}
			}
		}
		elevationY = num;
		return flag;
	}

	public void Init(BoxCollider zoneCollider)
	{
		if (zoneCollider == null)
		{
			Debug.LogError("ZoneCollider is null, that won't be");
			return;
		}
		EnsureCollectionsInitialized();
		Vector3 vector = zoneCollider.transform.TransformPoint(zoneCollider.center);
		Vector3 size = zoneCollider.size;
		wholeZoneRect = new Rect(new Vector2(vector.x - size.x / 2f, vector.z - size.z / 2f), new Vector2(size.x, size.z));
		movementGraphs.Clear();
		movementGraphs.Add(navigationGraph);
		movementGraphs.AddRange(additionalMovementGraphs);
	}

	public void PrepareForGame()
	{
		EnsureCollectionsInitialized();
		isActive = true;
		if (!IsContainer)
		{
			wgoDataList.Clear();
			customQualityWgoDataList.Clear();
			multiInventoryWgoDatas?.Clear();
			return;
		}
		if (navigationGraph != LazyConsts.Navigation.Graph.None)
		{
			LazySingleton<GlobalNavigationManager>.Instance.InitRecastGraph(navigationGraph, Center, wholeZoneRect.size, dontUseRecastFloor: false, 1f, navigationHoles);
		}
		foreach (SGuid wgoData3 in wgoDataList)
		{
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(wgoData3);
			if (wgoData != null)
			{
				WGODef wGODef = wgoData.Definition;
				if (wGODef != null && wGODef.forceSetNavigationHoleType == WGODef.ForceSetNavigationHoleType.InsideWorldZone)
				{
					AddCutUnitByBakedData(wgoData);
				}
			}
		}
		if (multiInventoryWgoDatas != null && (multiInventoryWgoDatas.Count != 0 || wgoDataList.Count <= 0))
		{
			return;
		}
		multiInventoryWgoDatas = new List<WgoData>();
		foreach (SGuid wgoData4 in wgoDataList)
		{
			WgoData wgoData2 = MainGame.Instance.GameSave.worldData.GetWgoData(wgoData4);
			if (wgoData2 != null)
			{
				WGODef wGODef2 = wgoData2.Definition;
				if (wGODef2 != null && wGODef2.OpenInMultiInventory)
				{
					multiInventoryWgoDatas.Add(wgoData2);
				}
			}
		}
	}

	public void NotifyWgoDataChanged()
	{
		this.OnWgoDataChanged?.Invoke();
	}

	public bool TryAddWgoData(WgoData wgoData)
	{
		if (!IsContainer || !IsActive)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		Vector2 point = new Vector2(wgoData.Position.x, wgoData.Position.z);
		if (wholeZoneRect.Contains(point))
		{
			wgoDataList.Add(wgoData.UniqueId);
			if (multiInventoryWgoDatas != null && wgoData.Definition.OpenInMultiInventory)
			{
				multiInventoryWgoDatas.Add(wgoData);
			}
			this.OnWgoDataAdded?.Invoke(wgoData);
			wgoData.WorldZoneData = this;
			flag = true;
			if (wgoData.Definition != null && wgoData.Definition.forceSetNavigationHoleType != 0)
			{
				AddCutUnitByBakedData(wgoData);
			}
			if (GardenBedNavigation.IsGardenPlot(wgoData))
			{
				GardenBedNavigation.TryRebuild(wgoData);
			}
		}
		if (base.Definition != null && base.Definition.hasCustomQualityZones && ContainsPointCustomQualityZonesRoughly(point) && ContainsCustomQualityZonePrecisely(wgoData.Position))
		{
			customQualityWgoDataList.Add(wgoData.UniqueId);
			flag2 = true;
		}
		TryCallOnMaxQualityChangedExpressions();
		if (flag)
		{
			this.OnWgoDataChanged?.Invoke();
		}
		if (flag2)
		{
			this.OnWgoDataToCustomQualityAdded?.Invoke(wgoData);
		}
		return flag;
	}

	public List<WgoData> GetWgoDataByRect(Rect rect)
	{
		List<WgoData> list = new List<WgoData>();
		for (int i = 0; i < wgoDataList.Count; i++)
		{
			SGuid sGuid = wgoDataList[i];
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(sGuid);
			if (wgoData != null && wgoData?.MainWgoPartData != null)
			{
				Rect collisionBoundsRect = wgoData.MainWgoPartData.GetCollisionBoundsRect(wgoData.Position);
				if (rect.Overlaps(collisionBoundsRect, allowInverse: true))
				{
					list.Add(wgoData);
				}
			}
		}
		return list;
	}

	private void AddCutUnitByBakedData(WgoData wgoData)
	{
		if (wgoData?.MainWgoPartData != null)
		{
			int stateHash = wgoData.MainWgoPartData.GetStateHash();
			if (wgoData.MainWgoPartData.BakedData.TryGetGraphUpdateSceneBoxData(stateHash, out var data))
			{
				LazySingleton<GlobalNavigationManager>.Instance.AddGraphSceneUpdateUnit(wgoData.UniqueId, navigationGraph, wgoData.Position, data);
			}
			if (wgoData.MainWgoPartData.BakedData.TryGetPlannerMeshData(stateHash, out var data2))
			{
				LazySingleton<GlobalNavigationManager>.Instance.AddCutUnit(wgoData.UniqueId, navigationGraph, wgoData.Position, data2);
			}
			else
			{
				LazySingleton<GlobalNavigationManager>.Instance.AddCutUnit(wgoData.UniqueId, navigationGraph, wgoData.MainWgoPartData.GetCollisionBoundsRect(wgoData.Position), wgoData.Position.y);
			}
			wgoData.BeginCustomNavMeshCutTracking();
		}
	}

	public void RemoveWgoData(WgoData wgoData)
	{
		if (wgoDataList.Remove(wgoData.UniqueId))
		{
			LazySingleton<GlobalNavigationManager>.Instance.RemoveCutUnit(wgoData.UniqueId);
			wgoData.StopCustomNavMeshCutTracking();
			if (customQualityWgoDataList.Remove(wgoData.UniqueId))
			{
				this.OnWgoDataFromCustomQualityRemoved?.Invoke(wgoData);
			}
			wgoData.WorldZoneData = null;
			this.OnWgoDataRemoved?.Invoke(wgoData);
			this.OnWgoDataChanged?.Invoke();
			if (wgoData.Definition.OpenInMultiInventory)
			{
				multiInventoryWgoDatas.Remove(wgoData);
			}
		}
		TryCallOnMaxQualityChangedExpressions();
	}

	public void AddPlayerData(PlayerData playerData)
	{
		playerDataList.Add(playerData);
		foreach (LazyExpression onEnterExpression in base.Definition.onEnterExpressions)
		{
			onEnterExpression.Evaluate();
		}
	}

	public void RemovePlayerData(PlayerData playerData)
	{
		playerDataList.Remove(playerData);
		foreach (LazyExpression onExitExpression in base.Definition.onExitExpressions)
		{
			onExitExpression.Evaluate();
		}
	}

	public float GetTotalQuality()
	{
		if (!IsContainer)
		{
			return 0f;
		}
		float num = 0f;
		foreach (SGuid item in (!base.Definition.hasCustomQualityZones) ? wgoDataList : customQualityWgoDataList)
		{
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(item);
			if (wgoData != null)
			{
				num += wgoData.Quality;
			}
		}
		float num2 = num + (float)additionalQuality;
		if (MainGame.Instance.gameState == MainGame.GameState.InGame && MainGame.PlayerData != null)
		{
			MainGame.PlayerData.SetResWithoutSystemsCheck("wz_" + id, num2);
		}
		TryUnlockGraveyardQualityAchievement(num2);
		return num2;
	}

	public int CountItemsOnTownPalettes(string itemId)
	{
		int num = 0;
		for (int i = 0; i < wgoDataList.Count; i++)
		{
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(wgoDataList[i]);
			if (wgoData != null && wgoData.Definition.interactionType == WGODef.InteractionType.TownPalette)
			{
				num += wgoData.Inventory.Data.GetTotalCountInInventory(itemId);
			}
		}
		return num;
	}

	public float GetTotalQuality(WorldZoneWgoQualityType qualityType)
	{
		if (!IsContainer)
		{
			return 0f;
		}
		float num = 0f;
		foreach (SGuid item in (!base.Definition.hasCustomQualityZones) ? wgoDataList : customQualityWgoDataList)
		{
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(item);
			if (wgoData == null)
			{
				continue;
			}
			switch (qualityType)
			{
			case WorldZoneWgoQualityType.Any:
				num += wgoData.Quality;
				break;
			case WorldZoneWgoQualityType.ConveyorPowerSource:
				if (wgoData.Quality >= 0f)
				{
					num += wgoData.Quality;
				}
				break;
			case WorldZoneWgoQualityType.ConveyorCells:
				if (wgoData.Quality < 0f)
				{
					num += Mathf.Abs(wgoData.Quality);
				}
				break;
			}
		}
		return num;
	}

	public void AddCustomQualityRect(Rect rect)
	{
		if (!base.Definition.hasCustomQualityZones)
		{
			return;
		}
		bool flag = false;
		customQualityZonesRectList.Add(rect);
		Debug.Log($"WorldZoneData.AddRect: added rect {rect}");
		GameSceneData gameSceneDataById = MainGame.Instance.GameSave.worldData.GetGameSceneDataById(gameSceneId);
		if (gameSceneDataById == null)
		{
			return;
		}
		foreach (WgoData wgoData in gameSceneDataById.wgoDataList)
		{
			Vector2 point = new Vector2(wgoData.Position.x, wgoData.Position.z);
			if (rect.Contains(point))
			{
				customQualityWgoDataList.Add(wgoData.UniqueId);
				this.OnWgoDataToCustomQualityAdded?.Invoke(wgoData);
				Debug.Log("WorldZoneData.AddRect: added CustomQualityWgoData " + wgoData.id);
				flag = true;
			}
		}
		FormCustomQualityZonesRoughRect();
		if (flag)
		{
			this.OnWgoDataChanged?.Invoke();
			TryCallOnMaxQualityChangedExpressions();
		}
	}

	public bool RemoveCustomQualityRect(Rect rect)
	{
		if (!base.Definition.hasCustomQualityZones)
		{
			return false;
		}
		int num = customQualityZonesRectList.FindIndex((Rect r) => AreRectsEqual(r, rect));
		if (num < 0)
		{
			return false;
		}
		customQualityZonesRectList.RemoveAt(num);
		RebuildCustomQualityWgoDataList();
		this.OnWgoDataChanged?.Invoke();
		TryCallOnMaxQualityChangedExpressions();
		Debug.Log($"WorldZoneData.RemoveRect: removed rect {rect}");
		return true;
	}

	public string GetQualityString(TextStyle customQualityStyle = null)
	{
		if (base.Definition == null)
		{
			Debug.LogError("WorldZone [" + id + "] Definition is null");
			return string.Empty;
		}
		string text = base.Definition.stringFormat.Replace("@", base.Definition.qualityIcon.FontIcon());
		Regex regex = new Regex("^(.*?)\\{\\$([a-zA-Z0-9_]+):([^\\}]+)\\}(.*?)$");
		while (true)
		{
			Match match = regex.Match(text);
			if (!match.Success)
			{
				break;
			}
			string type = match.Groups[2].Captures[0].ToString();
			text = match.Groups[1].Captures[0]?.ToString() + string.Format("{0:" + match.Groups[3].Captures[0]?.ToString() + "}", MainGame.PlayerController.PlayerData.GetRes(type)) + match.Groups[4].Captures[0];
		}
		Match match2 = new Regex("(.*)%([a-zA-Z_]+)(.*)").Match(text);
		if (match2.Success)
		{
			WorldZoneData worldZoneDataById = MainGame.Instance.GameSave.WorldData.GetWorldZoneDataById(match2.Groups[2].Captures[0].ToString());
			if (worldZoneDataById != null)
			{
				text = match2.Groups[1].Captures[0]?.ToString() + worldZoneDataById.GetTotalQuality() + match2.Groups[3].Captures[0];
			}
		}
		Regex regex2 = new Regex("(.*)LE\\{([^\\}]+)\\}(.*)");
		while (true)
		{
			Match match3 = regex2.Match(text);
			if (!match3.Success)
			{
				break;
			}
			LazyExpression lazyExpression = LazyExpressionBase.ParseExpression<LazyExpression>(match3.Groups[2].Captures[0].ToString());
			text = match3.Groups[1].Captures[0]?.ToString() + lazyExpression.EvaluateFloat(this) + match3.Groups[3].Captures[0];
		}
		text = string.Format(text, GetTotalQuality());
		if (customQualityStyle != null)
		{
			text = customQualityStyle.ApplyStyleToString(text);
		}
		return text;
	}

	public bool CanDeliveryOrderBeTakenOnExecution(DeliveryOrder deliveryOrder, int currentCount = 0)
	{
		int num = 0;
		foreach (WgoData multiInventoryWgoData in multiInventoryWgoDatas)
		{
			if (multiInventoryWgoData.Definition.inventoryWhiteList.Contains(deliveryOrder.Item.Definition) && !multiInventoryWgoData.Definition.inventoryBlackList.Contains(deliveryOrder.Item.Definition))
			{
				num += multiInventoryWgoData.Inventory.Data.GetTotalCountInInventory(deliveryOrder.Item.id);
				if (num >= deliveryOrder.Item.Count - currentCount)
				{
					return true;
				}
			}
		}
		return num >= deliveryOrder.Item.Count - currentCount;
	}

	public OrderBase GetOrderForCaretaker(Item executorCurrentItem = null)
	{
		if (orders == null || orders.Count == 0)
		{
			return null;
		}
		OrderBase result = null;
		int num = int.MinValue;
		foreach (OrderBase order in orders)
		{
			if (order.ExecutorUniqueId.IsEmpty && !(order is ConveyorPickupOrder) && (!(order is DeliveryOrder deliveryOrder) || CanDeliveryOrderBeTakenOnExecution(deliveryOrder, (executorCurrentItem != null) ? ((executorCurrentItem.id == deliveryOrder.Item.id) ? executorCurrentItem.Count : 0) : 0)))
			{
				int priority = order.GetPriority();
				if (priority > num)
				{
					num = priority;
					result = order;
				}
			}
		}
		return result;
	}

	public OrderBase GetOrderForConveyorTransporter()
	{
		if (orders == null || orders.Count == 0)
		{
			return null;
		}
		WorldZoneData worldZoneDataById = MainGame.WorldData.GetWorldZoneDataById("conveyor_storage");
		if (worldZoneDataById == null)
		{
			return null;
		}
		bool flag = false;
		foreach (WgoData multiInventoryWgoData in worldZoneDataById.MultiInventoryWgoDatas)
		{
			if (multiInventoryWgoData.Inventory.Data.InventoryFillSize < multiInventoryWgoData.Inventory.Data.InventorySize)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return null;
		}
		OrderBase result = null;
		int num = int.MinValue;
		foreach (OrderBase order in orders)
		{
			if (!order.ExecutorUniqueId.IsEmpty || !(order is ConveyorPickupOrder conveyorPickupOrder))
			{
				continue;
			}
			WgoData wgoData = MainGame.WorldData.GetWgoData(conveyorPickupOrder.TargetWgoUniqueId);
			if (wgoData != null && wgoData.Inventory.Data.GetTotalCountInInventory(conveyorPickupOrder.Item.id) >= conveyorPickupOrder.Item.Count)
			{
				int priority = order.GetPriority();
				if (priority > num)
				{
					num = priority;
					result = order;
				}
			}
		}
		return result;
	}

	public bool CanPlantOrderBeTakenOnExecution(PlantOrder plantOrder, out string enoughItemId, int currentCount = 0)
	{
		enoughItemId = string.Empty;
		WgoData wgoData = MainGame.WorldData.GetWgoData(plantOrder.TargetWgoUniqueId);
		if (wgoData == null)
		{
			return false;
		}
		if (wgoData.CraftComponent.IsStarted)
		{
			return true;
		}
		if (!plantOrder.isStarGroupItem)
		{
			return HasEnoughSeedsToPlant(plantOrder.Item, out enoughItemId, currentCount);
		}
		foreach (ItemDef item2 in GameBalance.Me.starGroupItemsCache[plantOrder.Item.id])
		{
			Item item = new Item(item2.id, plantOrder.Item.Count);
			if (HasEnoughSeedsToPlant(item, out enoughItemId, currentCount))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEnoughSeedsToPlant(Item item, out string seedItemId, int currentCount = 0)
	{
		seedItemId = string.Empty;
		int num = 0;
		foreach (WgoData multiInventoryWgoData in multiInventoryWgoDatas)
		{
			if (multiInventoryWgoData.Definition.inventoryWhiteList.Contains(item.Definition) && !multiInventoryWgoData.Definition.inventoryBlackList.Contains(item.Definition))
			{
				num += multiInventoryWgoData.Inventory.Data.GetTotalCountInInventory(item.id);
				if (num >= item.Count - currentCount)
				{
					seedItemId = item.id;
					return true;
				}
			}
		}
		if (num >= item.Count - currentCount)
		{
			seedItemId = item.id;
			return true;
		}
		return false;
	}

	public bool HasOtherFreeGardenerWithHigherMastery(ZombieWgoData currentGardener)
	{
		foreach (SGuid zombieOnSceneWgoId in MainGame.ZombieSystemData.zombieOnSceneWgoIds)
		{
			ZombieWgoData zombie = MainGame.ZombieSystemData.GetZombie(zombieOnSceneWgoId);
			if (zombie != null && zombie != currentGardener && zombie.WorldZoneData.id == currentGardener.WorldZoneData.id && zombie.ZombieType == ZombieType.Gardener && zombie.GardenerState == ZombieWgoData.ZombieGardenerState.OnStation && zombie.GetMasteryLevelForTalentBranch("talent_green") > currentGardener.GetMasteryLevelForTalentBranch("talent_green"))
			{
				return true;
			}
		}
		return false;
	}

	public OrderBase GetOrderForGardener(ZombieWgoData orderTaker, Type orderType = null, SGuid target = null)
	{
		if (orders == null || orders.Count == 0)
		{
			return null;
		}
		OrderBase result = null;
		int num = int.MinValue;
		bool flag = HasOtherFreeGardenerWithHigherMastery(orderTaker);
		List<SGuid> list = null;
		foreach (OrderBase order in orders)
		{
			if (!order.ExecutorUniqueId.IsEmpty || (!(order is GatherOrder) && !(order is PlantOrder)) || (orderType != null && order.GetType() != orderType) || (target != null && !order.TargetWgoUniqueId.Equals(target)))
			{
				continue;
			}
			string enoughItemId;
			if (MainGame.WorldData.GetWgoData(order.TargetWgoUniqueId) == null)
			{
				if (list == null)
				{
					list = new List<SGuid>();
				}
				list.Add(order.UniqueId);
			}
			else if ((!(order is GatherOrder) || !flag) && (!(order is PlantOrder plantOrder) || CanPlantOrderBeTakenOnExecution(plantOrder, out enoughItemId)))
			{
				int priority = order.GetPriority();
				if (priority > num)
				{
					num = priority;
					result = order;
				}
			}
		}
		if (list != null)
		{
			ClearOrders(list);
		}
		return result;
	}

	public void PlaceNewOrder(OrderBase orderBase)
	{
		orders.Add(orderBase);
		this.OnOrderAdded?.Invoke(orderBase);
	}

	public void RemoveOrder(SGuid orderUniqueId)
	{
		OrderBase orderBase = FindOrder(orderUniqueId);
		if (orderBase != null)
		{
			orders.Remove(orderBase);
			this.OnOrderRemoved?.Invoke(orderBase);
		}
	}

	public void RemoveOrdersByTarget(SGuid targetUniqueId)
	{
		if (orders == null || orders.Count == 0)
		{
			return;
		}
		foreach (OrderBase item in FindOrdersByTarget(targetUniqueId))
		{
			if (SGuid.IsNullOrEmpty(item.ExecutorUniqueId))
			{
				orders.Remove(item);
				this.OnOrderRemoved?.Invoke(item);
			}
		}
	}

	public void ClearOrders(List<SGuid> list)
	{
		foreach (SGuid item in list)
		{
			RemoveOrder(item);
		}
	}

	public OrderBase FindOrder(SGuid uniqueOrderId)
	{
		return orders.Find((OrderBase o) => o.UniqueId == uniqueOrderId);
	}

	public List<OrderBase> FindOrdersByTarget(SGuid targetUniqueId, Type orderType = null)
	{
		if (orderType != null)
		{
			return orders.FindAll((OrderBase o) => o.TargetWgoUniqueId == targetUniqueId && o.GetType() == orderType);
		}
		return orders.FindAll((OrderBase o) => o.TargetWgoUniqueId == targetUniqueId);
	}

	private void FormCustomQualityZonesRoughRect()
	{
		if (customQualityZonesRectList.Count == 0)
		{
			customQualityZonesRoughRect = Rect.zero;
			return;
		}
		Rect rect = customQualityZonesRectList[0];
		for (int i = 1; i < customQualityZonesRectList.Count; i++)
		{
			Rect rect2 = customQualityZonesRectList[i];
			float num = ((rect.xMin < rect2.xMin) ? rect.xMin : rect2.xMin);
			float num2 = ((rect.yMin < rect2.yMin) ? rect.yMin : rect2.yMin);
			float num3 = ((rect.xMax > rect2.xMax) ? rect.xMax : rect2.xMax);
			float num4 = ((rect.yMax > rect2.yMax) ? rect.yMax : rect2.yMax);
			float width = num3 - num;
			float height = num4 - num2;
			rect.Set(num, num2, width, height);
		}
		customQualityZonesRoughRect = rect;
	}

	private void RebuildCustomQualityWgoDataList()
	{
		HashSet<SGuid> hashSet = new HashSet<SGuid>(customQualityWgoDataList);
		customQualityWgoDataList.Clear();
		HashSet<SGuid> hashSet2 = new HashSet<SGuid>();
		for (int i = 0; i < wgoDataList.Count; i++)
		{
			SGuid sGuid = wgoDataList[i];
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(sGuid);
			if (wgoData != null && ContainsCustomQualityZonePrecisely(wgoData.Position))
			{
				customQualityWgoDataList.Add(sGuid);
				hashSet2.Add(sGuid);
			}
		}
		foreach (SGuid item in hashSet)
		{
			WgoData wgoData2 = MainGame.Instance.GameSave.worldData.GetWgoData(item);
			if (wgoData2 != null && !hashSet2.Contains(item))
			{
				this.OnWgoDataFromCustomQualityRemoved?.Invoke(wgoData2);
			}
		}
		FormCustomQualityZonesRoughRect();
	}

	private void EnsureCollectionsInitialized()
	{
		if (wgoDataList == null)
		{
			wgoDataList = new List<SGuid>();
		}
		if (customQualityWgoDataList == null)
		{
			customQualityWgoDataList = new List<SGuid>();
		}
		if (playerDataList == null)
		{
			playerDataList = new List<PlayerData>();
		}
		if (additionalMovementGraphs == null)
		{
			additionalMovementGraphs = new List<LazyConsts.Navigation.Graph>();
		}
		if (elevationAreas == null)
		{
			elevationAreas = new List<WorldZoneElevationAreaBakedData>();
		}
		if (navigationHoles == null)
		{
			navigationHoles = new List<WorldZoneNavigationHoleBakedData>();
		}
		if (customQualityZonesRectList == null)
		{
			customQualityZonesRectList = new List<Rect>();
		}
		if (orders == null)
		{
			orders = new List<OrderBase>();
		}
		if (movementGraphs == null)
		{
			movementGraphs = new List<LazyConsts.Navigation.Graph>();
		}
		if (multiInventoryWgoDatas == null)
		{
			multiInventoryWgoDatas = new List<WgoData>();
		}
	}

	private static bool AreRectsEqual(Rect a, Rect b)
	{
		if (Mathf.Abs(a.x - b.x) < 0.001f && Mathf.Abs(a.y - b.y) < 0.001f && Mathf.Abs(a.width - b.width) < 0.001f)
		{
			return Mathf.Abs(a.height - b.height) < 0.001f;
		}
		return false;
	}

	private bool ContainsPointCustomQualityZonesRoughly(Vector2 point)
	{
		if (customQualityZonesRectList.Count > 0)
		{
			return customQualityZonesRoughRect.Contains(point);
		}
		return false;
	}

	private void TryCallOnMaxQualityChangedExpressions()
	{
		int num = (int)GetTotalQuality() - maxReachedQuality;
		if (num <= 0)
		{
			return;
		}
		foreach (LazyExpression item in base.Definition.expressionsOnMaxQualityIncreased)
		{
			item.EvaluateValueDelta(num);
		}
		maxReachedQuality += num;
	}

	private void TryUnlockGraveyardQualityAchievement(float totalQuality)
	{
		if (id == "graveyard" && totalQuality >= 200f)
		{
			AchievementsSystem.Instance.Unlock("ach_graveyard_quality_200");
		}
	}

	public bool ContainsCustomQualityZonePrecisely(Vector3 point)
	{
		Vector2 point2 = new Vector2(point.x, point.z);
		foreach (Rect customQualityZonesRect in customQualityZonesRectList)
		{
			if (customQualityZonesRect.Contains(point2))
			{
				return true;
			}
		}
		return false;
	}
}
