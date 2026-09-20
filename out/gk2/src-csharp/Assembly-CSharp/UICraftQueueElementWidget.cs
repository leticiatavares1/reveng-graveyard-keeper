using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LazyButton))]
public class UICraftQueueElementWidget : LazyWidget<UICraftQueueElementWidgetData>
{
	private Action onPress;

	private Action onOver;

	private Action onOut;

	private Action onPressPlusQueue;

	private Action onPressMinusQueue;

	private Action onInfCraftButtonPressed;

	private Action<CraftElementBase> onQueueUpButtonPressed;

	private Action<CraftElementBase> onQueueDownButtonPressed;

	private Action onHide;

	[SerializeField]
	private UIItemCell outputItem;

	[SerializeField]
	private LazyButton infCraftButton;

	[SerializeField]
	private LazyButton minusCraftButton;

	[SerializeField]
	private LazyButton plusCraftButton;

	[SerializeField]
	private Image selectionFrame;

	[SerializeField]
	private UITalentIcon talentIcon;

	[SerializeField]
	private LazyButton queueUpButton;

	[SerializeField]
	private LazyButton queueDownButton;

	[SerializeField]
	private Image craftProgress;

	[SerializeField]
	private Image craftProgressRed;

	[SerializeField]
	private RectTransform customTooltipTarget;

	private LazyButton widgetButton;

	private bool subscribedToCraftQueueElementEvents;

	private readonly HoldRepeatValueChanger craftCountHold = new HoldRepeatValueChanger();

	public UIItemCell OutputItem => outputItem;

	public LazyButton QueueUpButton => queueUpButton;

	public LazyButton QueueDownButton => queueDownButton;

	public LazyButton MinusCraftButton => minusCraftButton;

	public LazyButton PlusCraftButton => plusCraftButton;

	public UICraftQueueElementWidgetData Data => data;

	public override void Init()
	{
		base.Init();
		TryGetComponent<LazyButton>(out widgetButton);
		widgetButton.onClick.AddListener(OnPress);
		widgetButton.onEnter.AddListener(OnOver);
		widgetButton.onExit.AddListener(OnOut);
		infCraftButton.onClick.AddListener(OnInfCraftButtonPressed);
		queueUpButton.onClick.AddListener(delegate
		{
			OnQueueUpButtonPressed();
		});
		queueDownButton.onClick.AddListener(delegate
		{
			OnQueueDownButtonPressed();
		});
		OutputItem.GamepadNavigationItem.SetCallbacks(widgetButton.onEnter.Invoke, widgetButton.onExit.Invoke, widgetButton.onClick.Invoke);
	}

	public override void DeInit()
	{
		base.DeInit();
		widgetButton.onClick.RemoveAllListeners();
		widgetButton.onEnter.RemoveAllListeners();
		widgetButton.onExit.RemoveAllListeners();
		infCraftButton.onClick.RemoveAllListeners();
		queueUpButton.onClick.RemoveAllListeners();
		queueDownButton.onClick.RemoveAllListeners();
	}

	protected override void SetData(UICraftQueueElementWidgetData data)
	{
		base.SetData(data);
		SubscribeToCraftQueueElementEvents();
	}

