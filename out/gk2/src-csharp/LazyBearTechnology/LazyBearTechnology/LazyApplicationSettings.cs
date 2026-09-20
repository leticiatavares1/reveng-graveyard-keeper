using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

[CreateAssetMenu(menuName = "Lazy/LazyApplicationSettings", fileName = "LazyApplicationSettings")]
public class LazyApplicationSettings : LazySingletonSO<LazyApplicationSettings>
{
	[SerializeField]
	private LazyBuildType current;

	[SerializeField]
	[Tooltip("If this list is empty, all settings will be taken from PlayerSettings")]
	private List<ApplicationSettings> settingsList = new List<ApplicationSettings>();

	public static ApplicationSettings GetCurrentSetting()
	{
		if (LazySingletonSO<LazyApplicationSettings>.Instance == null)
		{
			return null;
		}
		if (LazySingletonSO<LazyApplicationSettings>.Instance.settingsList.Count == 0)
		{
			return null;
		}
		if (LazySingletonSO<LazyApplicationSettings>.Instance.settingsList.Count == 1)
		{
			LazySingletonSO<LazyApplicationSettings>.Instance.current = LazySingletonSO<LazyApplicationSettings>.Instance.settingsList[0].buildType;
		}
		return LazySingletonSO<LazyApplicationSettings>.Instance.settingsList.Find((ApplicationSettings s) => s.buildType == LazySingletonSO<LazyApplicationSettings>.Instance.current);
	}

	public static ApplicationSettings GetSettingsByBuildType(int buildType)
	{
		if (LazySingletonSO<LazyApplicationSettings>.Instance == null)
		{
			return null;
		}
		if (LazySingletonSO<LazyApplicationSettings>.Instance.settingsList.Count == 0)
		{
			return null;
		}
		return LazySingletonSO<LazyApplicationSettings>.Instance.settingsList.Find((ApplicationSettings s) => s.buildType.value == buildType);
	}

	public static List<string> GetExclusiveDefinesExceptCurrent()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < LazySingletonSO<LazyApplicationSettings>.Instance.settingsList.Count; i++)
		{
			if (!(LazySingletonSO<LazyApplicationSettings>.Instance.settingsList[i].buildType == LazySingletonSO<LazyApplicationSettings>.Instance.current))
			{
				list.AddRange(LazySingletonSO<LazyApplicationSettings>.Instance.settingsList[i].exclusiveDefines);
			}
		}
		return list;
	}
}
