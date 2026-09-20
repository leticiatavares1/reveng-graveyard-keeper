using System;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using LazyBearTechnology;
using LinqTools;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UIMultiAnswer : MonoBehaviour
{
	public enum OverflowMode
	{
		FullDisplayOnSide,
		Scroll
	}

	private const float DISAPPEAR_ANIM_TIME = 0.2f;

	private const float APPEAR_ANIM_TIME = 0.3f;

	private const float SCROLL_RECT_PADDING = 20f;

	[SerializeField]
	private float sideDisplayHorizontalOffset = 70f;

	[SerializeField]
	private UIMultiAnswerOption answerOptionPrefab;

	[SerializeField]
	private CanvasGroup canvas;

	[SerializeField]
	protected GamepadNavigationController gamepadController;

	[SerializeField]
	private ContentSizeFitter contentSizeFitter;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private AutoScroll autoScroll;

	[SerializeField]
	private LayoutElement scrollViewportLayoutElement;

	[SerializeField]
	private RectMask2D rectMask2D;

	[SerializeField]
	private OverflowMode overflowMode;

	private static UIMultiAnswer instance;

	private static List<UIMultiAnswer> multiAnswers = new List<UIMultiAnswer>();

	private Transform targetTransform;

	private WgoData dialogParticipant;

	private Action<string> onChosen;

	private Action onDisappeared;

	private bool isMainMultianswer;

	private bool isControllable = true;

	[SerializeField]
	private List<UIMultiAnswerOption> answerOptions;

	private List<AnswerVisualData> visualData;

	private UIBasicBubble.ForceCornerPosition forcedCornerPosition;

	private UIMultiAnswerOption bottomOption;

	private bool interactable;

	private int maxIconsCount;

	private bool subscribedGamepadEvents;

	private bool opensDownward;

	private bool isScrollModeActive;

	private bool isSideDisplayActive;

	private VerticalLayoutGroup rootVerticalLayoutGroup;

	private int rootPaddingTopDefault = -1;

	private int rootPaddingBottomDefault = -1;

	public static bool IsShowing => multiAnswers.Count != 0;

	public bool IsScrollModeActive => isScrollModeActive;

	public bool IsSideDisplayActive => isSideDisplayActive;

	public int MaxIconsCount
	{
		get
		{
			return maxIconsCount;
		}
		set
		{
			if (value > maxIconsCount)
			{
				maxIconsCount = value;
			}
		}
	}

	private VerticalLayoutGroup RootVerticalLayoutGroup
	{
		get
		{
			if (rootVerticalLayoutGroup == null)
			{
				rootVerticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
			}
			return rootVerticalLayoutGroup;
		}
	}

	public void Init()
	{
		instance = this;
		base.gameObject.SetActive(value: false);
		answerOptionPrefab.gameObject.SetActive(value: false);
		DisableScrollRectMode();
		SetSortingOrder(isOverBlackout: false);
	}

	public void OnAnswerSelect(string answerId)
	{
		if (interactable)
		{
			LazyAudio.PlayAndForget("gui_click");
			Debug.Log("answer is interactable, invoking action");
			onChosen?.Invoke(answerId);
			StartDisappearAnimation();
		}
	}

	public static void ForceDisableAll()
	{
		for (int num = multiAnswers.Count - 1; num >= 0; num--)
		{
			UIMultiAnswer uIMultiAnswer = multiAnswers[num];
			uIMultiAnswer.ChangeInteractable(isInteractable: false);
			uIMultiAnswer.gamepadController.Disable();
			uIMultiAnswer.DisableBubble();
		}
	}

	private void ShowAnswers(List<AnswerVisualData> answers, UIBasicBubble.ForceCornerPosition forceCornerPosition = UIBasicBubble.ForceCornerPosition.Auto, bool isOverBlackout = false)
	{
		visualData = FormVisibleAnswers(answers);
		forcedCornerPosition = forceCornerPosition;
		if (visualData.Count == 0)
		{
			Debug.LogError("Multianswer doesn't have available answers");
			onChosen("Error");
			return;
		}
		CreateAnswerOptions(visualData);
		SetSortingOrder(isOverBlackout);
		base.gameObject.SetActive(value: true);
		DisableScrollRectMode();
		contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		(contentSizeFitter.transform as RectTransform).RefreshContentFitter();
		ApplyBubblePosition();
		if (!subscribedGamepadEvents)
		{
			LazyInput.OnInputChanged += HandleGamepadState;
			subscribedGamepadEvents = true;
		}
		HandleGamepadState();
	}

	private void SetSortingOrder(bool isOverBlackout)
	{
		Canvas component = GetComponent<Canvas>();
		component.overrideSorting = true;
		component.sortingOrder = (isOverBlackout ? 900 : 350);
	}

	private void HandleGamepadState()
	{
		if (this == null || base.gameObject == null || answerOptions == null)
		{
			LazyInput.OnInputChanged -= HandleGamepadState;
		}
		else
		{
			if (!base.gameObject.activeSelf || answerOptions.Count == 0)
			{
				return;
			}
			LazyInput.ClearKeyDown(GameKey.Select);
			LazyInput.ClearKey(GameKey.Select);
			if (LazyInput.IsGamepadActive)
			{
				gamepadController.Enable();
				if (isScrollModeActive)
				{
					autoScroll.enabled = true;
					autoScroll.SkipNextAutoscroll = true;
				}
				gamepadController.ReinitItems(focusOnFirstActive: false);
				{
					foreach (UIMultiAnswerOption answerOption in answerOptions)
					{
						if (!(answerOption == null))
						{
							gamepadController.SetFocusedItem(answerOption.Item);
							break;
						}
					}
					return;
				}
			}
			autoScroll.enabled = false;
			scrollRect.DOKill();
			gamepadController.Disable();
		}
	}

	private void ApplyBubblePosition()
	{
		Vector2 vector = CameraSystem.WorldToScreenPoint(targetTransform.position);
		if (answerOptions.IsNullOrEmpty())
		{
			Debug.LogError("Answers options are empty");
			return;
		}
		float bubbleWidthScreen;
		RectTransform ul;
		RectTransform ur;
		RectTransform dl;
		RectTransform dr;
		float num = CalculateBubbleHeightScreen(out bubbleWidthScreen, out ul, out ur, out dl, out dr);
		float num2 = 0f;
		for (int i = 0; i < answerOptions.Count; i++)
		{
			float extraWidth = answerOptions[i].GetExtraWidth();
			if (extraWidth > num2)
			{
				num2 = extraWidth;
			}
		}
		float maxSideDisplayIconsOffsetLocal = GetMaxSideDisplayIconsOffsetLocal(leftIcons: true);
		float maxSideDisplayIconsOffsetLocal2 = GetMaxSideDisplayIconsOffsetLocal(leftIcons: false);
		DecideBubbleOrientation(vector, num, out var needsScroll, out var useSideDisplay);
		isScrollModeActive = needsScroll;
		isSideDisplayActive = useSideDisplay;
		bool isLeftSide = (useSideDisplay ? DecideHorizontalSide(vector, bubbleWidthScreen, num2, sideDisplayHorizontalOffset, maxSideDisplayIconsOffsetLocal, maxSideDisplayIconsOffsetLocal2) : IsLeftSide(vector, bubbleWidthScreen, num2));
		float heightScreen = num;
		if (needsScroll)
		{
			ApplyScrollRootPadding();
			float availableHeightScreen = GetAvailableHeightScreen(vector, opensDownward);
			float a = CalculateScrollViewportHeightLocal(availableHeightScreen);
			float b = num / LazyUI.ScaleFactor;
			a = Mathf.Min(a, b);
			heightScreen = availableHeightScreen;
			EnableScrollRectMode(a);
			RebuildScrollLayout();
			ApplyViewportHeight(a);
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
			ApplyInitialScrollPosition();
		}
		Vector2 vector2 = vector;
		float num3;
		if (isSideDisplayActive)
		{
			float sideDisplayIconsOffsetScreen = GetSideDisplayIconsOffsetScreen(isLeftSide, maxSideDisplayIconsOffsetLocal, maxSideDisplayIconsOffsetLocal2);
			vector2.x = ApplySideDisplayHorizontalOffset(vector.x, isLeftSide, sideDisplayHorizontalOffset + sideDisplayIconsOffsetScreen);
			vector2.y = GetSideDisplayAnchorScreenY(num, opensDownward);
			num3 = 0f;
		}
		else
		{
			num3 = CalculateScreenClampOffsetY(vector2, heightScreen, opensDownward);
		}
		Vector2 anchorRelativePosition = GetAnchorRelativePosition(needsScroll, isLeftSide, ul, ur, dl, dr);
		base.transform.position = new Vector2(0f, 0f - num3) + vector2 - anchorRelativePosition * LazyUI.ScaleFactor;
	}

	private float CalculateBubbleHeightScreen(out float bubbleWidthScreen, out RectTransform ul, out RectTransform ur, out RectTransform dl, out RectTransform dr)
	{
		UIMultiAnswerOption uIMultiAnswerOption = answerOptions[0];
		UIMultiAnswerOption uIMultiAnswerOption2 = answerOptions.Last();
		RectTransform boundsRectTransform = uIMultiAnswerOption.GetBoundsRectTransform();
		RectTransform boundsRectTransform2 = uIMultiAnswerOption2.GetBoundsRectTransform();
		ul = uIMultiAnswerOption.GetCornerTransform(UIBasicBubble.BubbleCornerDirection.LeftUp);
		ur = uIMultiAnswerOption.GetCornerTransform(UIBasicBubble.BubbleCornerDirection.RightUp);
		dl = uIMultiAnswerOption2.GetCornerTransform(UIBasicBubble.BubbleCornerDirection.LeftDown);
		dr = uIMultiAnswerOption2.GetCornerTransform(UIBasicBubble.BubbleCornerDirection.RightDown);
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		boundsRectTransform.GetWorldCorners(array);
		boundsRectTransform2.GetWorldCorners(array2);
		Vector2 vector = base.transform.InverseTransformPoint(array[1]);
		Vector2 vector2 = base.transform.InverseTransformPoint(array[2]);
		Vector2 vector3 = base.transform.InverseTransformPoint(array2[0]);
		float num = vector.y - vector3.y;
		float num2 = vector2.x - vector.x;
		bubbleWidthScreen = num2 * LazyUI.ScaleFactor;
		return num * LazyUI.ScaleFactor;
	}

	private static float GetAvailableHeightScreen(Vector2 screenPos, bool opensDown)
	{
		float result = (float)Screen.height - screenPos.y;
		float y = screenPos.y;
		if (!opensDown)
		{
			return y;
		}
		return result;
	}

	private static bool IsLeftSide(Vector2 screenPos, float bubbleWidthScreen, float extraWidth)
	{
		return screenPos.x + bubbleWidthScreen + extraWidth * LazyUI.ScaleFactor < (float)Screen.width;
	}

	private void DecideBubbleOrientation(Vector2 screenPos, float bubbleHeightScreen, out bool needsScroll, out bool useSideDisplay)
	{
		useSideDisplay = false;
		if (overflowMode == OverflowMode.FullDisplayOnSide)
		{
			DecideFullDisplayOnSideOrientation(screenPos, bubbleHeightScreen, out needsScroll, out useSideDisplay);
		}
		else
		{
			DecideScrollOrientation(screenPos, bubbleHeightScreen, out needsScroll);
		}
	}

	private void DecideFullDisplayOnSideOrientation(Vector2 screenPos, float bubbleHeightScreen, out bool needsScroll, out bool useSideDisplay)
	{
		useSideDisplay = false;
		if (bubbleHeightScreen > (float)Screen.height)
		{
			DecideScrollOrientation(screenPos, bubbleHeightScreen, out needsScroll);
			return;
		}
		float num = (float)Screen.height - screenPos.y;
		float y = screenPos.y;
		switch (forcedCornerPosition)
		{
		case UIBasicBubble.ForceCornerPosition.BottomRight:
		case UIBasicBubble.ForceCornerPosition.BottomLeft:
			if (bubbleHeightScreen <= num)
			{
				opensDownward = true;
				needsScroll = false;
			}
			else if (bubbleHeightScreen <= y)
			{
				opensDownward = false;
				needsScroll = false;
			}
			else
			{
				useSideDisplay = true;
				opensDownward = true;
				needsScroll = false;
			}
			break;
		case UIBasicBubble.ForceCornerPosition.Auto:
			if (bubbleHeightScreen <= num)
			{
				opensDownward = true;
				needsScroll = false;
			}
			else if (bubbleHeightScreen <= y)
			{
				opensDownward = false;
				needsScroll = false;
			}
			else
			{
				useSideDisplay = true;
				opensDownward = true;
				needsScroll = false;
			}
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void DecideScrollOrientation(Vector2 screenPos, float bubbleHeightScreen, out bool needsScroll)
	{
		float num = (float)Screen.height - screenPos.y;
		float y = screenPos.y;
		switch (forcedCornerPosition)
		{
		case UIBasicBubble.ForceCornerPosition.BottomRight:
		case UIBasicBubble.ForceCornerPosition.BottomLeft:
			opensDownward = true;
			needsScroll = bubbleHeightScreen > num;
			break;
		case UIBasicBubble.ForceCornerPosition.Auto:
			if (bubbleHeightScreen <= num)
			{
				opensDownward = true;
				needsScroll = false;
			}
			else if (bubbleHeightScreen <= y)
			{
				opensDownward = false;
				needsScroll = false;
			}
			else
			{
				opensDownward = true;
				needsScroll = true;
			}
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private float GetMaxSideDisplayIconsOffsetLocal(bool leftIcons)
	{
		float num = 0f;
		for (int i = 0; i < answerOptions.Count; i++)
		{
			float num2 = (leftIcons ? answerOptions[i].GetLeftIconsExtraWidth() : answerOptions[i].GetRightIconsExtraWidth());
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	private static float GetSideDisplayIconsOffsetScreen(bool isLeftSide, float maxLeftIconsOffsetLocal, float maxRightIconsOffsetLocal)
	{
		return (isLeftSide ? maxLeftIconsOffsetLocal : maxRightIconsOffsetLocal) * LazyUI.ScaleFactor;
	}

	private static bool DecideHorizontalSide(Vector2 screenPos, float bubbleWidthScreen, float extraWidth, float horizontalOffset, float maxLeftIconsOffsetLocal, float maxRightIconsOffsetLocal)
	{
		float num = bubbleWidthScreen + extraWidth * LazyUI.ScaleFactor;
		float num2 = horizontalOffset + maxLeftIconsOffsetLocal * LazyUI.ScaleFactor;
		float num3 = horizontalOffset + maxRightIconsOffsetLocal * LazyUI.ScaleFactor;
		bool num4 = screenPos.x + num2 + num <= (float)Screen.width;
		bool flag = screenPos.x - num3 - num >= 0f;
		if (num4)
		{
			return true;
		}
		if (flag)
		{
			return false;
		}
		float num5 = (float)Screen.width - screenPos.x - num2;
		float num6 = screenPos.x - num3;
		return num5 >= num6;
	}

	private static float ApplySideDisplayHorizontalOffset(float npcScreenX, bool isLeftSide, float horizontalOffset)
	{
		if (!isLeftSide)
		{
			return npcScreenX - horizontalOffset;
		}
		return npcScreenX + horizontalOffset;
	}

	private static float GetSideDisplayAnchorScreenY(float bubbleHeightScreen, bool opensDown)
	{
		float num = (float)Screen.height * 0.5f;
		if (!opensDown)
		{
			return num + bubbleHeightScreen * 0.5f;
		}
		return num - bubbleHeightScreen * 0.5f;
	}

	private static RectTransform GetActiveCorner(bool opensDown, bool isLeftSide, RectTransform ul, RectTransform ur, RectTransform dl, RectTransform dr)
	{
		if (opensDown)
		{
			if (!isLeftSide)
			{
				return dr;
			}
			return dl;
		}
		if (!isLeftSide)
		{
			return ur;
		}
		return ul;
	}

	private Vector3 GetViewportAnchorWorldPosition(bool isLeftSide)
	{
		RectTransform viewport = scrollRect.viewport;
		Vector3[] array = new Vector3[4];
		viewport.GetWorldCorners(array);
		if (opensDownward)
		{
			if (!isLeftSide)
			{
				return array[3];
			}
			return array[0];
		}
		if (!isLeftSide)
		{
			return array[2];
		}
		return array[1];
	}

	private Vector2 GetAnchorRelativePosition(bool needsScroll, bool isLeftSide, RectTransform ul, RectTransform ur, RectTransform dl, RectTransform dr)
	{
		Vector3 position = GetActiveCorner(opensDownward, isLeftSide, ul, ur, dl, dr).position;
		if (needsScroll)
		{
			Vector3 viewportAnchorWorldPosition = GetViewportAnchorWorldPosition(isLeftSide);
			position = new Vector3(position.x, viewportAnchorWorldPosition.y, position.z);
		}
		return base.transform.InverseTransformPoint(position);
	}

	private void RebuildScrollLayout()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.transform as RectTransform);
		Canvas.ForceUpdateCanvases();
	}

	private float CalculateScrollViewportHeightLocal(float availableHeightScreen)
	{
		float num = availableHeightScreen - 20f;
		return Mathf.Max(0f, num / LazyUI.ScaleFactor);
	}

	private static float CalculateScreenClampOffsetY(Vector2 anchorScreenPos, float heightScreen, bool opensDown)
	{
		if (opensDown)
		{
			float b = anchorScreenPos.y + heightScreen - (float)Screen.height;
			return Mathf.Max(0f, b);
		}
		float b2 = anchorScreenPos.y - heightScreen;
		return Mathf.Min(0f, b2);
	}

	private void CacheRootPaddingDefaults()
	{
		if (rootPaddingTopDefault < 0)
		{
			VerticalLayoutGroup verticalLayoutGroup = RootVerticalLayoutGroup;
			rootPaddingTopDefault = verticalLayoutGroup.padding.top;
			rootPaddingBottomDefault = verticalLayoutGroup.padding.bottom;
		}
	}

	private void ApplyScrollRootPadding()
	{
		CacheRootPaddingDefaults();
		VerticalLayoutGroup verticalLayoutGroup = RootVerticalLayoutGroup;
		if (opensDownward)
		{
			verticalLayoutGroup.padding.top = 0;
		}
		else
		{
			verticalLayoutGroup.padding.bottom = 0;
		}
	}

	private void RestoreScrollRootPadding()
	{
		if (rootPaddingTopDefault >= 0)
		{
			VerticalLayoutGroup verticalLayoutGroup = RootVerticalLayoutGroup;
			verticalLayoutGroup.padding.top = rootPaddingTopDefault;
			verticalLayoutGroup.padding.bottom = rootPaddingBottomDefault;
		}
	}

	private void ApplyViewportHeight(float heightLocal)
	{
		scrollViewportLayoutElement.minHeight = heightLocal;
		scrollViewportLayoutElement.preferredHeight = heightLocal;
		scrollViewportLayoutElement.flexibleHeight = 0f;
		scrollRect.viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, heightLocal);
		LayoutElement component = scrollRect.GetComponent<LayoutElement>();
		if (component != null)
		{
			component.minHeight = heightLocal;
			component.preferredHeight = heightLocal;
			component.flexibleHeight = 0f;
		}
	}

	private void ApplyInitialScrollPosition()
	{
		scrollRect.DOKill();
		scrollRect.verticalNormalizedPosition = 1f;
		autoScroll.SkipNextAutoscroll = true;
	}

	private void ResetRectMaskPadding()
	{
		if (!(rectMask2D == null))
		{
			rectMask2D.padding = Vector4.zero;
		}
	}

	private List<AnswerVisualData> FormVisibleAnswers(List<AnswerVisualData> answers)
	{
		List<AnswerVisualData> list = new List<AnswerVisualData>();
		foreach (AnswerVisualData answer in answers)
		{
			if (!string.IsNullOrEmpty(answer.id))
			{
				KnowledgeSystem knowledgeSystem = MainGame.Instance.GameSave.knowledgeSystem;
				if ((!answer.hiddenByDefault || knowledgeSystem.unlockedPhrases.Contains(answer.id)) && !knowledgeSystem.blackListPhrases.Contains(answer.id))
				{
					list.Add(answer);
				}
			}
		}
		return list;
	}

	private void CreateAnswerOptions(List<AnswerVisualData> visualData)
	{
		answerOptions = new List<UIMultiAnswerOption>();
		float num = 0.3f / (float)visualData.Count;
		StringBuilder stringBuilder = new StringBuilder();
		float elementsSize = 0f;
		for (int i = 0; i < visualData.Count; i++)
		{
			stringBuilder.Clear();
			stringBuilder.Append("#").Append(i).Append(": ")
				.Append(visualData[i].id);
			UIMultiAnswerOption answerOption = answerOptionPrefab.Copy(null, activate: true, stringBuilder.ToString());
			answerOption.Show(visualData[i], this);
			if (i == visualData.Count - 1)
			{
				bottomOption = answerOption;
			}
			if (i > 0)
			{
				LazyTimer.AddTimer(0f, delegate
				{
					elementsSize += answerOption.gameObject.GetComponent<RectTransform>().sizeDelta.y;
				});
			}
			answerOption.Canvas.alpha = 0f;
			float animationDelay = num * (float)(visualData.Count - 1 - i);
			if (animationDelay <= 0f)
			{
				answerOption.AnimateAppearing(0.3f);
			}
			else
			{
				LazyTimer.AddTimer(animationDelay, delegate
				{
					answerOption.AnimateAppearing(0.3f - animationDelay);
				});
			}
			answerOptions.Add(answerOption);
		}
		ChangeInteractable(isInteractable: false);
		LazyTimer.AddTimer(0.3f, delegate
		{
			ChangeInteractable(isInteractable: true);
			if (LazyInput.IsGamepadActive)
			{
				HandleGamepadState();
			}
		});
		LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
		foreach (UIMultiAnswerOption answerOption2 in answerOptions)
		{
			answerOption2.ShowIcons();
		}
		for (int j = 0; j < answerOptions.Count; j++)
		{
			answerOptions[j].UpdateAnswerPosition();
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
	}

	protected void OnDestroy()
	{
		if (subscribedGamepadEvents)
		{
			LazyInput.OnInputChanged -= HandleGamepadState;
			subscribedGamepadEvents = false;
		}
	}

	private void ChangeInteractable(bool isInteractable)
	{
		interactable = isInteractable;
		foreach (UIMultiAnswerOption answerOption in answerOptions)
		{
			answerOption.ChangeInteractableByAnimation(isInteractable);
		}
	}

	private void StartDisappearAnimation()
	{
		ChangeInteractable(isInteractable: false);
		gamepadController.Disable();
		canvas.DOFade(0f, 0.2f).OnComplete(DisableBubble);
	}

	private void DisableBubble()
	{
		LazyInput.OnInputChanged -= HandleGamepadState;
		onDisappeared?.Invoke();
		multiAnswers.Remove(this);
		if (subscribedGamepadEvents)
		{
			LazyInput.OnInputChanged -= HandleGamepadState;
			subscribedGamepadEvents = false;
		}
		CanvasGroup[] componentsInChildren = GetComponentsInChildren<CanvasGroup>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].DOKill();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if (LazyInput.IsGamepadActive && interactable && !LazyWindowsStackController.HasAnyModalWindowOpened)
		{
			if (LazyInput.GetKeyDown(GameKey.Up))
			{
				LazyInput.ClearKeyDown(GameKey.Up);
				gamepadController.Navigate(GUIDirection.Up);
			}
			if (LazyInput.GetKeyDown(GameKey.Down))
			{
				LazyInput.ClearKeyDown(GameKey.Down);
				gamepadController.Navigate(GUIDirection.Down);
			}
			if (LazyInput.GetKeyDown(GameKey.Right))
			{
				LazyInput.ClearKeyDown(GameKey.Right);
				gamepadController.Navigate(GUIDirection.Right);
			}
			if (LazyInput.GetKeyDown(GameKey.Left))
			{
				LazyInput.ClearKeyDown(GameKey.Left);
				gamepadController.Navigate(GUIDirection.Left);
			}
			if (LazyInput.GetKeyDown(GameKey.DpadUp))
			{
				LazyInput.ClearKeyDown(GameKey.DpadUp);
				gamepadController.Navigate(GUIDirection.Up);
			}
			if (LazyInput.GetKeyDown(GameKey.DpadDown))
			{
				LazyInput.ClearKeyDown(GameKey.DpadDown);
				gamepadController.Navigate(GUIDirection.Down);
			}
			if (LazyInput.GetKeyDown(GameKey.DpadRight))
			{
				LazyInput.ClearKeyDown(GameKey.DpadRight);
				gamepadController.Navigate(GUIDirection.Right);
			}
			if (LazyInput.GetKeyDown(GameKey.DpadLeft))
			{
				LazyInput.ClearKeyDown(GameKey.DpadLeft);
				gamepadController.Navigate(GUIDirection.Left);
			}
			if (LazyInput.GetKeyDown(GameKey.Select))
			{
				LazyInput.ClearAllKeysDown();
				gamepadController.SelectFocusedItem();
			}
		}
	}

	private void HandleClosedAllWindows()
	{
		SetControlState(isControllable: true);
	}

	private void SetControlState(bool isControllable)
	{
		canvas.interactable = isControllable;
		this.isControllable = isControllable;
	}

	private void DisableScrollRectMode()
	{
		isScrollModeActive = false;
		isSideDisplayActive = false;
		scrollRect.enabled = false;
		scrollRect.vertical = false;
		autoScroll.enabled = false;
		scrollViewportLayoutElement.minHeight = 0f;
		scrollViewportLayoutElement.preferredHeight = -1f;
		scrollViewportLayoutElement.flexibleHeight = -1f;
		LayoutElement component = scrollRect.GetComponent<LayoutElement>();
		if (component != null)
		{
			component.minHeight = -1f;
			component.preferredHeight = -1f;
			component.flexibleHeight = -1f;
		}
		contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		RestoreScrollRootPadding();
		ResetRectMaskPadding();
	}

	private void EnableScrollRectMode(float viewportHeightLocal)
	{
		isScrollModeActive = true;
		scrollRect.vertical = true;
		scrollRect.enabled = true;
		autoScroll.enabled = LazyInput.IsGamepadActive;
		contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
		ApplyViewportHeight(viewportHeightLocal);
	}

	public static void ShowAnswers(List<AnswerVisualData> answers, Transform targetTransform, WgoData dialogParticipant, Action<string> onChosen, Action onDisappeared, UIBasicBubble.ForceCornerPosition forceCornerPosition = UIBasicBubble.ForceCornerPosition.Auto, bool isMainMultianswer = false, bool isOverBlackout = false)
	{
		if (instance == null)
		{
			Debug.LogError("MultiAnswer.ShowAnswers error: instance is null");
			return;
		}
		UIMultiAnswer uIMultiAnswer = instance.Copy();
		uIMultiAnswer.targetTransform = targetTransform;
		uIMultiAnswer.dialogParticipant = dialogParticipant;
		uIMultiAnswer.onChosen = onChosen;
		uIMultiAnswer.onDisappeared = onDisappeared;
		uIMultiAnswer.isMainMultianswer = isMainMultianswer;
		uIMultiAnswer.ShowAnswers(answers, forceCornerPosition, isOverBlackout);
		multiAnswers.Add(uIMultiAnswer);
	}
}
