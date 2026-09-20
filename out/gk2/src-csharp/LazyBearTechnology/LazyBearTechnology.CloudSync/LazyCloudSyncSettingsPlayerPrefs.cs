using UnityEngine;

namespace LazyBearTechnology.CloudSync;

public class LazyCloudSyncSettingsPlayerPrefs : ILazyCloudSyncSettings
{
	public void SetValue(string key, string value)
	{
		PlayerPrefs.SetString(key, value);
	}

	public string GetValue(string key)
	{
		return PlayerPrefs.GetString(key);
	}

	public bool HasValue(string key)
	{
		return PlayerPrefs.HasKey(key);
	}

	public void DeleteValue(string key)
	{
		PlayerPrefs.DeleteKey(key);
	}
}
