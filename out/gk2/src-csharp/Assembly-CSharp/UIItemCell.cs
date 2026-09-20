using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemCell : MonoBehaviour
{
	private const int SELECTION_BLINK_COUNT = 3;

	private const float SELECTION_BLINK_DELAY = 0.5f;

	public Action<UIItemCell> OnItemCellOver;

	public Action<UIItemCell> OnItemCellOut;

	public Action<UIItemCell> OnItemCellPress;

	public Action<UIItemCell> OnItemCellPress2;

	public Action<UIItemCell> OnItemCellDown;

	public Action<UIItemCell> CustomTooltipShowAction;

	public string ExtraRedTooltipLocId;

	public Action OnWidgetPress;

	[SerializeField]
	private ItemRelatedWidgetState widgetState = ItemRelatedWidgetState.NotSet;

	[Space]
	private Item displayingItem;

	private ItemCount displayingItemCount;

	private OutputPreview displayingOutputPreview;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Color backColorEmpty;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image starIcon;

	[SerializeField]
	private TextMeshProUGUI iconLabelPlaceholder;

	[SerializeField]
	private Image selection;

	[SerializeField]
	private Image selectionNonInteractable;

	[SerializeField]
	private TextMeshProUGUI countLabel;

	[SerializeField]
	private TextMeshProUGUI priceLabel;

	[SerializeField]
	private ImageColors colors;

	[SerializeField]
	private Image nonInteractableItemImg;

	[SerializeField]
	private LazyButton lazyButton;

	[SerializeField]
	private TextStyleComponent countLabelStyle;

	[SerializeField]
	private TextStyle countLabelNormal;

	[SerializeField]
	private TextStyle countLabelRed;

	[SerializeField]
	[Space]
	private Image statusIcon;

	private Vector2 defaultStatusIconAnchoredPosition;

	private bool hasCachedStatusIconPosition;

	private int value;

	private int hasItemCount;

	private bool isNeedItem;

	private bool drawCounter;

	private bool noSelectionFrames;

	private bool showMouseSelectionFrame = true;

	private TooltipPlacementPriority tooltipPlacementPriority;

	private GamepadNavigationItem gamepadNavigationItem;

	private bool isHovered;

	private bool isInteractable;

	private bool isSelected;

	private CancellationTokenSource selectionBlinkCancellationTokenSource;

	public LazyButton LazyButton => lazyButton;

	public Image StarIcon => starIcon;

	public Image StatusIcon => statusIcon;

	public int Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public bool NoSelectionFrames
	{
		get
		{
			return noSelectionFrames;
		}
		set
		{
			noSelectionFrames = value;
		}
	}

	public bool ShowMouseSelectionFrame
	{
		get
		{
			return showMouseSelectionFrame;
		}
		set
		{
			showMouseSelectionFrame = value;
		}
	}

	public TooltipPlacementPriority TooltipPlacementPriority
	{
		get
		{
			return tooltipPlacementPriority;
		}
		set
		{
			tooltipPlacementPriority = value;
		}
	}

	public int HasItemCount
	{
		get
		{
			return hasItemCount;
		}
		set
		{
			hasItemCount = value;
		}
	}

	public Item DisplayingItem => displayingItem;

	public OutputPreview DisplayingOutputPreview => displayingOutputPreview;

	public GamepadNavigationItem GamepadNavigationItem
	{
		get
		{
			TryInitGamepadNavigationItem();
			return gamepadNavigationItem;
		}
	}

	public bool IsInteractable => isInteractable;

	public Image Background => background;

	public Image Icon => icon;

	private void Awake()
	{
		TryInitGamepadNavigationItem();
		CacheDefaultStatusIconPosition();
	}

	public void SetWidgetState(ItemRelatedWidgetState state)
	{
		widgetState = state;
		isSelected = state == ItemRelatedWidgetState.Selected;
		switch (state)
		{
		case ItemRelatedWidgetState.Default:
		case ItemRelatedWidgetState.Selected:
			background.color = Color.white;
			nonInteractableItemImg.gameObject.SetActive(value: false);
			break;
		case ItemRelatedWidgetState.Disabled:
			background.color = backColorEmpty;
			nonInteractableItemImg.gameObject.SetActive(value: true);
			break;
		case ItemRelatedWidgetState.Inactive:
			background.color = backColorEmpty;
			nonInteractableItemImg.gameObject.SetActive(value: false);
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		case ItemRelatedWidgetState.NotSet:
			break;
		}
		RefreshSelectionFrame();
	}

	private void RefreshSelectionFrame()
	{
		if (isSelected && !noSelectionFrames)
		{
			if (selection != null)
			{
				selection.gameObject.SetActive(value: true);
			}
			if (selectionNonInteractable != null)
			{
				selectionNonInteractable.gameObject.SetActive(value: false);
			}
		}
		else if (!isHovered)
		{
			HideSelectionFrame();
		}
	}

	private void HideSelectionFrame()
	{
		if (selection != null)
		{
			selection.gameObject.SetActive(value: false);
		}
		if (selectionNonInteractable != null)
		{
			selectionNonInteractable.gameObject.SetActive(value: false);
		}
	}

	public void Draw(Item item, bool isNeedItem = false, int hasItemCount = -1, bool isCraftResult = false, int multiplier = 1, bool drawAsNonInteractable = false, int price = 0, bool drawCounter = true, bool forceNonEmpty = false, bool forceDrawCounter = false, ItemRelatedWidgetState customState = ItemRelatedWidgetState.NotSet, bool noSelectionFrames = false)
	{
		if (customState == ItemRelatedWidgetState.NotSet)
		{
			SetWidgetState(drawAsNonInteractable ? ItemRelatedWidgetState.Disabled : ItemRelatedWidgetState.Default);
		}
		else
		{
			SetWidgetState(drawAsNonInteractable ? ItemRelatedWidgetState.Disabled : customState);
		}
		if (item == null || (item.IsEmpty && !forceNonEmpty))
		{
			DrawEmpty(drawAsNonInteractable, resetWidgetState: false, noSelectionFrames);
			return;
		}
		displayingItem = item;
		this.noSelectionFrames = noSelectionFrames;
		showMouseSelectionFrame = true;
		tooltipPlacementPriority = TooltipPlacementPriority.TopRight;
		isInteractable = !drawAsNonInteractable;
		string id = item.id;
		value = item.Count;
		this.hasItemCount = hasItemCount;
		this.isNeedItem = isNeedItem;
		this.drawCounter = drawCounter;
		Sprite sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(item.Definition.iconId);
		if (sprite == null)
		{
			DrawPlaceHolder(id, isCraftResult, noSelectionFrames);
		}
		else
		{
			icon.gameObject.SetActive(value: true);
			iconLabelPlaceholder.gameObject.SetActive(value: false);
			icon.BlueColorReplace(colors.NormalColor);
			icon.sprite = sprite;
		}
		UpdateCountLabel(multiplier, forceDrawCounter);
		if (item.Definition.qualityType == ItemDef.QualityType.Star)
		{
			starIcon.gameObject.SetActive(value: true);
			Sprite sprite2 = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("item_star_" + item.Definition.quality);
			starIcon.sprite = sprite2;
		}
		else
		{
			starIcon.gameObject.SetActive(value: false);
		}
		if (price != 0)
		{
			UpdatePriceLabel(price);
		}
		else
		{
			ClearPriceLabel();
		}
		UpdateStatusIcon();
		CheckPreviousTooltip();
	}

	public void Draw(ItemCount itemCount, bool isNeedItem = false, int hasItemCount = -1, bool isCraftResult = false, int multiplier = 1, bool drawAsNonInteractable = false, int price = 0, bool drawCounter = true, ItemRelatedWidgetState customState = ItemRelatedWidgetState.NotSet, bool noSelectionFrames = false)
	{
		if (customState == ItemRelatedWidgetState.NotSet)
		{
			SetWidgetState(drawAsNonInteractable ? ItemRelatedWidgetState.Disabled : ItemRelatedWidgetState.Default);
		}
		else
		{
			SetWidgetState(drawAsNonInteractable ? ItemRelatedWidgetState.Disabled : customState);
		}
		if (itemCount == null || itemCount.IsEmpty)
		{
			DrawEmpty(drawAsNonInteractable, resetWidgetState: false, noSelectionFrames);
			return;
		}
		this.noSelectionFrames = noSelectionFrames;
		showMouseSelectionFrame = true;
		tooltipPlacementPriority = TooltipPlacementPriority.TopRight;
		displayingItemCount = itemCount;
		isInteractable = !drawAsNonInteractable;
		string itemId = itemCount.itemId;
		value = itemCount.count;
		this.hasItemCount = hasItemCount;
		this.isNeedItem = isNeedItem;
		this.drawCounter = drawCounter;
		Sprite sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(itemCount.Def.iconId);
		if (sprite == null)
		{
			DrawPlaceHolder(itemId, isCraftResult, noSelectionFrames);
		}
		else
		{
			icon.gameObject.SetActive(value: true);
			iconLabelPlaceholder.gameObject.SetActive(value: false);
			icon.BlueColorReplace(colors.NormalColor);
			icon.sprite = sprite;
		}
		UpdateCountLabel(multiplier);
		if (itemCount.Def.qualityType == ItemDef.QualityType.Star)
		{
			starIcon.gameObject.SetActive(value: true);
			Sprite sprite2 = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("item_star_" + itemCount.Def.quality);
			starIcon.sprite = sprite2;
		}
		else
		{
			starIcon.gameObject.SetActive(value: false);
		}
		if (price != 0)
		{
			UpdatePriceLabel(price);
		}
		else
		{
			ClearPriceLabel();
		}
		UpdateStatusIcon();
		CheckPreviousTooltip();
	}

	public void DrawPlaceHolder(string itemId, bool isCraftResult, bool noSelectionFrames = false)
	{
		iconLabelPlaceholder.gameObject.SetActive(value: true);
		icon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(isCraftResult ? "i_b_blueprint_placeholder" : "i_placeholder");
		icon.gameObject.SetActive(value: true);
		iconLabelPlaceholder.text = itemId;
		iconLabelPlaceholder.gameObject.SetActive(value: true);
		starIcon.gameObject.SetActive(value: false);
		this.noSelectionFrames = noSelectionFrames;
	}

	public void DrawCraftOutput(OutputPreview outputPreview, int customQuality = -1, CraftStatus craftStatus = CraftStatus.OK, ItemType requiredToolType = ItemType.None, int multiplier = 1, ItemRelatedWidgetState customState = ItemRelatedWidgetState.NotSet, bool noSelectionFrames = false)
	{
		if (customState == ItemRelatedWidgetState.NotSet)
		{
			SetWidgetState(ItemRelatedWidgetState.Default);
		}
		else
		{
			SetWidgetState(customState);
		}
		if (outputPreview == null)
		{
			DrawEmpty(drawAsNonInteractable: false, resetWidgetState: false, noSelectionFrames);
			return;
		}
		this.noSelectionFrames = noSelectionFrames;
		showMouseSelectionFrame = true;
		tooltipPlacementPriority = TooltipPlacementPriority.TopRight;
		displayingOutputPreview = outputPreview;
		isInteractable = true;
		string itemId = outputPreview.itemId;
		value = outputPreview.count;
		drawCounter = true;
		Sprite sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(outputPreview.IconId);
		if (sprite == null)
		{
			DrawPlaceHolder(itemId, isCraftResult: true, noSelectionFrames);
		}
		else
		{
			icon.gameObject.SetActive(value: true);
			iconLabelPlaceholder.gameObject.SetActive(value: false);
			icon.BlueColorReplace(colors.NormalColor);
			icon.sprite = sprite;
		}
		UpdateQualityIcon((customQuality > -1) ? customQuality : outputPreview.quality);
		UpdateCountLabel(multiplier);
		ClearPriceLabel();
		UpdateStatusIcon(craftStatus, requiredToolType);
		CheckPreviousTooltip();
	}

	public void DrawCustom(string iconId, int count, bool interactable = false, bool noSelectionFrames = false)
	{
		SetWidgetState(ItemRelatedWidgetState.Default);
		if (string.IsNullOrEmpty(iconId))
		{
			DrawEmpty(drawAsNonInteractable: false, resetWidgetState: false);
			return;
		}
		this.noSelectionFrames = noSelectionFrames;
		showMouseSelectionFrame = true;
		tooltipPlacementPriority = TooltipPlacementPriority.TopRight;
		isInteractable = interactable;
		value = count;
		drawCounter = true;
		Sprite sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(iconId);
		if (sprite == null)
		{
			DrawPlaceHolder(iconId, isCraftResult: true, noSelectionFrames);
		}
		else
		{
			icon.gameObject.SetActive(value: true);
			iconLabelPlaceholder.gameObject.SetActive(value: false);
			icon.BlueColorReplace(colors.NormalColor);
			icon.sprite = sprite;
		}
		UpdateCountLabel();
		starIcon.gameObject.SetActive(value: false);
		statusIcon.gameObject.SetActive(value: false);
		ClearPriceLabel();
		CheckPreviousTooltip();
	}

	public void DrawEmptyInteractable(bool drawAsNonInteractable = false, bool noSelectionFrames = false)
	{
		SetWidgetState(ItemRelatedWidgetState.Default);
		DrawEmpty(drawAsNonInteractable: false, resetWidgetState: false, noSelectionFrames);
	}

	public void DrawEmptyWithState(ItemRelatedWidgetState widgetState, bool drawAsNonInteractable = false, bool noSelectionFrames = false)
	{
		SetWidgetState(widgetState);
		DrawEmpty(drawAsNonInteractable, resetWidgetState: false, noSelectionFrames);
	}

	public void DrawEmpty(bool drawAsNonInteractable = false, bool resetWidgetState = true, bool noSelectionFrames = false)
	{
		Flush(resetWidgetState);
		this.noSelectionFrames = noSelectionFrames;
		isInteractable = !drawAsNonInteractable;
		icon.gameObject.SetActive(value: false);
		iconLabelPlaceholder.gameObject.SetActive(value: false);
		icon.BlueColorReplace(colors.NormalColor);
		icon.sprite = null;
		countLabel.text = string.Empty;
		starIcon.gameObject.SetActive(value: false);
		statusIcon.gameObject.SetActive(value: false);
		ClearPriceLabel();
		CheckPreviousTooltip();
	}

	public void Flush(bool resetWidgetState = true)
	{
		isSelected = false;
		StopSelectionBlinking();
		if (resetWidgetState)
		{
			SetWidgetState(ItemRelatedWidgetState.NotSet);
		}
		noSelectionFrames = false;
		showMouseSelectionFrame = true;
		tooltipPlacementPriority = TooltipPlacementPriority.TopRight;
		ClearPriceLabel();
		displayingItem = null;
		displayingItemCount = null;
		displayingOutputPreview = null;
		ExtraRedTooltipLocId = null;
		if (GamepadNavigationItem != null)
		{
			GamepadNavigationItem.group = 0;
		}
		ClearCallbacks();
	}

	public void OnMultiplierChange(int multiplier, bool forceDraw = false)
	{
		UpdateCountLabel(multiplier, forceDraw);
	}

	public void UpdateCountLabel(int multiplier = 1, bool forceDraw = false)
	{
		int actualValue = value * multiplier;
		bool flag = displayingItem != null && displayingItem.Definition.CanItemBeEquipped();
		if (isNeedItem || !flag)
		{
			DrawCount(actualValue, forceDraw);
			return;
		}
		countLabel.text = "item_icon-hand".FontIcon();
		countLabel.gameObject.SetActive(value: true);
	}

	public void UpdateHappinessStatusIcons(Vendor vendor, int extraSoldCount = 0, float extraUsedHappiness = 0f)
	{
		if (vendor.HasHappinessForItem(DisplayingItem?.id, extraSoldCount, extraUsedHappiness))
		{
			statusIcon.gameObject.SetActive(value: true);
			statusIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("widget_item_cell_happiness");
			statusIcon.SetNativeSize();
			SetStatusIconPosition();
		}
		else
		{
			statusIcon.gameObject.SetActive(value: false);
			SetStatusIconPosition();
		}
	}

	public void UpdateCountLabelAsNormal(int leftCount, int rightCount)
	{
		countLabel.text = leftCount + "/" + rightCount;
		countLabel.gameObject.SetActive(value: true);
		countLabelStyle.SetTextStyle(countLabelNormal);
	}

	public void UpdateCountLabelAsRegularItem(int multiplier = 1)
	{
		isNeedItem = false;
		DrawCount(value * multiplier, forceDraw: true);
	}

	public void UpdateStatusIcon(OrderBase orderBase)
	{
		statusIcon.gameObject.SetActive(value: true);
		statusIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(orderBase.GetStatusIcon());
		statusIcon.SetNativeSize();
		SetStatusIconPosition();
	}

	public void UpdateStatusIcon(Sprite sprite, Vector2? anchoredPosition = null)
	{
		statusIcon.gameObject.SetActive(value: true);
		statusIcon.sprite = sprite;
		statusIcon.SetNativeSize();
		SetStatusIconPosition(anchoredPosition);
	}

	public void UpdateStatusIcon(CraftStatus craftStatus = CraftStatus.OK, ItemType requiredItemType = ItemType.None)
	{
		if (craftStatus == CraftStatus.OK || craftStatus == CraftStatus.NotEnoughMastery || craftStatus == CraftStatus.NotEnoughEnergy || craftStatus == CraftStatus.NotEnoughInsanity)
		{
			statusIcon.gameObject.SetActive(value: false);
			SetStatusIconPosition();
			return;
		}
		statusIcon.gameObject.SetActive(value: true);
		if (craftStatus == CraftStatus.DoesntHaveRequiredTool)
		{
			statusIcon.sprite = CraftStatusIconHelper.GetCraftStatusIcon(craftStatus, requiredItemType);
			statusIcon.SetNativeSize();
		}
		else
		{
			statusIcon.sprite = CraftStatusIconHelper.GetCraftStatusIcon(craftStatus);
			statusIcon.SetNativeSize();
		}
		SetStatusIconPosition();
	}

	private void CacheDefaultStatusIconPosition()
	{
		if (!hasCachedStatusIconPosition && !(statusIcon == null))
		{
			defaultStatusIconAnchoredPosition = statusIcon.rectTransform.anchoredPosition;
			hasCachedStatusIconPosition = true;
		}
	}

	private void SetStatusIconPosition(Vector2? anchoredPosition = null)
	{
		if (!(statusIcon == null))
		{
			CacheDefaultStatusIconPosition();
			statusIcon.rectTransform.anchoredPosition = anchoredPosition ?? defaultStatusIconAnchoredPosition;
		}
	}

	public void SetNativeSizeForIcon()
	{
		icon.SetNativeSize();
	}

	public void UpdateQualityIcon(int quality)
	{
		if (quality >= 0)
		{
			starIcon.gameObject.SetActive(value: true);
			Sprite sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("item_star_" + quality);
			starIcon.sprite = sprite;
		}
		else
		{
			starIcon.gameObject.SetActive(value: false);
		}
	}

	public void ClearPriceLabel()
	{
		if (priceLabel == null)
		{
			Debug.LogError("Item has no price field", this);
		}
		else
		{
			priceLabel.text = "";
		}
	}

	public void UpdatePriceLabel(int price)
	{
		if (priceLabel == null)
		{
			Debug.LogError("Item has no price field", this);
		}
		else
		{
			priceLabel.text = Trading.FormatMoney(price, printZero: false, "\n");
		}
	}

	private void TryInitGamepadNavigationItem()
	{
		if (gamepadNavigationItem == null)
		{
			gamepadNavigationItem = GetComponent<GamepadNavigationItem>();
			if (gamepadNavigationItem != null)
			{
				gamepadNavigationItem.SetCallbacks(OnGamepadOver, OnGamepadOut, OnGamepadPress);
			}
		}
	}

	private void CheckPreviousTooltip()
	{
		if (isHovered && (displayingItem == null || displayingItem.IsEmpty))
		{
			if (UITooltip.IsTooltipShowingAtTarget(base.transform as RectTransform))
			{
				HideUITooltip();
			}
		}
		else if (isHovered)
		{
			ShowUITooltip();
		}
	}

	private void OnDisable()
	{
		StopSelectionBlinking();
		if (isHovered)
		{
			if (UITooltip.IsTooltipShowingAtTarget(base.transform as RectTransform))
			{
				HideUITooltip(immediately: true);
			}
			isHovered = false;
		}
		icon.BlueColorReplace(colors.NormalColor);
		isHovered = false;
		HideSelectionFrame();
	}

	private void OnDestroy()
	{
		StopSelectionBlinking();
	}

	public void OnOver()
	{
		StopSelectionBlinking();
		if (!noSelectionFrames)
		{
			if (LazyInput.IsGamepadActive || showMouseSelectionFrame)
			{
				if (nonInteractableItemImg != null && nonInteractableItemImg.gameObject.activeSelf && selectionNonInteractable != null)
				{
					selectionNonInteractable.gameObject.SetActive(value: true);
				}
				else
				{
					selection.gameObject.SetActive(value: true);
				}
			}
			icon.BlueColorReplace(colors.HighlightedColorMouse);
		}
		ShowUITooltip();
		OnItemCellOver?.Invoke(this);
	}

	public void OnOut()
	{
		icon.BlueColorReplace(colors.NormalColor);
		HideUITooltip();
		RefreshSelectionFrame();
		OnItemCellOut?.Invoke(this);
	}

	public void OnPress()
	{
		if (widgetState != ItemRelatedWidgetState.Disabled)
		{
			OnWidgetPress?.Invoke();
			if (isInteractable)
			{
				OnItemCellPress?.Invoke(this);
			}
		}
	}

	public void OnPress2()
	{
		if (widgetState != ItemRelatedWidgetState.Disabled)
		{
			OnWidgetPress?.Invoke();
			if (isInteractable && (LazyInput.IsGamepadActive || Input.GetMouseButtonDown(1)))
			{
				OnItemCellPress2?.Invoke(this);
			}
		}
	}

	public void OnDown()
	{
		if (isInteractable && widgetState != ItemRelatedWidgetState.Disabled && (LazyInput.IsGamepadActive || Input.GetMouseButtonDown(0)))
		{
			OnItemCellDown?.Invoke(this);
		}
	}

	public void OnGamepadOver()
	{
		if (widgetState != ItemRelatedWidgetState.Disabled)
		{
			OnWidgetPress?.Invoke();
		}
		OnOver();
	}

	public void OnGamepadOut()
	{
		OnOut();
	}

	public void OnGamepadPress()
	{
		OnPress();
	}

	public void OnGamepadPress2()
	{
		OnPress2();
	}

	public void ShowUITooltip()
	{
		isHovered = true;
		UITooltip.ShowItemCell(this);
	}

	public void HideUITooltip(bool immediately = false)
	{
		isHovered = false;
		if (immediately)
		{
			UITooltip.HideImmediately();
		}
		else
		{
			UITooltip.Hide();
		}
	}

	public void ClearCallbacks()
	{
		OnItemCellOver = null;
		OnItemCellOut = null;
		OnItemCellPress = null;
		OnItemCellPress2 = null;
		OnItemCellDown = null;
		OnWidgetPress = null;
		CustomTooltipShowAction = null;
	}

	public void StartSelectionBlinking()
	{
		StopSelectionBlinking();
		if (!noSelectionFrames)
		{
			selectionBlinkCancellationTokenSource = new CancellationTokenSource();
			SelectionBlinking(selectionBlinkCancellationTokenSource).Forget();
		}
	}

	private void StopSelectionBlinking()
	{
		if (selectionBlinkCancellationTokenSource != null)
		{
			CancellationTokenSource cancellationTokenSource = selectionBlinkCancellationTokenSource;
			selectionBlinkCancellationTokenSource = null;
			cancellationTokenSource.Cancel();
		}
		if (isSelected)
		{
			RefreshSelectionFrame();
		}
		else if (selection != null)
		{
			selection.gameObject.SetActive(value: false);
		}
	}

	private async UniTaskVoid SelectionBlinking(CancellationTokenSource cancellationTokenSource)
	{
		CancellationToken cancellationToken = cancellationTokenSource.Token;
		try
		{
			for (int i = 0; i < 3; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (selection == null)
				{
					break;
				}
				selection.gameObject.SetActive(value: true);
				await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				if (selection == null)
				{
					break;
				}
				selection.gameObject.SetActive(value: false);
				if (i < 2)
				{
					await UniTask.Delay(TimeSpan.FromSeconds(0.5), ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken);
				}
			}
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
		}
		finally
		{
			bool num = selectionBlinkCancellationTokenSource == cancellationTokenSource;
			if (num)
			{
				selectionBlinkCancellationTokenSource = null;
			}
			cancellationTokenSource.Dispose();
			if (num)
			{
				RefreshSelectionFrame();
			}
		}
	}

	private void DrawCount(int actualValue, bool forceDraw)
	{
		countLabel.text = (isNeedItem ? (hasItemCount + "/" + actualValue) : actualValue.ToString());
		countLabel.gameObject.SetActive((drawCounter && (isNeedItem || actualValue > 1)) || forceDraw);
		countLabelStyle.SetTextStyle((isNeedItem && hasItemCount < actualValue) ? countLabelRed : countLabelNormal);
	}
}
