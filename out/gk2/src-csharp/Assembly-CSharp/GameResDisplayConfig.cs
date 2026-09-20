using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/GameResDisplayConfig", fileName = "GameResDisplayConfig")]
public class GameResDisplayConfig : LazySingletonSO<GameResDisplayConfig>
{
	[SerializeField]
	private List<GameResDisplayData> datas = new List<GameResDisplayData>();

	public static GameResIconConfig GetConfigForRes(string res, GameResIconType iconType)
	{
		if ((object)iconType == null)
		{
			return null;
		}
		foreach (GameResDisplayData data in LazySingletonSO<GameResDisplayConfig>.Instance.datas)
		{
			if (!(data.atomType == res))
			{
				continue;
			}
			foreach (GameResIconConfig iconConfig in data.iconConfigs)
			{
				if (iconConfig.iconType.value == iconType.value)
				{
					return iconConfig;
				}
			}
		}
		return null;
	}
}
