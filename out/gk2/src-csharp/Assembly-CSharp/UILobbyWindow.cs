using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UILobbyWindow : LazyWindow<UILobbyWidgetData>
{
	[SerializeField]
	private LazyButton syncBtn;

	[SerializeField]
	private LazyButton startBtn;

	[SerializeField]
	private TextMeshProUGUI startBtnLabel;

	[SerializeField]
	private UIUserInfo userInfoPrefab;

	private Dictionary<ulong, UIUserInfo> userInfoObjects = new Dictionary<ulong, UIUserInfo>();

	private bool isHost;

	private void Awake()
	{
		startBtn.onClick.AddListener(OnStartBtnClicked);
		syncBtn.onClick.AddListener(delegate
		{
			startBtn.interactable = false;
			LobbyHelper.Host_SyncGameSaves();
			foreach (KeyValuePair<ulong, UIUserInfo> userInfoObject in userInfoObjects)
			{
				if (userInfoObject.Key != LazyNetwork.NetworkManager.ServerClientId)
				{
					userInfoObject.Value.Deactivate();
				}
			}
		});
	}

	public override void Open(UILobbyWidgetData data)
	{
		base.Open(data);
		isHost = LazyNetwork.NetworkManager.IsHost;
		syncBtn.gameObject.SetActive(isHost);
		startBtn.interactable = false;
		startBtnLabel.text = (isHost ? "Wait For Sync" : "Wait For Host");
		if (isHost)
		{
			if (data.isNewGame)
			{
				ulong clientId = NetworkManager.Singleton.LocalClient.ClientId;
				AddUserInfo(clientId);
				userInfoObjects[clientId].Activate();
			}
			LobbyHelper.OnAllClientsSynced = (Action)Delegate.Combine(LobbyHelper.OnAllClientsSynced, new Action(AllowToStartGame));
			LobbyHelper.OnClientAdded = (Action<ulong>)Delegate.Combine(LobbyHelper.OnClientAdded, new Action<ulong>(AddUserInfo));
			LobbyHelper.OnClientSynced = (Action<ulong>)Delegate.Combine(LobbyHelper.OnClientSynced, new Action<ulong>(UpdateUserInfo));
		}
	}

	private void AllowToStartGame()
	{
		startBtn.interactable = true;
		startBtnLabel.text = "Start Game";
	}

	private void UpdateUserInfo(ulong id)
	{
		userInfoObjects[id].Activate();
	}

	private void AddUserInfo(ulong id)
	{
		UIUserInfo uIUserInfo = userInfoPrefab.Copy();
		uIUserInfo.Init(id.ToString(), "");
		uIUserInfo.Deactivate();
		userInfoObjects.Add(id, uIUserInfo);
	}

	private void OnStartBtnClicked()
	{
		if (isHost)
		{
			Close();
			LobbyHelper.Host_StartGame();
		}
	}

	public override void Close()
	{
		if (isHost)
		{
			foreach (KeyValuePair<ulong, UIUserInfo> userInfoObject in userInfoObjects)
			{
				UnityEngine.Object.Destroy(userInfoObject.Value.gameObject);
			}
			userInfoObjects.Clear();
			LobbyHelper.OnAllClientsSynced = (Action)Delegate.Remove(LobbyHelper.OnAllClientsSynced, new Action(AllowToStartGame));
			LobbyHelper.OnClientAdded = (Action<ulong>)Delegate.Remove(LobbyHelper.OnClientAdded, new Action<ulong>(AddUserInfo));
			LobbyHelper.OnClientSynced = (Action<ulong>)Delegate.Remove(LobbyHelper.OnClientSynced, new Action<ulong>(UpdateUserInfo));
		}
		base.Close();
	}

	protected override void TestDraw()
	{
		Open(new UILobbyWidgetData());
	}
}
