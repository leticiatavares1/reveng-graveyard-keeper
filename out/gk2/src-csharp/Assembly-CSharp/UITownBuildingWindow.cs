using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UITownBuildingWindow : LazyWindow<UITownBuildingWindowData>
{
	[SerializeField]
	private UIInfoWidget uiInfoWidget;

	[SerializeField]
	private UITownBuildingWidget buildingElement;

	[SerializeField]
	[Space]
	private GameObject buildingsListContent;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private AutoScroll autoScroll;

	[SerializeField]
	private GameObject noBuildingsObj;

	[SerializeField]
	private TextMeshProUGUI noBuildingsText;

	private List<UITownBuildingWidget> displayedBuildItemGUIs = new List<UITownBuildingWidget>();

	private Action<TownBuildingDef, List<NeedItemData>> onBuildPressed;

	private UITownBuildingWidget foldedElement;

	public override void Init()
	{
		base.Init();
		buildingElement.gameObject.SetActive(value: false);
		base.GamepadNavigationController.loopVerticalNavigation = true;
	}

	public override void Redraw()
	{
		onBuildPressed = data.OnBuildPressed;
		MultiInventory multiInventory = new MultiInventory(data.PlayerData);
		foreach (TownBuildingDef item in data.BuildsToDisplay)
		{
			UITownBuildingWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UITownBuildingWidget>(buildingsListContent.transform);
			elementFromPool.Fold();
			displayedBuildItemGUIs.Add(elementFromPool);
			UITownBuildingWidgetData uITownBuildingWidgetData = new UITownBuildingWidgetData(item, multiInventory, OnBuildPressed, null, null, data.AssignedWgo.Data.WorldZoneData);
			elementFromPool.Init();
			elementFromPool.Draw(uITownBuildingWidgetData);
		}
		UpdateNoBuildingsObject();
		string customBuildDeskIcon = (string.IsNullOrEmpty(data.AssignedWgo.Data.Definition.craftIconId) ? "i_b_city" : data.AssignedWgo.Data.Definition.craftIconId);
		UIInfoWidgetData uIInfoWidgetData = new UIInfoWidgetData(data.AssignedWgo.Data, customBuildDeskIcon, defineIconBackgroundFromWgo: false);
		uIInfoWidgetData.CraftComponent = null;
		uiInfoWidget.Draw(uIInfoWidgetData);
		base.Redraw();
	}

	public override void Open(UITownBuildingWindowData data)
	{
		autoScroll.SkipNextAutoscroll = true;
		base.Open(data);
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		scrollRect.DOKill();
		scrollRect.verticalNormalizedPosition = 1f;
	}

	public override void Hide()
	{
		foreach (UITownBuildingWidget displayedBuildItemGUI in displayedBuildItemGUIs)
		{
			displayedBuildItemGUI.DeInit();
			displayedBuildItemGUI.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedBuildItemGUI);
		}
		displayedBuildItemGUIs.Clear();
		base.Hide();
	}

	private void OnBuildPressed(TownBuildingDef buildData, List<NeedItemData> needItems)
	{
		onBuildPressed?.Invoke(buildData, needItems);
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

	private bool FoldPress()
	{
		if (base.GamepadNavigationController.FocusedItem.TryGetComponent<UITownBuildingWidget>(out var component))
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
		if (gamepadNavigationItem.TryGetComponent<UITownBuildingWidget>(out var component) && component.DisplayedIngredients.Count > 0)
		{
			list.Add(new LazyGameKeyTip(GameKey.Fold, "tip_unfold"));
		}
		if (gamepadNavigationItem.TryGetComponent<UICraftItemCell>(out var _))
		{
			list.Add(new LazyGameKeyTip(GameKey.Fold, "tip_fold"));
		}
		if ((bool)closeButton)
		{
			list.Add(LazyGameKeyTip.Back());
		}
		lazyButtonTips.Print(list);
	}

	private void UpdateNoBuildingsObject()
	{
		bool flag = displayedBuildItemGUIs.Count > 0;
		noBuildingsObj.SetActive(!flag);
		if (!flag)
		{
			noBuildingsText.text = LLBase.L("ui_builddesk_town_is_empty");
		}
	}

	protected override void TestDraw()
	{
	}
}
