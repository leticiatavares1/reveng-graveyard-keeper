using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UINotificator : LazySingleton<UINotificator>, ILazyGUIElement
{
	public static bool isSilent;

	[SerializeField]
	private Transform appearPoint;

	[SerializeField]
	private Transform startPoint;

	[SerializeField]
	private float offsetY;

	[SerializeField]
	private float animationTime = 0.2f;

	[SerializeField]
	private GameObject prefabsParent;

	private List<UIBaseNotification> displayingItems = new List<UIBaseNotification>();

	public IReadOnlyList<UIBaseNotification> DisplayingItems => displayingItems;

	public void Init()
	{
		MainGame.OnGameStarted = (Action)Delegate.Combine(MainGame.OnGameStarted, new Action(OnGameStarted));
		MainGame.OnGoToMainMenu = (Action)Delegate.Combine(MainGame.OnGoToMainMenu, new Action(OnGoToMainMenu));
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UIItemAddNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UIItemRemoveNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UIInspirationNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UIQuestStartNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UIQuestCompleteNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UIMoneyChangedNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UISimpleTextNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UISimpleTextWithIconNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UINewBodyNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UITechRepUnlockedNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UITechRevealedNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UICustomizationUnlockedNotification>(includeInactive: true), 1);
		LazyPooler.CreatePool(prefabsParent.GetComponentInChildren<UITalentLevelUpRevealedNotification>(includeInactive: true), 1);
	}

	private void OnGameStarted()
	{
		MainGame.Instance.GameSave.talentSystemData.OnInspirationCompleted += HandleInspiration;
		MainGame.PlayerData.OnDropCollected += HandleAddItems;
		MainGame.PlayerData.inventory.OnInventoryFull += HandleInventoryFull;
		MainGame.Instance.GameSave.questSystemData.OnQuestStarted += HandleQuestStarted;
		MainGame.Instance.GameSave.questSystemData.OnQuestCompleted += HandleQuestCompleted;
		PlayerMoneyGameResSystem system = PlayerMoneyGameResSystem.GetSystem();
		system.onValueChangedDiff = (Action<float>)Delegate.Combine(system.onValueChangedDiff, new Action<float>(HandleMoneyChanged));
		KnowledgeSystem.OnTechRevealed = (Action<string>)Delegate.Combine(KnowledgeSystem.OnTechRevealed, new Action<string>(HandleTechRevealed));
		KnowledgeSystem.OnTechUnlocked = (Action<string, bool>)Delegate.Combine(KnowledgeSystem.OnTechUnlocked, new Action<string, bool>(HandleTechUnlocked));
		KnowledgeSystem.OnTalentLevelUpRevealed = (Action<string>)Delegate.Combine(KnowledgeSystem.OnTalentLevelUpRevealed, new Action<string>(HandleTalentLevelUpRevealed));
		PlayerCustomizationData.OnCustomizationUnlocked = (Action<Item>)Delegate.Combine(PlayerCustomizationData.OnCustomizationUnlocked, new Action<Item>(HandleCustomizationUnlocked));
	}

	private void OnGoToMainMenu()
	{
		MainGame.Instance.GameSave.talentSystemData.OnInspirationCompleted -= HandleInspiration;
		MainGame.PlayerData.OnDropCollected -= HandleAddItems;
		MainGame.PlayerData.inventory.OnInventoryFull -= HandleInventoryFull;
		MainGame.Instance.GameSave.questSystemData.OnQuestStarted -= HandleQuestStarted;
		MainGame.Instance.GameSave.questSystemData.OnQuestCompleted -= HandleQuestCompleted;
		PlayerMoneyGameResSystem system = PlayerMoneyGameResSystem.GetSystem();
		system.onValueChangedDiff = (Action<float>)Delegate.Remove(system.onValueChangedDiff, new Action<float>(HandleMoneyChanged));
		KnowledgeSystem.OnTechRevealed = (Action<string>)Delegate.Remove(KnowledgeSystem.OnTechRevealed, new Action<string>(HandleTechRevealed));
		KnowledgeSystem.OnTechUnlocked = (Action<string, bool>)Delegate.Remove(KnowledgeSystem.OnTechUnlocked, new Action<string, bool>(HandleTechUnlocked));
		KnowledgeSystem.OnTalentLevelUpRevealed = (Action<string>)Delegate.Remove(KnowledgeSystem.OnTalentLevelUpRevealed, new Action<string>(HandleTalentLevelUpRevealed));
		PlayerCustomizationData.OnCustomizationUnlocked = (Action<Item>)Delegate.Remove(PlayerCustomizationData.OnCustomizationUnlocked, new Action<Item>(HandleCustomizationUnlocked));
	}

	private void ShowNotification(UIBaseNotification notification)
	{
		notification.RectTransform.SetParent(base.transform);
		notification.RectTransform.anchorMin = new Vector2(1f, 0f);
		notification.RectTransform.anchorMax = new Vector2(1f, 0f);
		notification.RectTransform.pivot = new Vector2(1f, 0f);
		notification.CurrentTime = 0f;
		notification.IsTimerActive = true;
		notification.RectTransform.localPosition = appearPoint.localPosition;
		notification.gameObject.SetActive(value: true);
		notification.Draw();
		notification.RectTransform.RefreshContentFitter();
		displayingItems.Add(notification);
		UpdateNotificationsPosition();
	}

	private void HideNotification(UIBaseNotification notification)
	{
		notification.gameObject.SetActive(value: false);
		notification.ReleaseToPool();
	}

	private void UpdateNotificationsPosition()
	{
		if (displayingItems.Count != 0)
		{
			float num = 0f;
			for (int num2 = displayingItems.Count - 1; num2 >= 0; num2--)
			{
				displayingItems[num2].transform.DOKill();
				displayingItems[num2].transform.DOLocalMove(startPoint.transform.localPosition + new Vector3(0f, num), animationTime).SetEase(Ease.Linear);
				num += offsetY + displayingItems[num2].RectTransform.sizeDelta.y;
			}
		}
	}

	private void Update()
	{
		for (int num = displayingItems.Count - 1; num >= 0; num--)
		{
			UIBaseNotification notification = displayingItems[num];
			if (notification.IsTimerActive)
			{
				notification.CurrentTime += Time.deltaTime;
				if (notification.CurrentTime >= notification.displayingTime)
				{
					displayingItems.RemoveAt(num);
					notification.IsTimerActive = false;
					Vector3 localPosition = notification.transform.localPosition;
					localPosition.x += appearPoint.localPosition.x - startPoint.localPosition.x;
					notification.transform.DOKill();
					notification.transform.DOLocalMove(localPosition, animationTime).SetEase(Ease.Linear).onComplete = delegate
					{
						HideNotification(notification);
					};
				}
			}
		}
	}

	public void ShowSimpleTextNotification(string locale)
	{
		UISimpleTextNotification @object = LazyPooler.GetObject<UISimpleTextNotification>();
		@object.LocalizationKey = locale;
		@object.Text = LLBase.L(locale);
		ShowNotification(@object);
	}

	public void ShowSimpleTextWithIconNotification(string locale, string iconId)
	{
		UISimpleTextWithIconNotification @object = LazyPooler.GetObject<UISimpleTextWithIconNotification>();
		@object.LocalizationKey = locale;
		@object.Text = LLBase.L(locale);
		@object.IconId = iconId;
		ShowNotification(@object);
	}

	public void ShowNewBodyNotification(string bodyId)
	{
		BodyDef data = GameBalance.Me.GetData<BodyDef>(bodyId);
		if (data == null)
		{
			Debug.LogError("Can't ShowNewBodyNotification body def is null for:[" + bodyId + "]");
			return;
		}
		UINewBodyNotification @object = LazyPooler.GetObject<UINewBodyNotification>();
		@object.ItemDef = GameBalance.Me.GetData<ItemDef>(data.linkedBodyItemId);
		ShowNotification(@object);
	}

	public void ShowMoneyNotification(int money)
	{
		HandleMoneyChanged(money);
	}

	public void ShowSimpleTextNotificationOneTime(string locale)
	{
		if (isSilent)
		{
			return;
		}
		if (displayingItems.Count != 0)
		{
			foreach (UIBaseNotification displayingItem in displayingItems)
			{
				if (displayingItem is UISimpleTextNotification uISimpleTextNotification && !string.IsNullOrEmpty(uISimpleTextNotification.LocalizationKey) && uISimpleTextNotification.LocalizationKey == locale)
				{
					return;
				}
			}
		}
		ShowSimpleTextNotification(locale);
	}

	public void HandleInventoryFull()
	{
		ShowSimpleTextNotificationOneTime("inventory_is_full");
	}

	private void HandleAddItems(List<Item> items)
	{
		if (isSilent)
		{
			return;
		}
		foreach (Item item in items)
		{
			if (item == null)
			{
				Debug.LogError("UINotificator: error item is null");
				continue;
			}
			bool flag = false;
			for (int num = displayingItems.Count - 1; num >= 0; num--)
			{
				if (displayingItems[num] is UIItemAddNotification uIItemAddNotification && uIItemAddNotification.ItemId == item.id && uIItemAddNotification.gameObject.activeSelf)
				{
					uIItemAddNotification.AddCountToItem(item.Count);
					LazyAudio.Play("item_pickup");
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				UIItemAddNotification @object = LazyPooler.GetObject<UIItemAddNotification>();
				@object.ItemId = item.id;
				@object.DisplayingCount = item.Count;
				ShowNotification(@object);
				LazyAudio.Play("item_pickup");
			}
		}
	}

	private void HandleMoneyChanged(float money)
	{
		if (isSilent)
		{
			return;
		}
		if (displayingItems.Count != 0)
		{
			foreach (UIBaseNotification displayingItem in displayingItems)
			{
				if (displayingItem is UIMoneyChangedNotification uIMoneyChangedNotification)
				{
					uIMoneyChangedNotification.AddCount((int)money);
					LazyAudio.Play("coins_sound");
					return;
				}
			}
		}
		UIMoneyChangedNotification @object = LazyPooler.GetObject<UIMoneyChangedNotification>();
		@object.DisplayingCount = (int)money;
		ShowNotification(@object);
		LazyAudio.Play("coins_sound");
	}

	private void HandleTechRevealed(string techId)
	{
		if (!isSilent)
		{
			UITechRevealedNotification @object = LazyPooler.GetObject<UITechRevealedNotification>();
			@object.TechId = techId;
			@object.IsTechUnlocked = false;
			ShowNotification(@object);
		}
	}

	private void HandleTechUnlocked(string techId, bool forceSilent)
	{
		if (!(isSilent || forceSilent))
		{
			TechDef data = GameBalance.Me.GetData<TechDef>(techId);
			if (data == null || data.wgoRepLock.List.Count <= 0)
			{
				UITechRevealedNotification @object = LazyPooler.GetObject<UITechRevealedNotification>();
				@object.TechId = techId;
				@object.IsTechUnlocked = true;
				ShowNotification(@object);
			}
		}
	}

	private void HandleTechRepUnlocked(string techId)
	{
		if (!isSilent)
		{
			UITechRepUnlockedNotification @object = LazyPooler.GetObject<UITechRepUnlockedNotification>();
			@object.TechId = techId;
			ShowNotification(@object);
		}
	}

	private void HandleInspiration(string inspirationId)
	{
		if (!isSilent && !MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(CharacterWindowData.CharPage.Inspiration))
		{
			UIInspirationNotification @object = LazyPooler.GetObject<UIInspirationNotification>();
			@object.InspirationId = inspirationId;
			@object.InspirationTalentId = GameBalance.Me.GetData<InspirationDef>(inspirationId).talentId;
			ShowNotification(@object);
		}
	}

	private void HandleQuestStarted(QuestData questData)
	{
		if (!isSilent && !questData.isHidden)
		{
			UIQuestStartNotification @object = LazyPooler.GetObject<UIQuestStartNotification>();
			@object.QuestData = questData;
			ShowNotification(@object);
		}
	}

	private void HandleQuestCompleted(QuestData questData)
	{
		if (!isSilent && !questData.isHidden)
		{
			UIQuestCompleteNotification @object = LazyPooler.GetObject<UIQuestCompleteNotification>();
			@object.QuestData = questData;
			ShowNotification(@object);
		}
	}

	private void HandleCustomizationUnlocked(Item sourceItem)
	{
		if (!isSilent)
		{
			UICustomizationUnlockedNotification @object = LazyPooler.GetObject<UICustomizationUnlockedNotification>();
			@object.SourceItem = sourceItem;
			ShowNotification(@object);
		}
	}

	private void HandleTalentLevelUpRevealed(string talentLevelUpId)
	{
		if (!isSilent)
		{
			UITalentLevelUpRevealedNotification @object = LazyPooler.GetObject<UITalentLevelUpRevealedNotification>();
			@object.TalentLevelUpId = talentLevelUpId;
			ShowNotification(@object);
		}
	}

	public void ShowTechRevealNotification()
	{
		HandleTechRevealed("wood_basic");
	}

	public void ShowTechRepUnlockedNotification()
	{
		HandleTechRepUnlocked("stone_basic");
	}

	public void ShowItemAddNotification()
	{
		HandleAddItems(new List<Item>
		{
			new Item("faith", 15)
		});
	}

	public void ShowQuestStartNotification()
	{
		MainGame.Instance.GameSave.questSystemData.questCollection.questsCache["6_intro_guards_burial"].isHidden = false;
		MainGame.Instance.GameSave.questSystemData.StartQuest("6_intro_guards_burial");
	}

	public void ShowQuestCompleteNotification()
	{
		MainGame.Instance.GameSave.questSystemData.questCollection.questsCache["6_intro_guards_burial"].isHidden = false;
		MainGame.Instance.GameSave.questSystemData.CompleteQuest("6_intro_guards_burial");
	}

	public void ShowInspirationNotification1()
	{
		HandleInspiration("lumberjack_1");
	}

	public void ShowInspirationNotification2()
	{
		HandleInspiration("recycler_1");
	}

	public void ShowInspirationNotification3()
	{
		HandleInspiration("insp_mushroom_hunter_1");
	}

	public void ShowInspirationNotification4()
	{
		HandleInspiration("undertaker_1");
	}

	public void ShowInspirationNotification5()
	{
		HandleInspiration("last_respects_1");
	}

	public void ShowNewBodyNotification()
	{
		ShowNewBodyNotification("body_0_1");
	}

	public void ShowMoneyAddNotification()
	{
		HandleMoneyChanged(4321f);
	}

	public void ShowMoneyRemoveNotification()
	{
		HandleMoneyChanged(-1234f);
	}

	public void ShowSimpleTextNotification1()
	{
		ShowSimpleTextNotification("parishioner_fail_1");
	}

	public void ShowSimpleTextNotification2()
	{
		ShowSimpleTextNotification("1_intro_prison_wake_5");
	}
}
