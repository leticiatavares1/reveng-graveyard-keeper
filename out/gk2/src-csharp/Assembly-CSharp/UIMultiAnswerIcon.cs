using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiAnswerIcon : MonoBehaviour
{
	public enum DisplayType
	{
		Price,
		Reward,
		Lock,
		DayNumber,
		Order
	}

	private static string PLACEHOLDER_ICON_NAME = "placeholder_icon";

	[SerializeField]
	private LazyButton lazyButton;

	[SerializeField]
	private GameObject iconGameObject;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image backImage;

	[SerializeField]
	private Image decorImage;

	[SerializeField]
	private Image arrowImage;

	[SerializeField]
	private Image itemStar;

	[SerializeField]
	private TextMeshProUGUI countLabel;

	[SerializeField]
	private TextMeshProUGUI textRes;

	[SerializeField]
	private TextStyleComponent textStyleComponentRes;

	[SerializeField]
	private TextStyleComponent textStyleComponentItemCount;

	[SerializeField]
	private TextStyle availableTextStyle;

	[SerializeField]
	private TextStyle unavailableTextStyle;

	[SerializeField]
	private Color normalItemIconColor;

	[SerializeField]
	private Color selectedItemIconColor;

	[SerializeField]
	private Color normalDecorColor;

	[SerializeField]
	private Color selectedDecorColor;

	[SerializeField]
	private Color normalArrowColor;

	[SerializeField]
	private Color selectedArrowColor;

	private bool isHovered;

	private ItemCount itemCount;

	private VendorOrderDef vendorOrderDef;

	private List<Tween> activeColorTweens = new List<Tween>();

	public ItemCount ItemCount => itemCount;

	public VendorOrderDef VendorOrderDef => vendorOrderDef;

	private void Awake()
	{
		lazyButton.onEnter.AddListener(OnOver);
		lazyButton.onExit.AddListener(OnOut);
		lazyButton.SetCallbacksIntoGamepadNavigationItem();
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Setup(ItemCount item, DisplayType displayType)
	{
		lazyButton.interactable = true;
		iconGameObject.SetActive(value: true);
		textRes.gameObject.SetActive(value: false);
		itemCount = item;
		bool active = item.Def.qualityType == ItemDef.QualityType.Star && item.Def.quality > 0;
		Sprite sprite2 = (iconImage.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(item.Def.iconId, PLACEHOLDER_ICON_NAME));
		bool active2 = sprite2;
		itemStar.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("item_star_" + item.Def.quality);
		itemStar.gameObject.SetActive(active);
		if (displayType == DisplayType.Reward)
		{
			textStyleComponentItemCount.SetTextStyle(availableTextStyle);
		}
		else
		{
			textStyleComponentItemCount.SetTextStyle((MainGame.PlayerData.Inventory.Data.GetTotalCountInInventory(item.itemId) >= item.count) ? availableTextStyle : unavailableTextStyle);
		}
		int totalCountInInventory = MainGame.PlayerData.Inventory.Data.GetTotalCountInInventory(item.itemId);
		totalCountInInventory = ((totalCountInInventory > itemCount.count) ? itemCount.count : totalCountInInventory);
		countLabel.text = string.Empty;
		switch (displayType)
		{
		case DisplayType.Price:
			countLabel.text += $"{totalCountInInventory}/{item.count}";
			countLabel.gameObject.SetActive(value: true);
			break;
		case DisplayType.Lock:
			countLabel.text += $"{totalCountInInventory}/{item.count}";
			countLabel.gameObject.SetActive(value: true);
			break;
		case DisplayType.Reward:
			countLabel.text = $"{item.count}";
			countLabel.gameObject.SetActive(active2);
			break;
		default:
			countLabel.gameObject.SetActive(active2);
			break;
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

	public void ChangeColor(bool selected)
	{
		KillColorTweens();
		activeColorTweens.Add(decorImage.DOColor(selected ? selectedDecorColor : normalDecorColor, 0.5f));
		activeColorTweens.Add(backImage.DOColor(selected ? selectedArrowColor : normalArrowColor, 0.5f));
		if (arrowImage != null)
		{
			activeColorTweens.Add(arrowImage.DOColor(selected ? selectedArrowColor : normalArrowColor, 0.5f));
		}
	}

	public void Setup(GameResAtom res, DisplayType displayType)
	{
		lazyButton.interactable = false;
		iconGameObject.SetActive(value: false);
		textRes.gameObject.SetActive(value: true);
		float res2 = MainGame.PlayerData.GetRes(res.type);
		textRes.text = res.ToFormattedString(showOnlyType: false, delegate(string s, string s1)
		{
			if (displayType == DisplayType.Reward)
			{
				s1 = "+" + s1;
			}
			return s + "\n" + s1;
		});
		if (displayType != DisplayType.Reward)
		{
			textStyleComponentRes.SetTextStyle((res2 >= res.value) ? availableTextStyle : unavailableTextStyle);
		}
		else
		{
			textStyleComponentRes.SetTextStyle(availableTextStyle);
		}
	}

	public void SetupAsDay(string dayNumber, DisplayType displayType)
	{
		lazyButton.interactable = true;
		iconGameObject.SetActive(value: true);
		textRes.gameObject.SetActive(value: false);
		iconImage.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(dayNumber, PLACEHOLDER_ICON_NAME);
		textStyleComponentItemCount.SetTextStyle((MainGame.Instance.GameSave.environmentData.CurrentDayNumber == ConstDef.Get(dayNumber).IntValue) ? availableTextStyle : unavailableTextStyle);
		countLabel.text = string.Empty;
	}

	public void SetupAsOrder(string order, DisplayType displayType)
	{
		vendorOrderDef = GameBalance.Me.GetData<VendorOrderDef>(order);
		lazyButton.interactable = true;
		iconGameObject.SetActive(value: true);
		textRes.gameObject.SetActive(value: false);
		ItemDef data = GameBalance.Me.GetData<ItemDef>(vendorOrderDef.itemId);
		iconImage.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(data.iconId, PLACEHOLDER_ICON_NAME);
		itemStar.gameObject.SetActive(value: false);
		countLabel.gameObject.SetActive(value: false);
		textStyleComponentItemCount.SetTextStyle(MainGame.Instance.GameSave.vendorSystem.IsOrderFinished(order) ? availableTextStyle : unavailableTextStyle);
		countLabel.text = string.Empty;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnOver()
	{
		if (lazyButton.interactable)
		{
			ShowUITooltip();
			iconImage.BlueColorReplace(selectedItemIconColor);
			ChangeColor(selected: true);
		}
	}

	private void OnOut()
	{
		if (lazyButton.interactable)
		{
			HideUITooltip();
			iconImage.BlueColorReplace(normalItemIconColor);
			ChangeColor(selected: false);
		}
	}

	private void OnDisable()
	{
		if (isHovered)
		{
			if (UITooltip.IsTooltipShowingAtTarget(base.transform as RectTransform))
			{
				HideUITooltip(immediately: true);
			}
			iconImage.BlueColorReplace(normalItemIconColor);
			decorImage.color = normalDecorColor;
		}
	}

	public void ShowUITooltip()
	{
		isHovered = true;
		UITooltip.ShowMultiAnswerIcon(this);
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
}
