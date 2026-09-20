using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDialogWindow : LazyWindow<UIDialogWindowData>
{
	[SerializeField]
	private TextMeshProUGUI header;

	[SerializeField]
	private TextMeshProUGUI information;

	[SerializeField]
	private UIDialogWindowButton buttonPrefab;

	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private GenericWindowLayout genericWindowLayout;

	[SerializeField]
	private GameObject itemIconWithBackgroundParent;

	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private TextMeshProUGUI itemIconText;

	[SerializeField]
	private TextMeshProUGUI itemCounterText;

	[SerializeField]
	private TextStyleComponent itemCounterStyleComponent;

	[SerializeField]
	private TextStyle itemCounterEnough;

	[SerializeField]
	private TextStyle itemCounterNotEnough;

	[SerializeField]
	private TextMeshProUGUI informationBotText;

	[SerializeField]
	private Color toReplace = new Color(1f, 1f, 1f, 0f);

	[SerializeField]
	private RectTransform buttonsContent;

	private List<UIDialogWindowButton> activeButtons = new List<UIDialogWindowButton>();

	public static Pool pool;

	public override void Init()
	{
		base.Init();
		pool = LazyPooler.CreatePool(buttonPrefab);
		buttonPrefab.gameObject.SetActive(value: false);
	}

	public override void Open(UIDialogWindowData data)
	{
		base.Open(data);
		bool num = !string.IsNullOrEmpty(data.Header);
		information.text = data.Information;
		header.text = data.Header;
		if (num)
		{
			genericWindowLayout.UpdateSize(isSmall: false);
		}
		else
		{
			genericWindowLayout.UpdateSize(isSmall: false);
		}
		foreach (UIDialogWindowButton activeButton in activeButtons)
		{
			pool.ReleaseObject(activeButton);
		}
		activeButtons.Clear();
		for (int i = 0; i < data.ButtonsData.Count; i++)
		{
			UIDialogWindowButton orCreateObject = pool.GetOrCreateObject<UIDialogWindowButton>();
			UIDialogWindowData.ButtonData buttonData = data.ButtonsData[i];
			orCreateObject.transform.SetParent(buttonsContent);
			orCreateObject.Draw(buttonData);
			activeButtons.Add(orCreateObject);
			orCreateObject.transform.SetSiblingIndex(i);
		}
		UpdateGamepadDependentStuff();
		if (data.Item != null)
		{
			itemCell.Draw(data.Item, isNeedItem: false, -1, isCraftResult: false, 1, drawAsNonInteractable: false, 0, data.DrawItemCounter);
			itemCell.gameObject.SetActive(value: true);
			itemCell.transform.SetAsFirstSibling();
			if (LazyInput.IsGamepadActive)
			{
				base.GamepadNavigationController.ReinitItems(focusOnFirstActive: false);
				if (activeButtons.Count > 0)
				{
					base.GamepadNavigationController.SetFocusedItem(activeButtons[0].GetComponent<GamepadNavigationItem>());
				}
				else
				{
					base.GamepadNavigationController.FocusOnFirstActive();
				}
			}
		}
		else
		{
			itemCell.gameObject.SetActive(value: false);
			if (LazyInput.IsGamepadActive)
			{
				base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			}
		}
		itemIconWithBackgroundParent.SetActive(data.ShowAltVersion);
		informationBotText.gameObject.SetActive(data.ShowAltVersion && !string.IsNullOrEmpty(data.InformationBot));
		if (data.ShowAltVersion)
		{
			itemIconWithBackgroundParent.SetActive(value: true);
			itemIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(data.ItemIconId);
			itemIcon.BlueColorReplace(toReplace);
			itemIconText.text = data.ItemName;
			itemCounterText.text = $"{data.HasItemCount}/{data.NeedItemCount}";
			itemCounterStyleComponent.SetTextStyle((data.HasItemCount >= data.NeedItemCount) ? itemCounterEnough : itemCounterNotEnough);
			informationBotText.text = data.InformationBot;
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	protected override void InitCloseButton(LazyButton button)
	{
		button.onClick.AddListener(OnCloseButtonClicked);
	}

	private void OnCloseButtonClicked()
	{
		if (data?.CloseButtonAction != null)
		{
			data.CloseButtonAction();
		}
		else
		{
			Close();
		}
	}

	protected override bool OnPressedBack()
	{
		foreach (UIDialogWindowButton activeButton in activeButtons)
		{
			if (activeButton.ReplaceForGamepad && activeButton.KeyToReplace.value == GameKey.Back.value)
			{
				return false;
			}
		}
		if ((bool)closeButton && data.ShowCloseButton)
		{
			OnCloseButtonClicked();
			return true;
		}
		return false;
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Clear();
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		if (LazyInput.IsGamepadActive)
		{
			closeButton.gameObject.SetActive(value: false);
		}
		else if (data != null)
		{
			closeButton.gameObject.SetActive(data.ShowCloseButton);
		}
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, () => false);
		return gameKeyDelegates;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Open(new UIDialogWindowData("Header", "Information window test information text", new UIDialogWindowData.ButtonData(null, LLBase.L("btn_yes"), null, replaceForGamepad: true, GameKey.Select), new UIDialogWindowData.ButtonData(null, LLBase.L("btn_no"), null, replaceForGamepad: true, GameKey.Back)));
	}

	[LazyUITest]
	protected void TestDrawItemCell()
	{
		UIDialogWindowData.ButtonData firstOption = new UIDialogWindowData.ButtonData(delegate
		{
		}, LLBase.L("btn_extract"), null, replaceForGamepad: true, GameKey.ExtractBody);
		Open(new UIDialogWindowData(new Item("heart_1_1"), LLBase.L("extract_organ"), LLBase.L("chance_success") + " 50%", firstOption, drawCounter: false));
	}
}
