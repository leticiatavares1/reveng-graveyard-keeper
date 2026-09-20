namespace LazyBearTechnology;

public class LazyPlayerPrefs
{
	public string GetString(string key, string defaultValue = "")
	{
		return LazyAPI.Platform.PrefsGetString(key, defaultValue);
	}

	public bool HasKey(string key)
	{
		return LazyAPI.Platform.PrefsHasKey(key);
	}

	public int GetInt(string key, int defaultValue = 0)
	{
		return LazyAPI.Platform.PrefsGetInt(key, defaultValue);
	}

	public void SetString(string key, string value)
	{
		LazyAPI.Platform.PrefsSetString(key, value);
	}

	public void Save()
	{
		LazyAPI.Platform.PrefsSave();
	}
}
