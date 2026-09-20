using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkWidget : LazyWidget<PerkWidgetData>
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image blackout;

	[SerializeField]
	private GameObject selectionFrame;

	[SerializeField]
	protected TextMeshProUGUI durationLabel;

	[Space]
	[SerializeField]
	private TextMeshProUGUI dbgImageText;

	private Action<PerkWidgetData> onPress;

	private Action<PerkWidgetData> onOver;

	private Action<PerkWidgetData> onOut;

	private GamepadNavigationItem gamepadNavigationItem;

	public PerkData PerkData => data.PerkData;

	public GamepadNavigationItem GamepadNavigationItem
	{
		get
		{
			TryInitGamepadNavigationItem();
			return gamepadNavigationItem;
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		icon.sprite = data.PerkData.Definition.Icon;
		if (icon.sprite == null)
		{
			icon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("i_placeholder");
		}
		icon.SetNativeSize();
		onPress = data.OnPress;
		onOver = data.OnOver;
		onOut = data.OnOut;
		if (dbgImageText != null)
		{
			dbgImageText.text = data.PerkData.id;
			dbgImageText.gameObject.SetActive(icon.sprite == null);
		}
		blackout.gameObject.SetActive(!data.IsActive);
		durationLabel.gameObject.SetActive(!data.PerkData.Definition.hiddenTimer);
		durationLabel.text = PerkSystemData.GetFormattedDuration(data.PerkData.currentDuration);
	}

	public void ClearCallbacks()
	{
		onPress = null;
		onOver = null;
		onOut = null;
	}

	private void Awake()
	{
		button.onDown.AddListener(OnPress);
		button.onEnter.AddListener(OnOver);
		button.onExit.AddListener(OnOut);
		selectionFrame.SetActive(value: false);
		TryInitGamepadNavigationItem();
	}

	private void TryInitGamepadNavigationItem()
	{
		if (gamepadNavigationItem == null)
		{
			gamepadNavigationItem = GetComponent<GamepadNavigationItem>();
			if (gamepadNavigationItem != null)
			{
				gamepadNavigationItem.SetCallbacks(button.ForceOnEnter, button.ForceOnExit, button.ForceOnClick);
			}
		}
	}

	private void OnPress()
	{
		onPress?.Invoke(data);
	}

	private void OnOver()
	{
		selectionFrame.SetActive(value: true);
		UITooltip.ShowPerkWidget(this);
		onOver?.Invoke(data);
	}

	private void OnOut()
	{
		selectionFrame.SetActive(value: false);
		UITooltip.Hide();
		onOut?.Invoke(data);
	}

	private void OnDisable()
	{
		selectionFrame.SetActive(value: false);
		if (UITooltip.IsTooltipShowingAtTarget(base.transform as RectTransform))
		{
			UITooltip.HideImmediately();
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new PerkWidgetData(new PerkData("buff_alchomaster"), isActive: true, null, null, null));
	}
}
