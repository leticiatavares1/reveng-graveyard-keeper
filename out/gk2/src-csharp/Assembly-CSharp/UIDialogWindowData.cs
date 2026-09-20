using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIDialogWindowData : LazyWidgetDataBase
{
	public class ButtonData
	{
		public Action onPressed;

		public Func<bool> buttonAvailableCondition;

		public string text;

		public string textGamepad;

		public bool replaceForGamepad;

		public GameKey keyToReplace;

		public ButtonData(Action onPressed, string text, Func<bool> buttonAvailableCondition = null, bool replaceForGamepad = true, GameKey keyToReplace = null, string textGamepad = "")
		{
			this.text = text;
			this.onPressed = onPressed;
			this.buttonAvailableCondition = buttonAvailableCondition;
			this.replaceForGamepad = replaceForGamepad;
			this.keyToReplace = keyToReplace;
			this.textGamepad = textGamepad;
		}
	}

	public string Information { get; private set; }

	public string Header { get; private set; }

	public List<ButtonData> ButtonsData { get; set; }

	public bool ShowCloseButton { get; set; }

	public Action CloseButtonAction { get; set; }

	public Item Item { get; private set; }

	public bool DrawItemCounter { get; private set; }

	public string ItemIconId { get; private set; }

	public string ItemName { get; private set; }

	public string InformationBot { get; private set; }

	public int HasItemCount { get; private set; }

	public int NeedItemCount { get; private set; }

	public bool ShowAltVersion { get; private set; }

	public UIDialogWindowData(string header, string information, List<ButtonData> buttonsData)
	{
		Information = information;
		Header = header;
		ButtonsData = buttonsData;
		if (string.IsNullOrEmpty(header))
		{
			Debug.LogError("Header is empty. We want to show dialog windows only with header!!!");
		}
	}

	public UIDialogWindowData(string header, string information, ButtonData oneOption)
		: this(header, information, new List<ButtonData> { oneOption })
	{
		ShowCloseButton = true;
		CloseButtonAction = oneOption.onPressed;
	}

	public UIDialogWindowData(string header, string information, Action yesAction, Action noAction, bool replaceForGamepad = false)
		: this(header, information, new List<ButtonData>
		{
			new ButtonData(yesAction, LLBase.L("btn_yes"), null, replaceForGamepad: true, GameKey.Select),
			new ButtonData(noAction, LLBase.L("btn_no"), null, replaceForGamepad: true, GameKey.Back)
		})
	{
	}

	public UIDialogWindowData(string header, string information, ButtonData firstOption, ButtonData secondOption)
		: this(header, information, new List<ButtonData> { firstOption, secondOption })
	{
	}

	public UIDialogWindowData(Item item, string header, string information, ButtonData firstOption, bool drawCounter = true)
		: this(header, information, new List<ButtonData> { firstOption })
	{
		Item = item;
		DrawItemCounter = drawCounter;
	}

	public UIDialogWindowData(Item item, string header, string information, Action yesAction, Action noAction, bool replaceForGamepad = false, bool drawCounter = true)
		: this(header, information, new List<ButtonData>
		{
			new ButtonData(yesAction, LLBase.L("btn_yes"), null, replaceForGamepad: true, GameKey.Select),
			new ButtonData(noAction, LLBase.L("btn_no"), null, replaceForGamepad: true, GameKey.Back)
		})
	{
		Item = item;
		DrawItemCounter = drawCounter;
	}

	public UIDialogWindowData(Item item, string header, string information, string informationBot, int hasCount, int needCount, Action yesAction, Action noAction, bool replaceForGamepad = false)
		: this(header, information, new List<ButtonData>
		{
			new ButtonData(yesAction, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select),
			new ButtonData(noAction, LLBase.L("btn_cancel"), null, replaceForGamepad: true, GameKey.Back)
		})
	{
		ShowAltVersion = true;
		ItemIconId = item.Definition.iconId;
		ItemName = LLBase.L(item.id);
		InformationBot = informationBot;
		HasItemCount = hasCount;
		NeedItemCount = needCount;
	}

	public UIDialogWindowData(Item item, string header, string information, string informationBot, int hasCount, int needCount, Action yesAction, bool replaceForGamepad = false)
		: this(header, information, new List<ButtonData>
		{
			new ButtonData(yesAction, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select)
		})
	{
		ShowAltVersion = true;
		ItemIconId = item.Definition.iconId;
		ItemName = LLBase.L(item.id);
		InformationBot = informationBot;
		HasItemCount = hasCount;
		NeedItemCount = needCount;
	}
}
