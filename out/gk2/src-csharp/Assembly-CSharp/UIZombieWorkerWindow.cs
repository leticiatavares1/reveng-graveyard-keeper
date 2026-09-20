using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIZombieWorkerWindow : LazyWindow<UIZombieWorkerWindowData>
{
	[SerializeField]
	private Canvas contentCanvas;

	[SerializeField]
	private UIWorkerIcon workerIcon;

	[SerializeField]
	private TalentWidget[] talentWidgets;

	[SerializeField]
	private List<string> talents = new List<string>();

	[SerializeField]
	private TextMeshProUGUI nameLabel;

	[SerializeField]
	private TextMeshProUGUI whiteSkullsLabel;

	[SerializeField]
	private TextMeshProUGUI redSkullsLabel;

	[SerializeField]
	private GameObject mainTabWidget;

	[SerializeField]
	private TextMeshProUGUI nextTabGamepadHelper;

	[SerializeField]
	private TextMeshProUGUI prevTabGamepadHelper;

	[SerializeField]
	private TextMeshProUGUI nextSubTabGamepadHelper;

	[SerializeField]
	private TextMeshProUGUI prevSubTabGamepadHelper;

	[SerializeField]
	private TextStyle perksSkullsStyle;

	[SerializeField]
	private ZombieWindowTabButton characterTabButton;

	[SerializeField]
	private BodyOrgansInventoryWidget bodyOrgansInventoryWidget;

	[SerializeField]
	private BodyPocketInventoryWidget bodyPocketInventoryWidget;

	[SerializeField]
	private ZombieEquipmentInventoryWidget zombieEquipmentInventoryWidget;

	[SerializeField]
	private ZombieWindowTabButton perksTabButton;

	[SerializeField]
	private ZombieProgressionWidget zombieProgressionWidget;

	[SerializeField]
	private TextMeshProUGUI redSpheresLabel;

	[SerializeField]
	private TextMeshProUGUI greenSpheresLabel;

	[SerializeField]
	private TextMeshProUGUI blueSpheresLabel;

	private static bool lastOpenedPerksTab;

	public override void Init()
	{
		base.Init();
		characterTabButton.Init(RedrawCharacterTab);
		perksTabButton.Init(RedrawPerksTab);
		UIZombieWorkerWindowData.OnBodyItemAddOrRemove += RedrawTalentIcons;
		UIZombieWorkerWindowData.OnBodyItemAddOrRemove += RedrawMilitaryBaseWorldZoneWidget;
		ZombieWgoData.OnTalentLevelUpPurchased += OnTalentLevelUpPurchased;
		ZombieWgoData.OnTechPointsAddedToZombie += OnTechPointsChanged;
		UIMouseTooltip.Attach(talentWidgets[0].transform.parent.gameObject, "tt_zombie_mastery_levels", null, addRaycastTarget: true, disableChildRaycasts: false, new UIMouseTooltipEdges(0f, 0f, 10f, 12f));
		UIMouseTooltip.Attach(redSpheresLabel.transform.parent.gameObject, "tt_zombie_2", null, addRaycastTarget: true, disableChildRaycasts: true);
		UIMouseTooltip.Attach(whiteSkullsLabel.transform.parent.gameObject, "tt_zombie_3", null, addRaycastTarget: true, disableChildRaycasts: true, new UIMouseTooltipEdges(0f, 0f, 19f, 20f));
	}

	public override void Redraw()
	{
		base.Redraw();
		characterTabButton.UpdateText(LLBase.L("ui_zombie_wndw_char"));
		RedrawWorkerIcon();
		RedrawTalentIcons();
		RedrawSkulls();
		RedrawPerksTabLabel();
		RedrawName();
		RedrawSpheres();
		if (lastOpenedPerksTab)
		{
			RedrawPerksTab();
		}
		else
		{
			RedrawCharacterTab();
		}
		contentCanvas.sortingOrder = canvas.sortingOrder + 5;
	}

	public override void Open(UIZombieWorkerWindowData data)
	{
		base.Open(data);
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	public override void Hide()
	{
		zombieProgressionWidget.Hide();
		bodyOrgansInventoryWidget.Hide();
		bodyPocketInventoryWidget.Hide();
		zombieEquipmentInventoryWidget.Hide();
		base.Hide();
	}

	private void RedrawCharacterTab()
	{
		mainTabWidget.SetActive(value: true);
		zombieProgressionWidget.gameObject.SetActive(value: false);
		bodyOrgansInventoryWidget.Draw(data.BodyOrgansInventoryWidgetData);
		bodyPocketInventoryWidget.Draw(data.BodyPocketInventoryWidgetData);
		zombieEquipmentInventoryWidget.Draw(data.ZombieEquipmentInventoryWidgetData);
		characterTabButton.UpdateState(isActive: true, base.Canvas);
		perksTabButton.UpdateState(isActive: false, base.Canvas);
		lastOpenedPerksTab = false;
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private void RedrawPerksTab()
	{
		mainTabWidget.SetActive(value: false);
		zombieProgressionWidget.gameObject.SetActive(value: true);
		characterTabButton.UpdateState(isActive: false, base.Canvas);
		perksTabButton.UpdateState(isActive: true, base.Canvas);
		zombieProgressionWidget.Draw(data.ZombieProgressionWidgetData);
		lastOpenedPerksTab = true;
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private void OnTalentLevelUpPurchased()
	{
		if (base.IsShown)
		{
			RedrawPerksTabLabel();
			RedrawSpheres();
			RedrawTalentIcons();
		}
	}

	private void OnTechPointsChanged(WgoData wgoData, ZombieWgoData zombieWgoData, string type, int value)
	{
		if (base.IsShown && zombieWgoData == data.ZombieWgoData)
		{
			RedrawSpheres();
			if (zombieProgressionWidget.gameObject.activeSelf)
			{
				zombieProgressionWidget.Redraw();
			}
		}
	}

	private void RedrawSpheres()
	{
		redSpheresLabel.text = string.Format("{0}{1}", "tech_red".FontIcon(), data.ZombieWgoData.techRed);
		greenSpheresLabel.text = string.Format("{0}{1}", "tech_green".FontIcon(), data.ZombieWgoData.techGreen);
		blueSpheresLabel.text = string.Format("{0}{1}", "tech_blue".FontIcon(), data.ZombieWgoData.techBlue);
	}

	private void RedrawSkulls()
	{
		whiteSkullsLabel.text = string.Format("{0}{1}", "skull-zombie_window".FontIcon(), data.ZombieWgoData.WhiteSkulls);
		redSkullsLabel.text = string.Format("{0}{1}", "rskull-zombie_window".FontIcon(), data.ZombieWgoData.RedSkulls);
	}

	private void RedrawTalentIcons()
	{
		if (base.IsShown)
		{
			for (int i = 0; i < talentWidgets.Length; i++)
			{
				talentWidgets[i].Draw(new TalentWidgetData(talents[i], data.ZombieWgoData.GetMasteryLevelForTalentBranch(talents[i])));
			}
		}
	}

	private void RedrawMilitaryBaseWorldZoneWidget()
	{
		if (MainGame.Instance.GameSave.militaryBaseData.ContainsFighter(data.ZombieWgoData))
		{
			MainGame.Instance.GameSave.WorldData.GetWorldZoneDataById("town_guard_barracks")?.NotifyWgoDataChanged();
		}
	}

	private void RedrawName()
	{
		nameLabel.text = LLBase.L(data.ZombieWgoData.Name);
	}

	private void RedrawWorkerIcon()
	{
		workerIcon.ShowWithoutTalent(data.ZombieWgoData, ZombieSkinHelper.GetPresetForWgoData(data.ZombieWgoData, "zombie_worker"));
	}

	private void RedrawPerksTabLabel()
	{
		string text = perksSkullsStyle.ApplyStyleToString(string.Format("{0}/{1}{2}", data.ZombieWgoData.GetUsedPerksCount(), data.ZombieWgoData.RedSkulls, "rskull".FontIcon()));
		perksTabButton.UpdateText(LLBase.L("ui_zombie_wndw_perks") + " " + text);
	}

	private bool OnPressedPrevTab()
	{
		LazyAudio.PlayAndForget("tab_click");
		if (zombieProgressionWidget.gameObject.activeSelf)
		{
			RedrawCharacterTab();
		}
		else
		{
			RedrawPerksTab();
		}
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		return true;
	}

	private bool OnPressedNextTab()
	{
		LazyAudio.PlayAndForget("tab_click");
		if (zombieProgressionWidget.gameObject.activeSelf)
		{
			RedrawCharacterTab();
		}
		else
		{
			RedrawPerksTab();
		}
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		return true;
	}

	public bool OnPressedPrevSubTab()
	{
		if (zombieProgressionWidget.gameObject.activeSelf)
		{
			LazyAudio.PlayAndForget("tab_click");
			zombieProgressionWidget.TalentTabButtonsContainer.OnPressedPrevTechTab();
			if (LazyInput.IsGamepadActive)
			{
				base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			}
			return true;
		}
		return false;
	}

	public bool OnPressedNextSubTab()
	{
		if (zombieProgressionWidget.gameObject.activeSelf)
		{
			LazyAudio.PlayAndForget("tab_click");
			zombieProgressionWidget.TalentTabButtonsContainer.OnPressedNextTechTab();
			if (LazyInput.IsGamepadActive)
			{
				base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			}
			return true;
		}
		return false;
	}

	private bool OnItemPressed2()
	{
		if (LazyInput.IsGamepadActive)
		{
			GamepadNavigationItem focusedItem = base.GamepadNavigationController.FocusedItem;
			if (focusedItem != null && focusedItem.TryGetComponent<UIItemCell>(out var component) && component.DisplayingItem != null && !component.DisplayingItem.IsEmpty)
			{
				component.OnPress2();
				return true;
			}
		}
		return false;
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.NextTab, OnPressedNextTab);
		gameKeyDelegates.Add(GameKey.PrevTab, OnPressedPrevTab);
		gameKeyDelegates.Add(GameKey.NextSubTab, OnPressedNextSubTab);
		gameKeyDelegates.Add(GameKey.PrevSubTab, OnPressedPrevSubTab);
		gameKeyDelegates.Add(GameKey.ItemMove, OnItemPressed2);
		return gameKeyDelegates;
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		if (LazyInput.IsGamepadActive)
		{
			nextTabGamepadHelper.gameObject.SetActive(value: true);
			prevTabGamepadHelper.gameObject.SetActive(value: true);
			nextSubTabGamepadHelper.gameObject.SetActive(value: true);
			prevSubTabGamepadHelper.gameObject.SetActive(value: true);
			nextTabGamepadHelper.text = ControllerIconLibrary.GetIconId(GameKey.NextTab);
			prevTabGamepadHelper.text = ControllerIconLibrary.GetIconId(GameKey.PrevTab);
			nextSubTabGamepadHelper.text = ControllerIconLibrary.GetIconId(GameKey.NextSubTab);
			prevSubTabGamepadHelper.text = ControllerIconLibrary.GetIconId(GameKey.PrevSubTab);
		}
		else
		{
			nextTabGamepadHelper.gameObject.SetActive(value: false);
			prevTabGamepadHelper.gameObject.SetActive(value: false);
			nextSubTabGamepadHelper.gameObject.SetActive(value: false);
			prevSubTabGamepadHelper.gameObject.SetActive(value: false);
		}
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (gamepadNavigationItem != null && gamepadNavigationItem.TryGetComponent<UIItemCell>(out var component))
		{
			if (component.IsInteractable && component.OnItemCellPress != null)
			{
				list.Add(LazyGameKeyTip.Select());
			}
			if (component.DisplayingItem != null && !component.DisplayingItem.IsEmpty && component.IsInteractable && component.OnItemCellPress2 != null)
			{
				list.Add(new LazyGameKeyTip(GameKey.ItemMove, "tip_item_action"));
			}
		}
		if ((bool)closeButton)
		{
			list.Add(LazyGameKeyTip.Back());
		}
		lazyButtonTips.Print(list);
	}

	[LazyUITest]
	protected override void TestDraw()
	{
	}
}
