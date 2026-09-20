using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIAlchemyBoostsWindow : LazyWindow<UIAlchemyBoostsWindowData>
{
	[SerializeField]
	private UIBoostElement boostElement;

	[SerializeField]
	[Space]
	private GameObject listContent;

	[SerializeField]
	private ScrollRect scrollRect;

	private List<UIBoostElement> displayedBoostElements = new List<UIBoostElement>();

	private Action<CraftElement, List<NeedItemData>> onBoostPressed;

	private Func<CraftElement, List<NeedItemData>, bool> canCraft;

	private UIBoostElement foldedElement;

	public override void Init()
	{
		base.Init();
		boostElement.gameObject.SetActive(value: false);
	}

	public override void Redraw()
	{
		onBoostPressed = data.OnCraftPressed;
		canCraft = data.CanCraft;
		MultiInventory multiInventory = new MultiInventory(data.PlayerData);
		if (data.AdditionalInventories != null)
		{
			foreach (Inventory additionalInventory in data.AdditionalInventories)
			{
				multiInventory.Add(additionalInventory);
			}
		}
		foreach (CraftElement item in data.CraftsToDisplay)
		{
			UIBoostElement elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UIBoostElement>(listContent.transform);
			displayedBoostElements.Add(elementFromPool);
			UIBoostElementData uIBoostElementData = new UIBoostElementData(item, multiInventory, OnBoostPressed, canCraft, null, null, data.AssignedWgo.Data.WorldZoneData, data.AssignedWgo.Data);
			elementFromPool.Init();
			elementFromPool.Draw(uIBoostElementData);
		}
		base.Redraw();
	}

	public override void Open(UIAlchemyBoostsWindowData data)
	{
		base.Open(data);
		scrollRect.verticalNormalizedPosition = 1f;
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	public override void Hide()
	{
		foreach (UIBoostElement displayedBoostElement in displayedBoostElements)
		{
			displayedBoostElement.DeInit();
			displayedBoostElement.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedBoostElement);
		}
		displayedBoostElements.Clear();
		base.Hide();
	}

	private void OnBoostPressed(CraftElement craftElement, List<NeedItemData> needItems)
	{
		onBoostPressed?.Invoke(craftElement, needItems);
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Fold, FoldPress);
		return gameKeyDelegates;
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		if (!LazyInput.IsGamepadActive && foldedElement != null)
		{
			foldedElement.Fold();
			foldedElement = null;
		}
	}

	protected override bool OnPressedBack()
	{
		if (LazyInput.IsGamepadActive && base.GamepadNavigationController.FocusedItem != null && base.GamepadNavigationController.FocusedItem.TryGetComponent<UICraftItemCell>(out var _))
		{
			foldedElement.Fold();
			base.GamepadNavigationController.SetFocusedItem(foldedElement.GetComponentInParent<GamepadNavigationItem>());
			foldedElement = null;
			return true;
		}
		return base.OnPressedBack();
	}

	private bool FoldPress()
	{
		if (base.GamepadNavigationController.FocusedItem.TryGetComponent<UIBoostElement>(out var component))
		{
			component.Unfold();
			foldedElement = component;
			if (component.DisplayedIngredients.Count > 0)
			{
				base.GamepadNavigationController.SetFocusedItem(component.DisplayedIngredients[0].GamepadNavigationItem);
				return true;
			}
		}
		if (base.GamepadNavigationController.FocusedItem.TryGetComponent<UICraftItemCell>(out var _))
		{
			foldedElement.Fold();
			base.GamepadNavigationController.SetFocusedItem(foldedElement.GetComponentInParent<GamepadNavigationItem>());
			foldedElement = null;
			return true;
		}
		return false;
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		list.Add(LazyGameKeyTip.Select());
		if (gamepadNavigationItem.TryGetComponent<UIBoostElement>(out var component) && component.DisplayedIngredients.Count > 0)
		{
			list.Add(new LazyGameKeyTip(GameKey.Fold, "tip_unfold"));
		}
		if (gamepadNavigationItem.TryGetComponent<UICraftItemCell>(out var _))
		{
			list.Add(new LazyGameKeyTip(GameKey.Fold, "tip_fold"));
			list.Add(new LazyGameKeyTip(GameKey.Back, "tip_fold"));
		}
		else if ((bool)closeButton)
		{
			list.Add(LazyGameKeyTip.Back());
		}
		lazyButtonTips.Print(list);
	}

	protected override void TestDraw()
	{
	}
}
