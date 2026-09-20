using System;
using System.IO;
using LazyBearTechnology;
using UnityEngine;

public static class SteamWorkshopCreatorConfig
{
	[Serializable]
	private class FileData
	{
		public bool workshopCreatorMode;
	}

	private const string DefaultFileContents = "{\n  // Enables the Steam Workshop Creator window (Shift+F11).\n  // Keep this false unless you are publishing translations to the Workshop.\n  \"workshopCreatorMode\": false\n}\n";

	public static bool Enabled { get; private set; }

	public static void EnsureAndReload()
	{
		Enabled = false;
		string workshopConfigPath = ModsPaths.WorkshopConfigPath;
		try
		{
			string directoryName = Path.GetDirectoryName(workshopConfigPath);
			if (!string.IsNullOrEmpty(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			if (!File.Exists(workshopConfigPath))
			{
				File.WriteAllText(workshopConfigPath, "{\n  // Enables the Steam Workshop Creator window (Shift+F11).\n  // Keep this false unless you are publishing translations to the Workshop.\n  \"workshopCreatorMode\": false\n}\n");
			}
			string text = ModsJsonComments.Strip(File.ReadAllText(workshopConfigPath));
			if (!string.IsNullOrWhiteSpace(text))
			{
				Enabled = JsonUtility.FromJson<FileData>(text)?.workshopCreatorMode ?? false;
				Debug.Log($"[SteamWorkshopCreator] workshopCreatorMode={Enabled} ({workshopConfigPath})");
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[SteamWorkshopCreator] Failed to read '" + workshopConfigPath + "': " + ex.Message);
			Enabled = false;
		}
	}

	public static void Tick()
	{
		if (Enabled && Input.GetKeyDown(KeyCode.F11) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
		{
			UISteamWorkshopCreatorWindow.Toggle();
		}
	}
}
