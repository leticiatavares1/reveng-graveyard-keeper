using System;
using System.Collections.Generic;
using System.Text;
using LazyBearTechnology;
using UnityEngine;

public static class DLCEngine
{
	private static readonly Dictionary<DLCVersion, DLCInfo> dlcInfoByVersion = new Dictionary<DLCVersion, DLCInfo> { 
	{
		DLCVersion.Preorder,
		new DLCInfo
		{
			steamAppId = 5021030u,
			gogAppId = 0uL,
			epicGamesAppId = "",
			dlcIndex = 1,
			xboxAppId = "9N9SDNT9112W",
			ps4EntitlementLabel = "GK2PREORDERDLC00",
			ps5EntitlementLabel = "GK2PREORDERDLC00"
		}
	} };

	private static readonly Dictionary<DLCVersion, StoreProductInfo> storeProductInfoByVersion = new Dictionary<DLCVersion, StoreProductInfo> { 
	{
		DLCVersion.Preorder,
		new StoreProductInfo
		{
			steamUrl = "steam://advertise/5021030",
			epicGamesUrl = "",
			gogUrl = "",
			xboxProductId = "9N9SDNT9112W",
			nintendoApplicationId = "0100005027a18000",
			nintendo2ApplicationId = "0400543027ffc000",
			ps4ProductLabel = "GK2PREORDERDLC00",
			ps5ProductLabel = "GK2PREORDERDLC00"
		}
	} };

	private static readonly Dictionary<DLCVersion, bool> dlcStateByVersion = new Dictionary<DLCVersion, bool>();

	private static bool isDLCStateCached;

	public static IReadOnlyDictionary<DLCVersion, DLCInfo> DLCInfos => dlcInfoByVersion;

	public static IReadOnlyDictionary<DLCVersion, StoreProductInfo> StoreProductInfos => storeProductInfoByVersion;

	public static IEnumerable<DLCVersion> ConfiguredDLCVersions => dlcInfoByVersion.Keys;

	public static bool IsDLCAvailable(DLCVersion dlcVersion)
	{
		if (dlcVersion == DLCVersion.None)
		{
			return true;
		}
		return false;
	}

	public static void LogDLCStates()
	{
		StringBuilder stringBuilder = new StringBuilder("[DLCEngine] DLC states:");
		foreach (DLCVersion value in Enum.GetValues(typeof(DLCVersion)))
		{
			if (value != 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append($"[{value}]: {IsDLCAvailable(value)}");
			}
		}
		Debug.Log(stringBuilder.ToString());
	}

	private static bool IsDLCAvailableReal(DLCVersion dlcVersion)
	{
		bool result = true;
		if (dlcInfoByVersion.ContainsKey(dlcVersion))
		{
			SetDLCStates();
			result = dlcStateByVersion.TryGetValue(dlcVersion, out var value) && value;
		}
		return result;
	}

	private static void SetDLCStates()
	{
		if (isDLCStateCached)
		{
			return;
		}
		foreach (KeyValuePair<DLCVersion, DLCInfo> item in dlcInfoByVersion)
		{
			dlcStateByVersion[item.Key] = LazyAPI.Platform.IsDLCAvailable(item.Value);
		}
		isDLCStateCached = true;
		LogDLCStates();
	}

	public static void ResetDLCStateCached()
	{
		isDLCStateCached = false;
		dlcStateByVersion.Clear();
		LogDLCStates();
	}

	public static void OpenDlcInStore(DLCVersion dlcVersion)
	{
		if (!storeProductInfoByVersion.TryGetValue(dlcVersion, out var value))
		{
			Debug.LogError($"There is no store product info for DLC [{dlcVersion}]");
			return;
		}
		LazyAPI.Platform.OpenProductInStore(value);
		LazyAudio.PlayAndForget("gui_click");
	}
}
