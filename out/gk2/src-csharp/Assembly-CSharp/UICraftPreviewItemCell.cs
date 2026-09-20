using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UICraftPreviewItemCell : LazyWidget<UICraftPreviewItemCellData>
{
	public UIItemCell uiItemCell;

	public Image customImage;

	public Image customImageBack;

	[SerializeField]
	private GameObject uknownCraftBlocker;

	[SerializeField]
	private GameObject canNotStartCraftBlocker;

	[SerializeField]
	private GameObject customImageNonInteractableBlocker;

	[SerializeField]
	private GamepadNavigationItem gamepadNavigationItem;

	[SerializeField]
	private GamepadNavigationItem gamepadNavigationItemExtension;

	[SerializeField]
	private LazyButton tabButton;

	[SerializeField]
	private GameObject selectionFrameTab;

	public GamepadNavigationItem ItemCellGamepadNavigationItem => gamepadNavigationItem;

	private void Awake()
	{
		tabButton.onEnter.AddListener(OnOver);
		tabButton.onExit.AddListener(OnOut);
		gamepadNavigationItemExtension.SetCallbacks(tabButton.ForceOnEnter, tabButton.ForceOnExit, null);
	}

	public override void Redraw()
	{
		base.Redraw();
		tabButton.gameObject.SetActive(value: false);
		if (data.IsTab)
		{
			customImage.sprite = (data.UseTabAsIcon ? LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(data.TabId, "i_b_null") : LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("i_b_" + data.TabId, "i_b_null"));
			customImageBack.sprite = data.CustomImageBackSprite;
			customImageBack.SetNativeSize();
			customImageBack.gameObject.SetActive(value: true);
			uiItemCell.gameObject.SetActive(value: false);
			uknownCraftBlocker.gameObject.SetActive(value: false);
			canNotStartCraftBlocker.gameObject.SetActive(value: false);
			customImageNonInteractableBlocker.gameObject.SetActive(data.IsExtension && !data.IsExtensionAvailable);
			if (data.IsExtension)
			{
				tabButton.gameObject.SetActive(value: true);
				base.name = "Extension " + data.ExtensionId + " tabId:[" + data.TabId + "]";
			}
			return;
		}
		uiItemCell.gameObject.SetActive(value: true);
		customImageBack.gameObject.SetActive(value: false);
		if (!data.IsUnknown)
		{
			DrawCraftOutput(data.CraftDef.GetOutputPreview(data.WgoData));
			UIItemCell uIItemCell = uiItemCell;
			uIItemCell.OnItemCellPress = (Action<UIItemCell>)Delegate.Combine(uIItemCell.OnItemCellPress, (Action<UIItemCell>)delegate
			{
				OpenCraftSetupWindow();
			});
			uiItemCell.CustomTooltipShowAction = delegate(UIItemCell cell)
			{
				if (!data.IsUnknown)
				{
					UITooltip.ShowCraftInfo(cell, data.WgoData, data.CraftDef);
				}
			};
		}
		else
		{
			uiItemCell.CustomTooltipShowAction = null;
			uiItemCell.DrawEmpty(drawAsNonInteractable: true);
			UpdateLocks();
		}
	}

	public override void Hide()
	{
		base.Hide();
		uiItemCell.ClearCallbacks();
	}

	private void DrawCraftOutput(OutputPreview outputPreview)
	{
		uiItemCell.DrawCraftOutput(outputPreview);
		UpdateLocks();
	}

	public void UpdateLocks()
	{
		if (!data.IsTab)
		{
			uknownCraftBlocker.SetActive(data.IsUnknown);
			canNotStartCraftBlocker.SetActive(!data.CanStart && !data.IsUnknown);
		}
	}

	private void OnOver()
	{
		if (data.IsExtension)
		{
			selectionFrameTab.SetActive(value: true);
			LazyAudio.PlayAndForget("gui_hover_light");
			UITooltip.ShowExtensionInfo(this, data.ExtensionId);
		}
	}

	private void OnOut()
	{
		selectionFrameTab.SetActive(value: false);
		if (data.IsExtension)
		{
			UITooltip.Hide();
		}
	}

	private void OnDisable()
	{
		selectionFrameTab.SetActive(value: false);
		if (data.IsExtension && UITooltip.IsTooltipShowingAtTarget(base.transform as RectTransform))
		{
			UITooltip.HideImmediately();
		}
	}

	private void OpenCraftSetupWindow()
	{
		if (data.CraftDef.isFuelCraft)
		{
			UISingleCraftWindowData uISingleCraftWindowData = new UISingleCraftWindowData(data.WgoData, data.CraftDef, data.OnQueueAdded, data.OnCraftStarted);
			LazyUI.GetWindow<UIFuelCraftWindow>().Open(uISingleCraftWindowData);
		}
		else
		{
			UICraftSelectionWindowData uICraftSelectionWindowData = new UICraftSelectionWindowData(data.WgoData, data.CraftDef, data.OnQueueAdded, data.OnCraftStarted);
			uICraftSelectionWindowData.IsGravePartRemove = data.IsGravePartRemove;
			LazyUI.GetWindow<UICraftSelectionWindow>().Open(uICraftSelectionWindowData);
		}
	}

	protected override void TestDraw()
	{
	}
}
