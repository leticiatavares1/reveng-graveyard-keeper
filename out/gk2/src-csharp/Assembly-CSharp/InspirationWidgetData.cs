using System;
using LazyBearTechnology;

public class InspirationWidgetData : LazyWidgetDataBase
{
	public int CurrentLevel
	{
		get
		{
			if (InspirationData != null)
			{
				return InspirationData.curLevel;
			}
			return InspirationDef.lvl;
		}
	}

	public int CurrentLevelFrame
	{
		get
		{
			InspirationDef inspirationDef = null;
			if (InspirationData != null)
			{
				inspirationDef = InspirationDef.GetDataForLevel(InspirationData.id, InspirationData.curLevel);
			}
			if (inspirationDef == null)
			{
				inspirationDef = InspirationDef;
			}
			if (inspirationDef.lvlFrame > 0)
			{
				return inspirationDef.lvlFrame;
			}
			return inspirationDef.lvl;
		}
	}

	public string IdWithoutLevel
	{
		get
		{
			if (InspirationData != null)
			{
				return InspirationData.id;
			}
			return InspirationDef.idWithoutLvl;
		}
	}

	public int Price
	{
		get
		{
			if (InspirationData != null)
			{
				return InspirationDef.GetDataForLevel(InspirationData.id, InspirationData.curLevel).completionPrice;
			}
			return InspirationDef.completionPrice;
		}
	}

	public Action OnPress { get; private set; }

	public InspirationData InspirationData { get; private set; }

	public InspirationDef InspirationDef { get; private set; }

	public string TalentExpPointIconId { get; private set; }

	public InspirationWidgetData(InspirationData inspirationData, string talentExpPointIconId = null)
	{
		InspirationData = inspirationData;
		InspirationDef = GameBalance.Me.GetData<InspirationDef>(inspirationData.id + $"_{inspirationData.curLevel}");
		TalentExpPointIconId = talentExpPointIconId;
		OnPress = OnInspirationPress;
	}

	public InspirationWidgetData(InspirationDef inspirationDef)
	{
		InspirationDef = inspirationDef;
		OnPress = null;
	}

	private void OnInspirationPress()
	{
		if (InspirationData != null && MainGame.PlayerData.inventory.Data.HasItemQuantityInInventory("faith", Price))
		{
			MainGame.PlayerData.inventory.Data.RemoveItemFromInventoryById("faith", Price);
			MainGame.Instance.GameSave.talentSystemData.PurchaseInspiration(InspirationData.id);
		}
	}
}
