using System;
using System.Globalization;
using LazyBearTechnology;
using UnityEngine;

public class SaveSlotData : ISerializableData
{
	[NonSerialized]
	public string slotName;

	public int day;

	public bool isAutoSave;

	public string saveDateTime;

	public string serializedCulture;

	public string platform;

	public string gameSaveVersion;

	public int graveyardQuality;

	public int churchQuality;

	public int villageRep;

	public bool repValue;

	public bool isDemoSave;

	public bool importedFromDemo;

	public bool ShouldConvertDemoProgressOnLoad
	{
		get
		{
			if (!isDemoSave)
			{
				return importedFromDemo;
			}
			return true;
		}
	}

	public bool IsDemoSlotDeleted
	{
		get
		{
			return RemovedDemoSavesList.IsDeleted(slotName);
		}
		set
		{
			if (value)
			{
				RemovedDemoSavesList.TryAddSave(slotName);
			}
			else
			{
				RemovedDemoSavesList.TryRemoveSave(slotName);
			}
		}
	}

	public SaveSlotData Copy()
	{
		SaveSlotData saveSlotData = new SaveSlotData();
		saveSlotData.slotName = slotName;
		saveSlotData.day = day;
		saveSlotData.platform = platform;
		saveSlotData.gameSaveVersion = gameSaveVersion;
		saveSlotData.isAutoSave = isAutoSave;
		saveSlotData.saveDateTime = saveDateTime;
		saveSlotData.serializedCulture = serializedCulture;
		saveSlotData.isAutoSave = isAutoSave;
		saveSlotData.isDemoSave = isDemoSave;
		saveSlotData.importedFromDemo = importedFromDemo;
		saveSlotData.graveyardQuality = graveyardQuality;
		saveSlotData.churchQuality = churchQuality;
		saveSlotData.villageRep = villageRep;
		saveSlotData.repValue = repValue;
		return saveSlotData;
	}

	public DateTime GetSaveDateTime()
	{
		try
		{
			if (!string.IsNullOrEmpty(serializedCulture))
			{
				return DateTime.Parse(saveDateTime, new CultureInfo(serializedCulture));
			}
			return DateTime.Parse(saveDateTime);
		}
		catch (Exception)
		{
			Debug.LogError("Cant parse save slot:[" + slotName + "] DateTime!!!");
			return default(DateTime);
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterSerialize()
	{
	}
}
