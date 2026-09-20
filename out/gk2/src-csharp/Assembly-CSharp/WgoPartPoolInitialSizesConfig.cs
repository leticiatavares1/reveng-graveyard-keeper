using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/WgoPartPoolInitialSizesConfig", fileName = "WgoPartPoolInitialSizesConfig")]
public class WgoPartPoolInitialSizesConfig : LazySingletonSO<WgoPartPoolInitialSizesConfig>
{
	public GameRes initialSizeConfigs = new GameRes();

	public int GetSizeForPath(string path)
	{
		int @int = initialSizeConfigs.GetInt(path);
		if (@int > 0)
		{
			return @int;
		}
		return 0;
	}

	public void SetSizeForPath(string path, int size)
	{
		initialSizeConfigs.Set(path, size);
	}

	public void LogSizeForPath(string path)
	{
		Debug.Log($"Size for path:[{path}] is:[{GetSizeForPath(path)}]");
	}

	public void Clear()
	{
		initialSizeConfigs.Clear();
	}
}
