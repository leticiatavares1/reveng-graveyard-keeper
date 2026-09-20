using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LazyBearTechnology.CloudSync;

public class LazyCloudSync : MonoBehaviour
{
	public enum State
	{
		NotInited,
		Unknown,
		NotSetUp,
		Connected,
		Syncing,
		Offline
	}

	public enum CloudResult
	{
		Unknown = -1,
		OK,
		WrongSyncCode,
		CantLinkToSameDevice,
		AuthenticationError,
		ErrorDowloadingFile
	}

	private delegate IEnumerator EnumeratorDelegate();

	public const string SERVER_URL = "http://s2.lazybeargames.com/cloud/";

	private static bool initialized;

	private static LazyCloudSync instance;

	private ILazyCloudSyncSettings settingsInterface;

	private ILazyCloudSync syncInterface;

	private State currentState;

	private bool processingCoroutine;

	private Queue<EnumeratorDelegate> onCoroutineFinished = new Queue<EnumeratorDelegate>();

	private int time;

	private string code;

	public static State CurrentState
	{
		get
		{
			if (!(instance == null))
			{
				return instance.currentState;
			}
			return State.NotInited;
		}
	}

	public static void Init(ILazyCloudSync saver)
	{
		Init<LazyCloudSyncSettingsPlayerPrefs>(saver);
	}

	public static void Init<T>(ILazyCloudSync saver) where T : ILazyCloudSyncSettings, new()
	{
		if (initialized)
		{
			Debug.LogWarning("Trying to call LBCloudSync.Init() for the 2nd time. Skipping.");
			return;
		}
		instance = new GameObject("LBCloudSync").AddComponent<LazyCloudSync>();
		initialized = true;
		instance.settingsInterface = new T();
		instance.syncInterface = saver;
		instance.currentState = (IsRegisteredInCloud() ? State.Syncing : State.NotSetUp);
		Connect();
	}

	public static void Connect()
	{
		instance.DoRequest("ping");
	}

	public static void OnMainButtonClicked()
	{
		if (instance.processingCoroutine)
		{
			Debug.LogWarning("LBCloudSync.OnMainButtonClicked() is ignored while other request is in progress.");
			return;
		}
		switch (instance.currentState)
		{
		case State.NotSetUp:
		case State.Connected:
			if (IsRegisteredInCloud())
			{
				instance.syncInterface.ShowCloudSyncDialogWithBreakSyncButton();
			}
			else
			{
				instance.syncInterface.ShowCloudSyncDialogWithEnterCodeButton();
			}
			break;
		case State.Offline:
			Connect();
			break;
		default:
			Debug.LogWarning("Wrong state = " + instance.currentState);
			break;
		}
	}

	public static void StartSync()
	{
		if (!initialized)
		{
			Debug.LogError("LB Cloud Sync error: library was not initializaed. Call Init() method first.");
		}
		else if (DoIHaveActiveSyncCode())
		{
			instance.syncInterface.ShowCloudSyncDialogWithCode(instance.code);
		}
		else
		{
			instance.DoRequest("syncinit");
		}
	}

	public static void BreakSync()
	{
		Debug.Log("LB Cloud Sync: Break sync");
		instance.currentState = State.NotSetUp;
		instance.settingsInterface.DeleteValue("LBCloud_id");
		instance.settingsInterface.DeleteValue("LBCloud_pass");
		instance.settingsInterface.DeleteValue("LBCloud_code");
		instance.settingsInterface.DeleteValue("LBCloud_time");
		instance.settingsInterface.DeleteValue("LBCloud_was_synced");
	}

	public static void ProcessCloudCode(string code)
	{
		instance.DoRequest("linkme", new Dictionary<string, string> { { "code", code } });
	}

	private void DoRequest(string command, Dictionary<string, string> additionalFields = null)
	{
		Debug.Log("DoRequest " + command);
		if (processingCoroutine)
		{
			onCoroutineFinished.Enqueue(() => DoRequestCoroutine(command, additionalFields));
		}
		else
		{
			processingCoroutine = true;
			onCoroutineFinished.Clear();
			StartCoroutine(DoRequestCoroutine(command, additionalFields));
		}
	}

