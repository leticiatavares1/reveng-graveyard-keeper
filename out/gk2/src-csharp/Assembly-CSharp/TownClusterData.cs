using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TownClusterData : ScriptableObject
{
	[Serializable]
	public class ClusterWgoData
	{
		public string wgoId;

		public Vector3 position;
	}

	[Serializable]
	public class ClusterWsoData
	{
		public AssetReferenceGameObject assetReference;

		public Vector3 position;
	}

	[Serializable]
	public class TownClusterDataDto
	{
		public int id;

		public TownCluster.HandleDestroyedWsoMode handleDestroyedWsoMode;

		public List<SGuid> destroyedWgoUniqueIds = new List<SGuid>();

		public List<ClusterWgoData> repairedWgoData = new List<ClusterWgoData>();

		public List<SGuid> destroyedWsoUniqueIds = new List<SGuid>();

		public List<ClusterWgoData> repairedWsoData = new List<ClusterWgoData>();
	}

	public int id;

	[Space]
	public List<SGuid> destroyedWgoUniqueIds = new List<SGuid>();

	public List<ClusterWgoData> repairedWgoData = new List<ClusterWgoData>();

	[Space]
	public TownCluster.HandleDestroyedWsoMode handleDestroyedWsoMode;

	public List<SGuid> destroyedWsoUniqueIds = new List<SGuid>();

	public List<ClusterWsoData> repairedWsoData = new List<ClusterWsoData>();

	public TownClusterDataDto ToDto()
	{
		TownClusterDataDto townClusterDataDto = new TownClusterDataDto
		{
			id = id,
			handleDestroyedWsoMode = handleDestroyedWsoMode
		};
		townClusterDataDto.destroyedWgoUniqueIds.AddRange(destroyedWgoUniqueIds);
		townClusterDataDto.destroyedWsoUniqueIds.AddRange(destroyedWsoUniqueIds);
		foreach (ClusterWgoData repairedWgoDatum in repairedWgoData)
		{
			townClusterDataDto.repairedWgoData.Add(new ClusterWgoData
			{
				wgoId = repairedWgoDatum.wgoId,
				position = repairedWgoDatum.position
			});
		}
		foreach (ClusterWsoData repairedWsoDatum in repairedWsoData)
		{
			string empty = string.Empty;
			townClusterDataDto.repairedWsoData.Add(new ClusterWgoData
			{
				wgoId = empty,
				position = repairedWsoDatum.position
			});
		}
		return townClusterDataDto;
	}
}
