using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LinkedEntityWidget : LazyWidget<LinkedEntityWidgetData>
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	protected Image background;

	[SerializeField]
	private GameObject selectionFrame;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image qualityIcon;

	[SerializeField]
	private TextMeshProUGUI fontIcon;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private TextStyleComponent labelStyleComponent;

	[SerializeField]
	private GamepadNavigationItem gamepadNavigationItem;

	[SerializeField]
	private Image blockerImage;

	public LinkedEntityWidgetData WidgetData => data;

	public LazyButton Button => button;

	public override void Redraw()
	{
		base.Redraw();
		icon.sprite = data.Icon;
		qualityIcon.sprite = data.QualityIcon;
		qualityIcon.gameObject.SetActive(qualityIcon.sprite != null);
		label.text = data.LabelText;
		if (data.LabelCustomStyle != null)
		{
			labelStyleComponent.SetTextStyle(data.LabelCustomStyle);
			labelStyleComponent.ApplyStyle();
		}
		fontIcon.text = data.FontIcon;
		fontIcon.gameObject.SetActive(!string.IsNullOrEmpty(data.FontIcon));
		label.gameObject.SetActive(!string.IsNullOrEmpty(data.LabelText));
		icon.gameObject.SetActive(icon.sprite != null);
		button.onClick.RemoveAllListeners();
		background.enabled = data.LinkedEntityType != LinkedEntityType.BuildingDef && data.LinkedEntityType != LinkedEntityType.PerkDef && data.LinkedEntityType != LinkedEntityType.TownBuildingDef;
		if (!data.IsInactive)
		{
			button.onClick.AddListener(delegate
			{
				if (data.OnClicked != null)
				{
					data.OnClicked();
					selectionFrame.SetActive(value: false);
					UITooltip.HideImmediately();
				}
			});
		}
		blockerImage?.gameObject.SetActive(data.IsInactive);
	}

	private void Awake()
	{
		button.onEnter.AddListener(OnOver);
		button.onExit.AddListener(OnOut);
		button.SetCallbacksIntoGamepadNavigationItem();
		selectionFrame.SetActive(value: false);
	}

	private void OnOver()
	{
		if (data == null || data.IsInactive)
		{
			return;
		}
		if ((data.OnClicked != null || LazyInput.IsGamepadActive) && !data.NoSelectionFrames && selectionFrame != null)
		{
			selectionFrame.SetActive(value: true);
		}
		LinkedEntityWidgetData linkedEntityWidgetData = data;
		bool flag;
		if (linkedEntityWidgetData != null)
		{
			LinkedEntityType linkedEntityType = linkedEntityWidgetData.LinkedEntityType;
			if (linkedEntityType == LinkedEntityType.GameRes || linkedEntityType == LinkedEntityType.DayNumber)
			{
				flag = true;
				goto IL_0071;
			}
		}
		flag = false;
		goto IL_0071;
		IL_0071:
		if (!flag)
		{
			UITooltip.ShowLinkedEntity(this);
		}
	}

	private void OnOut()
	{
		if (selectionFrame != null)
		{
			selectionFrame.SetActive(value: false);
		}
		UITooltip.Hide();
	}

	private void OnDisable()
	{
		if (selectionFrame != null)
		{
			selectionFrame.SetActive(value: false);
		}
		if (UITooltip.IsTooltipShowingAtTarget(base.transform as RectTransform))
		{
			UITooltip.HideImmediately();
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new LinkedEntityWidgetData(GameBalance.Me.GetData<ItemDef>("pickaxe_0"), null));
	}
}