	public override void Redraw()
	{
		base.Redraw();
		onPress = data.OnPress;
		onOver = data.OnOver;
		onOut = data.OnOut;
		onHide = data.OnHide;
		onPressPlusQueue = data.OnPressPlusQueue;
		onPressMinusQueue = data.OnPressMinusQueue;
		onInfCraftButtonPressed = data.OnInfCraftButtonPress;
		onQueueUpButtonPressed = data.OnPressQueueUp;
		onQueueDownButtonPressed = data.OnPressQueueDown;
		outputItem.gameObject.SetActive(value: true);
		outputItem.DrawCraftOutput(data.CraftQueueElement.Def.GetOutputPreview(data.WgoData), -1, data.CraftQueueElement.CraftStatus, data.CraftQueueElement.ParamsData.RequiredToolType, data.CraftQueueElement.Count);
		outputItem.CustomTooltipShowAction = delegate(UIItemCell cell)
		{
			UICraftStatusInfoWidgetData statusInfoWidgetData = null;
			if (outputItem.StatusIcon != null && outputItem.StatusIcon.sprite != null && outputItem.StatusIcon.gameObject.activeSelf && outputItem.StatusIcon.gameObject.activeInHierarchy && data.CraftQueueElement.CraftStatus != CraftStatus.Other)
			{
				statusInfoWidgetData = new UICraftStatusInfoWidgetData(DefineDescriptionForCraftStatus(), TextAlignmentOptions.Center, null, outputItem.StatusIcon.sprite);
			}
			LazyAudio.PlayAndForget("gui_hover_light");
			UITooltip.ShowCraftInfo(cell, data.WgoData, data.CraftQueueElement.Def as CraftDef, data.CraftQueueElement.Requirements, customTooltipTarget, statusInfoWidgetData);
		};
		outputItem.GamepadNavigationItem.group = 1;
		craftProgress.transform.parent.gameObject.SetActive(data.CraftQueueElement.IsStarted);
		if (data.CraftQueueElement.IsStarted)
		{
			UpdateProgress();
		}
		HideSelection();
		UpdateTalentIcon();
		UpdateCount();
		UpdateStatusIcon(data.CraftQueueElement.CraftStatus);
	}

	public override void Hide()
	{
		outputItem.Flush();
		onHide?.Invoke();
		craftCountHold.Reset();
		UnsubscribeFromCraftQueueElementEvents();
		base.Hide();
	}

	public void HandleCountChange()
	{
		UpdateCount();
		if (data.CraftQueueElement.Count == 0)
		{
			data.OnRemoveFromQueuePressed?.Invoke(data.CraftQueueElement);
		}
	}

	public void UpdateCount()
	{
		outputItem.OnMultiplierChange(data.CraftQueueElement.Count);
	}

	public void UpdateStatusIcon(CraftStatus craftStartStatus)
	{
		if (craftStartStatus == CraftStatus.DoesntHaveRequiredTool)
		{
			outputItem.UpdateStatusIcon(craftStartStatus, data.CraftQueueElement.ParamsData.RequiredToolType);
		}
		else
		{
			outputItem.UpdateStatusIcon(craftStartStatus);
		}
	}

	private string DefineDescriptionForCraftStatus()
	{
		switch (data.CraftQueueElement.CraftStatus)
		{
		case CraftStatus.NotEnoughResources:
			return LLBase.L("ui_craft_status_NotEnoughResources");
		case CraftStatus.DoesntHaveRequiredTool:
			return LLBase.L("ui_craft_status_DoesntHaveRequiredTool");
		case CraftStatus.NotEnoughSpaceInWgo:
		case CraftStatus.NotEnoughSpaceInMultiInventory:
			return LLBase.L("ui_craft_status_NotEnoughSpaceInWgo");
		case CraftStatus.NotEnoughMastery:
			return LLBase.L("ui_craft_status_NotEnoughMastery");
		default:
			return LLBase.L("ui_craft_status_NotEnoughResources");
		}
	}

	public void UpdateTalentIcon()
	{
		if (data.CraftQueueElement.ParamsData.MasteryValue < data.CraftQueueElement.ParamsData.MasteryLock)
		{
			talentIcon.Draw(data.CraftQueueElement.ParamsData.TalentDef, data.CraftQueueElement.ParamsData.MasteryLock, isEnoughMastery: false, data.CraftQueueElement.Def.isStarCraft || data.CraftQueueElement.Def.isAutopsyCraft || data.CraftQueueElement.Def.isPocketExtractCraft);
		}
		else
		{
			talentIcon.Hide();
		}
	}

