using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Steamworks;
using UnityEngine;

public static class SteamWorkshopInstalledItems
{
	private const uint InstallFolderBufferBytes = 4096u;

	private const float DownloadRetrySeconds = 60f;

	private const float EnsureIntervalSeconds = 2f;

	private static Callback<DownloadItemResult_t> downloadCallback;

	private static Callback<ItemInstalled_t> installedCallback;

	private static CallResult<SteamUGCQueryCompleted_t> queryResult;

	private static CallResult<RemoteStorageSubscribePublishedFileResult_t> subscribeResult;

	private static readonly HashSet<ulong> downloadInFlight = new HashSet<ulong>();

	private static readonly HashSet<ulong> subscribeQueued = new HashSet<ulong>();

	private static readonly HashSet<ulong> subscribeAttempted = new HashSet<ulong>();

	private static readonly HashSet<ulong> ignoredIds = new HashSet<ulong>();

	private static readonly Queue<PublishedFileId_t> subscribeQueue = new Queue<PublishedFileId_t>();

	private static readonly Dictionary<ulong, float> nextRetryUnscaledTime = new Dictionary<ulong, float>();

	private static float lastEnsureUnscaledTime = -100f;

	private static bool detailsQueried;

	private static bool detailsReady;

	private static bool loggedAppInstall;

	private static bool subscribeCallInFlight;

	public static bool PendingRescan { get; set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatics()
	{
		downloadCallback = null;
		installedCallback = null;
		queryResult = null;
		subscribeResult = null;
		downloadInFlight.Clear();
		subscribeQueued.Clear();
		subscribeAttempted.Clear();
		ignoredIds.Clear();
		subscribeQueue.Clear();
		nextRetryUnscaledTime.Clear();
		lastEnsureUnscaledTime = -100f;
		detailsQueried = false;
		detailsReady = false;
		loggedAppInstall = false;
		subscribeCallInFlight = false;
		PendingRescan = false;
	}

	public static bool IsSteamReady()
	{
		if (Time.frameCount <= 0)
		{
			return false;
		}
		return SteamManager.Initialized;
	}

	public static void Tick()
	{
		if (!IsSteamReady())
		{
			return;
		}
		if (downloadCallback == null)
		{
			downloadCallback = Callback<DownloadItemResult_t>.Create(OnDownloadItem);
		}
		if (installedCallback == null)
		{
			installedCallback = Callback<ItemInstalled_t>.Create(OnItemInstalled);
		}
		if (queryResult == null)
		{
			queryResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnQueryCompleted);
		}
		if (subscribeResult == null)
		{
			subscribeResult = CallResult<RemoteStorageSubscribePublishedFileResult_t>.Create(OnSubscribed);
		}
		if (!(Time.unscaledTime - lastEnsureUnscaledTime < 2f))
		{
			lastEnsureUnscaledTime = Time.unscaledTime;
			if (!loggedAppInstall)
			{
				LogAppInstall();
			}
			EnsureDownloads();
		}
	}

	public static IReadOnlyList<string> GetInstalledFolders()
	{
		List<string> list = new List<string>();
		if (!IsSteamReady())
		{
			return list;
		}
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		if (numSubscribedItems == 0)
		{
			return list;
		}
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		uint subscribedItems = SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		for (uint num = 0u; num < subscribedItems; num++)
		{
			PublishedFileId_t nPublishedFileID = array[num];
			if (!ignoredIds.Contains(nPublishedFileID.m_PublishedFileId) && (SteamUGC.GetItemState(nPublishedFileID) & 2) == 0)
			{
				if (SteamUGC.GetItemInstallInfo(nPublishedFileID, out var _, out var pchFolder, 4096u, out var _) && !string.IsNullOrEmpty(pchFolder) && Directory.Exists(pchFolder))
				{
					AddFolder(list, pchFolder);
				}
				else
				{
					AddFolder(list, FindWorkshopContentFolder(nPublishedFileID.m_PublishedFileId));
				}
			}
		}
		return list;
	}

