using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UIMainMenuInfoPanel : MonoBehaviour, ILazyGUIElement
{
	[SerializeField]
	private TextMeshProUGUI versionLabel;

	[SerializeField]
	private TextMeshProUGUI developerNameLabel;

	[SerializeField]
	private TextMeshProUGUI publisherNameLabel;

	[SerializeField]
	private TextMeshProUGUI xboxUserLabel;

	[SerializeField]
	private TextStyle rightSideStyle;

	public void Init()
	{
		Draw();
	}

	public void ShowFullPanel()
	{
		base.gameObject.SetActive(value: true);
		SetMenuOnlyLabelsVisible(isVisible: true);
		Draw();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void ShowVersionInGameIfNeeded()
	{
		bool showVersionInGame = LazySingletonSO<GameInfo>.Instance.ShowVersionInGame;
		base.gameObject.SetActive(showVersionInGame);
		SetMenuOnlyLabelsVisible(isVisible: false);
	}

	private void SetMenuOnlyLabelsVisible(bool isVisible)
	{
		developerNameLabel.gameObject.SetActive(isVisible);
		publisherNameLabel.gameObject.SetActive(isVisible);
		if (!isVisible)
		{
			xboxUserLabel.gameObject.SetActive(value: false);
		}
	}

	private void Draw()
	{
		versionLabel.text = LLBase.L("ui_ver") + rightSideStyle.ApplyStyleToString(LazySingletonSO<GameInfo>.Instance.Version, staticFont: true);
		developerNameLabel.text = "game by: " + rightSideStyle.ApplyStyleToString("Lazy Bear Games", staticFont: true);
		publisherNameLabel.text = "published by: " + rightSideStyle.ApplyStyleToString("tinyBuild", staticFont: true);
		xboxUserLabel.gameObject.SetActive(value: false);
	}
}
