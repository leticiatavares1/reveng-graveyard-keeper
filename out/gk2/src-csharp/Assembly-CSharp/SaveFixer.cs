using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class SaveFixer
{
	private struct LoadedFixes
	{
		public List<SaveFixData> Fixes;

		public bool LoadFailed;

		public AsyncOperationHandle<IList<IResourceLocation>> LocationsHandle;

		public AsyncOperationHandle<IList<SaveFixData>> AssetsHandle;

		public static LoadedFixes FromExisting(SaveFixData fix)
		{
			LoadedFixes result = default(LoadedFixes);
			result.Fixes = new List<SaveFixData> { fix };
			return result;
		}

		public void Release()
		{
			if (AssetsHandle.IsValid())
			{
				Addressables.Release(AssetsHandle);
			}
			if (LocationsHandle.IsValid())
			{
				Addressables.Release(LocationsHandle);
			}
		}
	}

	public static bool Apply(GameSave gameSave, IList<GameSceneConfig> gameSceneConfigs)
	{
		if (gameSave == null || string.IsNullOrEmpty(gameSave.GameSaveVer))
		{
			return false;
		}
		return ApplyInternal(gameSave, gameSceneConfigs, null);
	}

	public static void ApplyForce(GameSave gameSave, IList<GameSceneConfig> gameSceneConfigs, SaveFixData forceFix)
	{
		if (gameSave != null && !string.IsNullOrEmpty(gameSave.GameSaveVer) && !(forceFix == null))
		{
			ApplyInternal(gameSave, gameSceneConfigs, forceFix);
		}
	}

	public static void LogApplicableFixes(GameSave gameSave)
	{
		if (!TryParseSaveVersion(gameSave, out var _))
		{
			Debug.LogError("[SaveFixer] Save version [" + gameSave?.GameSaveVer + "] is not parseable, skip applying fixes");
			return;
		}
		LoadedFixes loadedFixes = LoadFixesInRange(gameSave, null);
		try
		{
			if (loadedFixes.LoadFailed)
			{
				Debug.LogError("[SaveFixer] Failed to load fix assets. Save version [" + gameSave?.GameSaveVer + "] client [" + LazySingletonSO<GameInfo>.Instance?.Version + "]");
				return;
			}
			if (loadedFixes.Fixes.Count == 0)
			{
				Debug.Log("[SaveFixer] No fixes to apply. Save version [" + gameSave?.GameSaveVer + "] client [" + LazySingletonSO<GameInfo>.Instance?.Version + "]");
				return;
			}
			Debug.Log("[SaveFixer] Fixes that would apply to save [" + gameSave?.GameSaveVer + "] (client [" + LazySingletonSO<GameInfo>.Instance?.Version + "]):");
			foreach (SaveFixData fix in loadedFixes.Fixes)
			{
				int num = CountEnabledOperations(fix);
				Debug.Log($"[SaveFixer]  - {fix.name} ({num} operations)");
			}
		}
		finally
		{
			loadedFixes.Release();
		}
	}

	private static bool ApplyInternal(GameSave gameSave, IList<GameSceneConfig> gameSceneConfigs, SaveFixData forceFix)
	{
		if (forceFix == null && !TryParseSaveVersion(gameSave, out var _))
		{
			Debug.LogError("[SaveFixer] Save version [" + gameSave.GameSaveVer + "] is not parseable, keep save version");
			return false;
		}
		LoadedFixes loadedFixes = LoadFixesInRange(gameSave, forceFix);
		SaveFixContext saveFixContext = null;
		try
		{
			if (loadedFixes.LoadFailed)
			{
				Debug.LogError("[SaveFixer] Failed to load fix assets, keep save version [" + gameSave.GameSaveVer + "]");
				return false;
			}
			bool flag = loadedFixes.Fixes.Count > 0;
			bool flag2 = forceFix == null && ShouldApplyCodeFixes(gameSave);
			if (!flag && !flag2)
			{
				Debug.Log("[SaveFixer] No fixes to apply. Save version [" + gameSave.GameSaveVer + "] client [" + LazySingletonSO<GameInfo>.Instance?.Version + "]");
				return true;
			}
			saveFixContext = new SaveFixContext(gameSave, gameSceneConfigs);
			bool flag3 = false;
			if (flag2)
			{
				saveFixContext.Log("Applying code fixes to save [" + gameSave.GameSaveVer + "] → [" + LazySingletonSO<GameInfo>.Instance?.Version + "]");
				flag3 |= ApplyCodeFixes(saveFixContext);
			}
			if (flag)
			{
				saveFixContext.Log($"Applying {loadedFixes.Fixes.Count} fix asset(s) to save [{gameSave.GameSaveVer}] → [{LazySingletonSO<GameInfo>.Instance?.Version}]");
				foreach (SaveFixData fix in loadedFixes.Fixes)
				{
					flag3 |= ApplyFix(saveFixContext, fix);
				}
			}
			if (flag3)
			{
				Debug.LogError("[SaveFixer] One or more operations failed, keep save version [" + gameSave.GameSaveVer + "]");
				return false;
			}
			return true;
		}
		finally
		{
			saveFixContext?.UnloadLoadedContent();
			loadedFixes.Release();
		}
	}

	private static bool ApplyFix(SaveFixContext ctx, SaveFixData fix)
	{
		if (fix?.Operations == null)
		{
			return false;
		}
		bool result = false;
		ctx.Log("Applying fix [" + fix.name + "]");
		foreach (SaveFixOperation operation in fix.Operations)
		{
			if (operation != null && operation.isEnabled)
			{
				try
				{
					operation.Apply(ctx);
				}
				catch (Exception exception)
				{
					result = true;
					Debug.LogError("[SaveFixer] Operation [" + operation.Summary + "] in [" + fix.name + "] failed");
					Debug.LogException(exception);
				}
			}
		}
		return result;
	}

	private static LoadedFixes LoadFixesInRange(GameSave gameSave, SaveFixData forceFix)
	{
		if (forceFix != null)
		{
			return LoadedFixes.FromExisting(forceFix);
		}
		TryGetVersionRange(gameSave, out var saveVersion, out var clientVersion);
		LoadedFixes loadedFixes = default(LoadedFixes);
		loadedFixes.Fixes = new List<SaveFixData>();
		LoadedFixes result = loadedFixes;
		try
		{
			result.LocationsHandle = Addressables.LoadResourceLocationsAsync("SaveFixerAssets", typeof(SaveFixData));
			IList<IResourceLocation> list = result.LocationsHandle.WaitForCompletion();
			if (result.LocationsHandle.Status != AsyncOperationStatus.Succeeded || list == null)
			{
				result.LoadFailed = true;
				Debug.LogError("[SaveFixer] Failed to load SaveFixer asset locations");
				return result;
			}
			List<IResourceLocation> list2 = new List<IResourceLocation>();
			foreach (IResourceLocation item in list)
			{
				if (!TryGetLocationVersion(item, out var version))
				{
					Debug.LogWarning("[SaveFixer] Skip location [" + item?.PrimaryKey + "] — version is not parseable");
				}
				else if (IsInApplyRange(version, saveVersion, clientVersion))
				{
					list2.Add(item);
				}
			}
			if (list2.Count == 0)
			{
				return result;
			}
			list2.Sort(CompareLocationVersions);
			result.AssetsHandle = Addressables.LoadAssetsAsync<SaveFixData>(list2, null);
			IList<SaveFixData> list3 = result.AssetsHandle.WaitForCompletion();
			if (result.AssetsHandle.Status != AsyncOperationStatus.Succeeded || list3 == null)
			{
				result.LoadFailed = true;
				Debug.LogError("[SaveFixer] Failed to load filtered SaveFixer assets");
				return result;
			}
			foreach (SaveFixData item2 in list3)
			{
				if (item2 != null)
				{
					result.Fixes.Add(item2);
				}
			}
			result.Fixes.Sort(CompareFixVersions);
		}
		catch (Exception ex)
		{
			result.LoadFailed = true;
			Debug.LogError("[SaveFixer] Failed to load SaveFixer assets: " + ex.Message);
		}
		return result;
	}

	private static bool TryParseSaveVersion(GameSave gameSave, out GameSaveVersion saveVersion)
	{
		saveVersion = default(GameSaveVersion);
		if (gameSave != null)
		{
			return GameSaveVersion.TryParse(gameSave.GameSaveVer, out saveVersion);
		}
		return false;
	}

	private static void TryGetVersionRange(GameSave gameSave, out GameSaveVersion? saveVersion, out GameSaveVersion? clientVersion)
	{
		saveVersion = null;
		clientVersion = null;
		if (TryParseSaveVersion(gameSave, out var saveVersion2))
		{
			saveVersion = saveVersion2;
		}
		if (LazySingletonSO<GameInfo>.Instance != null && GameSaveVersion.TryParse(LazySingletonSO<GameInfo>.Instance.Version, out var result))
		{
			clientVersion = result;
		}
	}

	public static bool IsInApplyRange(GameSaveVersion fixVersion, GameSaveVersion? saveVersion, GameSaveVersion? clientVersion)
	{
		if (!saveVersion.HasValue)
		{
			return false;
		}
		if (clientVersion.HasValue && saveVersion.Value >= clientVersion.Value)
		{
			return false;
		}
		if (fixVersion.NumberAsInt < saveVersion.Value.NumberAsInt)
		{
			return false;
		}
		if (clientVersion.HasValue && fixVersion.NumberAsInt > clientVersion.Value.NumberAsInt)
		{
			return false;
		}
		return true;
	}

	private static bool ShouldApplyCodeFixes(GameSave gameSave)
	{
		TryGetVersionRange(gameSave, out var saveVersion, out var clientVersion);
		if (!saveVersion.HasValue)
		{
			return false;
		}
		if (clientVersion.HasValue && saveVersion.Value >= clientVersion.Value)
		{
			return false;
		}
		return true;
	}

	private static bool ApplyCodeFixes(SaveFixContext ctx)
	{
		try
		{
			SaveCodeFixes.Apply(ctx);
			return false;
		}
		catch (Exception exception)
		{
			Debug.LogError("[SaveFixer] Code fixes failed");
			Debug.LogException(exception);
			return true;
		}
	}

	private static bool TryGetLocationVersion(IResourceLocation location, out GameSaveVersion version)
	{
		version = default(GameSaveVersion);
		if (location == null || string.IsNullOrEmpty(location.PrimaryKey))
		{
			return false;
		}
		return GameSaveVersion.TryParse(GetAddressableAssetName(location.PrimaryKey), out version);
	}

	private static string GetAddressableAssetName(string address)
	{
		int num = address.LastIndexOf('/');
		string text = ((num >= 0) ? address.Substring(num + 1) : address);
		if (text.EndsWith(".asset"))
		{
			text = text.Substring(0, text.Length - ".asset".Length);
		}
		return text;
	}

	private static int CompareLocationVersions(IResourceLocation a, IResourceLocation b)
	{
		TryGetLocationVersion(a, out var version);
		TryGetLocationVersion(b, out var version2);
		return version.CompareTo(version2);
	}

	private static int CompareFixVersions(SaveFixData a, SaveFixData b)
	{
		a.TryGetVersion(out var parsed);
		b.TryGetVersion(out var parsed2);
		return parsed.CompareTo(parsed2);
	}

	private static int CountEnabledOperations(SaveFixData fix)
	{
		if (fix?.Operations == null)
		{
			return 0;
		}
		int num = 0;
		foreach (SaveFixOperation operation in fix.Operations)
		{
			if (operation != null && operation.isEnabled)
			{
				num++;
			}
		}
		return num;
	}
}
