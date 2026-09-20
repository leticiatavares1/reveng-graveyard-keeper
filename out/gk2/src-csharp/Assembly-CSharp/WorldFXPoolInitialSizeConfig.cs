using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/WorldFXPoolInitialSizeConfig", fileName = "WorldFXPoolInitialSizeConfig")]
public class WorldFXPoolInitialSizeConfig : LazySingletonSO<WorldFXPoolInitialSizeConfig>
{
	public GameRes initialSizeConfigs = new GameRes();

	public int GetSizeForName(string fxName)
	{
		int @int = initialSizeConfigs.GetInt(fxName);
		if (@int > 0)
		{
			return @int;
		}
		return 0;
	}

	public void SetSizeForName(string fxName, int size)
	{
		initialSizeConfigs.Set(fxName, size);
	}

	public void LogSizeForName(string fxName)
	{
		Debug.Log($"Size for fxName:[{fxName}] is:[{GetSizeForName(fxName)}]");
	}

	public void Clear()
	{
		initialSizeConfigs.Clear();
	}
}
