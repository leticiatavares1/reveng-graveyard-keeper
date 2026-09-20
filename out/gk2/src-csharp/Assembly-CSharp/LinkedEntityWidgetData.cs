using System;
using LazyBearTechnology;
using UnityEngine;

public class LinkedEntityWidgetData : LazyWidgetDataBase
{
	public Sprite Icon { get; private set; }

	public string FontIcon { get; private set; }

	public Sprite QualityIcon { get; private set; }

	public LinkedEntityType LinkedEntityType { get; private set; }

	public CraftDef CraftDef { get; private set; }

	public AlchemyFormulaDef AlchemyFormulaDef { get; private set; }

	public ItemDef ItemDef { get; private set; }

	public Item Item { get; private set; }

	public VendorOrderDef VendorOrderDef { get; private set; }

	public GameRes GameRes { get; private set; }

	public BuildingDef BuildingDef { get; private set; }

	public TownBuildingDef TownBuildingDef { get; private set; }

	public PerkDef PerkDef { get; private set; }

	public Action OnClicked { get; set; }

	public string LabelText { get; private set; }

	public TextStyle LabelCustomStyle { get; set; }

	public bool NoSelectionFrames { get; set; }

	public bool NotShowStudyWidgetInItemTooltips { get; set; }

	public bool ShowCraftedAtFromCraftDefInsteadOfItem { get; set; }

	public bool IsInactive { get; set; }

	public LinkedEntityWidgetData(CraftDef craftDef, Action onClicked)
	{
		ItemDef itemDef = craftDef.TryGetResultingItemDef();
		string craftResultIcon = craftDef.GetCraftResultIcon();
		if (string.IsNullOrEmpty(craftResultIcon))
		{
			if (itemDef != null)
			{
				Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(itemDef.iconId);
			}
			else
			{
				Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(craftDef.GetOutputPreview().IconId);
			}
		}
		else
		{
			Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(craftResultIcon);
		}
		LinkedEntityType = LinkedEntityType.CraftDef;
		CraftDef = craftDef;
		QualityIcon = null;
		OnClicked = onClicked;
		if (!craftDef.isStarCraft && itemDef != null && itemDef.qualityType == ItemDef.QualityType.Star)
		{
			QualityIcon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("item_star_" + itemDef.quality);
		}
	}

	public LinkedEntityWidgetData(AlchemyFormulaDef alchemyFormulaDef, Action onClicked)
	{
		ItemDef itemDef = alchemyFormulaDef.ItemDef;
		Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(itemDef.iconId);
		LinkedEntityType = LinkedEntityType.AlchemyFormula;
		AlchemyFormulaDef = alchemyFormulaDef;
		QualityIcon = null;
		OnClicked = onClicked;
	}

	public LinkedEntityWidgetData(ItemDef itemDef, Action onClicked)
	{
		Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(itemDef.iconId);
		QualityIcon = null;
		ItemDef = itemDef;
		LinkedEntityType = LinkedEntityType.ItemDef;
		if (itemDef.qualityType == ItemDef.QualityType.Star)
		{
			QualityIcon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("item_star_" + itemDef.quality);
		}
		OnClicked = onClicked;
	}

	public LinkedEntityWidgetData(Item item, Action onClicked)
	{
		Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(item.Definition.iconId);
		QualityIcon = null;
		Item = item;
		LinkedEntityType = LinkedEntityType.Item;
		if (item.Definition.qualityType == ItemDef.QualityType.Star)
		{
			QualityIcon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("item_star_" + item.Definition.quality);
		}
		if (item.Count > 1)
		{
			LabelText = item.Count.ToString();
		}
		OnClicked = onClicked;
	}

	public LinkedEntityWidgetData(string res, int count, Action onClicked)
	{
		Icon = null;
		GameRes = new GameRes(res, count);
		FontIcon = GameRes.ToFormattedString(showOnlyType: false, (string s, string s1) => s + s1);
		QualityIcon = null;
		LinkedEntityType = LinkedEntityType.GameRes;
		OnClicked = onClicked;
	}

	public LinkedEntityWidgetData(string dayNumber, Action onClicked)
	{
		Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(dayNumber);
		QualityIcon = null;
		LinkedEntityType = LinkedEntityType.DayNumber;
		OnClicked = onClicked;
	}

