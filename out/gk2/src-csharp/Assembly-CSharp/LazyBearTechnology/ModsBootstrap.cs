using System;
using System.IO;
using UnityEngine;

namespace LazyBearTechnology;

public static class ModsBootstrap
{
	private static bool workshopScanAttempted;

	public static void EnsureOnStartup()
	{
		workshopScanAttempted = false;
		try
		{
			Directory.CreateDirectory(ModsPaths.Root);
			File.WriteAllText(Path.Combine(ModsPaths.Root, "README.txt"), ModsDocumentation.Load("Mods/README"));
			VoiceOverModLoader.SearchRootsProvider = ModsPaths.GetModSearchRoots;
			SteamWorkshopCreatorConfig.EnsureAndReload();
			LanguageModLoader.Scan();
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[Mods] Failed to initialize mods folder: " + ex.Message);
		}
	}

	public static void Tick()
	{
		SteamWorkshopInstalledItems.Tick();
		if (!workshopScanAttempted && SteamWorkshopInstalledItems.IsSteamReady())
		{
			workshopScanAttempted = true;
			ReloadMods(scaffold: false);
		}
		else if (SteamWorkshopInstalledItems.PendingRescan)
		{
			SteamWorkshopInstalledItems.PendingRescan = false;
			ReloadMods(scaffold: false);
		}
		if (Input.GetKeyDown(KeyCode.F10) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
		{
			ReloadAndScaffold();
		}
	}

	public static void ReloadAndScaffold()
	{
		ReloadMods(scaffold: true);
	}

	private static void ReloadMods(bool scaffold)
	{
		try
		{
			Directory.CreateDirectory(ModsPaths.Root);
			SteamWorkshopCreatorConfig.EnsureAndReload();
			if (scaffold)
			{
				File.WriteAllText(Path.Combine(ModsPaths.Root, "README.txt"), ModsDocumentation.Load("Mods/README"));
				Directory.CreateDirectory(ModsPaths.LanguagesRoot);
				File.WriteAllText(Path.Combine(ModsPaths.LanguagesRoot, "README.txt"), ModsDocumentation.Load("Mods/Languages/README"));
				LanguageModLoader.EnsureExamplePack();
			}
			LanguageModLoader.Scan();
			if (GameSettings.Instance != null)
			{
				GameSettings.Instance.ApplyLanguageSettings();
			}
			if (GUIElements.Instance != null)
			{
				GUIElements.Instance.UpdateLocalizedLabels();
				TextStyleComponent.RefreshAll();
				LazyButton.RefreshAll();
			}
			UIGameSettingsWindow.RefreshLanguageSwitcherIfOpen();
			Debug.Log("[Mods] Reloaded. Folders: " + string.Join(" | ", ModsPaths.GetModSearchRoots()));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[Mods] Reload failed: " + ex.Message);
		}
	}
}