	private static void EnsureDownloads()
	{
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		if (numSubscribedItems == 0)
		{
			return;
		}
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		uint subscribedItems = SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		if (!detailsQueried)
		{
			detailsQueried = true;
			QueryDetails(array, subscribedItems);
		}
		else
		{
			if (!detailsReady)
			{
				return;
			}
			for (uint num = 0u; num < subscribedItems; num++)
			{
				PublishedFileId_t publishedFileId_t = array[num];
				ulong publishedFileId = publishedFileId_t.m_PublishedFileId;
				if (ignoredIds.Contains(publishedFileId))
				{
					continue;
				}
				uint itemState = SteamUGC.GetItemState(publishedFileId_t);
				bool flag = (itemState & 1) != 0;
				bool num2 = (itemState & 4) != 0;
				bool flag2 = (itemState & 8) != 0;
				bool flag3 = (itemState & 0x10) != 0;
				bool flag4 = (itemState & 0x20) != 0;
				if ((!num2 || flag2) && !(flag3 || flag4) && !downloadInFlight.Contains(publishedFileId) && (!nextRetryUnscaledTime.TryGetValue(publishedFileId, out var value) || !(Time.unscaledTime < value)))
				{
					if (!flag && !subscribeAttempted.Contains(publishedFileId))
					{
						EnqueueSubscribe(publishedFileId_t);
					}
					else
					{
						QueueDownload(publishedFileId_t, itemState);
					}
				}
			}
			PumpSubscribeQueue();
		}
	}

	private static void EnqueueSubscribe(PublishedFileId_t id)
	{
		ulong publishedFileId = id.m_PublishedFileId;
		if (!subscribeQueued.Contains(publishedFileId) && !subscribeAttempted.Contains(publishedFileId))
		{
			subscribeQueued.Add(publishedFileId);
			subscribeQueue.Enqueue(id);
		}
	}

	private static void PumpSubscribeQueue()
	{
		if (!subscribeCallInFlight && subscribeQueue.Count != 0)
		{
			PublishedFileId_t nPublishedFileID = subscribeQueue.Dequeue();
			ulong publishedFileId = nPublishedFileID.m_PublishedFileId;
			subscribeQueued.Remove(publishedFileId);
			SteamAPICall_t steamAPICall_t = SteamUGC.SubscribeItem(nPublishedFileID);
			if (steamAPICall_t == SteamAPICall_t.Invalid)
			{
				nextRetryUnscaledTime[publishedFileId] = Time.unscaledTime + 60f;
				Debug.Log($"[SteamWorkshop] SubscribeItem returned invalid call for {publishedFileId}. Retry in {60f:0}s.");
				return;
			}
			subscribeCallInFlight = true;
			subscribeAttempted.Add(publishedFileId);
			subscribeResult.Set(steamAPICall_t);
			uint itemState = SteamUGC.GetItemState(nPublishedFileID);
			Debug.Log($"[SteamWorkshop] SubscribeItem {publishedFileId} state={FormatState(itemState)}.");
		}
	}

	private static void QueueDownload(PublishedFileId_t id, uint state)
	{
		ulong publishedFileId = id.m_PublishedFileId;
		if (SteamUGC.DownloadItem(id, bHighPriority: true))
		{
			downloadInFlight.Add(publishedFileId);
			SteamUGC.GetItemDownloadInfo(id, out var punBytesDownloaded, out var punBytesTotal);
			Debug.Log($"[SteamWorkshop] Queued download for {publishedFileId} state={FormatState(state)} downloaded={punBytesDownloaded}/{punBytesTotal}.");
		}
		else
		{
			nextRetryUnscaledTime[publishedFileId] = Time.unscaledTime + 60f;
			Debug.Log($"[SteamWorkshop] DownloadItem returned false for {publishedFileId} state={FormatState(state)}. Retry in {60f:0}s.");
		}
	}

	private static void QueryDetails(PublishedFileId_t[] ids, uint count)
	{
		if (count != 0)
		{
			UGCQueryHandle_t uGCQueryHandle_t = SteamUGC.CreateQueryUGCDetailsRequest(ids, count);
			if (!(uGCQueryHandle_t == UGCQueryHandle_t.Invalid))
			{
				SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(uGCQueryHandle_t);
				queryResult.Set(hAPICall);
			}
		}
	}

