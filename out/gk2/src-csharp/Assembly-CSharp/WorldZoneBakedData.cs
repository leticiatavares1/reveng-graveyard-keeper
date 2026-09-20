using System.Collections.Generic;
using UnityEngine;

public class WorldZoneBakedData : ScriptableObject
{
	public string id;

	public string contentPartName;

	public Vector3 pos;

	public WorldZoneData.WorldZoneType worldZoneType;

	public LazyConsts.Navigation.Graph navigationGraph = LazyConsts.Navigation.Graph.None;

	public List<LazyConsts.Navigation.Graph> additionalMovementGraphs = new List<LazyConsts.Navigation.Graph>();

	public Rect wholeZoneRect;

	public List<WorldZonePrebuiltWgoParams> prebuiltWgoParams = new List<WorldZonePrebuiltWgoParams>();

	public List<WorldZoneElevationAreaBakedData> elevationAreas = new List<WorldZoneElevationAreaBakedData>();

	public List<WorldZoneNavigationHoleBakedData> navigationHoles = new List<WorldZoneNavigationHoleBakedData>();

	public int processingPriority;

	public void SetFrom(WorldZone worldZone)
	{
		if (worldZone == null)
		{
			return;
		}
		id = worldZone.Id;
		contentPartName = ResolveContentPartName(worldZone.transform);
		pos = worldZone.transform.position;
		worldZoneType = worldZone.WorldZoneType;
		navigationGraph = worldZone.NavigationGraph;
		additionalMovementGraphs = worldZone.AdditionalMovementGraphs;
		processingPriority = worldZone.ProcessingPriority;
		if (worldZone.ZoneCollider != null)
		{
			Vector3 vector = worldZone.ZoneCollider.transform.TransformPoint(worldZone.ZoneCollider.center);
			Vector3 size = worldZone.ZoneCollider.size;
			wholeZoneRect = new Rect(new Vector2(vector.x - size.x / 2f, vector.z - size.z / 2f), new Vector2(size.x, size.z));
		}
		elevationAreas.Clear();
		WorldZoneElevationArea[] componentsInChildren = worldZone.GetComponentsInChildren<WorldZoneElevationArea>(includeInactive: true);
		foreach (WorldZoneElevationArea worldZoneElevationArea in componentsInChildren)
		{
			if (!(worldZoneElevationArea == null))
			{
				elevationAreas.Add(worldZoneElevationArea.ToBakedData());
			}
		}
		navigationHoles.Clear();
		IReadOnlyList<Collider> navigationHoleColliders = worldZone.NavigationHoleColliders;
		if (navigationHoleColliders == null)
		{
			return;
		}
		for (int j = 0; j < navigationHoleColliders.Count; j++)
		{
			Collider collider = navigationHoleColliders[j];
			if (!(collider == null))
			{
				if (collider is BoxCollider boxCollider)
				{
					navigationHoles.Add(WorldZoneNavigationHoleBakedData.FromBoxCollider(boxCollider));
					continue;
				}
				Debug.LogWarning("WorldZone [" + worldZone.Id + "] navigation hole [" + collider.name + "] is not a BoxCollider and was skipped during bake.", collider);
			}
		}
	}

	private static string ResolveContentPartName(Transform worldZoneTransform)
	{
		SceneWgoContentPart componentInParent = worldZoneTransform.GetComponentInParent<SceneWgoContentPart>(includeInactive: true);
		if (componentInParent == null)
		{
			return string.Empty;
		}
		string text = componentInParent.name;
		if (!text.EndsWith("Data"))
		{
			return text;
		}
		return text.Substring(0, text.Length - 4);
	}

	public static bool TryGetPrebuiltWgoParams(IReadOnlyList<WorldZoneBakedData> bakedZones, SGuid wgoUniqueId, out WorldZonePrebuiltWgoParams prebuiltParams)
	{
		prebuiltParams = null;
		if (bakedZones == null || SGuid.IsNullOrEmpty(wgoUniqueId))
		{
			return false;
		}
		for (int i = 0; i < bakedZones.Count; i++)
		{
			WorldZoneBakedData worldZoneBakedData = bakedZones[i];
			if (worldZoneBakedData?.prebuiltWgoParams == null)
			{
				continue;
			}
			for (int j = 0; j < worldZoneBakedData.prebuiltWgoParams.Count; j++)
			{
				WorldZonePrebuiltWgoParams worldZonePrebuiltWgoParams = worldZoneBakedData.prebuiltWgoParams[j];
				if (worldZonePrebuiltWgoParams != null && !(worldZonePrebuiltWgoParams.wgoUniqueId != wgoUniqueId))
				{
					prebuiltParams = worldZonePrebuiltWgoParams;
					return true;
				}
			}
		}
		return false;
	}
}
