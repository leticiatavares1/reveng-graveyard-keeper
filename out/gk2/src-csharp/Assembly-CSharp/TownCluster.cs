using UnityEngine;

public class TownCluster : MonoBehaviour
{
	public enum HandleDestroyedWsoMode
	{
		Destroy,
		HouseRepair
	}

	private static string PATH_TO_TOWN_CLUSTER_DATA = "Assets/AddressableAssets/TownClusters";

	public int id;

	[SerializeField]
	private TownClusterData data;

	[SerializeField]
	[HideInInspector]
	private string instanceHash;
}
