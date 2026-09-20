using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentWidget : LazyWidget<TalentWidgetData>
{
	[Serializable]
	private class TalentViewData
	{
		public Sprite backSprite;

		public TextStyle textStyle;

		public string talentId;
	}

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private TextMeshProUGUI iconLabel;

	[SerializeField]
	private TextMeshProUGUI valueLabel;

	[SerializeField]
	private List<TalentViewData> viewDataList;

	public override void Redraw()
	{
		base.Redraw();
		iconLabel.text = data.TalentId.FontIcon();
		TalentViewData talentViewData = viewDataList.Find((TalentViewData p) => p.talentId == data.TalentId);
		backgroundImage.sprite = talentViewData.backSprite;
		backgroundImage.SetNativeSize();
		talentViewData.textStyle.ApplyStyle(valueLabel);
		valueLabel.text = data.MasteryValue.ToString();
	}

	public void DisableGamepadNavigation()
	{
		GamepadNavigationItem[] componentsInChildren = GetComponentsInChildren<GamepadNavigationItem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Active = false;
		}
	}

	public void EnableGamepadNavigation()
	{
		GamepadNavigationItem[] componentsInChildren = GetComponentsInChildren<GamepadNavigationItem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Active = true;
		}
	}

	protected override void TestDraw()
	{
	}
}
