using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public static class SaveSystem
{
	private const string META_FILE_EXTENSION = ".info";

	private const string DATA_FILE_EXTENSION = ".dat";

	private const string RELEASE_FOLDER_NAME = "Graveyard Keeper 2";

	private const string DEMO_FOLDER_NAME = "Graveyard Keeper 2 Demo";

	private const string STEAM_RELEASE_APP_ID = "4358690";

	private const string STEAM_DEMO_APP_ID = "5075680";

	private const string GAME_SETTINGS_PLAYER_PREFS_KEY = "settings";

	public const int LIMITED_SAVE_SLOTS_COUNT = 3;

	private static List<SaveSlotData> saveSlotDataList;

	private static List<byte[]> saveDataList;

	private static OdinBinaryFileSerializer odinBinaryFileSerializer;

	public static LazySaveSystem lazySaveSystem = new LazySaveSystem((typeof(GameSave), OdinBinaryFileSerializer), (typeof(SaveSlotData), new JsonFileSerializer(".info")), (typeof(GameSettings), new PlayerPrefsSerializer()));

	public static List<SaveSlotData> SaveSlotDataList
	{
		get
		{
			if (!IsLimitedSaveSlotsEnabled)
			{
				return ReadSaveSlotsData();
			}
			if (saveSlotDataList == null)
			{
				saveSlotDataList = ReadSaveSlotsData();
				saveDataList = new List<byte[]>();
				for (int i = 0; i < saveSlotDataList.Count; i++)
				{
					saveDataList.Add(null);
				}
			}
			List<SaveSlotData> list = new List<SaveSlotData>(new SaveSlotData[saveDataList.Count]);
			for (int j = 0; j < list.Count; j++)
			{
				list[j] = saveSlotDataList[j].Copy();
			}
			return list;
		}
	}

	public static bool IsLimitedSaveSlotsEnabled => false;

	public static string SaveFolder => Application.persistentDataPath + "/";

	public static string DemoSaveFolder => (Application.persistentDataPath + "/").Replace('\\', '/').Replace("Graveyard Keeper 2", "Graveyard Keeper 2 Demo");

	private static OdinBinaryFileSerializer OdinBinaryFileSerializer
	{
		get
		{
			if (odinBinaryFileSerializer != null)
			{
				return odinBinaryFileSerializer;
			}
			odinBinaryFileSerializer = new OdinBinaryFileSerializer(".dat");
			return odinBinaryFileSerializer;
		}
	}

	public static event Action OnSaveSlotsReadStarted;

	public static event Action OnSaveSlotsReadCompleted;

	public static event Action OnSaveSlotsReadFailed;

	public static event Action OnSaveLoadingStarted;

	public static event Action OnSaveLoadingEnded;

	public static event Action OnSaveWriteStarted;

	public static event Action OnSaveWriteStartedInstant;

	public static event Action OnSaveWriteEnded;

	public static string GetNameForLimitedSaveSlot(int slotIndex)
	{
		if (slotIndex < 1 || slotIndex > 3)
		{
			Debug.LogError($"Error: limited save slot index [{slotIndex}] is out of range");
			return null;
		}
		return $"{LazyAPI.Platform.GetPlatformName()}_{slotIndex}";
	}

	public static List<SaveSlotData> GetLimitedSaveSlotsData()
	{
		List<SaveSlotData> list = new List<SaveSlotData>(new SaveSlotData[3]);
		List<SaveSlotData> list2 = SaveSlotDataList;
		for (int i = 0; i < 3; i++)
		{
			string nameForLimitedSaveSlot = GetNameForLimitedSaveSlot(i + 1);
			for (int j = 0; j < list2.Count; j++)
			{
				if (IsCurrentApplicationLimitedSaveSlot(list2[j]) && list2[j].slotName == nameForLimitedSaveSlot)
				{
					list[i] = list2[j];
					break;
				}
			}
		}
		return list;
	}

	public static List<SaveSlotData> GetImportableSaveSlotsForLimitedSlots()
	{
		return GetImportableSaveSlotsForLimitedSlots(SaveSlotDataList);
	}

	private static List<SaveSlotData> GetImportableSaveSlotsForLimitedSlots(List<SaveSlotData> allSlots)
	{
		List<SaveSlotData> list = new List<SaveSlotData>();
		if (allSlots == null)
		{
			SaveImportLog("GetImportableSaveSlotsForLimitedSlots: allSlots is null");
			return list;
		}
		SaveImportLog($"GetImportableSaveSlotsForLimitedSlots: checking [{allSlots.Count}] slots");
		for (int i = 0; i < allSlots.Count; i++)
		{
			SaveSlotData saveSlotData = allSlots[i];
			if (saveSlotData == null || IsCurrentApplicationLimitedSaveSlot(saveSlotData))
			{
				SaveImportLog(string.Format("GetImportableSaveSlotsForLimitedSlots: skip slot[{0}] slotName:[{1}] isDemoSave:[{2}] reason:[{3}]", i, saveSlotData?.slotName, saveSlotData?.isDemoSave, (saveSlotData == null) ? "null" : "current limited slot"));
			}
			else
			{
				SaveImportLog($"GetImportableSaveSlotsForLimitedSlots: add slot[{i}] slotName:[{saveSlotData.slotName}] platform:[{saveSlotData.platform}] isDemoSave:[{saveSlotData.isDemoSave}]");
				list.Add(saveSlotData);
			}
		}
		list.Sort((SaveSlotData x, SaveSlotData y) => DateTime.Compare(y.GetSaveDateTime(), x.GetSaveDateTime()));
		SaveImportLog($"GetImportableSaveSlotsForLimitedSlots: result count [{list.Count}]");
		return list;
	}

	public static bool HasImportableSaveSlotsForLimitedSlots()
	{
		bool flag = GetImportableSaveSlotsForLimitedSlots().Count > 0;
		SaveImportLog($"HasImportableSaveSlotsForLimitedSlots: [{flag}]");
		return flag;
	}

	public static string GetLimitedSaveSlotSourceLabel(SaveSlotData slotData)
	{
		if (slotData == null)
		{
			return string.Empty;
		}
		if (slotData.isDemoSave)
		{
			return "DEMO";
		}
		return string.Empty;
	}

	private static void SaveImportLog(string message)
	{
	}

	private static void PrepareLoadedSaveSlotInfos(List<(SaveSlotData data, string fileName)> slots, string source, bool forceDemoSave)
	{
		if (slots == null)
		{
			SaveImportLog(source + ": LoadAll returned null");
			return;
		}
		SaveImportLog($"{source}: LoadAll returned [{slots.Count}] slot info files");
		for (int i = 0; i < slots.Count; i++)
		{
			SaveSlotData item = slots[i].data;
			string slotNameFromInfoFileName = GetSlotNameFromInfoFileName(slots[i].fileName);
			if (item == null)
			{
				SaveImportLog($"{source}: slot[{i}] file:[{slots[i].fileName}] data is null");
				continue;
			}
			item.slotName = slotNameFromInfoFileName;
			if (forceDemoSave && !item.isDemoSave)
			{
				SaveImportLog($"{source}: slot[{i}] file:[{slots[i].fileName}] is not marked as demo in metadata, forcing demo source flag");
				item.isDemoSave = true;
			}
			SaveImportLog($"{source}: slot[{i}] file:[{slots[i].fileName}] slotName:[{item.slotName}] platform:[{item.platform}] isDemoSave:[{item.isDemoSave}] repValue:[{item.repValue}] currentLimited:[{IsCurrentApplicationLimitedSaveSlot(item)}] canLoad:[{CanLoadSaveSlot(item)}]");
		}
	}

	private static string GetSlotNameFromInfoFileName(string fileName)
	{
		return fileName?.Replace(".info", "");
	}

	private static bool IsSameLoadedSaveSlot(SaveSlotData cachedSlotData, SaveSlotData requestedSlotData)
	{
		if (cachedSlotData == null || requestedSlotData == null)
		{
			return false;
		}
		if (cachedSlotData.slotName == requestedSlotData.slotName)
		{
			return cachedSlotData.isDemoSave == requestedSlotData.isDemoSave;
		}
		return false;
	}

	private static List<SaveSlotData> ReadSaveSlotsData()
	{
		bool slotReadFinished = false;
		SaveSystem.OnSaveSlotsReadStarted?.Invoke();
		List<SaveSlotData> saveSlots;
		try
		{
			saveSlots = null;
			ApplicationSettings currentSetting = LazyApplicationSettings.GetCurrentSetting();
			SaveImportLog($"ReadSaveSlotsData started. SaveFolder:[{SaveFolder}] DemoSaveFolder:[{DemoSaveFolder}] currentSettingsNull:[{currentSetting == null}]");
			lazySaveSystem.LoadAll(SaveFolder, delegate(List<(SaveSlotData data, string fileName)> list)
			{
				PrepareLoadedSaveSlotInfos(list, "Current application", forceDemoSave: false);
				AfterLoadAll();
			});
			return saveSlots;
		}
		catch (Exception arg)
		{
			Debug.Log($"Error during load save slots:[{arg}]");
			SaveImportLog($"ReadSaveSlotsData failed with exception:[{arg}]");
			if (!slotReadFinished)
			{
				SaveSystem.OnSaveSlotsReadCompleted?.Invoke();
			}
			return new List<SaveSlotData>();
		}
		void AfterLoadAll()
		{
			SaveImportLog($"ReadSaveSlotsData AfterLoadAll. Total raw info entries:[{P_0.list?.Count ?? 0}]");
			SaveSystem.OnSaveSlotsReadCompleted?.Invoke();
			slotReadFinished = true;
			saveSlots = new List<SaveSlotData>();
			foreach (var item in P_0.list)
			{
				string slotNameFromInfoFileName = GetSlotNameFromInfoFileName(item.fileName);
				item.data.slotName = slotNameFromInfoFileName;
				saveSlots.Add(item.data);
			}
			SaveImportLog($"ReadSaveSlotsData completed. SaveSlotData count:[{saveSlots.Count}] importable count:[{GetImportableSaveSlotsForLimitedSlots(saveSlots).Count}]");
		}
	}

	private static string GetFolderForSlotData(SaveSlotData slotData)
	{
		string saveFolder = SaveFolder;
		LazyApplicationSettings.GetCurrentSetting();
		SaveImportLog($"GetFolderForSlotData slotName:[{slotData?.slotName}] platform:[{slotData?.platform}] isDemoSave:[{slotData?.isDemoSave}] -> folder:[{saveFolder}]");
		return saveFolder;
	}

	private static bool IsLimitedSaveSlotName(string slotName)
	{
		for (int i = 1; i <= 3; i++)
		{
			if (slotName == GetNameForLimitedSaveSlot(i))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsCurrentApplicationLimitedSaveSlot(SaveSlotData slotData)
	{
		if (slotData == null)
		{
			return false;
		}
		return IsLimitedSaveSlotName(slotData.slotName);
	}

	private static bool IsImportedSaveSlotDeleted(SaveSlotData slotData)
	{
		if (slotData == null)
		{
			return true;
		}
		if (slotData.isDemoSave && slotData.IsDemoSlotDeleted)
		{
			return true;
		}
		return false;
	}

	public static void Load(SaveSlotData slotData, Action<GameSave> callback)
	{
		bool slotReadFinished = false;
		SaveSystem.OnSaveLoadingStarted?.Invoke();
		try
		{
			SaveImportLog($"Load started slotName:[{slotData?.slotName}] platform:[{slotData?.platform}] isDemoSave:[{slotData?.isDemoSave}]");
			if (!CanLoadSaveSlot(slotData))
			{
				Debug.LogWarning("Save slot [" + slotData?.slotName + "] can't be loaded");
				SaveImportLog("Load rejected by CanLoadSaveSlot slotName:[" + slotData?.slotName + "]");
				callback?.Invoke(null);
				SaveSystem.OnSaveLoadingEnded?.Invoke();
				return;
			}
			string folderForSlotData = GetFolderForSlotData(slotData);
			if (IsLimitedSaveSlotsEnabled)
			{
				for (int i = 0; i < saveSlotDataList.Count; i++)
				{
					if (IsSameLoadedSaveSlot(saveSlotDataList[i], slotData) && saveDataList[i] != null)
					{
						SaveImportLog($"Load served from cache slotName:[{slotData.slotName}] isDemoSave:[{slotData.isDemoSave}] index:[{i}] bytes:[{saveDataList[i].Length}]");
						OnLoaded(OdinBinaryFileSerializer.Deserialize<GameSave>(saveDataList[i]));
						return;
					}
				}
			}
			SaveImportLog("Load from folder:[" + folderForSlotData + "] slotName:[" + slotData.slotName + "]");
			lazySaveSystem.Load<GameSave>(folderForSlotData, slotData.slotName, OnLoaded);
		}
		catch (Exception ex)
		{
			Debug.LogError($"Error during load save:[{ex}]");
			SaveImportLog($"Load failed slotName:[{slotData?.slotName}] exception:[{ex}]");
			if (!slotReadFinished)
			{
				callback(null);
				SaveSystem.OnSaveLoadingEnded?.Invoke();
			}
		}
		void OnLoaded(GameSave loaded)
		{
			if (!slotReadFinished)
			{
				callback?.Invoke(loaded);
				SaveSystem.OnSaveLoadingEnded?.Invoke();
				slotReadFinished = true;
				if (IsLimitedSaveSlotsEnabled && loaded != null)
				{
					for (int j = 0; j < saveSlotDataList.Count; j++)
					{
						if (IsSameLoadedSaveSlot(saveSlotDataList[j], slotData))
						{
							saveDataList[j] = OdinBinaryFileSerializer.Serialize(loaded);
							SaveImportLog($"Load cached bytes for slotName:[{slotData.slotName}] isDemoSave:[{slotData.isDemoSave}] index:[{j}]");
						}
					}
				}
			}
		}
	}

	public static bool Remove(SaveSlotData slotData, Action callback)
	{
		string folderForSlotData = GetFolderForSlotData(slotData);
		try
		{
			bool flag = lazySaveSystem.Remove<GameSave>(folderForSlotData, slotData.slotName, callback);
			flag &= lazySaveSystem.Remove<SaveSlotData>(folderForSlotData, slotData.slotName, null);
			if (flag && IsLimitedSaveSlotsEnabled)
			{
				int num = -1;
				for (int i = 0; i < saveSlotDataList.Count; i++)
				{
					if (IsSameLoadedSaveSlot(saveSlotDataList[i], slotData))
					{
						num = i;
						break;
					}
				}
				if (num != -1)
				{
					saveSlotDataList.RemoveAt(num);
					saveDataList.RemoveAt(num);
				}
			}
			return flag;
		}
		catch (Exception arg)
		{
			Debug.Log($"Error during deleting save:[{arg}]");
			return false;
		}
	}

	public static void ImportSaveToLimitedSlot(SaveSlotData sourceSlotData, int targetSlotIndex, Action<bool> callback, bool triggerSaveWriteStarted = true)
	{
		SaveImportLog($"ImportSaveToLimitedSlot started sourceSlotName:[{sourceSlotData?.slotName}] sourcePlatform:[{sourceSlotData?.platform}] sourceIsDemo:[{sourceSlotData?.isDemoSave}] targetSlotIndex:[{targetSlotIndex}]");
		if (sourceSlotData == null)
		{
			Debug.LogError("Import save error: source slot is null");
			SaveImportLog("ImportSaveToLimitedSlot failed: source slot is null");
			callback?.Invoke(obj: false);
			return;
		}
		string targetSlotName = GetNameForLimitedSaveSlot(targetSlotIndex);
		if (string.IsNullOrEmpty(targetSlotName))
		{
			SaveImportLog($"ImportSaveToLimitedSlot failed: target slot name is empty for index:[{targetSlotIndex}]");
			callback?.Invoke(obj: false);
			return;
		}
		if (triggerSaveWriteStarted)
		{
			SaveSystem.OnSaveWriteStarted?.Invoke();
		}
		Load(sourceSlotData, delegate(GameSave loadedSave)
		{
			if (loadedSave == null)
			{
				SaveImportLog("ImportSaveToLimitedSlot failed: loaded save is null for sourceSlotName:[" + sourceSlotData.slotName + "]");
				FinishImport(success: false);
			}
			else
			{
				SaveSlotData saveSlotData = sourceSlotData.Copy();
				saveSlotData.slotName = targetSlotName;
				saveSlotData.platform = LazyAPI.Platform.GetPlatformName();
				saveSlotData.importedFromDemo = sourceSlotData.isDemoSave || sourceSlotData.importedFromDemo;
				saveSlotData.isDemoSave = false;
				SaveImportLog($"ImportSaveToLimitedSlot saving targetSlotName:[{saveSlotData.slotName}] targetPlatform:[{saveSlotData.platform}] importedFromDemo:[{saveSlotData.importedFromDemo}]");
				Save(saveSlotData, loadedSave, delegate
				{
					FinishImport(success: true);
				}, delegate
				{
					FinishImport(success: false);
				}, autoOnSaveStart: false);
			}
		});
		void FinishImport(bool success)
		{
			SaveImportLog($"ImportSaveToLimitedSlot finished sourceSlotName:[{sourceSlotData.slotName}] targetSlotName:[{targetSlotName}] success:[{success}]");
			callback?.Invoke(success);
			SaveSystem.OnSaveWriteEnded?.Invoke();
		}
	}

	public static void TriggerOnSaveStartEvent(bool instant = false)
	{
		if (instant)
		{
			SaveSystem.OnSaveWriteStartedInstant?.Invoke();
		}
		else
		{
			SaveSystem.OnSaveWriteStarted?.Invoke();
		}
	}

	public static bool CanLoadSaveSlot(SaveSlotData slotData)
	{
		if (IsImportedSaveSlotDeleted(slotData))
		{
			return false;
		}
		if (slotData.repValue)
		{
			return false;
		}
		return true;
	}

	public static SaveSlotData GetLastSaveSlot()
	{
		List<SaveSlotData> list = new List<SaveSlotData>();
		list.AddRange(IsLimitedSaveSlotsEnabled ? GetLimitedSaveSlotsData() : SaveSlotDataList);
		if (list.Count <= 0)
		{
			return null;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (!CanLoadSaveSlot(list[num]))
			{
				list.RemoveAt(num);
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		list.Sort((SaveSlotData x, SaveSlotData y) => DateTime.Compare(y.GetSaveDateTime(), x.GetSaveDateTime()));
		return list[0];
	}

	public static SaveSlotData GetActiveSaveData()
	{
		List<SaveSlotData> list = new List<SaveSlotData>();
		list.AddRange(IsLimitedSaveSlotsEnabled ? GetLimitedSaveSlotsData() : SaveSlotDataList);
		if (list.Count <= 0)
		{
			return null;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (!CanLoadSaveSlot(list[num]))
			{
				list.RemoveAt(num);
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		list.Sort((SaveSlotData x, SaveSlotData y) => DateTime.Compare(y.GetSaveDateTime(), x.GetSaveDateTime()));
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].repValue)
			{
				return list[i];
			}
		}
		return null;
	}

	public static void Save(SaveSlotData slotData, GameSave gameSave, Action callbackSuccessful = null, Action callbackUnsuccessful = null, bool autoOnSaveStart = true, Action<SaveSlotData, GameSave> postPrepareToSave = null)
	{
		if (slotData == null)
		{
			Debug.LogError("Save Error: Cannot save to a null slot");
			callbackUnsuccessful?.Invoke();
			return;
		}
		if (autoOnSaveStart)
		{
			SaveSystem.OnSaveWriteStarted?.Invoke();
		}
		gameSave.PrepareToSave(slotData);
		postPrepareToSave?.Invoke(slotData, gameSave);
		string saveFolder = SaveFolder;
		Debug.Log("Save slotData.slotName:[" + slotData.slotName + "]");
		bool flag = true;
		try
		{
			if (IsLimitedSaveSlotsEnabled)
			{
				gameSave.OnBeforeSerialize();
				byte[] array = OdinBinaryFileSerializer.Serialize(gameSave);
				Debug.Log($"Serialized save length: {array.Length}");
				if (array.Length == 0)
				{
					flag = false;
				}
				else
				{
					flag = OdinBinaryFileSerializer.SaveBytes(SaveFolder, slotData.slotName, array);
					if (flag)
					{
						flag &= lazySaveSystem.Save(slotData, SaveFolder, slotData.slotName, null);
					}
					if (flag)
					{
						LimitedSaveSlotsActionsForSaveWrite(slotData, array);
					}
				}
			}
			else
			{
				flag = lazySaveSystem.Save(gameSave, saveFolder, slotData.slotName, null);
				if (flag)
				{
					flag &= lazySaveSystem.Save(slotData, saveFolder, slotData.slotName, null);
				}
			}
			if (flag)
			{
				callbackSuccessful?.Invoke();
			}
			else
			{
				callbackUnsuccessful?.Invoke();
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Error during save:[{arg}]");
			callbackUnsuccessful?.Invoke();
		}
		if (autoOnSaveStart)
		{
			SaveSystem.OnSaveWriteEnded?.Invoke();
		}
	}

	private static void LimitedSaveSlotsActionsForSaveWrite(SaveSlotData slotData, byte[] gameSaveBytes)
	{
		bool flag = false;
		for (int i = 0; i < saveSlotDataList.Count; i++)
		{
			if (IsSameLoadedSaveSlot(saveSlotDataList[i], slotData))
			{
				saveSlotDataList[i] = slotData.Copy();
				saveDataList[i] = gameSaveBytes;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			saveSlotDataList.Add(slotData.Copy());
			saveDataList.Add(gameSaveBytes);
		}
	}

	public static string GetNameForNewSlot(List<SaveSlotData> existingSaveSlots)
	{
		int num = 1;
		while (num < 1000)
		{
			string text = num.ToString();
			text += "_demo";
			bool flag = false;
			foreach (SaveSlotData existingSaveSlot in existingSaveSlots)
			{
				if (existingSaveSlot.slotName == text)
				{
					flag = true;
					break;
				}
			}
			num++;
			if (!flag)
			{
				return LazyAPI.Platform.GetPlatformName() + "_" + text;
			}
		}
		Debug.LogError("Error: GetNameForNewSlot - too many iterations");
		return null;
	}

	public static GameSettings LoadGameSettings()
	{
		string gameSettingsPlayerPrefsKey = GetGameSettingsPlayerPrefsKey();
		string text = string.Empty;
		if (!string.IsNullOrEmpty(gameSettingsPlayerPrefsKey) && LazyAPI.PlayerPrefs.HasKey(gameSettingsPlayerPrefsKey))
		{
			text = LazyAPI.PlayerPrefs.GetString(gameSettingsPlayerPrefsKey);
		}
		if (string.IsNullOrEmpty(text))
		{
			Debug.Log("[SaveSystem] No stored game settings found (PlayerPrefs key '" + gameSettingsPlayerPrefsKey + "'). Creating defaults (first run).");
			return new GameSettings();
		}
		GameSettings gameSettings = JsonUtility.FromJson<GameSettings>(text);
		if (!text.Contains("\"gpuGraphicsDefaultApplied\""))
		{
			gameSettings.gpuGraphicsDefaultApplied = true;
			Debug.Log($"[SaveSystem] Stored game settings predate GPU-based tier detection; keeping existing graphics tier {gameSettings.graphicsTier}.");
		}
		return gameSettings;
	}

	public static void SaveGameSettings()
	{
		string gameSettingsPlayerPrefsKey = GetGameSettingsPlayerPrefsKey();
		if (string.IsNullOrEmpty(gameSettingsPlayerPrefsKey))
		{
			return;
		}
		string value = JsonUtility.ToJson(GameSettings.Instance);
		LazyAPI.PlayerPrefs.SetString(gameSettingsPlayerPrefsKey, value);
		try
		{
			LazyAPI.PlayerPrefs.Save();
		}
		catch (Exception arg)
		{
			Debug.LogError($"Error during save GameSettings:[{arg}]");
		}
	}

	private static string GetGameSettingsPlayerPrefsKey()
	{
		return "settings";
	}
}