	private static void OnQueryCompleted(SteamUGCQueryCompleted_t result, bool ioFailure)
	{
		detailsReady = true;
		if (ioFailure || result.m_eResult != EResult.k_EResultOK)
		{
			Debug.Log("[SteamWorkshop] UGC details query failed: " + (ioFailure ? "IO failure" : result.m_eResult.ToString()));
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[SteamWorkshop] Item details:");
		for (uint num = 0u; num < result.m_unNumResultsReturned; num++)
		{
			if (SteamUGC.GetQueryUGCResult(result.m_handle, num, out var pDetails))
			{
				ulong publishedFileId = pDetails.m_nPublishedFileId.m_PublishedFileId;
				stringBuilder.Append("\n  id=").Append(publishedFileId);
				stringBuilder.Append(" title='").Append(pDetails.m_rgchTitle).Append("'");
				stringBuilder.Append(" fileSize=").Append(pDetails.m_nFileSize);
				stringBuilder.Append(" totalFilesSize=").Append(pDetails.m_ulTotalFilesSize);
				stringBuilder.Append(" visibility=").Append(pDetails.m_eVisibility);
				stringBuilder.Append(" result=").Append(pDetails.m_eResult);
				if (pDetails.m_eResult == EResult.k_EResultFileNotFound)
				{
					ignoredIds.Add(publishedFileId);
					SteamUGC.UnsubscribeItem(pDetails.m_nPublishedFileId);
					stringBuilder.Append(" (dropped)");
				}
			}
		}
		SteamUGC.ReleaseQueryUGCRequest(result.m_handle);
		Debug.Log(stringBuilder.ToString());
	}

	private static void OnSubscribed(RemoteStorageSubscribePublishedFileResult_t result, bool ioFailure)
	{
		subscribeCallInFlight = false;
		ulong publishedFileId = result.m_nPublishedFileId.m_PublishedFileId;
		uint itemState = SteamUGC.GetItemState(result.m_nPublishedFileId);
		if (!ioFailure && result.m_eResult == EResult.k_EResultFileNotFound)
		{
			ignoredIds.Add(publishedFileId);
			SteamUGC.UnsubscribeItem(result.m_nPublishedFileId);
			Debug.Log($"[SteamWorkshop] SubscribeItem FileNotFound id={publishedFileId}. Dropping (deleted or never published).");
			PumpSubscribeQueue();
		}
		else if (ioFailure || result.m_eResult != EResult.k_EResultOK)
		{
			nextRetryUnscaledTime[publishedFileId] = Time.unscaledTime + 60f;
			subscribeAttempted.Remove(publishedFileId);
			Debug.Log(string.Format("[SteamWorkshop] SubscribeItem failed id={0} result={1} state={2}. Retry in {3:0}s.", publishedFileId, ioFailure ? "IO failure" : result.m_eResult.ToString(), FormatState(itemState), 60f));
			PumpSubscribeQueue();
		}
		else
		{
			Debug.Log($"[SteamWorkshop] SubscribeItem ok id={publishedFileId} state={FormatState(itemState)}.");
			QueueDownload(result.m_nPublishedFileId, itemState);
			PumpSubscribeQueue();
		}
	}

	private static void LogAppInstall()
	{
		loggedAppInstall = true;
		AppId_t appID = SteamUtils.GetAppID();
		bool flag = SteamApps.BIsAppInstalled(appID);
		bool flag2 = SteamApps.BIsSubscribedApp(appID);
		string pchFolder;
		uint appInstallDir = SteamApps.GetAppInstallDir(appID, out pchFolder, 1024u);
		Debug.Log($"[SteamWorkshop] App {appID} BIsAppInstalled={flag} BIsSubscribedApp={flag2} " + "installDir='" + ((appInstallDir != 0) ? pchFolder : "") + "'.");
	}

	private static void OnDownloadItem(DownloadItemResult_t result)
	{
		ulong publishedFileId = result.m_nPublishedFileId.m_PublishedFileId;
		downloadInFlight.Remove(publishedFileId);
		uint itemState = SteamUGC.GetItemState(result.m_nPublishedFileId);
		AppId_t appID = SteamUtils.GetAppID();
		if (result.m_eResult == EResult.k_EResultOK)
		{
			Debug.Log($"[SteamWorkshop] Download finished id={publishedFileId} appId={result.m_unAppID} localAppId={appID} state={FormatState(itemState)}.");
			PendingRescan = true;
		}
		else
		{
			nextRetryUnscaledTime[publishedFileId] = Time.unscaledTime + 60f;
			Debug.Log($"[SteamWorkshop] Download failed id={publishedFileId} result={result.m_eResult} appId={result.m_unAppID} localAppId={appID} state={FormatState(itemState)}. " + "If state lacks Subscribed, the Steam client is skipping the item: check Steam/logs/workshop_log.txt for 'No workshop depot defined' (Steamworks App Admin > Workshop > General > Workshop Depot, then Publish and restart Steam). " + $"Retry in {60f:0}s.");
		}
	}

	private static void OnItemInstalled(ItemInstalled_t result)
	{
		if (!(result.m_unAppID != SteamUtils.GetAppID()))
		{
			Debug.Log($"[SteamWorkshop] ItemInstalled id={result.m_nPublishedFileId.m_PublishedFileId}.");
			PendingRescan = true;
		}
	}

	private static string FormatState(uint state)
	{
		if (state == 0)
		{
			return "None";
		}
		List<string> list = new List<string>();
		if ((state & (true ? 1u : 0u)) != 0)
		{
			list.Add("Subscribed");
		}
		if ((state & 2u) != 0)
		{
			list.Add("Legacy");
		}
		if ((state & 4u) != 0)
		{
			list.Add("Installed");
		}
		if ((state & 8u) != 0)
		{
			list.Add("NeedsUpdate");
		}
		if ((state & 0x10u) != 0)
		{
			list.Add("Downloading");
		}
		if ((state & 0x20u) != 0)
		{
			list.Add("DownloadPending");
		}
		if ((state & 0x40u) != 0)
		{
			list.Add("DisabledLocally");
		}
		return string.Format("{0}({1})", state, string.Join("|", list));
	}

	private static string FindWorkshopContentFolder(ulong publishedFileId)
	{
		string text = SteamUtils.GetAppID().m_AppId.ToString();
		string text2 = publishedFileId.ToString();
		List<string> steamLibraryPaths = GetSteamLibraryPaths();
		for (int i = 0; i < steamLibraryPaths.Count; i++)
		{
			string text3 = Path.Combine(steamLibraryPaths[i], "steamapps", "workshop", "content", text, text2);
			if (Directory.Exists(text3))
			{
				return text3;
			}
		}
		return null;
	}

	private static List<string> GetSteamLibraryPaths()
	{
		List<string> list = new List<string>();
		string steamInstallPath = GetSteamInstallPath();
		if (string.IsNullOrEmpty(steamInstallPath))
		{
			return list;
		}
		AddFolder(list, steamInstallPath);
		string path = Path.Combine(steamInstallPath, "steamapps", "libraryfolders.vdf");
		if (!File.Exists(path))
		{
			return list;
		}
		try
		{
			string[] array = File.ReadAllLines(path);
			foreach (string text in array)
			{
				int num = text.IndexOf("\"path\"", StringComparison.OrdinalIgnoreCase);
				if (num < 0)
				{
					continue;
				}
				int num2 = text.IndexOf('"', num + 6);
				if (num2 >= 0)
				{
					int num3 = text.IndexOf('"', num2 + 1);
					if (num3 >= 0)
					{
						AddFolder(list, text.Substring(num2 + 1, num3 - num2 - 1).Replace("\\\\", "\\"));
					}
				}
			}
		}
		catch
		{
		}
		return list;
	}

	private static string GetSteamInstallPath()
	{
		try
		{
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
			if (registryKey?.GetValue("SteamPath") is string text && !string.IsNullOrWhiteSpace(text))
			{
				return text.Replace('/', Path.DirectorySeparatorChar);
			}
		}
		catch
		{
		}
		return null;
	}

	private static void AddFolder(List<string> folders, string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return;
		}
		try
		{
			path = Path.GetFullPath(path);
		}
		catch
		{
			return;
		}
		if (!Directory.Exists(path))
		{
			return;
		}
		for (int i = 0; i < folders.Count; i++)
		{
			if (string.Equals(folders[i], path, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
		}
		folders.Add(path);
	}
}