	private IEnumerator DoRequestCoroutine(string command, Dictionary<string, string> additionalFields = null)
	{
		Debug.Log("<color=cyan>LB Cloud command:</color> " + command);
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("ver", syncInterface.GetApplicationVersion());
		if (!settingsInterface.HasValue("LBCloud_clientid"))
		{
			GenerateNewCloudClientID();
		}
		if (command == "sync" || command == "syncinit")
		{
			LazyCloudSyncSaveData savegameData = syncInterface.GetSavegameData();
			wWWForm.AddField("save", savegameData.CombinedBody);
			wWWForm.AddField("stats", savegameData.header);
			if (command == "sync")
			{
				wWWForm.AddField("was_synced", settingsInterface.GetValue("LBCloud_was_synced"));
			}
		}
		wWWForm.AddField("client_id", settingsInterface.GetValue("LBCloud_clientid"));
		wWWForm.AddField("server_id", settingsInterface.GetValue("LBCloud_id"));
		wWWForm.AddField("password", settingsInterface.GetValue("LBCloud_pass"));
		wWWForm.AddField("platform", Application.platform.ToString());
		if (additionalFields != null)
		{
			foreach (KeyValuePair<string, string> additionalField in additionalFields)
			{
				wWWForm.AddField(additionalField.Key, additionalField.Value);
			}
		}
		UnityWebRequest webRequest = UnityWebRequest.Post("http://s2.lazybeargames.com/cloud/" + command, wWWForm);
		yield return webRequest;
		while (!webRequest.isDone)
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		Debug.Log($"<color=cyan>LB Cloud:</color> {command} request done");
		Debug.Log(webRequest.downloadHandler.text);
		LazyCloudSyncServerReply lazyCloudSyncServerReply;
		try
		{
			lazyCloudSyncServerReply = JsonUtility.FromJson<LazyCloudSyncServerReply>(webRequest.downloadHandler.text);
		}
		catch (Exception)
		{
			lazyCloudSyncServerReply = null;
		}
		if (lazyCloudSyncServerReply == null)
		{
			Debug.LogError("LB Cloud error parsing JSON result");
		}
		else if (lazyCloudSyncServerReply.CloudResult == CloudResult.AuthenticationError)
		{
			BreakSync();
		}
		else
		{
			switch (command)
			{
			case "ping":
				yield return ProcessResultPing(lazyCloudSyncServerReply);
				break;
			case "syncinit":
				yield return ProcessResultSyncInit(lazyCloudSyncServerReply);
				break;
			case "linkme":
				yield return ProcessResultLinkMe(lazyCloudSyncServerReply);
				break;
			case "sync":
				yield return ProcessResultSync(lazyCloudSyncServerReply);
				break;
			case "change_client_id":
				yield return ProcessResultChangeClientID(lazyCloudSyncServerReply);
				break;
			default:
				Debug.LogError("Unknown command: " + command);
				break;
			}
		}
		Debug.Log("_on_coroutine_finished.Count = " + onCoroutineFinished.Count);
		if (onCoroutineFinished.Count == 0)
		{
			processingCoroutine = false;
			yield break;
		}
		EnumeratorDelegate enumeratorDelegate = onCoroutineFinished.Dequeue();
		yield return enumeratorDelegate();
	}

	private IEnumerator ProcessResultPing(LazyCloudSyncServerReply result)
	{
		if (currentState == State.Syncing)
		{
			if (DoIHaveActiveSyncCode() && !result.IsAuthenicated)
			{
				currentState = State.NotSetUp;
				BreakSync();
			}
			else
			{
				DoSyncRequest();
			}
		}
		yield return 0;
	}

	private IEnumerator ProcessResultLinkMe(LazyCloudSyncServerReply result)
	{
		switch (result.CloudResult)
		{
		case CloudResult.WrongSyncCode:
		case CloudResult.CantLinkToSameDevice:
			syncInterface.ShowCloudSyncErrorMessage(result.CloudResult);
			break;
		case CloudResult.OK:
			settingsInterface.SetValue("LBCloud_id", result.serverId);
			settingsInterface.SetValue("LBCloud_pass", result.password);
			SetWasSynced(synced: false);
			DoSyncRequest();
			break;
		}
		yield return 0;
	}

	private void DoSyncRequest(bool force = false)
	{
		DoRequest("sync", force ? new Dictionary<string, string> { { "force", "1" } } : null);
	}

