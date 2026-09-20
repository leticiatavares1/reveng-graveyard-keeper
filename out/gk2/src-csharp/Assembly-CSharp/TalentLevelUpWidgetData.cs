using System;
using LazyBearTechnology;
using UnityEngine;

public class TalentLevelUpWidgetData : LazyWidgetDataBase
{
	public Vector2 localPosition;

	public Vector2 upConnectorPos;

	public Vector2 downConnectorPos;

	public Vector2 leftConnectorPos;

	public Vector2 rightConnectorPos;

	public TalentLevelUpDef Def { get; private set; }

	public TalentLevelUpDef.State State { get; private set; }

	public Action OnPress { get; private set; }

	public Action OnOver { get; private set; }

	public Action OnOut { get; private set; }

	public ZombieWgoData ZombieWgoData { get; private set; }

	public string PriceIconId { get; private set; }

	public TalentLevelUpWidgetData(TalentLevelUpDef def, TalentLevelUpDef.State state, string priceIconId)
	{
		Def = def;
		State = state;
		PriceIconId = priceIconId;
		this.OnPress = OnPress;
		void OnPress()
		{
			TalentSystemData talentSystemData = MainGame.Instance.GameSave.talentSystemData;
			if (talentSystemData.CanPurchaseLevel(def.id, out var _))
			{
				talentSystemData.PurchaseLevel(def.id);
				LazyAudio.PlayAndForget("coins_sound");
			}
		}
	}

	public TalentLevelUpWidgetData(TalentLevelUpDef def, TalentLevelUpDef.State state, ZombieWgoData zombieWgoData, Action onQueueChanged)
	{
		TalentLevelUpWidgetData talentLevelUpWidgetData = this;
		ZombieWgoData = zombieWgoData;
		Def = def;
		State = state;
		this.OnPress = OnPress;
		void OnPress()
		{
			if (!zombieWgoData.IsTalentLevelUpStudied(def.id))
			{
				if (talentLevelUpWidgetData.ZombieWgoData.IsParentsUnlockedForTalentLevelUp(def) && talentLevelUpWidgetData.ZombieWgoData.IsEnoughResourcesToBuyTalentLevelUp(def) && talentLevelUpWidgetData.ZombieWgoData.IsEnoughFreeSkullsToBuyTalentLevelUp(def))
				{
					talentLevelUpWidgetData.ZombieWgoData.PurchaseTalentLevelUp(def);
					LazyAudio.PlayAndForget("coins_sound");
				}
				onQueueChanged?.Invoke();
			}
		}
	}
}
