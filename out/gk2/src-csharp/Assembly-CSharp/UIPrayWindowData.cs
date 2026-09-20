using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIPrayWindowData : LazyWidgetDataBase
{
	public Action OnPraySlotPress { get; private set; }

	public PlayerData PlayerData { get; private set; }

	public WgoData PrayingStand { get; private set; }

	public CraftElementSermon CraftQueueElement { get; private set; }

	public string RewardBoxId { get; private set; }

	public SermonDef SermonDef { get; set; }

	private MultiInventory PlayerMultiInventory { get; set; }

	public int ChurchQuality { get; private set; }

	public int GraveyardQuality { get; private set; }

	public int Happiness { get; private set; }

	public PerkWidgetData BuffWidgetData { get; private set; }

	public int ResultVisitors { get; private set; }

	public UIPrayWindowData(PlayerData playerData, WgoData prayingStand, int graveyardQuality, int churchQuality, int happiness, string rewardBoxId)
	{
		PlayerData = playerData;
		PrayingStand = prayingStand;
		RewardBoxId = rewardBoxId;
		OnPraySlotPress = HandlePraySlotPress;
		ChurchQuality = Mathf.FloorToInt(churchQuality);
		GraveyardQuality = Mathf.FloorToInt(graveyardQuality);
		PlayerMultiInventory = new MultiInventory(playerData);
		Happiness = happiness;
		ResultVisitors = Math.Min(Happiness, ChurchQuality);
	}

	public bool CanStartCraft()
	{
		if (EnoughParishioners() && CraftQueueElement != null)
		{
			return PrayingStand.CraftComponent.GetStartCraftStatus(CraftQueueElement) == CraftStatus.OK;
		}
		return false;
	}

	public bool EnoughParishioners()
	{
		if (SermonDef == null)
		{
			return false;
		}
		return ResultVisitors >= SermonDef.minParishioners;
	}

	public void StartCraft(int parishionersCount, int chance)
	{
		PlayerData.SubRes("happiness", parishionersCount);
		MainGame.PlayerData.currentSermon = new SermonResultData(SermonDef.id, RewardBoxId, parishionersCount, UnityEngine.Random.Range(0f, 100f) <= (float)chance, ChurchQuality, GraveyardQuality);
		MainGame.WorldData.Cache.wgoDataByCustomTagsCache["church_tribune_real"][0].SetGameRes("cur_pray_ppl", parishionersCount);
		PrayingStand.CraftComponent.TryStartCurCraft();
		PrayingStand.CraftComponent.TryFinishCurCraft();
		MainGame.PlayerController.View.SetSermonIcon(SermonDef.PrayIcon);
		GlobalScriptsManager.FireEvent("System_Pray", "sermon_start", OnPrayFinished);
	}

	private void OnPrayFinished()
	{
		AchievementsSystem.Instance.TriggerCountable("sermon_done");
		WgoData rewardBox = MainGame.Instance.GameSave.worldData.GetWgoData(RewardBoxId);
		rewardBox.StoreSermonResult(MainGame.PlayerData.currentSermon);
		foreach (LazyExpression prayOnEndExpression in SermonDef.prayOnEndExpressions)
		{
			prayOnEndExpression.Evaluate();
		}
		UIPrayReportWindowData data = new UIPrayReportWindowData(MainGame.PlayerData.currentSermon, delegate
		{
			List<Item> list = OutputItems.MakeOutput(SermonDef.successRewardItem.MakePreOutput(rewardBox));
			for (int i = 0; i < list.Count; i++)
			{
				MainGame.Instance.dropSystem.DropItem(list[i], MainGame.PlayerData.currentGameSceneId, MainGame.PlayerData.position.Value);
			}
			if (!string.IsNullOrEmpty(SermonDef.successRewardBuff))
			{
				MainGame.Instance.GameSave.perkSystemData.AddPerk(SermonDef.successRewardBuff);
			}
		});
		LazyUI.GetWindow<UIPrayReportWindow>().Open(data);
	}

	public void EraseNonStartedCraft()
	{
		PrayingStand.CraftComponent.RemoveCurNotStartedCraft();
	}

	private void HandlePraySlotPress()
	{
		UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
		UIMultiInventoryWindowData data = new UIMultiInventoryWindowData(PlayerData, delegate(UIItemCell uiItemCell)
		{
			SermonDef sermonDef = GameBalance.GetSermonDef(uiItemCell.DisplayingItem.id);
			if (sermonDef != null)
			{
				CreateCraftElementForChosenSermon(sermonDef);
				LazyUI.GetWindow<UIPrayWindow>().Redraw();
				LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
			}
		}, CanUsePray);
		window.Open(data);
	}

	private bool CanUsePray(Item item)
	{
		if (item == null || item.IsEmpty)
		{
			return false;
		}
		if (item.Definition.type != ItemType.Preach)
		{
			return false;
		}
		if (GameBalance.GetSermonDef(item.id) == null)
		{
			return false;
		}
		return true;
	}

	private void CreateCraftElementForChosenSermon(SermonDef sermonDef)
	{
		SermonDef = sermonDef;
		if (!string.IsNullOrEmpty(SermonDef.successRewardBuff))
		{
			BuffWidgetData = new PerkWidgetData(new PerkData(SermonDef.successRewardBuff), isActive: true, null, null, null);
		}
		CraftQueueElement = new CraftElementSermon(sermonDef.id, 1, new List<NeedItemData>(), new CraftParamsData(sermonDef.id, new GameRes()));
		MainGame.Instance.craftSystem.AddCraftObject(PrayingStand.CraftComponent);
		PrayingStand.CraftComponent.RemoveCurNotStartedCraft();
		PrayingStand.CraftComponent.AddCraftNoStart(CraftQueueElement);
	}
}
