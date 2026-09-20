using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITooltip : UIBasicBubble, ILazyGUIElement
{
	private static UITooltip instance;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private LazyWidgetContainer widgetContainer;

	[SerializeField]
	private TextStyle headerTextStyle;

	[SerializeField]
	private TextStyle headerBoldTextStyle;

	[SerializeField]
	private TextStyle descriptionTextStyle;

	[SerializeField]
	private TextStyle prayDescriptionSelectedTextStyle;

	[SerializeField]
	private TextStyle smallDescriptionTextStyle;

	[SerializeField]
	private TextStyle selectedSmallDescriptionTextStyle;

	[SerializeField]
	private TextStyle perkDescriptionTextStyle;

	[SerializeField]
	private TextStyle greenEffectTextStyle;

	[SerializeField]
	private TextStyle redEffectTextStyle;

	[SerializeField]
	private TextStyle blackEffectTextStyle;

	[SerializeField]
	private TextStyle headerBoldTextStyleNotEnough;

	[SerializeField]
	private TextStyle headerBoldTextStyleGold;

	[SerializeField]
	private Vector2 offsetAdd;

	[SerializeField]
	private Vector2 alternativeDownOffsetAdd;

	private Sequence activeTweenSequence;

	private RectTransform target;

	private Vector2 offset;

	private Vector2 alternativeDownOffset;

	private Vector2 appearOffset;

	private TooltipPlacementPriority placementPriority;

	private readonly Vector3[] targetWorldCorners = new Vector3[4];

	private readonly Vector3[] bubbleWorldCorners = new Vector3[4];

	public void Init()
	{
		instance = this;
		LazyWindowsStackController.OnWindowClosed += delegate
		{
			Hide();
		};
		LazyWindowsStackController.OnWindowOpened += delegate
		{
			Hide();
		};
		canvas.overrideSorting = true;
		canvas.sortingOrder = 700;
		base.gameObject.SetActive(value: false);
	}

	public static void Show(List<LazyWidgetDataBase> dataList, RectTransform target, ForceCornerPosition forceCornerPosition = ForceCornerPosition.Auto, bool follow = true, TooltipPlacementPriority placementPriority = TooltipPlacementPriority.TopRight)
	{
		instance.ShowAtTargetAndCalculateOffsets(dataList, target, forceCornerPosition, follow, default(Vector2), placementPriority);
	}

	public static void Hide()
	{
		instance.DoHideAnimation();
	}

	public static void HideImmediately()
	{
		instance.HideTooltip();
	}

	public static bool IsTooltipShowingAtTarget(RectTransform target)
	{
		if (instance != null && instance.gameObject.activeSelf)
		{
			return instance.target == target;
		}
		return false;
	}

	private void ShowAtTargetAndCalculateOffsets(List<LazyWidgetDataBase> dataList, RectTransform target, ForceCornerPosition forceCornerPosition = ForceCornerPosition.Auto, bool follow = true, Vector2 appearOffset = default(Vector2), TooltipPlacementPriority placementPriority = TooltipPlacementPriority.TopRight)
	{
		this.appearOffset = appearOffset;
		this.placementPriority = placementPriority;
		CalculateTargetOffsets(target, out var targetOffset, out var targetAlternativeDownOffset);
		ShowAtTarget(dataList, target, targetOffset, targetAlternativeDownOffset, forceCornerPosition, follow);
	}

	private void CalculateTargetOffsets(RectTransform target, out Vector2 targetOffset, out Vector2 targetAlternativeDownOffset)
	{
		target.GetWorldCorners(targetWorldCorners);
		float num = Mathf.Min(targetWorldCorners[0].x, targetWorldCorners[1].x, targetWorldCorners[2].x, targetWorldCorners[3].x);
		float num2 = Mathf.Max(targetWorldCorners[0].x, targetWorldCorners[1].x, targetWorldCorners[2].x, targetWorldCorners[3].x);
		float y = Mathf.Min(targetWorldCorners[0].y, targetWorldCorners[1].y, targetWorldCorners[2].y, targetWorldCorners[3].y);
		float y2 = Mathf.Max(targetWorldCorners[0].y, targetWorldCorners[1].y, targetWorldCorners[2].y, targetWorldCorners[3].y);
		Vector2 vector = new Vector2((num + num2) * 0.5f, y2);
		Vector2 vector2 = new Vector2((num + num2) * 0.5f, y);
		targetOffset = vector - (Vector2)target.position + offsetAdd * LazyUI.ScaleFactor;
		if (appearOffset != Vector2.zero)
		{
			targetOffset += appearOffset * LazyUI.ScaleFactor;
		}
		targetAlternativeDownOffset = vector2 - vector + alternativeDownOffsetAdd * LazyUI.ScaleFactor;
	}

	private void ShowAtTarget(List<LazyWidgetDataBase> dataList, RectTransform target, Vector2 offset, Vector2 alternativeDownOffset = default(Vector2), ForceCornerPosition forceCornerPosition = ForceCornerPosition.Auto, bool follow = true)
	{
		if (target == null)
		{
			Debug.LogError("Null target for tooltip");
			return;
		}
		bool num = !base.gameObject.activeSelf || this.target != target;
		ShowAtPosition(dataList, target.position + (Vector3)offset, alternativeDownOffset, forceCornerPosition);
		if (follow)
		{
			this.target = target;
			this.offset = offset;
			this.alternativeDownOffset = alternativeDownOffset;
		}
		else
		{
			this.target = null;
		}
		if (num && base.gameObject.activeSelf)
		{
			LazyAudio.PlayAndForget("gui_hover_light");
		}
	}

	private void ShowAtPosition(List<LazyWidgetDataBase> dataList, Vector2 screenPosition, Vector3 alternativeDownPosition = default(Vector3), ForceCornerPosition forceCornerPosition = ForceCornerPosition.Auto)
	{
		if (dataList.Count == 0)
		{
			return;
		}
		while (dataList.Count > 0)
		{
			if (!(dataList[dataList.Count - 1] is UITooltipSeparatorWidgetData))
			{
				break;
			}
			dataList.RemoveAt(dataList.Count - 1);
		}
		target = null;
		widgetContainer.ConstructWidgets(dataList);
		base.gameObject.SetActive(value: true);
		base.transform.localScale = Vector3.one;
		Canvas.ForceUpdateCanvases();
		((RectTransform)base.transform).RefreshContentFitter();
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.RootTransform);
		if (alternativeDownPosition != default(Vector3))
		{
			if ((uint)(forceCornerPosition - 6) <= 2u)
			{
				UpdatePositionAndCorner(screenPosition + (Vector2)alternativeDownPosition, forceCornerPosition);
			}
			else
			{
				UpdatePositionAndCorner(screenPosition, alternativeDownPosition);
			}
		}
		else
		{
			UpdatePositionAndCorner(screenPosition, forceCornerPosition);
		}
		DoAppearAnimation();
	}

	private void Update()
	{
		if (target != null)
		{
			CalculateTargetOffsets(target, out offset, out alternativeDownOffset);
			UpdatePositionAndCorner(target.position + (Vector3)offset, alternativeDownOffset);
		}
		widgetContainer.CustomUpdate();
	}

	protected override void UpdatePositionAndCorner(Vector3 screenPos, Vector3 offsetForDownPosition)
	{
		Bounds currentScreenBounds = GetCurrentScreenBounds();
		Vector3 vector = screenPos + offsetForDownPosition;
		bool num = placementPriority == TooltipPlacementPriority.BottomRight || placementPriority == TooltipPlacementPriority.BottomLeft;
		int num2;
		int num3;
		if (placementPriority != TooltipPlacementPriority.TopLeft)
		{
			num2 = ((placementPriority == TooltipPlacementPriority.BottomLeft) ? 1 : 0);
			if (num2 == 0)
			{
				num3 = 1;
				goto IL_0040;
			}
		}
		else
		{
			num2 = 1;
		}
		num3 = 3;
		goto IL_0040;
		IL_0040:
		BubbleCornerDirection preferredCorner = (BubbleCornerDirection)num3;
		BubbleCornerDirection otherCorner = ((num2 != 0) ? BubbleCornerDirection.LeftDown : BubbleCornerDirection.RightDown);
		BubbleCornerDirection preferredCorner2 = ((num2 != 0) ? BubbleCornerDirection.RightUp : BubbleCornerDirection.LeftUp);
		BubbleCornerDirection otherCorner2 = ((num2 == 0) ? BubbleCornerDirection.RightUp : BubbleCornerDirection.LeftUp);
		BubbleCornerDirection bubbleCornerDirection = PickBestHorizontalCorner(screenPos, preferredCorner, otherCorner, currentScreenBounds);
		BubbleCornerDirection bubbleCornerDirection2 = PickBestHorizontalCorner(vector, preferredCorner2, otherCorner2, currentScreenBounds);
		Vector3 vector2 = (num ? vector : screenPos);
		BubbleCornerDirection bubbleCornerDirection3 = (num ? bubbleCornerDirection2 : bubbleCornerDirection);
		Vector3 vector3 = (num ? screenPos : vector);
		BubbleCornerDirection bubbleCornerDirection4 = (num ? bubbleCornerDirection : bubbleCornerDirection2);
		if (IsCandidateInsideScreen(vector2, bubbleCornerDirection3, currentScreenBounds))
		{
			ApplyCornerIndex(vector2, Vector3.zero, bubbleCornerDirection3);
			return;
		}
		if (IsCandidateInsideScreen(vector3, bubbleCornerDirection4, currentScreenBounds))
		{
			ApplyCornerIndex(vector3, Vector3.zero, bubbleCornerDirection4);
			return;
		}
		float candidateVisibleArea = GetCandidateVisibleArea(vector2, bubbleCornerDirection3, currentScreenBounds);
		float candidateVisibleArea2 = GetCandidateVisibleArea(vector3, bubbleCornerDirection4, currentScreenBounds);
		ApplyCornerIndex((candidateVisibleArea >= candidateVisibleArea2) ? vector2 : vector3, Vector3.zero, (candidateVisibleArea >= candidateVisibleArea2) ? bubbleCornerDirection3 : bubbleCornerDirection4);
	}

	private BubbleCornerDirection PickBestHorizontalCorner(Vector3 screenPos, BubbleCornerDirection preferredCorner, BubbleCornerDirection otherCorner, Bounds screenBounds)
	{
		if (IsCandidateInsideScreen(screenPos, preferredCorner, screenBounds))
		{
			return preferredCorner;
		}
		if (IsCandidateInsideScreen(screenPos, otherCorner, screenBounds))
		{
			return otherCorner;
		}
		float candidateVisibleArea = GetCandidateVisibleArea(screenPos, preferredCorner, screenBounds);
		float candidateVisibleArea2 = GetCandidateVisibleArea(screenPos, otherCorner, screenBounds);
		if (!(candidateVisibleArea >= candidateVisibleArea2))
		{
			return otherCorner;
		}
		return preferredCorner;
	}

	private bool IsCandidateInsideScreen(Vector3 screenPos, BubbleCornerDirection corner, Bounds screenBounds)
	{
		Rect candidateRect = GetCandidateRect(screenPos, corner);
		if (candidateRect.xMin >= screenBounds.min.x && candidateRect.xMax <= screenBounds.max.x && candidateRect.yMin >= screenBounds.min.y)
		{
			return candidateRect.yMax <= screenBounds.max.y;
		}
		return false;
	}

	private float GetCandidateVisibleArea(Vector3 screenPos, BubbleCornerDirection corner, Bounds screenBounds)
	{
		Rect candidateRect = GetCandidateRect(screenPos, corner);
		float num = Mathf.Max(0f, Mathf.Min(candidateRect.xMax, screenBounds.max.x) - Mathf.Max(candidateRect.xMin, screenBounds.min.x));
		float num2 = Mathf.Max(0f, Mathf.Min(candidateRect.yMax, screenBounds.max.y) - Mathf.Max(candidateRect.yMin, screenBounds.min.y));
		return num * num2;
	}

	private Rect GetCandidateRect(Vector3 screenPos, BubbleCornerDirection corner)
	{
		base.RootTransform.GetWorldCorners(bubbleWorldCorners);
		Rect result = new Rect((Vector2)bubbleWorldCorners[0], (Vector2)(bubbleWorldCorners[2] - bubbleWorldCorners[0]));
		Vector2 vector = (Vector2)screenPos - (Vector2)(corners[(int)corner].rectTransform.position - base.RootTransform.position);
		result.position += vector - (Vector2)base.RootTransform.position;
		return result;
	}

	private Bounds GetCurrentScreenBounds()
	{
		Rect safeArea = Screen.safeArea;
		return new Bounds((Vector3)safeArea.center, (Vector3)safeArea.size);
	}

	private void HideTooltip()
	{
		base.gameObject.SetActive(value: false);
		target = null;
	}

	private void DoAppearAnimation()
	{
		base.gameObject.SetActive(value: true);
		activeTweenSequence?.Kill();
		activeTweenSequence = DOTween.Sequence();
		canvasGroup.alpha = 0f;
		activeTweenSequence.Join(canvasGroup.DOFade(1f, 0f));
	}

	private void DoHideAnimation()
	{
		activeTweenSequence?.Kill();
		activeTweenSequence = DOTween.Sequence();
		activeTweenSequence.Join(canvasGroup.DOFade(0f, 0f));
		activeTweenSequence.AppendCallback(HideTooltip);
	}

	public static void ShowSimpleInfo(Transform transform, string text, Vector2 appearOffset = default(Vector2), string header = null)
	{
		List<LazyWidgetDataBase> list = BuildSimpleTooltipWidgets(text, header);
		if (list.Count != 0)
		{
			instance.ShowAtTargetAndCalculateOffsets(list, transform as RectTransform, ForceCornerPosition.Auto, follow: true, appearOffset);
		}
	}

	public static void ShowPerk(PerkDef perkDef, RectTransform target, Vector2 appearOffset = default(Vector2))
	{
		if (perkDef != null && !(target == null))
		{
			List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
			AddPerkWidgets(list, perkDef);
			instance.ShowAtTargetAndCalculateOffsets(list, target, ForceCornerPosition.Auto, follow: true, appearOffset);
		}
	}

	private static List<LazyWidgetDataBase> BuildSimpleTooltipWidgets(string text, string header)
	{
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		string body = text ?? string.Empty;
		if (string.IsNullOrEmpty(header))
		{
			TrySplitHeaderAndBody(body, out header, out body);
		}
		if (!string.IsNullOrEmpty(header))
		{
			list.Add(new UITooltipTextWidgetData(ApplyBracketBold(header), TextAlignmentOptions.Center, instance.headerTextStyle));
			if (!string.IsNullOrEmpty(body))
			{
				list.Add(new UITooltipSeparatorWidgetData());
			}
		}
		if (!string.IsNullOrEmpty(body))
		{
			TextAlignmentOptions textAlignmentOptions = (string.IsNullOrEmpty(header) ? TextAlignmentOptions.TopJustified : TextAlignmentOptions.Center);
			list.Add(new UITooltipTextWidgetData(ApplyBracketBold(body), textAlignmentOptions, instance.descriptionTextStyle));
		}
		return list;
	}

	private static bool TrySplitHeaderAndBody(string text, out string header, out string body)
	{
		header = null;
		body = text;
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		int num = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
		int num2 = 4;
		int num3 = text.IndexOf("\n\n", StringComparison.Ordinal);
		if (num3 >= 0 && (num < 0 || num3 < num))
		{
			num = num3;
			num2 = 2;
		}
		if (num <= 0)
		{
			return false;
		}
		header = text.Substring(0, num).Trim();
		body = text.Substring(num + num2).Trim();
		if (string.IsNullOrEmpty(header))
		{
			header = null;
			body = text;
			return false;
		}
		return true;
	}

	private static string ApplyBracketBold(string text)
	{
		if (string.IsNullOrEmpty(text) || instance.headerBoldTextStyle == null)
		{
			return text;
		}
		return instance.headerBoldTextStyle.ColorizeTags(text);
	}

	public static void ShowExtensionInfo(UICraftPreviewItemCell previewItemCell, string extensionId)
	{
		Transform obj = previewItemCell.transform;
		string text = LLBase.L("tt_extension");
		string header = LLBase.L(extensionId);
		ShowSimpleInfo(obj, text, default(Vector2), header);
	}

	public static void ShowMultiAnswerIcon(UIMultiAnswerIcon multiAnswerIcon)
	{
		if (!(multiAnswerIcon == null))
		{
			if (multiAnswerIcon.VendorOrderDef != null)
			{
				List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
				ItemDef data = GameBalance.Me.GetData<ItemDef>(multiAnswerIcon.VendorOrderDef.itemId);
				string text = TryConstructHeaderWithPrefix($"[{data.GetHeader()}]x{multiAnswerIcon.VendorOrderDef.count}", LLBase.L("ui_order_header_tooltip"));
				list.Add(new UITooltipTextWidgetData(text, TextAlignmentOptions.Center, instance.headerTextStyle));
				AddItemWidgets(list, data, addHeader: false, "", notShowStudyWidget: true);
				instance.ShowAtTargetAndCalculateOffsets(list, multiAnswerIcon.transform as RectTransform);
			}
			else if (multiAnswerIcon.ItemCount != null && multiAnswerIcon.ItemCount.Def != null)
			{
				List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
				AddItemWidgets(list, multiAnswerIcon.ItemCount.Def);
				instance.ShowAtTargetAndCalculateOffsets(list, multiAnswerIcon.transform as RectTransform);
			}
		}
	}

	public static void ShowOrderWidget(UIVendorOrderWidget orderWidget)
	{
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		AddItemWidgets(list, GameBalance.Me.GetData<ItemDef>(orderWidget.Data.VendorOrderData.Definition.itemId));
		UITooltipTextWidgetData item = new UITooltipTextWidgetData(string.Format("{0}: {1}+{2}", LLBase.L("ui_vendor_order_reward_tooltip"), "happiness".FontIcon(), orderWidget.Data.VendorOrderData.Definition.happinessReward.EvaluateInt()), TextAlignmentOptions.Center, instance.descriptionTextStyle);
		list.Add(item);
		if (orderWidget.Data.VendorOrderData.Definition.isUrgent)
		{
			UITooltipTextWidgetData item2 = new UITooltipTextWidgetData(LLBase.L("ui_vendor_order_urgent_tooltip") + ": " + "day_pride".FontIcon(), TextAlignmentOptions.Center, instance.descriptionTextStyle);
			list.Add(item2);
		}
		instance.ShowAtTargetAndCalculateOffsets(list, orderWidget.transform as RectTransform);
	}

	public static void ShowItemCell(UIItemCell uiItemCell)
	{
		if (uiItemCell == null)
		{
			return;
		}
		if (uiItemCell.CustomTooltipShowAction != null)
		{
			uiItemCell.CustomTooltipShowAction?.Invoke(uiItemCell);
		}
		else if (uiItemCell.DisplayingOutputPreview != null)
		{
			ShowCraftOutputCell(uiItemCell);
		}
		else
		{
			if (uiItemCell.DisplayingItem == null || string.IsNullOrEmpty(uiItemCell.DisplayingItem.id) || uiItemCell.DisplayingItem.id == "empty")
			{
				return;
			}
			List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
			AddItemWidgets(list, uiItemCell.DisplayingItem.Definition);
			if (!string.IsNullOrEmpty(uiItemCell.ExtraRedTooltipLocId))
			{
				if (list.Count > 0)
				{
					if (!(list[list.Count - 1] is UITooltipSeparatorWidgetData))
					{
						list.Add(new UITooltipSeparatorWidgetData());
					}
				}
				list.Add(new UITooltipTextWidgetData(LLBase.L(uiItemCell.ExtraRedTooltipLocId), TextAlignmentOptions.Center, instance.redEffectTextStyle));
			}
			UITooltip uITooltip = instance;
			RectTransform obj = uiItemCell.transform as RectTransform;
			TooltipPlacementPriority tooltipPlacementPriority = ResolvePlacementPriority(uiItemCell);
			uITooltip.ShowAtTargetAndCalculateOffsets(list, obj, ForceCornerPosition.Auto, follow: true, default(Vector2), tooltipPlacementPriority);
		}
	}

	private static TooltipPlacementPriority ResolvePlacementPriority(UIItemCell uiItemCell)
	{
		if (uiItemCell == null || !LazyInput.IsGamepadActive)
		{
			return TooltipPlacementPriority.TopRight;
		}
		return uiItemCell.TooltipPlacementPriority;
	}

	public static void ShowCraftOutputCell(UIItemCell uiItemCell)
	{
		if (uiItemCell == null || uiItemCell.DisplayingOutputPreview == null || AlchemyMixDef.IsUnknownMixResult(uiItemCell.DisplayingOutputPreview.craftId))
		{
			return;
		}
		ItemDef itemDef = null;
		CraftDefBase craftDefBase = GameBalance.GetCraftDef(uiItemCell.DisplayingOutputPreview.craftId);
		if (craftDefBase == null)
		{
			craftDefBase = GameBalance.GetAlchemyMixDef(uiItemCell.DisplayingOutputPreview.craftId);
		}
		using (List<ChanceOutputItem>.Enumerator enumerator = craftDefBase.outputItems.chanceOutputItems.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				ChanceOutputItem current = enumerator.Current;
				itemDef = ((!current.isStarGroup) ? GameBalance.Me.GetData<ItemDef>(current.id) : GameBalance.Me.starGroupItemsCache[current.id][0]);
			}
		}
		if (itemDef == null)
		{
			foreach (GroupChanceOutputItem groupChanceOutputItem in GameBalance.GetCraftDef(uiItemCell.DisplayingOutputPreview.craftId).outputItems.groupChanceOutputItems)
			{
				using List<ChanceOutputItem>.Enumerator enumerator = groupChanceOutputItem.chanceItems.GetEnumerator();
				if (enumerator.MoveNext())
				{
					ChanceOutputItem current2 = enumerator.Current;
					itemDef = ((!current2.isStarGroup) ? GameBalance.Me.GetData<ItemDef>(current2.id) : GameBalance.Me.starGroupItemsCache[current2.id][0]);
				}
			}
		}
		if (itemDef != null)
		{
			List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
			AddItemWidgets(list, itemDef, addHeader: true, "", notShowStudyWidget: false, nowShowCraftedAt: false, (craftDefBase as CraftDef)?.GetPossibleResultingItemDefs());
			UITooltip uITooltip = instance;
			RectTransform obj = uiItemCell.transform as RectTransform;
			TooltipPlacementPriority tooltipPlacementPriority = ResolvePlacementPriority(uiItemCell);
			uITooltip.ShowAtTargetAndCalculateOffsets(list, obj, ForceCornerPosition.Auto, follow: true, default(Vector2), tooltipPlacementPriority);
		}
	}

	public static void ShowTalentLevelUpWidget(TalentLevelUpWidget widget)
	{
		if (!(widget == null) && widget.Data != null)
		{
			List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
			AddTalentLevelUpWidgets(list, widget.Data.Def);
			instance.ShowAtTargetAndCalculateOffsets(list, widget.transform as RectTransform);
		}
	}

	public static void ShowGardenFertilizerSlot(UIGardenBedSlot fertilizerSlot)
	{
		if (!(fertilizerSlot == null) && fertilizerSlot.PerkData != null)
		{
			List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
			AddPerkWidgets(list, fertilizerSlot.PerkData.Definition);
			instance.ShowAtTargetAndCalculateOffsets(list, fertilizerSlot.transform as RectTransform);
		}
	}

	public static void ShowPerkWidget(PerkWidget perkWidget)
	{
		if (!(perkWidget == null) && perkWidget.PerkData != null)
		{
			List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
			AddPerkWidgets(list, perkWidget.PerkData.Definition);
			instance.ShowAtTargetAndCalculateOffsets(list, perkWidget.transform as RectTransform);
		}
	}

	public static void ShowLinkedEntity(LinkedEntityWidget entityWidget)
	{
		if (entityWidget == null || entityWidget.WidgetData == null)
		{
			return;
		}
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		string text = TryConstructHeaderWithPrefix(entityWidget.WidgetData.GetLinkedEntityHeader(), entityWidget.WidgetData.GetLinkedEntityPrefix());
		list.Add(new UITooltipTextWidgetData(text, TextAlignmentOptions.Center, instance.headerTextStyle));
		switch (entityWidget.WidgetData.LinkedEntityType)
		{
		case LinkedEntityType.CraftDef:
		{
			string str2 = LLBase.L("ui_crafted_at") + ": ";
			string text3 = instance.selectedSmallDescriptionTextStyle.ApplyStyleToString(str2) ?? "";
			for (int j = 0; j < entityWidget.WidgetData.CraftDef.craftsIn.Count; j++)
			{
				if (j > 0)
				{
					text3 += ", ";
				}
				text3 += LLBase.L(entityWidget.WidgetData.CraftDef.craftsIn[j]);
			}
			bool flag = false;
			if (entityWidget.WidgetData.CraftDef.id.StartsWith("fake_"))
			{
				if (!(list[list.Count - 1] is UITooltipSeparatorWidgetData))
				{
					list.Add(new UITooltipSeparatorWidgetData());
				}
				UITooltipTextWidgetData item2 = new UITooltipTextWidgetData(text3, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
				list.Add(item2);
			}
			else
			{
				ItemDef itemDef = entityWidget.WidgetData.CraftDef.TryGetResultingItemDef();
				if (entityWidget.WidgetData.CraftDef.id.EndsWith("_boost"))
				{
					AddBoostCraftWidgets(list, entityWidget.WidgetData.CraftDef);
				}
				else
				{
					if (itemDef != null)
					{
						AddItemWidgets(list, itemDef, addHeader: false, "", entityWidget.WidgetData.NotShowStudyWidgetInItemTooltips, entityWidget.WidgetData.ShowCraftedAtFromCraftDefInsteadOfItem, entityWidget.WidgetData.CraftDef.GetPossibleResultingItemDefs());
						flag = itemDef.type == ItemType.Preach;
					}
					else
					{
						string text4 = entityWidget.WidgetData.CraftDef.id + "_d";
						string text5 = LLBase.L(text4);
						if (text5 != text4)
						{
							UITooltipTextWidgetData item3 = new UITooltipTextWidgetData(text5, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
							list.Add(item3);
						}
					}
					if (entityWidget.WidgetData.ShowCraftedAtFromCraftDefInsteadOfItem && entityWidget.WidgetData.CraftDef.craftsIn.Count > 0)
					{
						if (!(list[list.Count - 1] is UITooltipSeparatorWidgetData))
						{
							list.Add(new UITooltipSeparatorWidgetData());
						}
						UITooltipTextWidgetData item4 = new UITooltipTextWidgetData(text3, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
						list.Add(item4);
					}
				}
			}
			TryAddRequiredExtensionWidget(list, entityWidget.WidgetData.CraftDef);
			if (!flag)
			{
				TryAddNeedItemsWidget(list, entityWidget.WidgetData.CraftDef.needItems);
			}
			break;
		}
		case LinkedEntityType.AlchemyFormula:
		{
			AddItemWidgets(list, entityWidget.WidgetData.AlchemyFormulaDef.ItemDef, addHeader: false, "", entityWidget.WidgetData.NotShowStudyWidgetInItemTooltips, nowShowCraftedAt: true);
			if (entityWidget.WidgetData.AlchemyFormulaDef.craftsIn.Count <= 0)
			{
				break;
			}
			string str3 = LLBase.L("ui_crafted_at") + ": ";
			string text6 = instance.selectedSmallDescriptionTextStyle.ApplyStyleToString(str3) ?? "";
			for (int k = 0; k < entityWidget.WidgetData.AlchemyFormulaDef.craftsIn.Count; k++)
			{
				if (k > 0)
				{
					text6 += ", ";
				}
				text6 += LLBase.L(entityWidget.WidgetData.AlchemyFormulaDef.craftsIn[k]);
			}
			if (!(list[list.Count - 1] is UITooltipSeparatorWidgetData))
			{
				list.Add(new UITooltipSeparatorWidgetData());
			}
			UITooltipTextWidgetData item5 = new UITooltipTextWidgetData(text6, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
			list.Add(item5);
			break;
		}
		case LinkedEntityType.ItemDef:
			AddItemWidgets(list, entityWidget.WidgetData.ItemDef, addHeader: false, "", entityWidget.WidgetData.NotShowStudyWidgetInItemTooltips, entityWidget.WidgetData.ShowCraftedAtFromCraftDefInsteadOfItem);
			break;
		case LinkedEntityType.Item:
			AddItemWidgets(list, entityWidget.WidgetData.Item.Definition, addHeader: false, "", entityWidget.WidgetData.NotShowStudyWidgetInItemTooltips, entityWidget.WidgetData.ShowCraftedAtFromCraftDefInsteadOfItem);
			break;
		case LinkedEntityType.GameRes:
		case LinkedEntityType.DayNumber:
			return;
		case LinkedEntityType.BuildingDef:
		{
			AddBuildingWidgets(list, entityWidget.WidgetData.BuildingDef, addHeader: false);
			WGODef data = GameBalance.Me.GetData<WGODef>(entityWidget.WidgetData.BuildingDef.wgoId);
			if (data != null && GameBalance.Me.IsWorkbenchExtensionId(data.id))
			{
				string str = LLBase.L("ui_hint_extention_for") + " ";
				string text2 = instance.selectedSmallDescriptionTextStyle.ApplyStyleToString(str) ?? "";
				for (int i = 0; i < GameBalance.Me.workbenchParentsByExtensionCache[data.id].Count; i++)
				{
					if (i > 0)
					{
						text2 += ", ";
					}
					text2 += LLBase.L(GameBalance.Me.workbenchParentsByExtensionCache[data.id][i].id);
				}
				if (!(list[list.Count - 1] is UITooltipSeparatorWidgetData))
				{
					list.Add(new UITooltipSeparatorWidgetData());
				}
				UITooltipTextWidgetData item = new UITooltipTextWidgetData(text2, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
				list.Add(item);
			}
			TryAddNeedItemsWidget(list, entityWidget.WidgetData.BuildingDef.needItems);
			break;
		}
		case LinkedEntityType.TownBuildingDef:
			AddTownBuildingWidgets(list, entityWidget.WidgetData.TownBuildingDef, addHeader: false);
			TryAddNeedItemsWidget(list, entityWidget.WidgetData.TownBuildingDef.needItems);
			break;
		case LinkedEntityType.PerkDef:
			AddPerkWidgets(list, entityWidget.WidgetData.PerkDef, addHeader: false);
			break;
		case LinkedEntityType.Order:
			AddItemWidgets(list, entityWidget.WidgetData.ItemDef, addHeader: false, "", entityWidget.WidgetData.NotShowStudyWidgetInItemTooltips, entityWidget.WidgetData.ShowCraftedAtFromCraftDefInsteadOfItem);
			break;
		default:
			Debug.LogError("Type:[LinkedEntityType] is not implemented for tooltip!!!");
			return;
		}
		if (list.Count > 1 && !(list[1] is UITooltipSeparatorWidgetData))
		{
			list.Insert(1, new UITooltipSeparatorWidgetData());
		}
		instance.ShowAtTargetAndCalculateOffsets(list, entityWidget.transform as RectTransform);
	}

	public static void ShowCraftRequirementDescription(UICraftRequirementWidget craftRequirementWidget, string requirementId, CraftDefBase craftDef, List<PerkData> perks, Item toolForWork)
	{
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		AddCraftRequirementWidgets(list, requirementId, craftDef, perks, toolForWork);
		instance.ShowAtTargetAndCalculateOffsets(list, craftRequirementWidget.transform as RectTransform);
	}

	public static void ShowProgressTicksInfo(UIProgressCellsInfoWidget progressCellsInfoWidget, int masteryValue, int masteryLock, bool isStarCraft, TalentDef talentDef)
	{
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		UITooltipProgressTicksWidgetData item = new UITooltipProgressTicksWidgetData(masteryValue, masteryLock, isStarCraft, talentDef);
		list.Add(item);
		instance.ShowAtTargetAndCalculateOffsets(list, progressCellsInfoWidget.transform as RectTransform);
	}

	public static void ShowProgressTickBonus(ProgressCell progressCell, PerkDef perkDefBonus = null, ItemDef itemDefBonus = null)
	{
		if (perkDefBonus != null || itemDefBonus != null)
		{
			List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
			UITooltipProgressCellWidgetData item = new UITooltipProgressCellWidgetData(perkDefBonus, itemDefBonus);
			list.Add(item);
			instance.ShowAtTargetAndCalculateOffsets(list, progressCell.transform as RectTransform);
		}
	}

	public static void ShowAlchemyBoostInfo(RectTransform target, CraftDef boost)
	{
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		AddBoostCraftWidgets(list, boost);
		instance.ShowAtTargetAndCalculateOffsets(list, target);
	}

	public static void ShowCraftInfo(UIItemCell uiItemCell, WgoData wgoData, CraftDef craftDef, List<NeedItemData> customItems = null, RectTransform customTarget = null, UICraftStatusInfoWidgetData statusInfoWidgetData = null)
	{
		if (uiItemCell == null || uiItemCell.DisplayingOutputPreview == null)
		{
			return;
		}
		if (craftDef.id.StartsWith("mix"))
		{
			if (!AlchemyMixDef.IsUnknownMixResult(craftDef.id))
			{
				ShowMixCraftInfo(uiItemCell, wgoData, craftDef, customTarget);
			}
			return;
		}
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		int num;
		string text;
		if (craftDef.addItemsToWgoOnFinish.chanceOutputItems.Count > 0)
		{
			num = ((craftDef.addItemsToWgoOnFinish.chanceOutputItems[0].id == "alchemy_flask") ? 1 : 0);
			if (num != 0)
			{
				text = string.Format("{0} {1}{2}", LLBase.L("ui_place"), "alchemy_flask".FontIcon(), craftDef.addItemsToWgoOnFinish.chanceOutputItems[0].count.EvaluateInt(wgoData));
				goto IL_01cd;
			}
		}
		else
		{
			num = 0;
		}
		TalentDef data = GameBalance.Me.GetData<TalentDef>(wgoData.Definition.talent);
		IWorker worker = wgoData.Worker;
		if (worker == null)
		{
			worker = MainGame.PlayerController;
		}
		bool flag = worker.GetMasteryLevelForTalentBranch(wgoData.Definition.talent, craftDef) >= craftDef.talentLock;
		text = string.Concat(str2: string.Concat(str2: craftDef.isStarCraft ? (instance.headerBoldTextStyleGold.ApplyStyleToString($"1-{craftDef.talentLock}") ?? "") : ((!flag) ? (instance.headerBoldTextStyleNotEnough.ApplyStyleToString(craftDef.talentLock.ToString()) ?? "") : (instance.headerBoldTextStyle.ApplyStyleToString(craftDef.talentLock.ToString()) ?? "")), str0: data.id.FontIcon(), str1: " ").NOBR(), str0: LLBase.L(craftDef.id), str1: " ");
		goto IL_01cd;
		IL_01cd:
		UITooltipTextWidgetData item = new UITooltipTextWidgetData(text, TextAlignmentOptions.Center, instance.headerTextStyle);
		list.Add(item);
		if (num == 0)
		{
			ItemDef itemDef = craftDef.TryGetResultingItemDef();
			if (itemDef != null && itemDef.type != ItemType.Preach)
			{
				int count = list.Count;
				AddItemEffectWidgets(list, itemDef, craftDef.GetPossibleResultingItemDefs());
				if (list.Count > count)
				{
					list.Insert(count, new UITooltipSeparatorWidgetData());
				}
			}
		}
		UITooltipNeedsItemWidgetData uITooltipNeedsItemWidgetData = ((customItems == null) ? new UITooltipNeedsItemWidgetData(craftDef, wgoData.CraftComponent) : new UITooltipNeedsItemWidgetData(craftDef, wgoData.CraftComponent, customItems));
		if (uITooltipNeedsItemWidgetData.CraftItemCellsData.Count > 0)
		{
			if (!(list[list.Count - 1] is UITooltipSeparatorWidgetData))
			{
				list.Add(new UITooltipSeparatorWidgetData());
			}
			list.Add(uITooltipNeedsItemWidgetData);
		}
		if (statusInfoWidgetData != null)
		{
			list.Add(statusInfoWidgetData);
		}
		instance.ShowAtTargetAndCalculateOffsets(list, (customTarget == null) ? (uiItemCell.transform as RectTransform) : customTarget);
	}

	public static void ShowExhumeButtonWidget(LazyButton exhumeButton)
	{
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		UITooltipTextWidgetData item = new UITooltipTextWidgetData(LLBase.L("ui_cant_exhume") ?? "", TextAlignmentOptions.Center, instance.descriptionTextStyle);
		list.Add(item);
		instance.ShowAtTargetAndCalculateOffsets(list, exhumeButton.transform as RectTransform);
	}

	public static void ShowResurrectionPrepareButtonWidget(LazyButton exhumeButton, string reason)
	{
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		UITooltipTextWidgetData item = new UITooltipTextWidgetData(LLBase.L(reason) ?? "", TextAlignmentOptions.Center, instance.descriptionTextStyle);
		list.Add(item);
		instance.ShowAtTargetAndCalculateOffsets(list, exhumeButton.transform as RectTransform);
	}

	private static void ShowMixCraftInfo(UIItemCell uiItemCell, WgoData wgoData, CraftDef craftDef, RectTransform customTarget = null)
	{
		if (uiItemCell == null || uiItemCell.DisplayingOutputPreview == null)
		{
			return;
		}
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		TalentDef data = GameBalance.Me.GetData<TalentDef>(wgoData.Definition.talent);
		string text = instance.headerBoldTextStyle.ApplyStyleToString(craftDef.talentLock.ToString()) ?? "";
		UITooltipTextWidgetData item = new UITooltipTextWidgetData(LLBase.L(craftDef.id) + " " + data.id.FontIcon() + " " + text, TextAlignmentOptions.Center, instance.headerTextStyle);
		list.Add(item);
		list.Add(new UIMixInfoWidgetData(GameBalance.GetAlchemyMixDef(craftDef.id)));
		string text2 = string.Empty;
		foreach (NeedItemData needItem in craftDef.needItems)
		{
			if (!needItem.IsGroup && needItem.ItemDef != null && needItem.ItemDef.isFuel)
			{
				text2 += $"{needItem.id.FontIcon()}{needItem.GetCount(wgoData)}, ";
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = text2.Remove(text2.Length - 2, 2);
			list.Add(new UITooltipTextWidgetData(text2 ?? "", TextAlignmentOptions.Center, instance.descriptionTextStyle));
		}
		instance.ShowAtTargetAndCalculateOffsets(list, (customTarget == null) ? (uiItemCell.transform as RectTransform) : customTarget);
	}

	private static void TryAddNeedItemsWidget(List<LazyWidgetDataBase> widgetData, List<NeedItemData> needItems)
	{
		if (needItems == null || needItems.Count == 0)
		{
			return;
		}
		UITooltipNeedsItemWidgetData uITooltipNeedsItemWidgetData = new UITooltipNeedsItemWidgetData(needItems, MainGame.PlayerController.WorkerMultiInventory);
		if (uITooltipNeedsItemWidgetData.CraftItemCellsData.Count == 0)
		{
			return;
		}
		if (widgetData.Count > 0)
		{
			if (!(widgetData[widgetData.Count - 1] is UITooltipSeparatorWidgetData))
			{
				widgetData.Add(new UITooltipSeparatorWidgetData());
			}
		}
		widgetData.Add(uITooltipNeedsItemWidgetData);
	}

	private static void TryAddRequiredExtensionWidget(List<LazyWidgetDataBase> widgetData, CraftDef craftDef)
	{
		if (craftDef == null || string.IsNullOrEmpty(craftDef.extensionNeedId))
		{
			return;
		}
		string str = LLBase.L("required_extension") + " ";
		string text = instance.selectedSmallDescriptionTextStyle.ApplyStyleToString(str) + LLBase.L(craftDef.extensionNeedId);
		if (widgetData.Count > 0)
		{
			if (widgetData[widgetData.Count - 1] is UITooltipTextWidgetData uITooltipTextWidgetData && uITooltipTextWidgetData.Text.Contains(LLBase.L("ui_crafted_at")))
			{
				widgetData[widgetData.Count - 1] = new UITooltipTextWidgetData(uITooltipTextWidgetData.Text + "\n" + text, uITooltipTextWidgetData.TextAlignmentOptions, uITooltipTextWidgetData.TextStyle);
				return;
			}
		}
		if (widgetData.Count > 0)
		{
			if (!(widgetData[widgetData.Count - 1] is UITooltipSeparatorWidgetData))
			{
				widgetData.Add(new UITooltipSeparatorWidgetData());
			}
		}
		widgetData.Add(new UITooltipTextWidgetData(text, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle));
	}

	private static void AddBuildingWidgets(List<LazyWidgetDataBase> widgetData, BuildingDef buildingDef, bool addHeader = true, string headerPrefix = "")
	{
		if (addHeader)
		{
			UITooltipTextWidgetData item = new UITooltipTextWidgetData(TryConstructHeaderWithPrefix(buildingDef.GetHeader(), headerPrefix), TextAlignmentOptions.Center, instance.headerTextStyle);
			widgetData.Add(item);
		}
	}

	private static void AddTownBuildingWidgets(List<LazyWidgetDataBase> widgetData, TownBuildingDef townBuildingDef, bool addHeader = true)
	{
		if (addHeader)
		{
			string text = TryConstructHeaderWithPrefix(townBuildingDef.GetHeader(), townBuildingDef.GetHeaderPrefix());
			widgetData.Add(new UITooltipTextWidgetData(text, TextAlignmentOptions.Center, instance.headerTextStyle));
		}
		string text2 = townBuildingDef.id + "_d";
		string text3 = LLBase.L(text2);
		if (text3 != text2)
		{
			widgetData.Add(new UITooltipTextWidgetData(text3, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle));
		}
	}

	private static void AddTalentLevelUpWidgets(List<LazyWidgetDataBase> widgetData, TalentLevelUpDef def)
	{
		if (!string.IsNullOrEmpty(def.linkedPerk))
		{
			PerkDef data = GameBalance.Me.GetData<PerkDef>(def.linkedPerk);
			if (data != null)
			{
				AddPerkWidgets(widgetData, data);
			}
		}
		else
		{
			UITooltipTextWidgetData item = new UITooltipTextWidgetData(string.Format("{0}: {1} +{2}", LLBase.L("ui_add_mastery"), def.talentId.FontIcon(), def.talentValueAdd), TextAlignmentOptions.Center, instance.headerTextStyle);
			widgetData.Add(item);
		}
	}

	private static void AddPerkWidgets(List<LazyWidgetDataBase> widgetData, PerkDef perkDef, bool addHeader = true, string headerPrefix = "")
	{
		if (addHeader)
		{
			UITooltipTextWidgetData item = new UITooltipTextWidgetData(TryConstructHeaderWithPrefix(perkDef.GetHeader(), headerPrefix), TextAlignmentOptions.Center, instance.headerTextStyle);
			widgetData.Add(item);
			widgetData.Add(new UITooltipSeparatorWidgetData());
		}
		TryAddDescriptionWidget(widgetData, perkDef.id);
	}

	private static void AddBoostCraftWidgets(List<LazyWidgetDataBase> widgetData, CraftDef boost, string headerPrefix = "")
	{
		UITooltipTextWidgetData item = new UITooltipTextWidgetData(TryConstructHeaderWithPrefix(LLBase.L(boost.id), headerPrefix), TextAlignmentOptions.Center, instance.headerTextStyle);
		widgetData.Add(item);
		widgetData.Add(new UITooltipSeparatorWidgetData());
		UITooltipTextWidgetData item2 = new UITooltipTextWidgetData(boost.Description, TextAlignmentOptions.Center, instance.descriptionTextStyle);
		widgetData.Add(item2);
	}

	private static void AddItemEffectWidgets(List<LazyWidgetDataBase> widgetData, ItemDef itemDef, IReadOnlyList<ItemDef> qualityVariants = null)
	{
		if (itemDef == null)
		{
			return;
		}
		IReadOnlyList<ItemDef> readOnlyList = ResolveItemEffectVariants(itemDef, qualityVariants);
		itemDef = readOnlyList[0];
		int num = itemDef.quality;
		int num2 = itemDef.quality;
		int num3 = Mathf.Abs((int)itemDef.GetGameResOnUse("energy"));
		int num4 = num3;
		int num5 = itemDef.talentBonus;
		int num6 = itemDef.talentBonus;
		int num7 = ((itemDef.damage != null) ? itemDef.damage.EvaluateInt() : 0);
		int num8 = num7;
		int num9 = itemDef.redSkulls;
		int num10 = itemDef.redSkulls;
		int num11 = itemDef.whiteSkulls;
		int num12 = itemDef.whiteSkulls;
		int num13 = itemDef.bagSizeX;
		int num14 = itemDef.bagSizeX;
		int num15 = itemDef.bagSizeY;
		int num16 = itemDef.bagSizeY;
		int num17 = itemDef.talentValue;
		int num18 = itemDef.talentValue;
		int num19 = itemDef.whiteSkullsMinCollar;
		int num20 = itemDef.whiteSkullsMaxCollar;
		int num21 = itemDef.redSkullsMinCollar;
		int num22 = itemDef.redSkullsMaxCollar;
		bool flag = itemDef.whiteSkullsMaxCollar > itemDef.whiteSkullsMinCollar;
		bool flag2 = itemDef.redSkullsMaxCollar > itemDef.redSkullsMinCollar;
		Dictionary<string, Vector2Int> dictionary = new Dictionary<string, Vector2Int>();
		GameRes gameResOnUse = itemDef.GetGameResOnUse();
		for (int i = 0; i < gameResOnUse.List.Count; i++)
		{
			int num23 = (int)gameResOnUse.List[i].value;
			dictionary[gameResOnUse.List[i].type] = new Vector2Int(num23, num23);
		}
		for (int j = 1; j < readOnlyList.Count; j++)
		{
			ItemDef itemDef2 = readOnlyList[j];
			num = Math.Min(num, itemDef2.quality);
			num2 = Math.Max(num2, itemDef2.quality);
			int val = Mathf.Abs((int)itemDef2.GetGameResOnUse("energy"));
			num3 = Math.Min(num3, val);
			num4 = Math.Max(num4, val);
			num5 = Math.Min(num5, itemDef2.talentBonus);
			num6 = Math.Max(num6, itemDef2.talentBonus);
			int val2 = ((itemDef2.damage != null) ? itemDef2.damage.EvaluateInt() : 0);
			num7 = Math.Min(num7, val2);
			num8 = Math.Max(num8, val2);
			num9 = Math.Min(num9, itemDef2.redSkulls);
			num10 = Math.Max(num10, itemDef2.redSkulls);
			num11 = Math.Min(num11, itemDef2.whiteSkulls);
			num12 = Math.Max(num12, itemDef2.whiteSkulls);
			num13 = Math.Min(num13, itemDef2.bagSizeX);
			num14 = Math.Max(num14, itemDef2.bagSizeX);
			num15 = Math.Min(num15, itemDef2.bagSizeY);
			num16 = Math.Max(num16, itemDef2.bagSizeY);
			num17 = Math.Min(num17, itemDef2.talentValue);
			num18 = Math.Max(num18, itemDef2.talentValue);
			if (itemDef2.whiteSkullsMaxCollar > itemDef2.whiteSkullsMinCollar)
			{
				if (!flag)
				{
					num19 = itemDef2.whiteSkullsMinCollar;
					num20 = itemDef2.whiteSkullsMaxCollar;
					flag = true;
				}
				else
				{
					num19 = Math.Min(num19, itemDef2.whiteSkullsMinCollar);
					num20 = Math.Max(num20, itemDef2.whiteSkullsMaxCollar);
				}
			}
			if (itemDef2.redSkullsMaxCollar > itemDef2.redSkullsMinCollar)
			{
				if (!flag2)
				{
					num21 = itemDef2.redSkullsMinCollar;
					num22 = itemDef2.redSkullsMaxCollar;
					flag2 = true;
				}
				else
				{
					num21 = Math.Min(num21, itemDef2.redSkullsMinCollar);
					num22 = Math.Max(num22, itemDef2.redSkullsMaxCollar);
				}
			}
			GameRes gameResOnUse2 = itemDef2.GetGameResOnUse();
			for (int k = 0; k < gameResOnUse2.List.Count; k++)
			{
				string type = gameResOnUse2.List[k].type;
				int num24 = (int)gameResOnUse2.List[k].value;
				if (dictionary.TryGetValue(type, out var value))
				{
					dictionary[type] = new Vector2Int(Math.Min(value.x, num24), Math.Max(value.y, num24));
				}
				else
				{
					dictionary[type] = new Vector2Int(num24, num24);
				}
			}
		}
		if (itemDef.itemGroupIds.Contains("bodypart"))
		{
			string skullsRangeAsString = ItemDef.GetSkullsRangeAsString(num9, num10, num11, num12, instance.redEffectTextStyle, instance.descriptionTextStyle);
			if (!string.IsNullOrEmpty(skullsRangeAsString))
			{
				UITooltipTextWidgetData item = new UITooltipTextWidgetData(LLBase.L("ui_skulls") + ": " + skullsRangeAsString, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
				widgetData.Add(item);
			}
		}
		if (!string.IsNullOrEmpty(itemDef.qualityIcon) && num2 != 0)
		{
			UITooltipTextWidgetData item2 = new UITooltipTextWidgetData(LLBase.L("icon_" + itemDef.qualityIcon) + ": " + itemDef.qualityIcon.FontIcon() + " " + FormatStyledIntRange(num, num2), TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
			widgetData.Add(item2);
		}
		if (itemDef.isTool)
		{
			if (itemDef.HasGameResOnUse("energy"))
			{
				string text = (GameResDisplayConfig.GetConfigForRes("energy", GameResIconType.Common).iconName.FontIcon() + FormatStyledIntRange(num3, num4)).NOBR();
				UITooltipTextWidgetData item3 = new UITooltipTextWidgetData(LLBase.L("ui_energy_cons") + ": " + text, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
				widgetData.Add(item3);
			}
			if (num6 > 0 && itemDef.talentIds.Count > 0)
			{
				UITooltipTextWidgetData item4 = new UITooltipTextWidgetData(LLBase.L("ui_mastery") + ": +" + itemDef.talentIds[0].FontIcon() + " " + FormatStyledIntRange(num5, num6), TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
				widgetData.Add(item4);
			}
		}
		if (itemDef.isWeapon && (num7 != 0 || num8 != 0))
		{
			string iconName = "equip_icon_sword";
			switch (itemDef.type)
			{
			case ItemType.Bow:
				iconName = "equip_icon_arrow";
				break;
			case ItemType.Pike:
				iconName = "squad_equip_icon-spear";
				break;
			}
			UITooltipTextWidgetData item5 = new UITooltipTextWidgetData(LLBase.L("ui_attack") + ": " + iconName.FontIcon() + FormatStyledIntRange(num7, num8), TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
			widgetData.Add(item5);
		}
		if (itemDef.type == ItemType.BodyArmor && num2 != 0)
		{
			UITooltipTextWidgetData item6 = new UITooltipTextWidgetData(LLBase.L("ui_defence") + ": " + "equip_icon_armor".FontIcon() + FormatStyledIntRange(num, num2), TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
			widgetData.Add(item6);
		}
		if (itemDef.CanBeUsed)
		{
			List<PerkDef> perksOnUse = itemDef.GetPerksOnUse();
			bool flag3 = gameResOnUse.IsEmpty();
			if (!flag3 || perksOnUse.Count > 0)
			{
				string text2 = LLBase.L("ui_effect_on_use");
				if (!flag3 && !itemDef.isTool)
				{
					text2 += ": ";
					bool flag4 = true;
					for (int l = 0; l < gameResOnUse.List.Count; l++)
					{
						string type2 = gameResOnUse.List[l].type;
						Vector2Int vector2Int = dictionary[type2];
						GameResIconConfig configForRes = GameResDisplayConfig.GetConfigForRes(type2, GameResIconType.Common);
						string text3 = FormatOnUseResRange((configForRes != null) ? configForRes.iconName.FontIcon() : type2.FontIcon(), vector2Int.x, vector2Int.y, type2 == "insanity");
						if (!flag4)
						{
							text3 = " " + text3;
						}
						flag4 = false;
						text2 += text3;
					}
				}
				if (perksOnUse.Count > 0)
				{
					text2 = ((!(text2 != LLBase.L("ui_effect_on_use"))) ? (text2 + ": ") : (text2 + ", "));
					string text4 = string.Empty;
					for (int m = 0; m < perksOnUse.Count; m++)
					{
						string text5 = LLBase.L(perksOnUse[m].id);
						string text6 = perksOnUse[m].id + "_d";
						string text7 = LLBase.L(text6);
						if (text7 != text6)
						{
							string text8 = instance.perkDescriptionTextStyle.ApplyStyleToString(text5);
							text4 = text4 + text8 + " (" + text7 + ")";
						}
						else
						{
							text4 += text5;
						}
						text2 += text4;
						if (m < perksOnUse.Count - 1)
						{
							text2 += ", ";
						}
					}
				}
				UITooltipTextWidgetData item7 = new UITooltipTextWidgetData(text2, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
				widgetData.Add(item7);
			}
		}
		if (!string.IsNullOrEmpty(itemDef.bodyLinkedPerk))
		{
			PerkDef data = GameBalance.Me.GetData<PerkDef>(itemDef.bodyLinkedPerk);
			string text9 = LLBase.L("ui_body_linked_perk");
			text9 += ": ";
			string empty = string.Empty;
			string text10 = LLBase.L(data.id);
			string text11 = data.id + "_d";
			string text12 = LLBase.L(text11);
			if (text12 != text11)
			{
				string text13 = instance.perkDescriptionTextStyle.ApplyStyleToString(text10);
				empty = empty + text13 + " (" + text12 + ")";
			}
			else
			{
				empty += text10;
			}
			text9 += empty;
			UITooltipTextWidgetData item8 = new UITooltipTextWidgetData(text9, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
			widgetData.Add(item8);
		}
		if (itemDef.type == ItemType.Collar)
		{
			if (flag)
			{
				string text14 = instance.descriptionTextStyle.ApplyStyleToString("skull".FontIcon() + FormatIntRange(num19, num20));
				UITooltipTextWidgetData item9 = new UITooltipTextWidgetData(LLBase.L("ui_skulls_white") + ": " + text14, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
				widgetData.Add(item9);
			}
			if (flag2)
			{
				string text15 = instance.descriptionTextStyle.ApplyStyleToString("rskull".FontIcon() + FormatIntRange(num21, num22));
				UITooltipTextWidgetData item10 = new UITooltipTextWidgetData(LLBase.L("ui_skulls_red") + ": " + text15, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
				widgetData.Add(item10);
			}
		}
		if (itemDef.type == ItemType.Embalm)
		{
			string skullsRangeAsString2 = ItemDef.GetSkullsRangeAsString(num9, num10, num11, num12, instance.redEffectTextStyle, instance.descriptionTextStyle);
			if (!string.IsNullOrEmpty(skullsRangeAsString2))
			{
				UITooltipTextWidgetData item11 = new UITooltipTextWidgetData(string.Concat(LLBase.L("ui_embalming_effect") + ":\n", skullsRangeAsString2), TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
				widgetData.Add(item11);
			}
		}
		if (itemDef.type == ItemType.Bag)
		{
			UITooltipTextWidgetData item12 = new UITooltipTextWidgetData(LLBase.L("ui_bag_size") + ": " + FormatStyledIntRange(num13, num14) + "x" + FormatStyledIntRange(num15, num16), TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
			widgetData.Add(item12);
		}
		if (num18 > 0 && !string.IsNullOrEmpty(itemDef.talentType))
		{
			UITooltipTextWidgetData item13 = ((!itemDef.isSeed) ? new UITooltipTextWidgetData(LLBase.L("ui_mastery") + ": " + itemDef.talentType.FontIcon() + " " + FormatStyledIntRange(num17, num18), TextAlignmentOptions.Center, instance.smallDescriptionTextStyle) : new UITooltipTextWidgetData(LLBase.L("item_tooltip_required_mastery") + " " + itemDef.talentType.FontIcon() + " " + FormatStyledIntRange(num17, num18), TextAlignmentOptions.Center, instance.smallDescriptionTextStyle));
			widgetData.Add(item13);
		}
	}

	private static IReadOnlyList<ItemDef> ResolveItemEffectVariants(ItemDef itemDef, IReadOnlyList<ItemDef> qualityVariants)
	{
		if (qualityVariants == null || qualityVariants.Count <= 1 || !AreNumericQualityVariants(qualityVariants))
		{
			return new ItemDef[1] { itemDef };
		}
		return qualityVariants;
	}

	private static bool AreNumericQualityVariants(IReadOnlyList<ItemDef> items)
	{
		ItemDef itemDef = items[0];
		List<PerkDef> perksOnUse = itemDef.GetPerksOnUse();
		GameRes gameResOnUse = itemDef.GetGameResOnUse();
		bool flag = itemDef.itemGroupIds.Contains("bodypart");
		for (int i = 1; i < items.Count; i++)
		{
			ItemDef itemDef2 = items[i];
			if (itemDef2.type != itemDef.type || itemDef2.qualityIcon != itemDef.qualityIcon || itemDef2.isTool != itemDef.isTool || itemDef2.isWeapon != itemDef.isWeapon || itemDef2.isSeed != itemDef.isSeed || itemDef2.isBag != itemDef.isBag || itemDef2.talentType != itemDef.talentType || itemDef2.bodyLinkedPerk != itemDef.bodyLinkedPerk || itemDef2.CanBeUsed != itemDef.CanBeUsed || itemDef2.itemGroupIds.Contains("bodypart") != flag)
			{
				return false;
			}
			if (!AreSameStrings(itemDef.talentIds, itemDef2.talentIds))
			{
				return false;
			}
			if (!AreSamePerks(perksOnUse, itemDef2.GetPerksOnUse()))
			{
				return false;
			}
			if (!AreSameResTypes(gameResOnUse, itemDef2.GetGameResOnUse()))
			{
				return false;
			}
		}
		return true;
	}

	private static bool AreSameStrings(List<string> a, List<string> b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null || a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	private static bool AreSamePerks(List<PerkDef> a, List<PerkDef> b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null || a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			if (a[i] != b[i] && (a[i] == null || b[i] == null || a[i].id != b[i].id))
			{
				return false;
			}
		}
		return true;
	}

	private static bool AreSameResTypes(GameRes a, GameRes b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null || a.List.Count != b.List.Count)
		{
			return false;
		}
		for (int i = 0; i < a.List.Count; i++)
		{
			if (!b.Has(a.List[i].type))
			{
				return false;
			}
		}
		return true;
	}

	private static string FormatIntRange(int min, int max)
	{
		if (min != max)
		{
			return $"{min}-{max}";
		}
		return min.ToString();
	}

	private static string FormatStyledIntRange(int min, int max)
	{
		return instance.descriptionTextStyle.ApplyStyleToString(FormatIntRange(min, max));
	}

	private static string FormatOnUseResRange(string icon, int min, int max, bool isInsanity)
	{
		TextStyle textStyle;
		string str;
		if (min >= 0 && max >= 0)
		{
			textStyle = (isInsanity ? instance.blackEffectTextStyle : instance.greenEffectTextStyle);
			str = ((min == max) ? $"+{min}" : $"+{min}-{max}");
		}
		else if (min <= 0 && max <= 0)
		{
			textStyle = (isInsanity ? instance.blackEffectTextStyle : instance.redEffectTextStyle);
			str = ((min == max) ? min.ToString() : $"{min}-{max}");
		}
		else
		{
			textStyle = (isInsanity ? instance.blackEffectTextStyle : instance.descriptionTextStyle);
			string arg = ((max > 0) ? $"+{max}" : max.ToString());
			str = $"{min}-{arg}";
		}
		return (icon + textStyle.ApplyStyleToString(str)).NOBR();
	}

	private static void AddItemWidgets(List<LazyWidgetDataBase> widgetData, ItemDef itemDef, bool addHeader = true, string headerPrefix = "", bool notShowStudyWidget = false, bool nowShowCraftedAt = false, IReadOnlyList<ItemDef> qualityVariants = null)
	{
		if (addHeader)
		{
			UITooltipTextWidgetData item = new UITooltipTextWidgetData(TryConstructHeaderWithPrefix(itemDef.GetHeader(), headerPrefix), TextAlignmentOptions.Center, instance.headerTextStyle);
			widgetData.Add(item);
			widgetData.Add(new UITooltipSeparatorWidgetData());
		}
		if (itemDef.type == ItemType.Preach)
		{
			SermonDef sermonDef = GameBalance.GetSermonDef(itemDef.id);
			if (sermonDef != null)
			{
				widgetData.Add(new UITooltipTextWidgetData(instance.prayDescriptionSelectedTextStyle.ApplyStyleToString(LLBase.L("ui_sermon_tooltip_need_start")) + " " + string.Format("{0}{1}", "happiness_cross".FontIcon(), sermonDef.minParishioners), TextAlignmentOptions.Center, instance.descriptionTextStyle));
				widgetData.Add(new UITooltipTextWidgetData(instance.prayDescriptionSelectedTextStyle.ApplyStyleToString(LLBase.L("ui_sermon_tooltip_need_success")) + " " + string.Format("{0}{1}", "happiness_cross".FontIcon(), sermonDef.sermonDifficulty), TextAlignmentOptions.Center, instance.descriptionTextStyle));
				widgetData.Add(new UITooltipTextWidgetData(instance.prayDescriptionSelectedTextStyle.ApplyStyleToString(LLBase.L("ui_sermon_tooltip_base_reward")) + "\n" + string.Format("{0}{1:0.#}/", "faith".FontIcon(), sermonDef.baseFaithReward.EvaluateFloat()) + "happiness_cross".FontIcon() + " + " + Trading.FormatMoney(sermonDef.baseMoneyReward.EvaluateInt()) + "/" + "happiness_cross".FontIcon(), TextAlignmentOptions.Center, instance.descriptionTextStyle));
				string text = string.Empty;
				float num = sermonDef.successRewardSmileFaith.EvaluateFloat();
				float num2 = sermonDef.successRewardCemeteryFaith.EvaluateFloat();
				float num3 = sermonDef.successRewardChurchFaith.EvaluateFloat();
				float num4 = sermonDef.successRewardSmileMoney.EvaluateFloat();
				float num5 = sermonDef.successRewardCemeteryMoney.EvaluateFloat();
				float num6 = sermonDef.successRewardChurchMoney.EvaluateFloat();
				if (num > 0f)
				{
					text += string.Format("{0}{1:0.#}/{2}", "faith".FontIcon(), num, "happiness".FontIcon());
				}
				if (num2 > 0f)
				{
					text += string.Format(" + {0}{1:0.#}/{2}", "faith".FontIcon(), num2, "wskull".FontIcon());
				}
				if (num3 > 0f)
				{
					text += string.Format(" + {0}{1:0.#}/{2}", "faith".FontIcon(), num3, "cross".FontIcon());
				}
				if (num4 > 0f)
				{
					text = text + " + " + Trading.FormatMoney((int)num4) + "/" + "happiness".FontIcon();
				}
				if (num5 > 0f)
				{
					text = text + " + " + Trading.FormatMoney((int)num5) + "/" + "wskull".FontIcon();
				}
				if (num6 > 0f)
				{
					text = text + " + " + Trading.FormatMoney((int)num6) + "/" + "cross".FontIcon();
				}
				if (!string.IsNullOrEmpty(text))
				{
					if (text.StartsWith(" +"))
					{
						text = text.Substring(3);
					}
					widgetData.Add(new UITooltipTextWidgetData(instance.prayDescriptionSelectedTextStyle.ApplyStyleToString(LLBase.L("ui_sermon_tooltip_success_reward")) + "\n" + text, TextAlignmentOptions.Center, instance.descriptionTextStyle));
				}
				if (sermonDef.successRewardItem.HasOutputItems)
				{
					foreach (ChanceOutputItem chanceOutputItem in sermonDef.successRewardItem.chanceOutputItems)
					{
						widgetData.Add(new UITooltipTextWidgetData(string.Format("{0} {1}{2}", instance.prayDescriptionSelectedTextStyle.ApplyStyleToString(LLBase.L("ui_sermon_tooltip_item")), LLBase.L(chanceOutputItem.id), chanceOutputItem.count), TextAlignmentOptions.Center, instance.descriptionTextStyle));
					}
					foreach (GroupChanceOutputItem groupChanceOutputItem in sermonDef.successRewardItem.groupChanceOutputItems)
					{
						if (groupChanceOutputItem.chanceItems.Count > 0)
						{
							widgetData.Add(new UITooltipTextWidgetData(string.Format("{0} {1}{2}", instance.prayDescriptionSelectedTextStyle.ApplyStyleToString(LLBase.L("ui_sermon_tooltip_item")), LLBase.L(groupChanceOutputItem.chanceItems[0].id), groupChanceOutputItem.chanceItems[0].count), TextAlignmentOptions.Center, instance.descriptionTextStyle));
						}
					}
				}
				if (!string.IsNullOrEmpty(sermonDef.successRewardBuff))
				{
					PerkDef data = GameBalance.Me.GetData<PerkDef>(sermonDef.successRewardBuff);
					if (data != null)
					{
						widgetData.Add(new UITooltipTextWidgetData(instance.prayDescriptionSelectedTextStyle.ApplyStyleToString(LLBase.L("ui_sermon_tooltip_buff")) + " " + LLBase.L(sermonDef.successRewardBuff) + "(" + PerkSystemData.GetFormattedDuration(data.duration) + ")", TextAlignmentOptions.Center, instance.descriptionTextStyle));
					}
				}
				if (nowShowCraftedAt || !GameBalance.Me.craftInItemsCacheShownInTooltips.ContainsKey(itemDef.id) || GameBalance.Me.craftInItemsCacheShownInTooltips[itemDef.id].Count <= 0)
				{
					return;
				}
				string str = LLBase.L("ui_crafted_at") + ": ";
				string text2 = instance.selectedSmallDescriptionTextStyle.ApplyStyleToString(str) ?? "";
				for (int i = 0; i < GameBalance.Me.craftInItemsCacheShownInTooltips[itemDef.id].Count; i++)
				{
					if (i > 0)
					{
						text2 += ", ";
					}
					text2 += LLBase.L(GameBalance.Me.craftInItemsCacheShownInTooltips[itemDef.id][i]);
				}
				if (!(widgetData[widgetData.Count - 1] is UITooltipSeparatorWidgetData))
				{
					widgetData.Add(new UITooltipSeparatorWidgetData());
				}
				UITooltipTextWidgetData item2 = new UITooltipTextWidgetData(text2, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
				widgetData.Add(item2);
				return;
			}
			Debug.LogError("SermonDef is null for itemDef: " + itemDef.id + ". Show common tooltip");
		}
		if (TryAddDescriptionWidget(widgetData, itemDef.id))
		{
			widgetData.Add(new UITooltipSeparatorWidgetData());
		}
		AddItemEffectWidgets(widgetData, itemDef, qualityVariants);
		SurveyDef surveyDefForItemOrNull = GameBalance.GetSurveyDefForItemOrNull(itemDef.id);
		if (surveyDefForItemOrNull != null && MainGame.Instance.GameSave.knowledgeSystem.IsSurveyCompleted(surveyDefForItemOrNull))
		{
			string runesAsString = itemDef.GetRunesAsString();
			if (!string.IsNullOrEmpty(runesAsString))
			{
				runesAsString = instance.descriptionTextStyle.ApplyStyleToString(runesAsString);
				UITooltipTextWidgetData item3 = new UITooltipTextWidgetData(LLBase.L("item_runes") + ": \n" + runesAsString, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
				widgetData.Add(item3);
			}
		}
		else if (surveyDefForItemOrNull != null && !string.IsNullOrEmpty(itemDef.GetRunesAsString()))
		{
			UITooltipTextWidgetData item4 = new UITooltipTextWidgetData(LLBase.L("tut_survey_runes") + "???", TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
			widgetData.Add(item4);
		}
		if (surveyDefForItemOrNull != null)
		{
			if (surveyDefForItemOrNull.isScienceFuelCraft)
			{
				int num7 = ((surveyDefForItemOrNull.outputItems.chanceOutputItems.Count != 0) ? surveyDefForItemOrNull.outputItems.chanceOutputItems[0].count.EvaluateInt() : 0);
				if (num7 > 0)
				{
					UITooltipTextWidgetData item5 = new UITooltipTextWidgetData(LLBase.L("hint_science_decompose") + " " + "science".FontIcon() + instance.descriptionTextStyle.ApplyStyleToString(num7.ToString()), TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
					widgetData.Add(item5);
				}
			}
			else if (!MainGame.Instance.GameSave.knowledgeSystem.IsSurveyCompleted(surveyDefForItemOrNull) && !notShowStudyWidget)
			{
				string text3 = string.Empty;
				if (surveyDefForItemOrNull.techRed > 0)
				{
					text3 += "tech_red".FontIcon();
				}
				if (surveyDefForItemOrNull.techGreen > 0)
				{
					text3 += "tech_green".FontIcon();
				}
				if (surveyDefForItemOrNull.techBlue > 0)
				{
					text3 += "tech_blue".FontIcon();
				}
				if (!string.IsNullOrEmpty(text3))
				{
					text3 = " (" + text3 + ")";
					if (!(widgetData[widgetData.Count - 1] is UITooltipSeparatorWidgetData))
					{
						widgetData.Add(new UITooltipSeparatorWidgetData());
					}
					UITooltipTextWidgetData item6 = new UITooltipTextWidgetData(LLBase.L("hint_survey") + ":\n" + LLBase.L("survey_not_complete") + text3, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
					widgetData.Add(item6);
				}
			}
		}
		if (nowShowCraftedAt || !GameBalance.Me.craftInItemsCacheShownInTooltips.ContainsKey(itemDef.id) || GameBalance.Me.craftInItemsCacheShownInTooltips[itemDef.id].Count <= 0)
		{
			return;
		}
		string str2 = LLBase.L("ui_crafted_at") + ": ";
		string text4 = instance.selectedSmallDescriptionTextStyle.ApplyStyleToString(str2) ?? "";
		for (int j = 0; j < GameBalance.Me.craftInItemsCacheShownInTooltips[itemDef.id].Count; j++)
		{
			if (j > 0)
			{
				text4 += ", ";
			}
			text4 += LLBase.L(GameBalance.Me.craftInItemsCacheShownInTooltips[itemDef.id][j]);
		}
		if (!(widgetData[widgetData.Count - 1] is UITooltipSeparatorWidgetData))
		{
			widgetData.Add(new UITooltipSeparatorWidgetData());
		}
		UITooltipTextWidgetData item7 = new UITooltipTextWidgetData(text4, TextAlignmentOptions.Center, instance.smallDescriptionTextStyle);
		widgetData.Add(item7);
	}

	private static string TryConstructHeaderWithPrefix(string text, string headerPrefix)
	{
		if (!string.IsNullOrEmpty(headerPrefix))
		{
			text = headerPrefix + ": " + text;
		}
		return text;
	}

	private static bool TryAddDescriptionWidget(List<LazyWidgetDataBase> widgetData, string id)
	{
		string text = id + "_d";
		string text2 = LLBase.L(text);
		if (text2 != text)
		{
			UITooltipTextWidgetData item = new UITooltipTextWidgetData(text2, TextAlignmentOptions.Center, instance.descriptionTextStyle);
			widgetData.Add(item);
			return true;
		}
		return false;
	}

	private static void TryAddCraftNeedsWidgets(List<LazyWidgetDataBase> widgetData, CraftDef craftDef)
	{
		if (craftDef == null)
		{
			return;
		}
		ItemDef itemDef = craftDef.TryGetResultingItemDef();
		if (itemDef != null && itemDef.type == ItemType.Preach)
		{
			return;
		}
		UITooltipNeedsItemWidgetData uITooltipNeedsItemWidgetData = new UITooltipNeedsItemWidgetData(craftDef, new MultiInventory(MainGame.PlayerData));
		if (uITooltipNeedsItemWidgetData.CraftItemCellsData.Count == 0)
		{
			return;
		}
		if (widgetData.Count != 0)
		{
			if (widgetData[widgetData.Count - 1] is UITooltipSeparatorWidgetData)
			{
				goto IL_0063;
			}
		}
		widgetData.Add(new UITooltipSeparatorWidgetData());
		goto IL_0063;
		IL_0063:
		widgetData.Add(uITooltipNeedsItemWidgetData);
	}

	private static void AddCraftRequirementWidgets(List<LazyWidgetDataBase> widgetData, string requirementId, CraftDefBase craftDef, List<PerkData> perks, Item toolForWork)
	{
		if (!(requirementId == "energy"))
		{
			if (!(requirementId == "insanity"))
			{
				return;
			}
			UITooltipTextWidgetData item = new UITooltipTextWidgetData(LLBase.L("tt_insanity_spendings"), TextAlignmentOptions.Center, instance.headerTextStyle);
			widgetData.Add(item);
			widgetData.Add(new UITooltipSeparatorWidgetData());
			if (!craftDef.insanityPerTick.EvaluateFloat().EqualsTo(0f))
			{
				UITooltipTextWidgetData item2 = new UITooltipTextWidgetData(LLBase.L(craftDef.id ?? "") + ": " + "insanity".FontIcon() + craftDef.insanityPerTick.EvaluateFloat() + "\n", TextAlignmentOptions.Center, instance.descriptionTextStyle);
				widgetData.Add(item2);
			}
			if (!toolForWork.IsEmpty && !toolForWork.Definition.GetGameResOnUse("insanity").EqualsTo(0f))
			{
				UITooltipTextWidgetData item3 = new UITooltipTextWidgetData(LLBase.L(toolForWork.id) + ": " + "insanity".FontIcon() + toolForWork.Definition.GetGameResOnUse("insanity") + "\n", TextAlignmentOptions.Center, instance.descriptionTextStyle);
				widgetData.Add(item3);
			}
			{
				foreach (PerkData perk in perks)
				{
					if (!perk.Definition.insanityAdd.EqualsTo(0f))
					{
						UITooltipTextWidgetData item4 = new UITooltipTextWidgetData(LLBase.L(perk.id) + ": " + "insanity".FontIcon() + perk.Definition.insanityAdd + "\n", TextAlignmentOptions.Center, instance.descriptionTextStyle);
						widgetData.Add(item4);
					}
				}
				return;
			}
		}
		UITooltipTextWidgetData item5 = new UITooltipTextWidgetData(LLBase.L("tt_energy_spendings"), TextAlignmentOptions.Center, instance.headerTextStyle);
		widgetData.Add(item5);
		widgetData.Add(new UITooltipSeparatorWidgetData());
		if (!craftDef.energyPerTick.EvaluateFloat().EqualsTo(0f))
		{
			UITooltipTextWidgetData item6 = new UITooltipTextWidgetData(LLBase.L(craftDef.id ?? "") + ": " + "energy".FontIcon() + craftDef.energyPerTick.EvaluateFloat() + "\n", TextAlignmentOptions.Center, instance.descriptionTextStyle);
			widgetData.Add(item6);
		}
		if (!toolForWork.IsEmpty && !toolForWork.Definition.GetGameResOnUse("energy").EqualsTo(0f))
		{
			UITooltipTextWidgetData item7 = new UITooltipTextWidgetData(LLBase.L(toolForWork.id) + ": " + "energy".FontIcon() + toolForWork.Definition.GetGameResOnUse("energy") * -1f + "\n", TextAlignmentOptions.Center, instance.descriptionTextStyle);
			widgetData.Add(item7);
		}
		foreach (PerkData perk2 in perks)
		{
			if (!perk2.Definition.energyAdd.EqualsTo(0f))
			{
				UITooltipTextWidgetData item8 = new UITooltipTextWidgetData(LLBase.L(perk2.id) + ": " + "energy".FontIcon() + perk2.Definition.energyAdd + "\n", TextAlignmentOptions.Center, instance.descriptionTextStyle);
				widgetData.Add(item8);
			}
		}
	}
}
