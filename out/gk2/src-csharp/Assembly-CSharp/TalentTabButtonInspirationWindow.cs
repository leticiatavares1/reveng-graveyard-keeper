using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentTabButtonInspirationWindow : TalentTabButton
{
	[Serializable]
	private class TabTalentViewData
	{
		public Sprite sprite;

		public string talentId;
	}

	[SerializeField]
	private Image[] imageIcons;

	[SerializeField]
	private List<TabTalentViewData> viewDataList;

	[SerializeField]
	private TextMeshProUGUI[] valueLabels;

	[SerializeField]
	private GameObject activeObject;

	[SerializeField]
	private GameObject inactiveObject;

	[SerializeField]
	protected GameObject[] actionIndicators;

	[SerializeField]
	private Image inactiveImage;

	[SerializeField]
	private Material inactiveIconMaterial;

	public override void UpdateState(bool isActive, Canvas parentCanvas)
	{
		base.isActive = isActive;
		if (isActive)
		{
			backgroundImage.sprite = activeBackSprite;
			SetActionIndicatorState(isActive: false);
			activeObject.SetActive(value: true);
			inactiveObject.SetActive(value: false);
		}
		else
		{
			backgroundImage.sprite = inactiveBackSprite;
			activeObject.SetActive(value: false);
			inactiveObject.SetActive(value: true);
		}
		backgroundImage.SetNativeSize();
		DrawTalentIcon();
		button.interactable = !isActive;
		UpdateSorting(parentCanvas);
		Image[] array = imageIcons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material = (isActive ? null : inactiveIconMaterial);
		}
	}

	protected override void DrawTalentIcon()
	{
		TabTalentViewData tabTalentViewData = viewDataList.Find((TabTalentViewData p) => p.talentId == base.TalentId);
		Image[] array = imageIcons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sprite = tabTalentViewData.sprite;
		}
		DrawMasteryValue();
	}

	public override void SetActionIndicatorState(bool isActive)
	{
		GameObject[] array = actionIndicators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(isActive);
		}
	}

	public override void DrawMasteryValue()
	{
		if (MainGame.Instance.gameState != 0)
		{
			TextMeshProUGUI[] array = valueLabels;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].text = MainGame.PlayerController.GetMasteryLevelForTalentBranch(talentId).ToString();
			}
		}
	}

	protected override void OnSelect()
	{
		if (inactiveImage != null)
		{
			inactiveImage.sprite = inactiveBackSpriteSelected;
			Image[] array = imageIcons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].material = null;
			}
		}
	}

	protected override void OnDeselect()
	{
		if (inactiveImage != null)
		{
			inactiveImage.sprite = inactiveBackSprite;
			Image[] array = imageIcons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].material = inactiveIconMaterial;
			}
		}
	}

	protected override void OnDisable()
	{
		OnDeselect();
	}
}
