using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Steamworks;
using UnityEngine;

public class SteamWorkshopCreatorService
{
	private List<SteamWorkshopItemRecord> items;

	private SteamWorkshopItemRecord pendingRecord;

	private string pendingChangeNote;

	private int lastProgressPercent = -1;

	private bool omitTags;

	private bool retryWithoutTagsOnInvalidParam;

	private bool queuedSubmit;

	private float busyElapsed;

	private float lastHeartbeat;

	private string lastSubmitDetails = "";

	private string stagingFolder;

	private EItemUpdateStatus lastProgressStatus;

	private CallResult<CreateItemResult_t> createItemResult;

	private CallResult<SubmitItemUpdateResult_t> submitItemResult;

	private CallResult<DeleteItemResult_t> deleteItemResult;

	private UGCUpdateHandle_t updateHandle = UGCUpdateHandle_t.Invalid;

	private PublishedFileId_t queuedSubmitId;

	public bool IsBusy { get; private set; }

	public IReadOnlyList<SteamWorkshopItemRecord> Items => items;

	public event Action<string> Logged;

	public event Action ItemsChanged;

	public SteamWorkshopCreatorService()
	{
		items = SteamWorkshopCreatorStore.Load();
		createItemResult = CallResult<CreateItemResult_t>.Create(OnCreateItem);
		submitItemResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItem);
		deleteItemResult = CallResult<DeleteItemResult_t>.Create(OnDeleteItem);
	}

	public void Dispose()
	{
		CleanupStaging();
		createItemResult?.Dispose();
		submitItemResult?.Dispose();
		deleteItemResult?.Dispose();
		createItemResult = null;
		submitItemResult = null;
		deleteItemResult = null;
	}

	public void Tick()
	{
		if (!IsBusy)
		{
			return;
		}
		busyElapsed += Time.unscaledDeltaTime;
		if (busyElapsed - lastHeartbeat >= 2f)
		{
			lastHeartbeat = busyElapsed;
			Log($"Waiting for Steam... {busyElapsed:0}s");
		}
		if (busyElapsed > 120f)
		{
			Fail("Timed out waiting for Steam after 120 seconds. The previous submit callback may have been dropped; try Upload again.");
			return;
		}
		if (queuedSubmit)
		{
			queuedSubmit = false;
			Log("Continuing submit on the next frame...");
			SubmitContent(queuedSubmitId);
			return;
		}
		bool flag = false;
		if (!(updateHandle != UGCUpdateHandle_t.Invalid))
		{
			return;
		}
		ulong punBytesProcessed;
		ulong punBytesTotal;
		EItemUpdateStatus itemUpdateProgress = SteamUGC.GetItemUpdateProgress(updateHandle, out punBytesProcessed, out punBytesTotal);
		if (itemUpdateProgress != 0)
		{
			flag = true;
			int num = (int)((punBytesTotal != 0) ? (punBytesProcessed * 100 / punBytesTotal) : 0);
			if (itemUpdateProgress != lastProgressStatus || num != lastProgressPercent)
			{
				lastProgressStatus = itemUpdateProgress;
				lastProgressPercent = num;
				Log($"{DescribeStatus(itemUpdateProgress)} ({punBytesProcessed}/{punBytesTotal})");
			}
		}
	}

	public void CreateAndUpload(string title, string tag, string localFolder)
	{
		if (!TryBeginBusy())
		{
			return;
		}
		if (string.IsNullOrWhiteSpace(title))
		{
			Fail("Title is empty.");
			return;
		}
		if (string.IsNullOrWhiteSpace(tag))
		{
			Fail("Tag is empty.");
			return;
		}
		if (tag.IndexOf(',') >= 0)
		{
			Fail("Tag contains a comma, which Steam rejects: '" + tag + "'");
			return;
		}
		if (!TryNormalizeFolder(localFolder, out var folder, out var error))
		{
			Fail(error);
			return;
		}
		EnsureThumbnail(folder);
		pendingRecord = new SteamWorkshopItemRecord
		{
			title = title.Trim(),
			tag = tag.Trim(),
			localFolder = folder
		};
		pendingChangeNote = "Initial upload";
		if (!SteamManager.Initialized)
		{
			Fail("Steam is not initialized.");
			return;
		}
		Log("Creating workshop item '" + pendingRecord.title + "'...");
		AppId_t appID = SteamUtils.GetAppID();
		LogAppId(appID);
		SteamAPICall_t hAPICall = SteamUGC.CreateItem(appID, EWorkshopFileType.k_EWorkshopFileTypeFirst);
		createItemResult.Set(hAPICall);
	}

	public void Upload(SteamWorkshopItemRecord record)
	{
		if (!TryBeginBusy())
		{
			return;
		}
		if (record == null)
		{
			Fail("No item selected.");
			return;
		}
		if (record.publishedFileId == 0L)
		{
			Fail("Item has no Steam published file id. Create it first.");
			return;
		}
		if (!TryNormalizeFolder(record.localFolder, out var folder, out var error))
		{
			Fail(error);
			return;
		}
		record.localFolder = folder;
		EnsureThumbnail(folder);
		pendingRecord = record;
		pendingChangeNote = "Updated";
		if (!SteamManager.Initialized)
		{
			Fail("Steam is not initialized.");
		}
		else
		{
			SubmitContent(new PublishedFileId_t(record.publishedFileId));
		}
	}

	public void Delete(SteamWorkshopItemRecord record)
	{
		if (!TryBeginBusy())
		{
			return;
		}
		if (record == null)
		{
			Fail("No item selected.");
			return;
		}
		pendingRecord = record;
		if (record.publishedFileId == 0L)
		{
			RemoveLocal(record);
			Log("Removed local creator config. Nothing to delete on Steam.");
			FinishBusy();
			this.ItemsChanged?.Invoke();
		}
		else if (!SteamManager.Initialized)
		{
			Fail("Steam is not initialized.");
		}
		else
		{
			Log($"Deleting Steam item {record.publishedFileId}...");
			SteamAPICall_t hAPICall = SteamUGC.DeleteItem(new PublishedFileId_t(record.publishedFileId));
			deleteItemResult.Set(hAPICall);
		}
	}

	private bool TryBeginBusy()
	{
		if (IsBusy)
		{
			Log("Busy: wait for the current Steam operation to finish.");
			return false;
		}
		IsBusy = true;
		lastProgressPercent = -1;
		omitTags = false;
		retryWithoutTagsOnInvalidParam = true;
		queuedSubmit = false;
		busyElapsed = 0f;
		lastHeartbeat = 0f;
		lastSubmitDetails = "";
		lastProgressStatus = EItemUpdateStatus.k_EItemUpdateStatusInvalid;
		updateHandle = UGCUpdateHandle_t.Invalid;
		return true;
	}

	private void FinishBusy()
	{
		CleanupStaging();
		IsBusy = false;
		pendingRecord = null;
		pendingChangeNote = null;
		omitTags = false;
		retryWithoutTagsOnInvalidParam = true;
		queuedSubmit = false;
		updateHandle = UGCUpdateHandle_t.Invalid;
	}

	private void Fail(string message)
	{
		Log("Error: " + message);
		if (!string.IsNullOrEmpty(lastSubmitDetails))
		{
			Log(lastSubmitDetails);
		}
		FinishBusy();
	}

	private void Log(string message)
	{
		Debug.Log("[SteamWorkshopCreator] " + message);
		this.Logged?.Invoke(message);
	}

	private void OnCreateItem(CreateItemResult_t result, bool ioFailure)
	{
		if (ioFailure || result.m_eResult != EResult.k_EResultOK)
		{
			Fail("CreateItem failed: " + DescribeResult(ioFailure, result.m_eResult, result.m_nPublishedFileId.m_PublishedFileId, result.m_bUserNeedsToAcceptWorkshopLegalAgreement));
			return;
		}
		ulong publishedFileId = result.m_nPublishedFileId.m_PublishedFileId;
		pendingRecord.publishedFileId = publishedFileId;
		UpsertLocal(pendingRecord);
		SteamWorkshopCreatorStore.Save(items);
		this.ItemsChanged?.Invoke();
		Log($"Created item id {publishedFileId}.");
		if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			OpenLegalAgreement(result.m_nPublishedFileId);
		}
		queuedSubmitId = result.m_nPublishedFileId;
		queuedSubmit = true;
		Log("Submit queued for the next frame (Steam rejects SubmitItemUpdate from inside CreateItem).");
	}

	private void SubmitContent(PublishedFileId_t publishedFileId)
	{
		AppId_t appID = SteamUtils.GetAppID();
		string localFolder = pendingRecord.localFolder;
		if (!TryCreateUploadStaging(localFolder, out var stagedFolder, out var skippedCount, out var error))
		{
			Fail(error);
			return;
		}
		DescribeFolder(stagedFolder, out var fileCount, out var totalBytes, out var fileSample);
		if (skippedCount > 0)
		{
			Log($"Skipping {skippedCount} file(s) whose names start with '_'.");
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		Log($"Starting content update. App ID {appID.m_AppId}, item {publishedFileId.m_PublishedFileId}, files {fileCount}, bytes {totalBytes}.");
		updateHandle = SteamUGC.StartItemUpdate(appID, publishedFileId);
		if (updateHandle == UGCUpdateHandle_t.Invalid)
		{
			lastSubmitDetails = BuildSubmitDetails(appID, publishedFileId, stagedFolder, fileCount, totalBytes, fileSample, titleOk: false, descOk: false, tagsOk: false, visOk: false, contentOk: false, previewOk: false, "StartItemUpdate returned invalid handle");
			Fail("StartItemUpdate returned an invalid handle.");
			return;
		}
		flag = SteamUGC.SetItemTitle(updateHandle, pendingRecord.title);
		if (!flag)
		{
			Log("Warning: SetItemTitle failed.");
		}
		flag2 = SteamUGC.SetItemDescription(updateHandle, pendingRecord.title);
		if (!flag2)
		{
			Log("Warning: SetItemDescription failed.");
		}
		if (!omitTags)
		{
			flag3 = SteamUGC.SetItemTags(updateHandle, new List<string> { pendingRecord.tag });
			if (!flag3)
			{
				Log("Warning: SetItemTags failed for Type='" + pendingRecord.tag + "'.");
			}
			else
			{
				Log("Set workshop tag Type='" + pendingRecord.tag + "'.");
			}
		}
		else
		{
			Log("Submitting without workshop tags.");
			flag3 = true;
		}
		flag4 = SteamUGC.SetItemVisibility(updateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityUnlisted);
		if (!flag4)
		{
			Log("Warning: SetItemVisibility failed.");
		}
		flag5 = SteamUGC.SetItemContent(updateHandle, stagedFolder);
		if (!flag5)
		{
			lastSubmitDetails = BuildSubmitDetails(appID, publishedFileId, stagedFolder, fileCount, totalBytes, fileSample, flag, flag2, flag3, flag4, contentOk: false, previewOk: false, "SetItemContent returned false");
			Fail("SetItemContent failed. Steam rejected the folder path.");
			return;
		}
		string text = FindThumbnailPath(localFolder);
		if (!string.IsNullOrEmpty(text))
		{
			flag6 = SteamUGC.SetItemPreview(updateHandle, text);
			if (!flag6)
			{
				Log("Warning: SetItemPreview failed for '" + text + "'.");
			}
			else
			{
				Log("Set preview image '" + Path.GetFileName(text) + "'.");
			}
		}
		else
		{
			Log("Warning: no Thumbnail.png/.jpg/.jpeg in the folder; submitting without a preview.");
		}
		lastSubmitDetails = BuildSubmitDetails(appID, publishedFileId, stagedFolder, fileCount, totalBytes, fileSample, flag, flag2, flag3, flag4, flag5, flag6, text);
		SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(updateHandle, pendingChangeNote ?? "Updated");
		submitItemResult.Set(hAPICall);
		Log("SubmitItemUpdate started.");
	}

	private void OnSubmitItem(SubmitItemUpdateResult_t result, bool ioFailure)
	{
		updateHandle = UGCUpdateHandle_t.Invalid;
		if (ioFailure || result.m_eResult != EResult.k_EResultOK)
		{
			ulong publishedFileId = result.m_nPublishedFileId.m_PublishedFileId;
			if (publishedFileId == 0L && pendingRecord != null)
			{
				publishedFileId = pendingRecord.publishedFileId;
			}
			if (!ioFailure && result.m_eResult == EResult.k_EResultInvalidParam && retryWithoutTagsOnInvalidParam && !omitTags && pendingRecord != null)
			{
				retryWithoutTagsOnInvalidParam = false;
				omitTags = true;
				Log(DescribeResult(ioFailure: false, result.m_eResult, publishedFileId, result.m_bUserNeedsToAcceptWorkshopLegalAgreement));
				Log("Retrying the same upload without workshop tags on the next frame. InvalidParam often means the tag is not listed in Steamworks App Admin > Workshop, or a required tag group is missing.");
				queuedSubmitId = new PublishedFileId_t(pendingRecord.publishedFileId);
				queuedSubmit = true;
			}
			else
			{
				Fail("SubmitItemUpdate failed: " + DescribeResult(ioFailure, result.m_eResult, publishedFileId, result.m_bUserNeedsToAcceptWorkshopLegalAgreement));
				if (!ioFailure && result.m_eResult == EResult.k_EResultInvalidParam)
				{
					Log("Download will keep failing until this upload succeeds. Check Steamworks App Admin > Workshop: user tags must include this exact tag (and any Required tag group must be filled). File transfers can be on and Steam still returns InvalidParam for a bad/required tag. Re-upload from Shift+F11 after that, then subscribe again.");
				}
			}
		}
		else
		{
			if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				OpenLegalAgreement(result.m_nPublishedFileId);
			}
			Log($"Upload complete. Item {result.m_nPublishedFileId}");
			OpenItemPage(result.m_nPublishedFileId);
			SteamWorkshopInstalledItems.PendingRescan = true;
			FinishBusy();
			this.ItemsChanged?.Invoke();
		}
	}

	private void OnDeleteItem(DeleteItemResult_t result, bool ioFailure)
	{
		if (ioFailure || (result.m_eResult != EResult.k_EResultOK && result.m_eResult != EResult.k_EResultFileNotFound))
		{
			Fail("DeleteItem failed: " + DescribeResult(ioFailure, result.m_eResult, result.m_nPublishedFileId.m_PublishedFileId, needsLegal: false));
			return;
		}
		RemoveLocal(pendingRecord);
		Log("Deleted on Steam and removed from the local creator list. Linked folder files were left intact.");
		FinishBusy();
		this.ItemsChanged?.Invoke();
	}

	private void OpenLegalAgreement(PublishedFileId_t id)
	{
		Log("Steam Workshop legal agreement must be accepted. Opening overlay...");
		OpenItemPage(id);
	}

	private static void OpenItemPage(PublishedFileId_t id)
	{
		SteamFriends.ActivateGameOverlayToWebPage($"steam://url/CommunityFilePage/{id}");
	}

	private static string DescribeStatus(EItemUpdateStatus status)
	{
		return status switch
		{
			EItemUpdateStatus.k_EItemUpdateStatusPreparingConfig => "Preparing config", 
			EItemUpdateStatus.k_EItemUpdateStatusPreparingContent => "Preparing content", 
			EItemUpdateStatus.k_EItemUpdateStatusUploadingContent => "Uploading content", 
			EItemUpdateStatus.k_EItemUpdateStatusUploadingPreviewFile => "Uploading preview", 
			EItemUpdateStatus.k_EItemUpdateStatusCommittingChanges => "Committing changes", 
			_ => status.ToString(), 
		};
	}

	private string DescribeResult(bool ioFailure, EResult result, ulong publishedFileId, bool needsLegal)
	{
		if (ioFailure)
		{
			return "IO failure talking to Steam.";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(result);
		stringBuilder.Append(". ");
		stringBuilder.Append(ExplainResult(result));
		if (publishedFileId != 0L)
		{
			stringBuilder.Append(" publishedFileId=").Append(publishedFileId);
		}
		if (needsLegal)
		{
			stringBuilder.Append(" legalAgreementRequired=true");
		}
		if (SteamManager.Initialized)
		{
			stringBuilder.Append(" steamUser=").Append(SteamUser.GetSteamID());
			stringBuilder.Append(" persona=").Append(SteamFriends.GetPersonaName());
		}
		return stringBuilder.ToString();
	}

	private static string ExplainResult(EResult result)
	{
		return result switch
		{
			EResult.k_EResultInvalidParam => "A parameter Steam rejected at submit time: a workshop tag that is not in the Steamworks tag list, a missing required tag group, empty/invalid title or description, a content folder Steam cannot use, or SubmitItemUpdate called from inside another Steam callback.", 
			EResult.k_EResultFail => "Generic Steam failure. For DownloadItem this usually means the workshop item has no committed files.", 
			EResult.k_EResultAccessDenied => "This Steam account does not own a license for this App ID.", 
			EResult.k_EResultFileNotFound => "Steam could not read the content folder or item metadata.", 
			EResult.k_EResultLimitExceeded => "Preview/content too large, or Steam Cloud quota exceeded.", 
			EResult.k_EResultLockingFailed => "Failed to acquire the UGC lock. Try again.", 
			EResult.k_EResultTimeout => "Steam timed out.", 
			EResult.k_EResultDuplicateName => "Duplicate name.", 
			_ => result.ToString(), 
		};
	}

	private void LogAppId(AppId_t appId)
	{
		if (appId.m_AppId == 480)
		{
			Log("Warning: Steam App ID is 480 (Spacewar). Check steam_appid.txt next to the executable / project root.");
		}
		string text = Path.Combine(Directory.GetCurrentDirectory(), "steam_appid.txt");
		if (File.Exists(text))
		{
			Log("steam_appid.txt at " + text + ": '" + File.ReadAllText(text).Trim() + "'");
		}
		else
		{
			Log("steam_appid.txt not found at " + text + ".");
		}
	}

	private string BuildSubmitDetails(AppId_t appId, PublishedFileId_t publishedFileId, string folder, int fileCount, long totalBytes, string fileSample, bool titleOk, bool descOk, bool tagsOk, bool visOk, bool contentOk, bool previewOk, string extra)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Submit payload: appId=").Append(appId.m_AppId);
		stringBuilder.Append(" publishedFileId=").Append(publishedFileId.m_PublishedFileId);
		stringBuilder.Append(" title='").Append(pendingRecord?.title).Append("' len=")
			.Append((pendingRecord?.title?.Length).GetValueOrDefault());
		stringBuilder.Append(" tag='").Append(pendingRecord?.tag).Append("' omitTags=")
			.Append(omitTags);
		stringBuilder.Append(" visibility=Private");
		stringBuilder.Append(" changeNote='").Append(pendingChangeNote).Append("'");
		stringBuilder.Append(" folder='").Append(folder).Append("'");
		stringBuilder.Append(" files=").Append(fileCount);
		stringBuilder.Append(" bytes=").Append(totalBytes);
		stringBuilder.Append(" SetItemTitle=").Append(titleOk);
		stringBuilder.Append(" SetItemDescription=").Append(descOk);
		stringBuilder.Append(" SetItemTags=").Append(tagsOk);
		stringBuilder.Append(" SetItemVisibility=").Append(visOk);
		stringBuilder.Append(" SetItemContent=").Append(contentOk);
		stringBuilder.Append(" SetItemPreview=").Append(previewOk);
		if (!string.IsNullOrEmpty(fileSample))
		{
			stringBuilder.Append(" sample=[").Append(fileSample).Append(']');
		}
		if (!string.IsNullOrEmpty(extra))
		{
			stringBuilder.Append(" extra=").Append(extra);
		}
		stringBuilder.Append(" Check: Steamworks App Admin > Workshop tags must include this exact tag; Workshop > General must allow ISteamUGC file transfers.");
		return stringBuilder.ToString();
	}

	private static bool TryNormalizeFolder(string localFolder, out string folder, out string error)
	{
		folder = null;
		error = null;
		if (string.IsNullOrWhiteSpace(localFolder))
		{
			error = "Folder path is empty.";
			return false;
		}
		try
		{
			folder = Path.GetFullPath(localFolder.Trim());
		}
		catch (Exception ex)
		{
			error = "Folder path is invalid: " + localFolder + " (" + ex.Message + ")";
			return false;
		}
		if (!Directory.Exists(folder))
		{
			error = "Folder does not exist: " + folder;
			return false;
		}
		string[] files;
		try
		{
			files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
		}
		catch (Exception ex2)
		{
			error = "Cannot read folder " + folder + ": " + ex2.Message;
			return false;
		}
		if (files.Length == 0)
		{
			error = "Content folder has no files: " + folder;
			return false;
		}
		if (!HasUploadableFiles(folder))
		{
			error = "No files to upload in " + folder + ". Names starting with '_' are skipped.";
			return false;
		}
		return true;
	}

	private static bool HasUploadableFiles(string folder)
	{
		try
		{
			string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				if (!ShouldSkipUploadFile(files[i]))
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	private static bool ShouldSkipUploadFile(string path)
	{
		string fileName = Path.GetFileName(path);
		if (string.IsNullOrEmpty(fileName))
		{
			return true;
		}
		if (fileName[0] == '_')
		{
			return true;
		}
		if (fileName.Equals("Thumbnail.png", StringComparison.OrdinalIgnoreCase) || fileName.Equals("Thumbnail.jpg", StringComparison.OrdinalIgnoreCase) || fileName.Equals("Thumbnail.jpeg", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	private bool TryCreateUploadStaging(string sourceFolder, out string stagedFolder, out int skippedCount, out string error)
	{
		stagedFolder = null;
		skippedCount = 0;
		error = null;
		CleanupStaging();
		string text = Path.Combine(Application.persistentDataPath, "SteamWorkshop", "UploadStaging", Guid.NewGuid().ToString("N"));
		int copied = 0;
		int skipped = 0;
		try
		{
			CopyUploadTree(sourceFolder, text, ref copied, ref skipped);
		}
		catch (Exception ex)
		{
			TryDeleteDirectory(text);
			error = "Failed to stage upload files: " + ex.Message;
			return false;
		}
		if (copied == 0)
		{
			TryDeleteDirectory(text);
			error = "No files to upload after skipping names that start with '_'.";
			return false;
		}
		stagingFolder = text;
		stagedFolder = text;
		skippedCount = skipped;
		return true;
	}

	private static void CopyUploadTree(string source, string dest, ref int copied, ref int skipped)
	{
		Directory.CreateDirectory(dest);
		string[] files = Directory.GetFiles(source);
		for (int i = 0; i < files.Length; i++)
		{
			if (ShouldSkipUploadFile(files[i]))
			{
				skipped++;
				continue;
			}
			File.Copy(files[i], Path.Combine(dest, Path.GetFileName(files[i])), overwrite: true);
			copied++;
		}
		string[] directories = Directory.GetDirectories(source);
		for (int j = 0; j < directories.Length; j++)
		{
			string fileName = Path.GetFileName(directories[j]);
			if (string.IsNullOrEmpty(fileName) || fileName[0] == '_' || fileName[0] == '~')
			{
				skipped++;
			}
			else
			{
				CopyUploadTree(directories[j], Path.Combine(dest, fileName), ref copied, ref skipped);
			}
		}
	}

	private void CleanupStaging()
	{
		if (!string.IsNullOrEmpty(stagingFolder))
		{
			TryDeleteDirectory(stagingFolder);
			stagingFolder = null;
		}
	}

	private static void TryDeleteDirectory(string path)
	{
		if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
		{
			return;
		}
		try
		{
			Directory.Delete(path, recursive: true);
		}
		catch
		{
		}
	}

	private static void DescribeFolder(string folder, out int fileCount, out long totalBytes, out string fileSample)
	{
		fileCount = 0;
		totalBytes = 0L;
		fileSample = "";
		if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
			fileCount = files.Length;
			for (int i = 0; i < files.Length; i++)
			{
				try
				{
					totalBytes += new FileInfo(files[i]).Length;
				}
				catch
				{
				}
				if (i < 8)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(Path.GetRelativePath(folder, files[i]));
				}
			}
			if (files.Length > 8)
			{
				stringBuilder.Append(", ...");
			}
		}
		catch (Exception ex)
		{
			stringBuilder.Append("error: ").Append(ex.Message);
		}
		fileSample = stringBuilder.ToString();
	}

	public string EnsureThumbnail(string folder)
	{
		string text = FindThumbnailPath(folder);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		string text2 = Path.Combine(folder, "Thumbnail.png");
		try
		{
			Texture2D texture2D = new Texture2D(256, 256, TextureFormat.RGB24, mipChain: false);
			Color32[] array = new Color32[65536];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Color32(0, 0, 0, byte.MaxValue);
			}
			texture2D.SetPixels32(array);
			File.WriteAllBytes(text2, texture2D.EncodeToPNG());
			UnityEngine.Object.Destroy(texture2D);
			Log("Created empty 256x256 Thumbnail.png in '" + folder + "'.");
			return text2;
		}
		catch (Exception ex)
		{
			Log("Failed to create Thumbnail.png in '" + folder + "': " + ex.Message);
			return null;
		}
	}

	public static string FindThumbnailPath(string folder)
	{
		if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
		{
			return null;
		}
		string[] array = new string[3] { "Thumbnail.png", "Thumbnail.jpg", "Thumbnail.jpeg" };
		for (int i = 0; i < array.Length; i++)
		{
			string text = Path.Combine(folder, array[i]);
			if (File.Exists(text))
			{
				return text;
			}
		}
		try
		{
			string[] files = Directory.GetFiles(folder, "Thumbnail.*");
			for (int j = 0; j < files.Length; j++)
			{
				string extension = Path.GetExtension(files[j]);
				if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
				{
					return files[j];
				}
			}
		}
		catch
		{
		}
		return null;
	}

	private void UpsertLocal(SteamWorkshopItemRecord record)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].publishedFileId == record.publishedFileId && record.publishedFileId != 0L)
			{
				items[i] = record;
				SteamWorkshopCreatorStore.Save(items);
				return;
			}
		}
		items.Add(record);
		SteamWorkshopCreatorStore.Save(items);
	}

	private void RemoveLocal(SteamWorkshopItemRecord record)
	{
		if (record != null)
		{
			items.RemoveAll((SteamWorkshopItemRecord item) => (record.publishedFileId != 0L && item.publishedFileId == record.publishedFileId) || (record.publishedFileId == 0L && item == record));
			SteamWorkshopCreatorStore.Save(items);
		}
	}
}
