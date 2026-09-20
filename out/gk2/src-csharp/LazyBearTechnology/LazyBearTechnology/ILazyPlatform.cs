using System;

namespace LazyBearTechnology;

public interface ILazyPlatform
{
	event Action<PlayerInfo> OnUserDataChanged;

	event Action<string> OnDifferentUserFound;

	event Action OnNoUsersFound;

	event Action OnAllControllersDisabled;

	void Init();

	LazyPlatform GetPlatformId();

	string GetPlatformName();

	void AddUser(bool tryAddSilently = true);

	void Update();

	string GetUniqueUserId();

	bool IsGuest();

	void UnlockAchievement(string id);

	void SetAchievementProgress(string id, int currentValue, int fullValue);

	bool ClearAchievementById(string id);

	bool ClearAllAchievements();

	bool PrefsHasKey(string key);

	string PrefsGetString(string key, string defaultValue = "");

	int PrefsGetInt(string key, int defaultValue = 0);

	void PrefsSetString(string key, string value);

	void PrefsSave();

	void ShowKeyboard(Action<string> callback, int textMaxLength, string headerText);

	bool IsDLCAvailable(DLCInfo dlcInfo);

	void OpenProductInStore(StoreProductInfo storeProductInfo);
}