	public LinkedEntityWidgetData(VendorOrderDef vendorOrder, Action onClicked)
	{
		VendorOrderDef = vendorOrder;
		ItemDef = GameBalance.Me.GetData<ItemDef>(vendorOrder.itemId);
		Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(ItemDef.iconId);
		QualityIcon = null;
		LinkedEntityType = LinkedEntityType.Order;
		OnClicked = onClicked;
	}

	public LinkedEntityWidgetData(BuildingDef buildingDef, Action onClicked, int count = 1, bool isInactive = false)
	{
		Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(buildingDef.BuildResultIcon);
		BuildingDef = buildingDef;
		LinkedEntityType = LinkedEntityType.BuildingDef;
		QualityIcon = null;
		OnClicked = onClicked;
		if (count > 1)
		{
			LabelText = count.ToString();
		}
		IsInactive = isInactive;
	}

	public LinkedEntityWidgetData(PerkDef perkDef, Action onClicked)
	{
		Icon = perkDef.Icon;
		PerkDef = perkDef;
		LinkedEntityType = LinkedEntityType.PerkDef;
		QualityIcon = null;
		OnClicked = onClicked;
	}

	public LinkedEntityWidgetData(TownBuildingDef townBuildingDef, Action onClicked)
	{
		Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(townBuildingDef.BuildResultIcon);
		TownBuildingDef = townBuildingDef;
		LinkedEntityType = LinkedEntityType.TownBuildingDef;
		QualityIcon = null;
		OnClicked = onClicked;
	}

	public string GetLinkedEntityPrefix()
	{
		switch (LinkedEntityType)
		{
		case LinkedEntityType.CraftDef:
		{
			if (CraftDef.id.StartsWith("fake_"))
			{
				return LLBase.L("ui_create");
			}
			ItemDef itemDef = CraftDef.TryGetResultingItemDef();
			if (itemDef != null)
			{
				return itemDef.GetItemCraftPrefix();
			}
			return string.Empty;
		}
		case LinkedEntityType.ItemDef:
			return ItemDef.GetItemCraftPrefix();
		case LinkedEntityType.Item:
			return Item.Definition.GetItemCraftPrefix();
		case LinkedEntityType.BuildingDef:
			return BuildingDef.GetHeaderPrefix();
		case LinkedEntityType.TownBuildingDef:
			return TownBuildingDef.GetHeaderPrefix();
		case LinkedEntityType.PerkDef:
			return PerkDef.GetHeaderPrefix();
		case LinkedEntityType.AlchemyFormula:
			return AlchemyFormulaDef.ItemDef.GetItemCraftPrefix();
		case LinkedEntityType.Order:
			return LLBase.L("ui_order_header_tooltip");
		default:
			throw new ArgumentOutOfRangeException("LinkedEntityType", LinkedEntityType, null);
		}
	}

	public string GetLinkedEntityHeader()
	{
		switch (LinkedEntityType)
		{
		case LinkedEntityType.CraftDef:
		{
			if (CraftDef.id.StartsWith("fake_"))
			{
				return LLBase.L(CraftDef.id);
			}
			ItemDef itemDef = CraftDef.TryGetResultingItemDef();
			if (itemDef != null)
			{
				return itemDef.GetHeader();
			}
			return LLBase.L(CraftDef.id);
		}
		case LinkedEntityType.ItemDef:
			return ItemDef.GetHeader();
		case LinkedEntityType.Item:
			return Item.Definition.GetHeader();
		case LinkedEntityType.BuildingDef:
			return BuildingDef.GetHeader();
		case LinkedEntityType.TownBuildingDef:
			return TownBuildingDef.GetHeader();
		case LinkedEntityType.PerkDef:
			return PerkDef.GetHeader();
		case LinkedEntityType.AlchemyFormula:
			return AlchemyFormulaDef.ItemDef.GetHeader();
		case LinkedEntityType.Order:
		{
			ItemDef data = GameBalance.Me.GetData<ItemDef>(VendorOrderDef.itemId);
			return $"[{data.GetHeader()}]x{VendorOrderDef.count}";
		}
		default:
			throw new ArgumentOutOfRangeException("LinkedEntityType", LinkedEntityType, null);
		}
	}
}
