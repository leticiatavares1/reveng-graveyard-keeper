using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechTreeElementWidget : TechTreeElementBaseWidget
{
	[SerializeField]
	private GameObject content;

	[SerializeField]
	private Image background;

	[SerializeField]
	private TextStyle priceNormalStyle;

	[SerializeField]
	private TextStyle priceNotEnoughStyle;

	[SerializeField]
	private TextStyle priceDisabledStyle;

	[SerializeField]
	private TextMeshProUGUI priceLabel;

	[SerializeField]
	private TextMeshProUGUI[] headers;

	private List<LinkedEntityWidget> shownUnlocks = new List<LinkedEntityWidget>();

	public List<LinkedEntityWidget> ShownUnlocks => shownUnlocks;

	public Image Background => background;

	public override void Redraw()
	{
		base.Redraw();
		ClearContent();
		button.interactable = false;
		priceLabel.gameObject.SetActive(value: true);
		content.gameObject.SetActive(value: true);
		hiddenObj.SetActive(value: false);
		visibleObj.SetActive(value: false);
		availableObj.SetActive(value: false);
		unlockedObj.SetActive(value: false);
		TechState visualTechState = data.VisualTechState;
		base.name = $"{data.techDef.id}({visualTechState})";
		for (int i = 0; i < headers.Length; i++)
		{
			headers[i].text = LLBase.L(data.techDef.id);
		}
		string empty = string.Empty;
		switch (visualTechState)
		{
		case TechState.Visible:
			empty = data.techDef.GetPriceLabel(priceDisabledStyle, priceNotEnoughStyle, GameResIconType.TechPointSmall);
			break;
		case TechState.Available:
			empty = data.techDef.GetPriceLabel(priceNormalStyle, priceNotEnoughStyle, GameResIconType.TechPointSmall);
			break;
		}
		priceLabel.text = empty;
		for (int j = 0; j < data.techDef.linkedEntityWidgetDatas.Count; j++)
		{
			LinkedEntityWidget orCreateObject = TechTreePageWidget.UnlocksPool.GetOrCreateObject<LinkedEntityWidget>();
			orCreateObject.transform.SetParent(content.transform);
			data.techDef.linkedEntityWidgetDatas[j].NoSelectionFrames = !LazyInput.IsGamepadActive;
			data.techDef.linkedEntityWidgetDatas[j].OnClicked = base.OnClicked;
			orCreateObject.Draw(data.techDef.linkedEntityWidgetDatas[j]);
			shownUnlocks.Add(orCreateObject);
		}
		switch (visualTechState)
		{
		case TechState.Hidden:
			hiddenObj.SetActive(value: true);
			priceLabel.gameObject.SetActive(value: false);
			content.gameObject.SetActive(value: false);
			break;
		case TechState.Visible:
			visibleObj.SetActive(value: true);
			button.interactable = true;
			break;
		case TechState.Available:
			availableObj.SetActive(value: true);
			button.interactable = true;
			break;
		case TechState.Unlocked:
			button.interactable = true;
			unlockedObj.SetActive(value: true);
			priceLabel.gameObject.SetActive(value: false);
			break;
		}
		for (int k = 0; k < shownUnlocks.Count; k++)
		{
			LinkedEntityWidget linkedEntityWidget = shownUnlocks[k];
			linkedEntityWidget.transform.localScale = Vector3.one;
			linkedEntityWidget.transform.SetSiblingIndex(k);
		}
	}

	public override void Hide()
	{
		ClearContent();
		base.Hide();
	}

	private void ClearContent()
	{
		foreach (LinkedEntityWidget shownUnlock in shownUnlocks)
		{
			TechTreePageWidget.UnlocksPool.ReleaseObject(shownUnlock);
		}
		shownUnlocks.Clear();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new TechTreeElementWidgetData());
	}
}
