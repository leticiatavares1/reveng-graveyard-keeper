using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class WorldZone : MonoBehaviour
{
	private const string WORLD_ZONE_PREFAB_PATH = "Assets/AddressableAssets/WorldZones";

	[SerializeField]
	private string id;

	[SerializeField]
	private WorldZoneData.WorldZoneType worldZoneType;

	[SerializeField]
	private BoxCollider zoneCollider;

	[SerializeField]
	[Tooltip("Baked data asset - created/updated during content bake")]
	private WorldZoneBakedData bakedData;

	[SerializeField]
	[Tooltip("Links this WorldZone with Astar")]
	private LazyConsts.Navigation.Graph navigationGraph = LazyConsts.Navigation.Graph.None;

	[SerializeField]
	private List<LazyConsts.Navigation.Graph> additionalMovementGraphs = new List<LazyConsts.Navigation.Graph>();

	[SerializeField]
	[Tooltip("Box colliders baked into WorldZoneBakedData as unwalkable Recast holes. Only BoxCollider is supported for now. Runtime scene objects are not required after bake.")]
	private List<Collider> navigationHoleColliders = new List<Collider>();

	[NonSerialized]
	private WorldZoneData worldZoneData;

	private List<Wgo> wgos = new List<Wgo>();

	private List<Collider> colliders = new List<Collider>();

	[SerializeField]
	private int processingPriority;

	public List<Wgo> Wgos => wgos;

	public int WgosVersion { get; private set; }

	public WorldZoneData Data => worldZoneData;

	public WorldZoneBakedData BakedData
	{
		get
		{
			return bakedData;
		}
		set
		{
			bakedData = value;
		}
	}

	public string Id => id;

	public WorldZoneData.WorldZoneType WorldZoneType => worldZoneType;

	public BoxCollider ZoneCollider => zoneCollider;

	public LazyConsts.Navigation.Graph NavigationGraph => navigationGraph;

	public List<LazyConsts.Navigation.Graph> AdditionalMovementGraphs => additionalMovementGraphs;

	public IReadOnlyList<Collider> NavigationHoleColliders => navigationHoleColliders;

	public HashSet<IChunkableObject> AllStaticObjectsInZone { get; set; }

	public int ProcessingPriority => processingPriority;

	public float GroundPlaneY => base.transform.position.y;

	public void Init(WorldZoneData worldZoneData)
	{
		this.worldZoneData = worldZoneData;
		this.worldZoneData.worldZoneType = worldZoneType;
		if (worldZoneData.IsContainer)
		{
			worldZoneData.OnWgoDataAdded += HandleWgoDataAdded;
			worldZoneData.OnWgoDataToCustomQualityAdded += HandleWgoDataCustomQualityZoneAdded;
			worldZoneData.OnWgoDataFromCustomQualityRemoved += HandleWgoDataCustomQualityZoneRemoved;
			worldZoneData.OnWgoDataRemoved += HandleWgoDataRemoved;
			worldZoneData.OnWgoDataChanged += RedrawWgosWidgets;
		}
		worldZoneData.OnActiveStateChanged += ApplyActiveState;
		ApplyActiveState(worldZoneData.IsActive);
		InitGDPoints();
		DisableNavigationHoleColliders();
	}

	private void DisableNavigationHoleColliders()
	{
		if (navigationHoleColliders == null)
		{
			return;
		}
		for (int i = 0; i < navigationHoleColliders.Count; i++)
		{
			Collider collider = navigationHoleColliders[i];
			if (!(collider == null))
			{
				collider.gameObject.SetActive(value: false);
			}
		}
	}

	private void InitGDPoints()
	{
		GDPoint[] componentsInChildren = GetComponentsInChildren<GDPoint>(includeInactive: true);
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		GdPointsData gdPointsData = MainGame.Instance.GameSave.worldData.gdPointsData;
		GDPoint[] array = componentsInChildren;
		foreach (GDPoint gDPoint in array)
		{
			GDPointData gDPointDataByView = gdPointsData.GetGDPointDataByView(gDPoint);
			if (gDPointDataByView != null)
			{
				gDPoint.Init(gDPointDataByView);
				gDPoint.gameObject.SetActive(gDPointDataByView.Enabled);
			}
			else
			{
				gDPointDataByView = new GDPointData(gDPoint, worldZoneData.gameSceneId, Vector3.zero, isWaypoint: false);
				gdPointsData.AddScenePoint(gDPointDataByView);
				gDPoint.Init(gDPointDataByView);
			}
		}
	}

	public void AddWgosOnGameSceneStart()
	{
		foreach (SGuid wgoData2 in worldZoneData.wgoDataList)
		{
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(wgoData2);
			if (wgoData != null)
			{
				HandleWgoDataAdded(wgoData);
			}
		}
	}

	[CanBeNull]
	public static WorldZone Spawn(WorldZoneData data, Transform parent)
	{
		GameObject gameObject = Addressables.LoadAssetAsync<GameObject>("Assets/AddressableAssets/WorldZones/" + data.id + ".prefab").WaitForCompletion();
		if (!gameObject || !gameObject.TryGetComponent<WorldZone>(out var component))
		{
			Debug.LogError("WorldZone [" + data.id + "] not found");
			return null;
		}
		WorldZone worldZone = UnityEngine.Object.Instantiate(component, parent);
		worldZone?.Init(data);
		worldZone.transform.position = data.pos;
		data.Init(worldZone.ZoneCollider);
		return worldZone;
	}

	private void OnDestroy()
	{
		if (worldZoneData != null)
		{
			worldZoneData.OnWgoDataAdded -= HandleWgoDataAdded;
			worldZoneData.OnWgoDataToCustomQualityAdded -= HandleWgoDataCustomQualityZoneAdded;
			worldZoneData.OnWgoDataFromCustomQualityRemoved -= HandleWgoDataCustomQualityZoneRemoved;
			worldZoneData.OnWgoDataRemoved -= HandleWgoDataRemoved;
			worldZoneData.OnWgoDataChanged -= RedrawWgosWidgets;
			worldZoneData.OnActiveStateChanged -= ApplyActiveState;
		}
	}

	private void ApplyActiveState(bool isActive)
	{
		if (base.gameObject.activeSelf != isActive)
		{
			base.gameObject.SetActive(isActive);
		}
	}

	public Vector3 GetBuildPos()
	{
		return VisualConsts.GetRoundedPosXZ(base.transform.position, BuildConsts.BUILD_GRID_SIZE);
	}

	public List<BuildElevationArea> GetBuildElevationAreas()
	{
		List<BuildElevationArea> list = new List<BuildElevationArea>();
		WorldZoneElevationArea[] componentsInChildren = GetComponentsInChildren<WorldZoneElevationArea>(includeInactive: true);
		if (componentsInChildren.Length != 0)
		{
			foreach (WorldZoneElevationArea worldZoneElevationArea in componentsInChildren)
			{
				if (!(worldZoneElevationArea == null) && !(worldZoneElevationArea.FootprintCollider == null))
				{
					list.Add(new BuildElevationArea(worldZoneElevationArea.GetXZRect(), worldZoneElevationArea.ElevationY, worldZoneElevationArea.GroundPlaneY));
				}
			}
			return list;
		}
		if (worldZoneData?.elevationAreas != null)
		{
			float y = base.transform.position.y;
			for (int j = 0; j < worldZoneData.elevationAreas.Count; j++)
			{
				WorldZoneElevationAreaBakedData worldZoneElevationAreaBakedData = worldZoneData.elevationAreas[j];
				list.Add(new BuildElevationArea(worldZoneElevationAreaBakedData.xzRect, worldZoneElevationAreaBakedData.elevationY, y));
			}
		}
		return list;
	}

	public bool TryGetBuildElevationY(float x, float z, out float elevationY)
	{
		Vector2 xz = new Vector2(x, z);
		float num = float.MinValue;
		bool flag = false;
		WorldZoneElevationArea[] componentsInChildren = GetComponentsInChildren<WorldZoneElevationArea>(includeInactive: true);
		foreach (WorldZoneElevationArea worldZoneElevationArea in componentsInChildren)
		{
			if (!(worldZoneElevationArea == null) && worldZoneElevationArea.ContainsXZ(xz) && (!flag || worldZoneElevationArea.ElevationY > num))
			{
				num = worldZoneElevationArea.ElevationY;
				flag = true;
			}
		}
		if (!flag && worldZoneData != null && worldZoneData.TryGetBuildElevationY(x, z, out var elevationY2))
		{
			num = elevationY2;
			flag = true;
		}
		elevationY = num;
		return flag;
	}

	private void Awake()
	{
		if (zoneCollider == null && !TryGetComponent<BoxCollider>(out zoneCollider))
		{
			Debug.LogError("WorldZone [" + id + "] must have zoneCollider");
		}
		else
		{
			zoneCollider.isTrigger = true;
		}
	}

	public void RedrawWgosWidgets()
	{
		foreach (Wgo wgo in wgos)
		{
			wgo.DrawWidgets();
		}
	}

	public void HideWgosWorldZoneWidgets()
	{
		foreach (Wgo wgo in wgos)
		{
			wgo.HideWorldZoneWidgets();
		}
	}

	private void HandleWgoDataAdded(WgoData wgoData)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(wgoData.UniqueId);
		if (!(wgoViewGlobal == null) && !wgos.Contains(wgoViewGlobal))
		{
			if (!worldZoneData.Definition.hasCustomQualityZones || (worldZoneData.Definition.hasCustomQualityZones && worldZoneData.ContainsCustomQualityZonePrecisely(wgoData.Position)))
			{
				wgoViewGlobal.SetWorldZoneWidgets(worldZoneData);
			}
			wgos.Add(wgoViewGlobal);
			WgosVersion++;
		}
	}

	private void HandleWgoDataCustomQualityZoneAdded(WgoData wgoData)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(wgoData.UniqueId);
		if (!(wgoViewGlobal == null))
		{
			wgoViewGlobal.SetWorldZoneWidgets(worldZoneData);
		}
	}

	private void HandleWgoDataCustomQualityZoneRemoved(WgoData wgoData)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(wgoData.UniqueId);
		if (!(wgoViewGlobal == null))
		{
			wgoViewGlobal.SetWorldZoneWidgets(null);
		}
	}

	private void HandleWgoDataRemoved(WgoData wgoData)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(wgoData.UniqueId);
		if (wgoViewGlobal == null)
		{
			Wgo wgo = wgos.Find((Wgo x) => x.Data.UniqueId == wgoData.UniqueId);
			if (wgo != null)
			{
				wgos.Remove(wgo);
				WgosVersion++;
			}
		}
		else
		{
			wgoViewGlobal.SetWorldZoneWidgets(null);
			if (wgos.Remove(wgoViewGlobal))
			{
				WgosVersion++;
			}
		}
	}

	private void OnNavGraphChanged()
	{
		if (Data != null)
		{
			Data.navigationGraph = navigationGraph;
		}
	}
}
