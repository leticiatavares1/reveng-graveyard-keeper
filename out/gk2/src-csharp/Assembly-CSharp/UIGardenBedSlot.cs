using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGardenBedSlot : MonoBehaviour
{
	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private Sprite backDefault;

	[SerializeField]
	private Sprite backLocked;

	[SerializeField]
	private Sprite backEmpty;

	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private GamepadNavigationItem gamepadNavigationItem;

	[SerializeField]
	private TextMeshProUGUI masteryLabel;

	[SerializeField]
	private Image statusImage;

	public int slotIndex;

	private Action<UIGardenBedSlot> onPress;

	private PerkData perkData;

	public PerkData PerkData => perkData;

	private void Awake()
	{
		button.onEnter.AddListener(OnOver);
		button.onExit.AddListener(OnOut);
		gamepadNavigationItem.SetCallbacks(button.ForceOnEnter, button.ForceOnExit, null);
	}

	public void DrawFertilizerSlot(PerkData perkData, Action<UIGardenBedSlot> onPress, bool allowInteraction = true)
	{
		this.perkData = perkData;
		this.onPress = onPress;
		itemCell.DrawCustom(perkData.Definition.IconId, 1, interactable: true);
		itemCell.CustomTooltipShowAction = delegate
		{
			UITooltip.ShowGardenFertilizerSlot(this);
		};
		itemCell.ShowMouseSelectionFrame = allowInteraction;
		statusImage.gameObject.SetActive(value: false);
		gamepadNavigationItem.Active = true;
		button.interactable = true;
		TalentDef data = GameBalance.Me.GetData<TalentDef>("talent_green");
		masteryLabel.text = $"+<space=1px>{data.id.FontIcon()}<space=2px>{perkData.Definition.craftMasteryBonus}";
		masteryLabel.gameObject.SetActive(value: true);
		UIMouseTooltip.Attach(masteryLabel.gameObject, "tt_garden_3_add", null, addRaycastTarget: true);
	}

	public void DrawSeedSlot(Item seed)
	{
		perkData = null;
		onPress = null;
		itemCell.Draw(seed);
		itemCell.ClearCallbacks();
		itemCell.ShowMouseSelectionFrame = false;
		itemCell.TooltipPlacementPriority = TooltipPlacementPriority.BottomRight;
		masteryLabel.gameObject.SetActive(value: false);
		statusImage.gameObject.SetActive(value: false);
		gamepadNavigationItem.Active = true;
		button.interactable = true;
	}

	public void DrawEmpty(Action<UIGardenBedSlot> onPress, bool allowInteraction = true)
	{
		this.onPress = (allowInteraction ? onPress : null);
		perkData = null;
		itemCell.DrawEmpty(!allowInteraction, resetWidgetState: true, !allowInteraction);
		itemCell.ClearCallbacks();
		if (allowInteraction)
		{
			itemCell.OnItemCellPress = OnPress;
		}
		statusImage.gameObject.SetActive(allowInteraction);
		if (allowInteraction)
		{
			statusImage.sprite = backEmpty;
		}
		gamepadNavigationItem.Active = allowInteraction;
		button.interactable = allowInteraction;
		masteryLabel.gameObject.SetActive(value: false);
	}

	public void DrawLocked()
	{
		perkData = null;
		onPress = null;
		itemCell.DrawEmpty(drawAsNonInteractable: true, resetWidgetState: true, noSelectionFrames: true);
		statusImage.gameObject.SetActive(value: true);
		statusImage.sprite = backLocked;
		gamepadNavigationItem.Active = false;
		button.interactable = false;
		masteryLabel.gameObject.SetActive(value: false);
	}

	private void OnOver()
	{
		if (perkData != null)
		{
			UITooltip.ShowGardenFertilizerSlot(this);
		}
	}

	private void OnOut()
	{
		UITooltip.Hide();
	}

	private void OnPress(UIItemCell cell)
	{
		onPress(this);
	}

	private void OnDisable()
	{
		if (UITooltip.IsTooltipShowingAtTarget(base.transform as RectTransform))
		{
			UITooltip.HideImmediately();
		}
	}
}
