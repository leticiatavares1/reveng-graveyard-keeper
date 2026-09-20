using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UIHotBarItemCell : MonoBehaviour
{
	[SerializeField]
	private UIItemCell uiItemCell;

	[SerializeField]
	private TextStyle gameKeyStyle;

	[SerializeField]
	private TextStyle gameKeyNumberStyle;

	[SerializeField]
	private TextMeshProUGUI keyLabel;

	private int index;

	public UIItemCell UIItemCell => uiItemCell;

	public void Init(int index)
	{
		this.index = index;
	}

	public void Draw(Item item, bool isUsable, Action<UIItemCell> onItemCellPress)
	{
		uiItemCell.Draw(item, isNeedItem: false, -1, isCraftResult: false, 1, drawAsNonInteractable: false, 0, drawCounter: true, forceNonEmpty: true, forceDrawCounter: true);
		if (isUsable)
		{
			UIItemCell uIItemCell = uiItemCell;
			uIItemCell.OnItemCellPress = (Action<UIItemCell>)Delegate.Combine(uIItemCell.OnItemCellPress, onItemCellPress);
		}
		UpdateBtnText();
	}

	public void DrawNonInteractable(Item item)
	{
		uiItemCell.Draw(item, isNeedItem: false, -1, isCraftResult: false, 1, drawAsNonInteractable: true, 0, drawCounter: true, forceNonEmpty: true, forceDrawCounter: true);
		UpdateBtnText();
	}

	public void Set(Item item)
	{
		MainGame.PlayerData.SetHotBarItemAtIndex(item.id, index);
	}

	public void UpdateBtnText()
	{
		if (!(keyLabel == null))
		{
			int gameKeyValue = 181;
			switch (index)
			{
			case 0:
				gameKeyValue = GameKey.UseHotBarItem1.value;
				break;
			case 1:
				gameKeyValue = GameKey.UseHotBarItem2.value;
				break;
			case 2:
				gameKeyValue = GameKey.UseHotBarItem3.value;
				break;
			case 3:
				gameKeyValue = GameKey.UseHotBarItem4.value;
				break;
			}
			string keycodeString = LazyInput.ControllerIconLibrary.GetKeycodeString(LazyInput.GameBindings.keyBindings.Find((KeyBinding b) => b.gameKey.value == gameKeyValue).keyCode);
			string text = ((!(gameKeyNumberStyle != null)) ? (keycodeString ?? "") : (gameKeyNumberStyle.ApplyStyleToString(keycodeString) ?? ""));
			keyLabel.text = text;
			gameKeyStyle.ApplyStyle(keyLabel);
		}
	}
}
