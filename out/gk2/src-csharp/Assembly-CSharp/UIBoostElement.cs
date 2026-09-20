using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBoostElement : LazyWidget<UIBoostElementData>
{
	private Action<List<NeedItemData>> onPress;

	private Action onOver;

	private Action onOut;

	[SerializeField]
	private Image resultIcon;

	[SerializeField]
	private Transform ingredientsContainer;

	[SerializeField]
	private Image selectionFrame;

	[SerializeField]
	private TextMeshProUGUI nameLabel;

	[SerializeField]
	private TextMeshProUGUI descriptionLabel;

	[SerializeField]
	private GameObject canNotCraftOverlay;

	private LazyButton widgetButton;

	private List<UICraftItemCell> displayedIngredients = new List<UICraftItemCell>();

	public List<UICraftItemCell> DisplayedIngredients => displayedIngredients;

	public override void Init()
	{
		base.Init();
		TryGetComponent<LazyButton>(out widgetButton);
		widgetButton.onEnter.AddListener(OnOver);
		widgetButton.onExit.AddListener(OnOut);
		widgetButton.onClick.AddListener(OnPress);
	}

	public override void DeInit()
	{
		base.DeInit();
		widgetButton.onEnter.RemoveAllListeners();
		widgetButton.onExit.RemoveAllListeners();
		widgetButton.onClick.RemoveAllListeners();
	}

	protected void Update()
	{
	}

	public override void Redraw()
	{
		base.Redraw();
		onPress = data.OnPress;
		onOver = data.OnOver;
		onOut = data.OnOut;
		resultIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(data.CraftElement.Definition.GetCraftResultIcon());
		foreach (UICraftItemCellData craftItemCellsDatum in data.CraftItemCellsData)
		{
			UICraftItemCell elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UICraftItemCell>(ingredientsContainer.transform);
			elementFromPool.Draw(craftItemCellsDatum, null);
			elementFromPool.GamepadNavigationItem.group = 1;
			elementFromPool.ItemCell.OnItemCellPress = delegate
			{
				OnPress();
			};
			displayedIngredients.Add(elementFromPool);
		}
		nameLabel.text = LLBase.L(data.Name);
		descriptionLabel.text = data.Description;
		descriptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(descriptionLabel.text));
		if (data.CanCraft(data.GetCurrentNeedItems()))
		{
			canNotCraftOverlay.SetActive(value: false);
		}
		else
		{
			canNotCraftOverlay.SetActive(value: true);
		}
		Fold();
	}

	public void Fold()
	{
		foreach (UICraftItemCell displayedIngredient in displayedIngredients)
		{
			displayedIngredient.GamepadNavigationItem.Active = false;
		}
	}

	public void Unfold()
	{
		foreach (UICraftItemCell displayedIngredient in displayedIngredients)
		{
			displayedIngredient.GamepadNavigationItem.Active = true;
		}
	}

	public override void Hide()
	{
		foreach (UICraftItemCell displayedIngredient in displayedIngredients)
		{
			displayedIngredient.Flush();
			displayedIngredient.GamepadNavigationItem.group = 1;
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedIngredient);
		}
		displayedIngredients.Clear();
		selectionFrame.gameObject.SetActive(value: false);
		base.Hide();
	}

	private void OnOver()
	{
		selectionFrame.gameObject.SetActive(value: true);
		onOver?.Invoke();
	}

	private void OnOut()
	{
		selectionFrame.gameObject.SetActive(value: false);
		onOut?.Invoke();
	}

	private void OnPress()
	{
		List<NeedItemData> currentNeedItems = data.GetCurrentNeedItems();
		if (data.CanCraft(currentNeedItems))
		{
			onPress?.Invoke(data.GetCurrentNeedItems());
		}
	}

	protected override void TestDraw()
	{
	}
}
