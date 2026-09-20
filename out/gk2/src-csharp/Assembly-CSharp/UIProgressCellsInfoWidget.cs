using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressCellsInfoWidget : LazyWidget<UIProgressCellsInfoWidgetData>
{
	[SerializeField]
	public Transform cellsParent;

	[SerializeField]
	private TextMeshProUGUI chanceLabel;

	[SerializeField]
	private GameObject backgroundCellImagePrefab;

	[SerializeField]
	private RectTransform backGroupParent;

	private List<UIProgressCellsInfoWidgetCell> progressCells = new List<UIProgressCellsInfoWidgetCell>();

	private List<GameObject> backgroundCellImages = new List<GameObject>();

	private bool isHovered;

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < ConstDef.Get("max_cells_per_one_hit").IntValue; i++)
		{
			backgroundCellImages.Add(UnityEngine.Object.Instantiate(backgroundCellImagePrefab, backGroupParent));
		}
		backgroundCellImagePrefab.gameObject.SetActive(value: false);
	}

	public override void Redraw()
	{
		base.Redraw();
		DrawCells();
		AttachTickTooltip();
	}

	public override void Hide()
	{
		base.Hide();
		foreach (GameObject backgroundCellImage in backgroundCellImages)
		{
			backgroundCellImage.gameObject.SetActive(value: false);
		}
		HideCells();
	}

	private void HideCells()
	{
		foreach (UIProgressCellsInfoWidgetCell progressCell in progressCells)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(progressCell);
		}
		progressCells.Clear();
	}

	private void DrawCells()
	{
		int masteryValue = data.MasteryValue;
		int masteryLock = data.MasteryLock;
		int num = 0;
		float num2 = 0f;
		chanceLabel.gameObject.SetActive(value: false);
		if (!data.IsStarCraft)
		{
			if (masteryValue < masteryLock)
			{
				num = 0;
			}
			else
			{
				num = Math.Clamp(masteryValue / masteryLock, 0, ConstDef.Get("max_cells_per_one_hit").IntValue);
				num2 = (float)masteryValue % (float)masteryLock / (float)masteryLock;
			}
		}
		else
		{
			if (masteryValue < masteryLock)
			{
				UIProgressCellsInfoWidgetCell elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UIProgressCellsInfoWidgetCell>(cellsParent);
				elementFromPool.Show(isEmpty: false, isChance: true);
				progressCells.Add(elementFromPool);
				chanceLabel.text = $"{(int)(100f * ((float)masteryValue / (float)masteryLock))}%";
				chanceLabel.gameObject.SetActive(value: true);
				RefreshBackgroundCells();
				((RectTransform)cellsParent).RefreshContentFitter();
				return;
			}
			num = Math.Clamp(masteryValue / masteryLock, 0, ConstDef.Get("max_cells_per_one_hit").IntValue);
			num2 = (float)masteryValue % (float)masteryLock / (float)masteryLock;
		}
		if (num <= 0)
		{
			Hide();
			return;
		}
		for (int i = 0; i < ConstDef.Get("max_cells_per_one_hit").IntValue; i++)
		{
			UIProgressCellsInfoWidgetCell elementFromPool2 = UIPrefabsPooler.Instance.GetElementFromPool<UIProgressCellsInfoWidgetCell>(cellsParent);
			if (num - 1 >= i)
			{
				elementFromPool2.Show(isEmpty: false, isChance: false);
			}
			else if (num == i && num2 > 0f)
			{
				elementFromPool2.Show(isEmpty: false, isChance: false, num2);
			}
			else
			{
				elementFromPool2.Show(isEmpty: true, isChance: false);
			}
			progressCells.Add(elementFromPool2);
		}
		RefreshBackgroundCells();
		((RectTransform)cellsParent).RefreshContentFitter();
	}

	private void RefreshBackgroundCells()
	{
		foreach (GameObject backgroundCellImage in backgroundCellImages)
		{
			backgroundCellImage.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < progressCells.Count; i++)
		{
			backgroundCellImages[i].gameObject.SetActive(value: true);
		}
	}

	private void AttachTickTooltip()
	{
		if (base.gameObject.activeSelf && data?.TalentDef != null)
		{
			UIMouseTooltip.AttachCustom(base.gameObject, ShowTickTooltip, addRaycastTarget: true, disableChildRaycasts: true);
		}
	}

	private void ShowTickTooltip()
	{
		if (data?.TalentDef != null)
		{
			string text = data.TalentDef.id.FontIcon();
			string text2 = $"{text}<space=2px>{data.FormatPlayerMasteryValue()} / {text}<space=2px>{data.MasteryLock}".NOBR();
			string header = null;
			if (!string.IsNullOrEmpty(data.TooltipHeaderLngId) && LL.HasLocalizedValueForCurrentLang(data.TooltipHeaderLngId))
			{
				header = LLBase.L(data.TooltipHeaderLngId);
			}
			Vector2 appearOffset = Vector2.zero;
			UIMouseTooltip component = GetComponent<UIMouseTooltip>();
			if (component != null)
			{
				appearOffset = component.GetResolvedAppearOffset();
			}
			UITooltip.ShowSimpleInfo(base.transform, text2, appearOffset, header);
		}
	}

	public void OnOver()
	{
	}

	public void OnOut()
	{
	}

	public void ShowUITooltip()
	{
		isHovered = true;
		UITooltip.ShowProgressTicksInfo(this, data.MasteryValue, data.MasteryLock, data.IsStarCraft, data.TalentDef);
	}

	public void HideUITooltip(bool immediately = false)
	{
		isHovered = false;
		if (immediately)
		{
			UITooltip.HideImmediately();
		}
		else
		{
			UITooltip.Hide();
		}
	}

	protected override void TestDraw()
	{
	}
}
