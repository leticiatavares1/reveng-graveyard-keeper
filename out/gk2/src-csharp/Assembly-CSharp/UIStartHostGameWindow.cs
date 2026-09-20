using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UIStartHostGameWindow : LazyWindow<LazyWidgetDataBase>
{
	[SerializeField]
	private LazyButton autoFillIpBtn;

	[SerializeField]
	private TMP_InputField ipAddressInputField;

	[SerializeField]
	private TMP_InputField portInputField;

	[SerializeField]
	private LazyButton startButton;

	public override void Init()
	{
		startButton.onClick.AddListener(OnStartButtonClicked);
		closeButton.onClick.AddListener(Close);
		autoFillIpBtn.onClick.AddListener(delegate
		{
			ipAddressInputField.text = LobbyHelper.GetLocalIpAddress();
		});
		base.Init();
	}

	public void OnStartButtonClicked()
	{
		string text = ipAddressInputField.text;
		ushort port = ushort.Parse(portInputField.text);
		if (LazyNetwork.NetworkManager.StartHostGame(text, port))
		{
			LazyUI.GetWindow<UIMainMenuWindow>().Close();
		}
		LobbyHelper.Host_Init();
		LazyUI.GetWindow<UILobbyWindow>().Open(new UILobbyWidgetData
		{
			isNewGame = true
		});
		Close();
	}

	public override void Close()
	{
		base.Close();
		LazyUI.GetWindow<UIMainMenuWindow>().Open(null);
	}

	protected override void TestDraw()
	{
	}
}