	private IEnumerator ProcessResultSyncInit(LazyCloudSyncServerReply result)
	{
		if (result.IsError)
		{
			Debug.LogError("Sync error: " + result.result);
			yield break;
		}
		settingsInterface.SetValue("LBCloud_id", result.serverId);
		settingsInterface.SetValue("LBCloud_pass", result.password);
		time = GetDateTicksInSeconds() + result.lifetime;
		code = result.code;
		settingsInterface.SetValue("LBCloud_code", code);
		settingsInterface.SetValue("LBCloud_time", time.ToString());
		SetWasSynced(synced: true);
		syncInterface.ShowCloudSyncDialogWithCode(code);
	}

	private IEnumerator ProcessResultSync(LazyCloudSyncServerReply result)
	{
		string result2 = result.result;
		if (!(result2 == "save"))
		{
			if (result2 == "ok")
			{
				currentState = State.Connected;
				SetWasSynced(synced: true);
			}
			else
			{
				SetWasSynced(synced: false);
			}
			yield break;
		}
		UnityWebRequest webRequest = UnityWebRequest.Get(result.file);
		yield return webRequest;
		if (!string.IsNullOrEmpty(webRequest.error))
		{
			Debug.LogError("LB Cloud sync: Error downloading webRequest: " + webRequest.error);
			syncInterface.ShowCloudSyncErrorMessage(CloudResult.ErrorDowloadingFile);
			yield break;
		}
		LazyCloudSyncSaveData remoteSave = new LazyCloudSyncSaveData(webRequest.downloadHandler.text);
		if (result.DoOverwrite)
		{
			OverwriteWithRemoteSave(remoteSave);
			SetWasSynced(synced: true);
			DoRequest("change_client_id");
			yield break;
		}
		syncInterface.ShowCloudSyncSaveChooserDialog(syncInterface.GetSavegameData(), delegate
		{
			DoSyncRequest(force: true);
		}, remoteSave, delegate
		{
			OverwriteWithRemoteSave(remoteSave);
			DoRequest("change_client_id");
		});
	}

	private IEnumerator ProcessResultChangeClientID(LazyCloudSyncServerReply result)
	{
		if (!result.IsResultOK)
		{
			Debug.LogError("LB Cloud Sync change_client_id error: " + result.result);
		}
		else
		{
			currentState = State.Connected;
		}
		yield break;
	}

	private void GenerateNewCloudClientID()
	{
		string text = UnityEngine.Random.Range(0, 999999999) + "_" + GetDateTicksInSeconds();
		settingsInterface.SetValue("LBCloud_clientid", text);
		Debug.Log("Cloud client id: " + text);
	}

	private static int GetDateTicksInSeconds()
	{
		DateTime dateTime = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
		return (int)(DateTime.UtcNow - dateTime).TotalSeconds;
	}

	private void SetWasSynced(bool synced)
	{
		Debug.Log("<color=cyan>SetWasSynced:</color> " + synced);
		if (synced)
		{
			settingsInterface.SetValue("LBCloud_was_synced", "1");
		}
		else
		{
			settingsInterface.DeleteValue("LBCloud_was_synced");
		}
	}

	public static bool IsRegisteredInCloud()
	{
		if (instance.settingsInterface.HasValue("LBCloud_id") && instance.settingsInterface.HasValue("LBCloud_pass"))
		{
			return !string.IsNullOrEmpty(instance.settingsInterface.GetValue("LBCloud_id"));
		}
		return false;
	}

	private static bool DoIHaveActiveSyncCode()
	{
		if (instance.settingsInterface.HasValue("LBCloud_code"))
		{
			return Convert.ToInt32(instance.settingsInterface.GetValue("LBCloud_time")) - GetDateTicksInSeconds() > 0;
		}
		return false;
	}

	public static string GetSyncCodeTimeLeft()
	{
		int num = instance.time - GetDateTicksInSeconds();
		if (num <= 0)
		{
			instance.syncInterface.OnCloudSyncCodeExpired();
			return "";
		}
		int num2 = num / 60;
		num -= num2 * 60;
		string arg = ((num >= 10) ? "" : "0") + num;
		return $"{num2}:{arg}";
	}

	private void OverwriteWithRemoteSave(LazyCloudSyncSaveData remoteSave)
	{
		Debug.LogError("Overwrite: " + remoteSave.header);
		syncInterface.SetSavegameData(remoteSave);
		currentState = State.Connected;
	}

	public static void SaveToCloud()
	{
		if (!(instance == null) && IsRegisteredInCloud())
		{
			instance.DoSyncRequest();
			instance.SetWasSynced(synced: false);
		}
	}
}
