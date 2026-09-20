using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SteamWorkshopCreatorStore
{
	[Serializable]
	private class FileData
	{
		public List<SteamWorkshopItemRecord> items = new List<SteamWorkshopItemRecord>();
	}

	private const string FolderName = "SteamWorkshop";

	private const string FileName = "workshop_items.json";

	public static string FilePath => Path.Combine(Application.persistentDataPath, "SteamWorkshop", "workshop_items.json");

	public static List<SteamWorkshopItemRecord> Load()
	{
		try
		{
			if (!File.Exists(FilePath))
			{
				return new List<SteamWorkshopItemRecord>();
			}
			string text = File.ReadAllText(FilePath);
			if (string.IsNullOrWhiteSpace(text))
			{
				return new List<SteamWorkshopItemRecord>();
			}
			FileData fileData = JsonUtility.FromJson<FileData>(text);
			if (fileData?.items == null)
			{
				return new List<SteamWorkshopItemRecord>();
			}
			return fileData.items;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[SteamWorkshopCreator] Failed to load items: " + ex.Message);
			return new List<SteamWorkshopItemRecord>();
		}
	}

	public static void Save(List<SteamWorkshopItemRecord> items)
	{
		try
		{
			string directoryName = Path.GetDirectoryName(FilePath);
			if (!string.IsNullOrEmpty(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			FileData obj = new FileData
			{
				items = (items ?? new List<SteamWorkshopItemRecord>())
			};
			File.WriteAllText(FilePath, JsonUtility.ToJson(obj, prettyPrint: true));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[SteamWorkshopCreator] Failed to save items: " + ex.Message);
		}
	}
}
