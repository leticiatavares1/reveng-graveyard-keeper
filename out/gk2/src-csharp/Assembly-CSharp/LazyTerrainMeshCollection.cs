using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(fileName = "LazyTerrainMeshCollection", menuName = "LazyTerrainMeshCollection", order = 1)]
public class LazyTerrainMeshCollection : LazySingletonSO<LazyTerrainMeshCollection>
{
	public const string AssetPath = "Assets/AddressableAssets/Configurations/LazyTerrainMeshCollection.asset";

	public const string LegacyAssetPath = "Assets/AddressableAssets/Configurations/LazyTerrainMeshCollection_Legacy.asset";

	public const string AddressableGroup = "Configurations";

	public const string AddressableAddress = "LazyTerrainMeshCollection";

	[SerializeField]
	private List<Mesh> meshes = new List<Mesh>();

	public static void Runtime_StripCpuMeshData(Mesh mesh)
	{
		if (!(mesh == null) && mesh.isReadable)
		{
			mesh.UploadMeshData(markNoLongerReadable: true);
		}
	}

	public void Runtime_StripAllMeshCpuData()
	{
		for (int i = 0; i < meshes.Count; i++)
		{
			Runtime_StripCpuMeshData(meshes[i]);
		}
	}
}
