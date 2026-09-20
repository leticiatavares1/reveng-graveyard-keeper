using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITechTreeElementWindow : LazyWindow<UITechTreeElementWindowData>
{
	[SerializeField]
	private TextMeshProUGUI header;

	[SerializeField]
	private TextMeshProUGUI topLabel;

	[SerializeField]
	private TextMeshProUGUI botLabel;

	[SerializeField]
	private TextMeshProUGUI resourcesLabel;

	[SerializeField]
	private TextStyle enoughStyle;

	[SerializeField]
	private TextStyle notEnoughStyle;

	[SerializeField]
	private LinkedEntityWidgetWithText[] linkedEntityWidgets;

	[SerializeField]
	private UIDialogWindowButton buttonPrefab;

	[SerializeField]
	private RectTransform buttonsContent;

	private List<UIDialogWindowButton> activeButtons = new List<UIDialogWindowButton>();

	public override void Init()
	{
		base.Init();
		buttonPrefab.gameObject.SetActive(value: false);
	}

	public override void Open(UITechTreeElementWindowData data)
	{
		base.Open(data);
		header.text = data.HeaderText;
		if (data.TechDef.TechState != TechState.Unlocked)
		{
			resourcesLabel.text = data.TechDef.GetPriceLabel(enoughStyle, notEnoughStyle, GameResIconType.Common);
			resourcesLabel.gameObject.SetActive(value: true);
		}
		else
		{
			resourcesLabel.gameObject.SetActive(value: false);
		}
		if (string.IsNullOrEmpty(data.BotText))
		{
			botLabel.gameObject.SetActive(value: false);
		}
		else
		{
			botLabel.gameObject.SetActive(value: true);
			botLabel.text = data.BotText;
		}
		if (string.IsNullOrEmpty(data.TopText))
		{
			topLabel.gameObject.SetActive(value: false);
		}
		else
		{
			topLabel.gameObject.SetActive(value: true);
			topLabel.text = data.TopText;
		}
		LinkedEntityWidgetWithText[] array = linkedEntityWidgets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		for (int j = 0; j < data.TechDef.linkedEntityWidgetDatas.Count; j++)
		{
			linkedEntityWidgets[j].Draw(data.TechDef.linkedEntityWidgetDatas[j]);
			linkedEntityWidgets[j].gameObject.SetActive(value: true);
			linkedEntityWidgets[j].Button.GetComponent<GamepadNavigationItem>().OnSelect.RemoveAllListeners();
		}
		foreach (UIDialogWindowButton activeButton in activeButtons)
		{
			UIDialogWindow.pool.ReleaseObject(activeButton);
		}
		activeButtons.Clear();
		for (int k = 0; k < data.ButtonsData.Count; k++)
		{
			UIDialogWindowButton orCreateObject = UIDialogWindow.pool.GetOrCreateObject<UIDialogWindowButton>();
			UIDialogWindowData.ButtonData buttonData = data.ButtonsData[k];
			orCreateObject.transform.SetParent(buttonsContent);
			orCreateObject.Draw(buttonData);
			activeButtons.Add(orCreateObject);
			orCreateObject.transform.SetSiblingIndex(k);
		}
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Clear();
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, () => false);
		return gameKeyDelegates;
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
		return base.OnPressedBack();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		TechDef techDef = GameBalance.Me.GetData<TechDef>("wood_basic");
		UIDialogWindowData.ButtonData item = new UIDialogWindowData.ButtonData(Unlock, LLBase.L("btn_unlock"), () => techDef.TechState == TechState.Available && techDef.EnoughResources, replaceForGamepad: true, GameKey.Select);
		UIDialogWindowData.ButtonData item2 = new UIDialogWindowData.ButtonData(LazyUI.GetWindow<UITechTreeElementWindow>().Close, LLBase.L("btn_cancel"), null, replaceForGamepad: true, GameKey.Back);
		UITechTreeElementWindowData uITechTreeElementWindowData = new UITechTreeElementWindowData(techDef, new List<UIDialogWindowData.ButtonData> { item, item2 }, techDef.ParentsUnlocked ? string.Empty : LLBase.L("tech_not_all_techs_unlocked"));
		LazyUI.GetWindow<UITechTreeElementWindow>().Open(uITechTreeElementWindowData);
		void Unlock()
		{
			techDef.Unlock();
			LazyAudio.PlayAndForget("unlock");
			LazyUI.GetWindow<UITechTreeElementWindow>().Close();
		}
	}
}
