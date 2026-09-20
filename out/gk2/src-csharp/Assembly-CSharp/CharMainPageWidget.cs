using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class CharMainPageWidget : LazyWidget<CharMainPageWidgetData>
{
	[Space]
	[SerializeField]
	private MultiInventoryWidget multiInventoryWidget;

	[SerializeField]
	private ToolBeltInventoryWidget toolBeltInventoryWidget;

	[SerializeField]
	private BagInventoryWidget bagInventoryWidget;

	[SerializeField]
	private GameObject bagStateShading;

	[SerializeField]
	private LazyButton closeBagButton;

	[SerializeField]
	private PerksWidget perksWidget;

	[SerializeField]
	private PerksWidget buffsWidget;

	[SerializeField]
	private GameObject perksFooter;

	[SerializeField]
	private MoneyWidget moneyWidget;

	[Space]
	[SerializeField]
	private List<TalentWidget> talentWidgets = new List<TalentWidget>();

	[SerializeField]
	private List<string> talents = new List<string>();

	[SerializeField]
	private RectTransform masteryLevelsHoverArea;

	private bool subscribed;

	private CharacterWindow CharacterWindow => LazyUI.GetWindow<CharacterWindow>();

	public override void Init()
	{
		base.Init();
		closeBagButton.onClick.AddListener(OnCloseBagBtnPressed);
		perksWidget.onContentChanged += RefreshContentFitter;
		buffsWidget.onContentChanged += RefreshContentFitter;
	}

	public override void Redraw()
	{
		multiInventoryWidget.Draw(data.MultiInventoryWidgetData);
		toolBeltInventoryWidget.Draw(data.ToolBeltInventoryWidgetData);
		if (data.IsBagShown)
		{
			bagInventoryWidget.Draw(data.BagInventoryWidgetData);
		}
		else
		{
			bagInventoryWidget.Hide();
		}
		perksWidget.Draw(data.PerksWidgetData);
		buffsWidget.Draw(data.BuffsWidgetData);
		moneyWidget.Draw(data.MoneyWidgetData);
		perksFooter.gameObject.SetActive(data.PerksWidgetData.Perks.Count > 0);
		DrawTalentToolRowWidgets();
		EnsureMasteryLevelsTooltip();
		TrySubscribe();
		multiInventoryWidget.OnMoveAllSimilarBtnInteractableChanged = OnMoveAllSimilarBtnInteractableChanged;
	}

	public override void Hide()
	{
		if (multiInventoryWidget != null)
		{
			multiInventoryWidget.OnMoveAllSimilarBtnInteractableChanged = null;
		}
		TryUnsubscribe();
		data?.HideBag();
		multiInventoryWidget.Hide();
		toolBeltInventoryWidget.Hide();
		if (data != null && data.IsBagShown)
		{
			bagInventoryWidget.Hide();
		}
		perksWidget.Hide();
		buffsWidget.Hide();
		moneyWidget.Hide();
		base.Hide();
	}

	private void TrySubscribe()
	{
		if (!subscribed)
		{
			data.ToolBeltInventoryWidgetData.Inventory.OnItemsAdd += OnToolbeltItemsChanged;
			data.ToolBeltInventoryWidgetData.Inventory.OnItemsRemove += OnToolbeltItemsChanged;
			data.PlayerData.OnItemUsed += OnPlayerItemUsed;
			subscribed = true;
		}
	}

	private void TryUnsubscribe()
	{
		if (subscribed)
		{
			data.ToolBeltInventoryWidgetData.Inventory.OnItemsAdd -= OnToolbeltItemsChanged;
			data.ToolBeltInventoryWidgetData.Inventory.OnItemsRemove -= OnToolbeltItemsChanged;
			data.PlayerData.OnItemUsed -= OnPlayerItemUsed;
			subscribed = false;
		}
	}

	private void RefreshContentFitter()
	{
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private void OnToolbeltItemsChanged(List<Item> items)
	{
		DrawTalentToolRowWidgets();
		toolBeltInventoryWidget.Draw(data.ToolBeltInventoryWidgetData);
	}

	private void OnPlayerItemUsed(Item item)
	{
		OnToolbeltItemsChanged(null);
	}

	private void DrawTalentToolRowWidgets()
	{
		for (int i = 0; i < talentWidgets.Count; i++)
		{
			talentWidgets[i].Draw(new TalentWidgetData(talents[i], MainGame.PlayerController.GetMasteryLevelForTalentBranch(talents[i])));
		}
	}

	private void EnsureMasteryLevelsTooltip()
	{
		if (talentWidgets == null || talentWidgets.Count == 0 || talentWidgets[0] == null)
		{
			return;
		}
		RectTransform rectTransform = talentWidgets[0].transform.parent.parent.parent as RectTransform;
		if (rectTransform == null)
		{
			return;
		}
		if (masteryLevelsHoverArea == null)
		{
			masteryLevelsHoverArea = UIMouseTooltip.GetOrCreateOverlay(rectTransform, "MasteryLevelsHoverArea");
		}
		List<RectTransform> list = new List<RectTransform>();
		for (int i = 0; i < talentWidgets.Count; i++)
		{
			if (talentWidgets[i] != null)
			{
				list.Add((RectTransform)talentWidgets[i].transform);
			}
		}
		UIMouseTooltip.FitOverlayToWorldRects(masteryLevelsHoverArea, list);
		masteryLevelsHoverArea.SetAsLastSibling();
		UIMouseTooltip.Attach(masteryLevelsHoverArea.gameObject, "tt_your_mastery_levels", null, addRaycastTarget: true);
	}

	public void OnBagShown(Item bag)
	{
		GamepadNavigationItem[] componentsInChildren = toolBeltInventoryWidget.GetComponentsInChildren<GamepadNavigationItem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Active = false;
		}
		perksWidget.DisableGamepadNavigation();
		buffsWidget.DisableGamepadNavigation();
		CharacterWindow.NavigationController.navigationGroupSources.Clear();
		int count = multiInventoryWidget.DrawnInventories.Count;
		List<GamepadNavigationController.NavigationGroupTarget> list = new List<GamepadNavigationController.NavigationGroupTarget>();
		List<GamepadNavigationController.NavigationGroupTarget> list2 = new List<GamepadNavigationController.NavigationGroupTarget>();
		list.Add(new GamepadNavigationController.NavigationGroupTarget(1000, GUIDirection.Right));
		for (int j = 0; j < count; j++)
		{
			list2.Add(new GamepadNavigationController.NavigationGroupTarget(j, GUIDirection.Left));
		}
		for (int k = 0; k < count; k++)
		{
			InventoryWidget inventoryWidget = multiInventoryWidget.DrawnInventories[k];
			List<GamepadNavigationController.NavigationGroupTarget> list3 = new List<GamepadNavigationController.NavigationGroupTarget>();
			list3.AddRange(list);
			list3.Add(new GamepadNavigationController.NavigationGroupTarget(k - 1, GUIDirection.Up));
			list3.Add(new GamepadNavigationController.NavigationGroupTarget(k + 1, GUIDirection.Down));
			CharacterWindow.NavigationController.navigationGroupSources.Add(new GamepadNavigationController.NavigationGroupSource(k, list3));
			for (int l = 0; l < inventoryWidget.Cells.Count; l++)
			{
				inventoryWidget.Cells[l].GamepadNavigationItem.group = k;
			}
		}
		List<GamepadNavigationController.NavigationGroupTarget> list4 = new List<GamepadNavigationController.NavigationGroupTarget>();
		list4.AddRange(list2);
		CharacterWindow.NavigationController.navigationGroupSources.Add(new GamepadNavigationController.NavigationGroupSource(1000, list4));
		bagInventoryWidget.Draw(data.BagInventoryWidgetData);
		for (int m = 0; m < bagInventoryWidget.Cells.Count; m++)
		{
			bagInventoryWidget.Cells[m].GamepadNavigationItem.group = 1000;
		}
		bagStateShading.SetActive(value: true);
		multiInventoryWidget.EnableBagMode(bag, data.OnMoveAllSimilarItemFromPlayerToBag);
		((RectTransform)bagInventoryWidget.transform).RefreshContentFitter();
		LazyAudio.PlayAndForget("bag_open");
		StartCoroutine(RebuildAfterBagShownNextFrame());
	}

	private IEnumerator RebuildAfterBagShownNextFrame()
	{
		yield return new WaitForEndOfFrame();
		((RectTransform)multiInventoryWidget.transform).RefreshContentFitter();
	}

	public void OnBagHide(Item bag)
	{
		for (int i = 0; i < multiInventoryWidget.DrawnInventories.Count; i++)
		{
			InventoryWidget inventoryWidget = multiInventoryWidget.DrawnInventories[i];
			for (int j = 0; j < inventoryWidget.Cells.Count; j++)
			{
				inventoryWidget.Cells[j].GamepadNavigationItem.group = 0;
			}
		}
		CharacterWindow.NavigationController.navigationGroupSources.Clear();
		GamepadNavigationItem[] componentsInChildren = toolBeltInventoryWidget.GetComponentsInChildren<GamepadNavigationItem>();
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			componentsInChildren[k].Active = true;
		}
		perksWidget.EnableGamepadNavigation();
		buffsWidget.EnableGamepadNavigation();
		bagInventoryWidget.Hide();
		bagStateShading.SetActive(value: false);
		multiInventoryWidget.DisableBagMode(bag);
		LazyAudio.PlayAndForget("bag_close");
	}

	public bool IsBagModeEnabled()
	{
		if (data != null && multiInventoryWidget != null)
		{
			return multiInventoryWidget.IsBagSelected;
		}
		return false;
	}

	public void OnAllToChestPressed()
	{
		if (IsBagModeEnabled())
		{
			data.OnMoveAllSimilarItemFromPlayerToBag?.Invoke();
		}
	}

	private void OnCloseBagBtnPressed()
	{
		data.HideBag();
	}

	private void OnMoveAllSimilarBtnInteractableChanged()
	{
		if (CharacterWindow != null)
		{
			CharacterWindow.RefreshGamepadTips();
		}
	}

	public override List<LazyGameKeyTip> GetTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (gamepadNavigationItem != null && gamepadNavigationItem.TryGetComponent<UIItemCell>(out var component) && component != null)
		{
			if (component.DisplayingItem != null && !component.DisplayingItem.IsEmpty && component.IsInteractable && component.OnItemCellPress != null)
			{
				list.Add(LazyGameKeyTip.Select());
			}
			if (component.DisplayingItem != null && !component.DisplayingItem.IsEmpty && component.IsInteractable && component.OnItemCellPress2 != null)
			{
				list.Add(new LazyGameKeyTip(GameKey.ItemMove, "tip_item_action"));
			}
		}
		if (multiInventoryWidget != null)
		{
			MultiInventoryWidgetData multiInventoryWidgetData = multiInventoryWidget.Data;
			if (multiInventoryWidgetData != null && multiInventoryWidgetData.WidgetSelectionMode == MultiInventoryWidgetMode.BagMode && multiInventoryWidget.IsMoveAllSimilarBtnInteractable)
			{
				list.Add(new LazyGameKeyTip(GameKey.MoveAllItemsFromPlayer, "tip_move_similar_items"));
			}
		}
		return list;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new CharMainPageWidgetData(MainGame.Instance.GameSave));
	}
}
