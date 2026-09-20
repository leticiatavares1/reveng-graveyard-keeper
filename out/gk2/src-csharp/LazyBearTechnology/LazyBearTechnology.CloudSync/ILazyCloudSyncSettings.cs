namespace LazyBearTechnology.CloudSync;

public interface ILazyCloudSyncSettings
{
	void SetValue(string key, string value);

	string GetValue(string key);

	bool HasValue(string key);

	void DeleteValue(string key);
}
