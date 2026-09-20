using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIVendorOrderWidget : LazyWidget<UIVendorOrderWidgetData>
{
	[SerializeField]
	private LazyButton btnGrey;

	[SerializeField]
	private LazyButton btnGreen;

	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private GameObject decorTop;

	[SerializeField]
	private GameObject decorBot;

	[SerializeField]
	private GameObject completedCheckbox;

	[SerializeField]
	private GameObject lockedObject;

	[SerializeField]
	private GameObject urgentObject;

	[SerializeField]
	private Image repeatableObject;

	[SerializeField]
	private Sprite repeatableGrey;

	[SerializeField]
	private Sprite repeatableYellow;

	[SerializeField]
	private TextMeshProUGUI rewardLabel;

	[SerializeField]
	private GameObject plusObj;

	[SerializeField]
	private Sprite[] tierIcons;

	[SerializeField]
	private Image tierIcon;

	private bool isInitialized;

	public LazyButton BtnGrey => btnGrey;

	public UIVendorOrderWidgetData Data => data;

	public override void Init()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			base.Init();
			btnGrey.onClick.AddListener(OnPress);
			btnGrey.onEnter.AddListener(OnOver);
			btnGrey.onExit.AddListener(OnOut);
			btnGrey.onNotInteractableEnter.AddListener(OnOver);
			btnGrey.onNotInteractableExit.AddListener(OnOut);
			btnGreen.onClick.AddListener(OnPress);
			btnGreen.onEnter.AddListener(OnOver);
			btnGreen.onExit.AddListener(OnOut);
			btnGreen.onNotInteractableEnter.AddListener(OnOver);
			btnGreen.onNotInteractableExit.AddListener(OnOut);
			btnGrey.SetCallbacksIntoGamepadNavigationItem();
			btnGreen.SetCallbacksIntoGamepadNavigationItem();
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		tierIcon.gameObject.SetActive(value: false);
		btnGrey.gameObject.SetActive(value: false);
		btnGreen.gameObject.SetActive(value: false);
		decorTop.gameObject.SetActive(value: true);
		decorBot.gameObject.SetActive(value: true);
		btnGreen.interactable = true;
		btnGrey.interactable = true;
		if (data.IsEmpty)
		{
			btnGrey.gameObject.SetActive(value: true);
			completedCheckbox.SetActive(value: false);
			lockedObject.SetActive(value: false);
			urgentObject.SetActive(value: false);
			repeatableObject.gameObject.SetActive(value: false);
			rewardLabel.text = string.Empty;
			itemCell.DrawEmptyInteractable();
			plusObj.gameObject.SetActive(value: true);
		}
		else
		{
			bool flag = data.VendorOrderData.State == VendorOrderState.Finished;
			bool flag2 = data.Vendor.CurTier < data.VendorOrderData.Tier;
			bool isUrgent = data.VendorOrderData.Definition.isUrgent;
			bool isRenewable = data.VendorOrderData.Definition.isRenewable;
			bool isFinishedOnce = data.VendorOrderData.IsFinishedOnce;
			repeatableObject.sprite = (isFinishedOnce ? repeatableGrey : repeatableYellow);
			rewardLabel.text = string.Format("{0}+{1}", "happiness".FontIcon(), data.VendorOrderData.Definition.happinessReward.EvaluateFloat());
			WorldZoneData worldZoneDataById = MainGame.WorldData.GetWorldZoneDataById("warehouse");
			WorldZoneData worldZoneDataById2 = MainGame.WorldData.GetWorldZoneDataById("warehouse_cellar");
			int hasItemCount = worldZoneDataById.CountItemsOnTownPalettes(data.VendorOrderData.Definition.itemId) + worldZoneDataById2.CountItemsOnTownPalettes(data.VendorOrderData.Definition.itemId) + data.VendorOrderData.Count;
			itemCell.Draw(new Item(data.VendorOrderData.Definition.itemId, data.VendorOrderData.Definition.count), isNeedItem: true, hasItemCount, isCraftResult: false, 1, drawAsNonInteractable: false, 0, data.VendorOrderData.State != VendorOrderState.Finished);
			completedCheckbox.SetActive(flag);
			repeatableObject.gameObject.SetActive(value: false);
			if (isRenewable)
			{
				repeatableObject.gameObject.SetActive(!flag2);
			}
			if (flag)
			{
				lockedObject.SetActive(value: false);
				urgentObject.SetActive(value: false);
				if (isRenewable)
				{
					if (data.IsRenewableGreen)
					{
						btnGreen.gameObject.SetActive(value: true);
					}
					else
					{
						btnGrey.gameObject.SetActive(value: true);
					}
				}
				else
				{
					btnGreen.gameObject.SetActive(value: true);
				}
			}
			else
			{
				btnGrey.gameObject.SetActive(value: true);
				lockedObject.SetActive(flag2);
				if (flag2)
				{
					rewardLabel.text = string.Empty;
					tierIcon.sprite = tierIcons[data.VendorOrderData.Tier - 1];
					tierIcon.gameObject.SetActive(value: true);
					btnGrey.interactable = false;
				}
				urgentObject.SetActive(isUrgent && !flag2);
				if (isUrgent)
				{
					decorBot.gameObject.SetActive(value: false);
				}
			}
			plusObj.gameObject.SetActive(value: false);
		}
		if (!string.IsNullOrEmpty(rewardLabel.text))
		{
			decorTop.gameObject.SetActive(value: false);
		}
		if (data.ForcedInteractableState.hasForceState)
		{
			LazyButton lazyButton = btnGrey;
			bool interactable = (btnGreen.interactable = data.ForcedInteractableState.isInteractable);
			lazyButton.interactable = interactable;
		}
		itemCell.LazyButton.interactable = false;
	}

	private void OnPress()
	{
		data.OnPress?.Invoke(this);
	}

	private void OnOver()
	{
		if (!Data.IsEmpty && data.Vendor.CurTier >= data.VendorOrderData.Tier)
		{
			UITooltip.ShowOrderWidget(this);
		}
	}

	private void OnOut()
	{
		UITooltip.Hide();
	}

	private void OnDisable()
	{
		if (UITooltip.IsTooltipShowingAtTarget(base.transform as RectTransform))
		{
			UITooltip.HideImmediately();
		}
	}

	protected override void TestDraw()
	{
	}
}
