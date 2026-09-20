using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiAnswerOption : MonoBehaviour
{
	private const float OFFSET_ONE_ICON = 35f;

	private const float MIN_HEIGHT_OPTION = 28f;

	private const float MIN_HEIGHT_OPTION_EXTENDED = 38f;

	private const float PREFERRED_WIDTH_OPTION_DEFAULT = 282f;

	private const int TEXT_PADDING_SIDE_DEFAULT = 9;

	private const float MIN_WIDTH_MID_SECTION_DEFAULT = 130f;

	private const string LOCK_ICON_GREEN = "ui_reply_lock_grn";

	private const string LOCK_ICON_RED = "ui_reply_lock_red";

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private CanvasGroup canvas;

	[SerializeField]
	private CanvasGroup labelCanvas;

	[SerializeField]
	private TextStyleComponent textStyleComponent;

	[SerializeField]
	private TextStyle greyTextStyle;

	[SerializeField]
	private TextStyle hightlightedTextStyle;

	[Space]
	[SerializeField]
	private List<UIBubbleCorner> corners;

	[SerializeField]
	private List<Image> cornerImages;

	[SerializeField]
	private Image optionButtonImage;

	[SerializeField]
	private Color originalColor;

	[SerializeField]
	private Color highlightedColor;

	[SerializeField]
	private LayoutElement optionLayoutElement;

	[SerializeField]
	private LayoutElement midSectionElement;

	[SerializeField]
	private RectTransform contentRectTransform;

	[Space]
	[SerializeField]
	private UIMultiAnswerIconGroup leftIconGroup;

	[SerializeField]
	private UIMultiAnswerIconGroup rightIconGroup;

	[Space]
	[SerializeField]
	private RectTransform leftArrow;

	[SerializeField]
	private RectTransform rightArrow;

	[SerializeField]
	private Image lockIconImage;

	[Space]
	[SerializeField]
	private GamepadNavigationItem gamepadNavigationItem;

	public int iconsCount;

	private RectTransform currentBackground;

	private Image currentBackgroundImage;

	private UIMultiAnswer multiAnswer;

	private AnswerVisualData visualData;

	private bool isOver;

	private bool isLockIcons;

	private bool available = true;

	private bool forceSelectionNoHandle;

	private bool interactableByAnimation = true;

	private List<Tween> activeColorTweens = new List<Tween>();

	public GamepadNavigationItem Item => gamepadNavigationItem;

	public CanvasGroup Canvas => canvas;

	public bool Available => available;

	private void Awake()
	{
		gamepadNavigationItem.SetCallbacks(OnItemOver, OnItemOut, OnItemPress);
	}

	public void Show(AnswerVisualData visualData, UIMultiAnswer multiAnswer)
	{
		label.text = hightlightedTextStyle.TranslateAndColorizeTags(visualData.id);
		this.visualData = visualData;
		this.multiAnswer = multiAnswer;
		foreach (UIBubbleCorner corner in corners)
		{
			corner.gameObject.SetActive(value: false);
		}
		base.gameObject.SetActive(value: true);
	}

	public void ShowIcons()
	{
		iconsCount = 0;
		SmartRes smartRes = visualData?.answerData?.lockRes;
		SmartRes smartRes2 = visualData?.answerData?.costRes;
		string text = visualData?.answerData?.dayNumber;
		string text2 = visualData?.answerData?.order;
		bool flag = !string.IsNullOrEmpty(text);
		bool flag2 = !string.IsNullOrEmpty(text2);
		PlayerData playerData = MainGame.PlayerData;
		int leftIndex;
		if (smartRes != null || smartRes2 != null || flag || flag2)
		{
			int num = 0;
			num += CountItems(smartRes);
			num += CountItems(smartRes2);
			num += (flag ? 1 : 0);
			num += (flag2 ? 1 : 0);
			iconsCount += num;
			leftIconGroup.Show(num);
			leftArrow.gameObject.SetActive(value: false);
			lockIconImage.gameObject.SetActive(smartRes != null || flag || flag2);
			leftIndex = 0;
			ProcessGameRes(smartRes?.gameRes?.List, isLock: true);
			ProcessGameRes(smartRes2?.gameRes?.List, isLock: false);
			ProcessItems(smartRes?.items, isLock: true);
			ProcessItems(smartRes2?.items, isLock: false);
			if (flag)
			{
				leftIconGroup.Icons[leftIndex].SetupAsDay(text, UIMultiAnswerIcon.DisplayType.DayNumber);
				if (MainGame.Instance.GameSave.environmentData.CurrentDayNumber != ConstDef.Get(text).IntValue)
				{
					available = false;
				}
			}
			if (flag2)
			{
				leftIconGroup.Icons[leftIndex].SetupAsOrder(text2, UIMultiAnswerIcon.DisplayType.Order);
				if (!MainGame.Instance.GameSave.vendorSystem.IsOrderFinished(text2))
				{
					available = false;
				}
			}
		}
		else
		{
			AnswerVisualData answerVisualData = visualData;
			if (answerVisualData != null)
			{
				AnswerData answerData = answerVisualData.answerData;
				if (answerData != null && answerData.notAvailable)
				{
					available = false;
				}
			}
			leftIconGroup.HideAll();
			leftArrow.gameObject.SetActive(value: true);
		}
		SmartRes smartRes3 = visualData?.answerData?.rewardRes;
		SmartRes smartRes4 = visualData?.answerData?.fakeRewardRes;
		int rightIndex;
		if (smartRes3 != null || smartRes4 != null)
		{
			int num2 = 0;
			num2 += CountItems(smartRes3);
			num2 += CountItems(smartRes4);
			iconsCount += num2;
			rightIconGroup.Show(num2);
			rightArrow.gameObject.SetActive(value: true);
			rightIndex = 0;
			ProcessGameRes(smartRes3?.gameRes?.List);
			ProcessGameRes(smartRes4?.gameRes?.List);
			ProcessItems(smartRes3?.items);
			ProcessItems(smartRes4?.items);
		}
		else
		{
			rightIconGroup.HideAll();
			rightArrow.gameObject.SetActive(value: false);
		}
		if (!available)
		{
			lockIconImage.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("ui_reply_lock_red");
			textStyleComponent.SetTextStyle(greyTextStyle);
		}
		else
		{
			lockIconImage.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("ui_reply_lock_grn");
		}
		multiAnswer.MaxIconsCount = iconsCount;
		optionLayoutElement.minHeight = ((iconsCount > 0) ? 38f : 28f);
		midSectionElement.minHeight = optionLayoutElement.minHeight;
		optionLayoutElement.preferredWidth = 282f;
		midSectionElement.minWidth = 130f;
		static int CountItems(SmartRes res)
		{
			if (res != null)
			{
				return (res.gameRes?.List?.Count).GetValueOrDefault() + (res.items?.Count ?? 0);
			}
			return 0;
		}
		static int CountItems(SmartRes res)
		{
			if (res != null)
			{
				return (res.gameRes?.List?.Count).GetValueOrDefault() + (res.items?.Count ?? 0);
			}
			return 0;
		}
		void ProcessGameRes(List<GameResAtom> resList, bool isLock)
		{
			if (resList == null)
			{
				return;
			}
			foreach (GameResAtom res in resList)
			{
				UIMultiAnswerIcon uIMultiAnswerIcon2 = leftIconGroup.Icons[leftIndex];
				UIMultiAnswerIcon.DisplayType displayType2 = (isLock ? UIMultiAnswerIcon.DisplayType.Lock : UIMultiAnswerIcon.DisplayType.Price);
				uIMultiAnswerIcon2.Setup(res, displayType2);
				if (!playerData.IsEnoughRes(res))
				{
					available = false;
				}
				leftIndex++;
			}
		}
		void ProcessGameRes(List<GameResAtom> resList)
		{
			if (resList == null)
			{
				return;
			}
			foreach (GameResAtom res2 in resList)
			{
				rightIconGroup.Icons[rightIndex].Setup(res2, UIMultiAnswerIcon.DisplayType.Reward);
				rightIndex++;
			}
		}
		void ProcessItems(List<ItemCount> items, bool isLock)
		{
			if (items == null)
			{
				return;
			}
			foreach (ItemCount item in items)
			{
				UIMultiAnswerIcon uIMultiAnswerIcon = leftIconGroup.Icons[leftIndex];
				UIMultiAnswerIcon.DisplayType displayType = (isLock ? UIMultiAnswerIcon.DisplayType.Lock : UIMultiAnswerIcon.DisplayType.Price);
				uIMultiAnswerIcon.Setup(item, displayType);
				if (!playerData.Inventory.Data.HasItemQuantityInInventory(item.itemId, item.count))
				{
					available = false;
				}
				leftIndex++;
			}
		}
		void ProcessItems(List<ItemCount> items)
		{
			if (items == null)
			{
				return;
			}
			foreach (ItemCount item2 in items)
			{
				rightIconGroup.Icons[rightIndex].Setup(item2, UIMultiAnswerIcon.DisplayType.Reward);
				rightIndex++;
			}
		}
	}

	public void ChangeInteractableByAnimation(bool isInteractable)
	{
		interactableByAnimation = isInteractable;
	}

	public void AnimateAppearing(float animDuration)
	{
		canvas.alpha = 0f;
		labelCanvas.alpha = 1f;
		labelCanvas.ignoreParentGroups = true;
		canvas.DOFade(1f, animDuration).SetEase(Ease.Linear).onComplete = delegate
		{
			labelCanvas.ignoreParentGroups = false;
		};
	}

	public void UpdateAnswerPosition()
	{
	}

	public void OnItemOver()
	{
		if (!isOver)
		{
			isOver = true;
			ChangeColor(highlightedColor);
			Debug.Log($"[ma]:OnItemOver {visualData.id} - {highlightedColor}");
			if (!(multiAnswer != null) || !multiAnswer.IsScrollModeActive)
			{
				contentRectTransform.sizeDelta = new Vector2(284f, contentRectTransform.sizeDelta.y);
				optionLayoutElement.preferredWidth = 284f;
				midSectionElement.minWidth = 132f;
			}
		}
	}

	public void OnItemOut()
	{
		if (isOver)
		{
			isOver = false;
			ChangeColor(originalColor);
			Debug.Log($"[ma]:OnItemOut {visualData.id} - {originalColor}");
			if (!(multiAnswer != null) || !multiAnswer.IsScrollModeActive)
			{
				contentRectTransform.sizeDelta = new Vector2(282f, contentRectTransform.sizeDelta.y);
				optionLayoutElement.preferredWidth = 282f;
				midSectionElement.minWidth = 130f;
			}
		}
	}

	public void OnItemPress()
	{
		if (!forceSelectionNoHandle && (!available || !interactableByAnimation))
		{
			return;
		}
		if (forceSelectionNoHandle)
		{
			multiAnswer.OnAnswerSelect(visualData.id);
			return;
		}
		AnswerData answerData = visualData.answerData;
		if (answerData != null)
		{
			if (answerData.costRes != null)
			{
				HandlePrice(visualData.answerData.costRes);
			}
			if (answerData.rewardRes != null)
			{
				HandleRewards(visualData.answerData.rewardRes);
			}
		}
		Debug.Log("[ma]: " + visualData.id);
		multiAnswer.OnAnswerSelect(visualData.id);
	}

	public void ChangeColor(Color color)
	{
		KillColorTweens();
		activeColorTweens.Add(optionButtonImage.DOColor(color, 0.5f));
		if (rightArrow.gameObject.activeInHierarchy)
		{
			activeColorTweens.Add(rightArrow.GetComponent<Image>().DOColor(color, 1f));
		}
		if (leftArrow.gameObject.activeInHierarchy)
		{
			activeColorTweens.Add(leftArrow.GetComponent<Image>().DOColor(color, 1f));
		}
		foreach (UIBubbleCorner corner in corners)
		{
			if (corner.gameObject.activeInHierarchy)
			{
				activeColorTweens.Add(corner.customImage.DOColor(color, 1f));
			}
		}
	}

	private void OnDestroy()
	{
		KillColorTweens();
	}

	private void KillColorTweens()
	{
		foreach (Tween activeColorTween in activeColorTweens)
		{
			activeColorTween.Kill();
		}
		activeColorTweens.Clear();
	}

	private void HandlePrice(SmartRes priceSmartRes)
	{
		if (priceSmartRes.items != null)
		{
			foreach (ItemCount item in priceSmartRes.items)
			{
				MainGame.PlayerData.Inventory.RemoveItemById(item.itemId, item.count);
			}
		}
		if (priceSmartRes.gameRes != null)
		{
			MainGame.PlayerData.SubRes(visualData.answerData.costRes.gameRes);
		}
	}

	private void HandleRewards(SmartRes rewardSmartRes)
	{
		if (rewardSmartRes.items != null)
		{
			List<Item> list = ItemCount.CreateItems(rewardSmartRes.items);
			if (!MainGame.PlayerData.Inventory.AddItemsToInventory(list))
			{
				foreach (Item item in list)
				{
					MainGame.Instance.dropSystem.DropItem(item, MainGame.PlayerData.currentGameSceneId, MainGame.PlayerController.PlayerData.position.Value);
				}
			}
		}
		if (rewardSmartRes.gameRes != null)
		{
			Debug.Log($"[Is playerdata is null - {MainGame.PlayerData == null}], is gameRes is null - [{visualData.answerData.rewardRes.gameRes == null}]");
			MainGame.PlayerData.AddRes(visualData.answerData.rewardRes.gameRes);
		}
	}

	public float GetExtraWidth()
	{
		return GetRightIconsExtraWidth();
	}

	public float GetLeftIconsExtraWidth()
	{
		return GetActiveIconGroupExtraWidth(leftIconGroup);
	}

	public float GetRightIconsExtraWidth()
	{
		return GetActiveIconGroupExtraWidth(rightIconGroup);
	}

	private static float GetActiveIconGroupExtraWidth(UIMultiAnswerIconGroup iconGroup)
	{
		if (!iconGroup.gameObject.activeInHierarchy)
		{
			return 0f;
		}
		int num = 0;
		foreach (UIMultiAnswerIcon icon in iconGroup.Icons)
		{
			if (icon.gameObject.activeInHierarchy)
			{
				num++;
			}
		}
		if (num != 0)
		{
			return 35f * (float)num + (float)num;
		}
		return 0f;
	}

	public RectTransform GetCornerTransform(UIBasicBubble.BubbleCornerDirection corner)
	{
		return corners[(int)corner].rectTransform;
	}

	public RectTransform GetBoundsRectTransform()
	{
		return (RectTransform)base.transform;
	}
}