	public void UpdateQueueButtons()
	{
		int num = data.CraftQueue.IndexOf(data.CraftQueueElement);
		bool flag = num == 0;
		bool flag2 = num == data.CraftQueue.Count - 1;
		bool isStarted = data.CraftQueueElement.IsStarted;
		bool flag3 = num == 1 && !data.CraftQueue[num - 1].IsStarted;
		queueUpButton.gameObject.SetActive(value: true);
		queueUpButton.interactable = !flag && (num != 1 || flag3);
		queueDownButton.gameObject.SetActive(value: true);
		queueDownButton.interactable = !flag2 && !(flag && isStarted);
	}

	private void UnsubscribeFromCraftQueueElementEvents()
	{
		if (subscribedToCraftQueueElementEvents)
		{
			data.CraftQueueElement.OnStatusChanged -= UpdateStatusIcon;
			data.CraftQueueElement.OnCountChanged -= UpdateCount;
			data.CraftQueueElement.OnProgressChanged -= UpdateProgress;
			subscribedToCraftQueueElementEvents = false;
		}
	}

	private void SubscribeToCraftQueueElementEvents()
	{
		if (!subscribedToCraftQueueElementEvents)
		{
			data.CraftQueueElement.OnStatusChanged += UpdateStatusIcon;
			data.CraftQueueElement.OnCountChanged += UpdateCount;
			data.CraftQueueElement.OnProgressChanged += UpdateProgress;
			subscribedToCraftQueueElementEvents = true;
		}
	}

	public void OnOver()
	{
		onOver?.Invoke();
		ShowSelection();
	}

	private void OnOut()
	{
		onOut?.Invoke();
		HideSelection();
	}

	private void OnPress()
	{
		onPress?.Invoke();
	}

	private void ShowSelection()
	{
		plusCraftButton.interactable = !data.CraftQueueElement.IsStarted && !data.IsMulticraftDisabled;
		plusCraftButton.gameObject.SetActive(value: true);
		minusCraftButton.interactable = !data.CraftQueueElement.IsStarted;
		minusCraftButton.gameObject.SetActive(value: true);
		selectionFrame.gameObject.SetActive(value: true);
		UpdateQueueButtons();
	}

	private void HideSelection()
	{
		infCraftButton.gameObject.SetActive(value: false);
		minusCraftButton.gameObject.SetActive(value: false);
		plusCraftButton.gameObject.SetActive(value: false);
		selectionFrame.gameObject.SetActive(value: false);
		queueUpButton.gameObject.SetActive(value: false);
		queueDownButton.gameObject.SetActive(value: false);
	}

	public void ChangeCount(int delta)
	{
		if (delta == 0 || data == null)
		{
			return;
		}
		if (delta > 0)
		{
			if (!(plusCraftButton == null) && plusCraftButton.interactable)
			{
				data.AddCount(delta);
				UpdateCount();
			}
		}
		else if (!(minusCraftButton == null) && minusCraftButton.interactable)
		{
			data.AddCount(delta);
			HandleCountChange();
		}
	}

	private void Update()
	{
		if (data == null)
		{
			craftCountHold.Reset();
		}
		else
		{
			craftCountHold.Tick(HoldRepeatValueChanger.GetPointerHoldDirection(plusCraftButton, minusCraftButton), ChangeCount);
		}
	}

	private void OnInfCraftButtonPressed()
	{
		onInfCraftButtonPressed?.Invoke();
		UpdateCount();
	}

	private void OnQueueUpButtonPressed()
	{
		if (queueUpButton.interactable)
		{
			onQueueUpButtonPressed?.Invoke(data.CraftQueueElement);
		}
	}

	private void OnQueueDownButtonPressed()
	{
		if (queueDownButton.interactable)
		{
			onQueueDownButtonPressed?.Invoke(data.CraftQueueElement);
		}
	}

	private void UpdateProgress()
	{
		craftProgress.fillAmount = data.CraftQueueElement.ProgressTimeNormalized;
		craftProgressRed.fillAmount = data.CraftQueueElement.ProgressTimeNormalizedFailed;
	}

	protected override void TestDraw()
	{
	}
}
