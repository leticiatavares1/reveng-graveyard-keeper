using System;
using LazyBearTechnology;

public class CharacterWindowData : LazyWidgetDataBase
{
	public enum CharPage
	{
		Undefined,
		Main,
		TechTree,
		Inspiration,
		QuestTree,
		Map
	}

	public Action<CharPage, bool> onPageStatusChanged;

	private bool subscribedEvents;

	public PlayerData PlayerData { get; private set; }

	public CharPage Page { get; private set; }

	public CharMainPageWidgetData CharMainPageWidgetData { get; private set; }

	public CharInspirationPageWidgetData InspirationPageWidgetData { get; private set; }

	public MapPageWidgetData MapPageWidgetData { get; private set; }

	public string FocusOnQuest { get; set; }

	public bool HasAvailableActionOnInspirationPage { get; private set; }

	public CharacterWindowData(GameSave gameSave, CharPage page = CharPage.Main, string inspirationTalentId = null)
	{
		PlayerData = gameSave.playerData;
		SetPage(page);
		CharMainPageWidgetData = new CharMainPageWidgetData(gameSave);
		InspirationPageWidgetData = new CharInspirationPageWidgetData(gameSave.talentSystemData, inspirationTalentId);
		MapPageWidgetData = new MapPageWidgetData(gameSave, milestonesInteractable: false, string.Empty);
		UpdateHasAvailableActionOnInspirationPageStatus();
	}

	public void SubscribeEvents()
	{
		if (!subscribedEvents)
		{
			MainGame.Instance.GameSave.talentSystemData.OnInspirationProgressChanged += UpdateHasAvailableActionOnInspirationPageStatus;
			MainGame.Instance.GameSave.talentSystemData.OnInspirationPurchased += UpdateHasAvailableActionOnInspirationPageStatus;
			MainGame.Instance.GameSave.talentSystemData.OnInspirationCompleted += UpdateHasAvailableActionOnInspirationPageStatus;
			MainGame.Instance.GameSave.talentSystemData.OnTalentLevelPurchased += OnTalentLevelPurchased;
			subscribedEvents = true;
		}
	}

	public void UnsubscribeEvents()
	{
		if (subscribedEvents)
		{
			MainGame.Instance.GameSave.talentSystemData.OnInspirationProgressChanged -= UpdateHasAvailableActionOnInspirationPageStatus;
			MainGame.Instance.GameSave.talentSystemData.OnInspirationPurchased -= UpdateHasAvailableActionOnInspirationPageStatus;
			MainGame.Instance.GameSave.talentSystemData.OnInspirationCompleted -= UpdateHasAvailableActionOnInspirationPageStatus;
			MainGame.Instance.GameSave.talentSystemData.OnTalentLevelPurchased -= OnTalentLevelPurchased;
			subscribedEvents = false;
		}
	}

	public void UpdateHasAvailableActionOnInspirationPageStatus(string id = "")
	{
		HasAvailableActionOnInspirationPage = false;
		TalentSystemData talentSystemData = MainGame.Instance.GameSave.talentSystemData;
		foreach (TalentData talentDatum in talentSystemData.talentData)
		{
			if (talentDatum.HasAvailableTalentLevelUpActionIndicator)
			{
				HasAvailableActionOnInspirationPage = true;
				break;
			}
			if (talentSystemData.HasTwoZeroFaithInspirationsToBuy(talentDatum))
			{
				HasAvailableActionOnInspirationPage = true;
				break;
			}
		}
		onPageStatusChanged?.Invoke(CharPage.Inspiration, HasAvailableActionOnInspirationPage);
	}

	private void OnTalentLevelPurchased(string talentId, string talentLevelId)
	{
		UpdateHasAvailableActionOnInspirationPageStatus(talentId);
	}

	public void SetPage(CharPage page)
	{
		Page = page;
		PlayerData.lastOpenedPage = page;
	}

	public void UpdateTalentInInspirationWidgetData(string talentId)
	{
		InspirationPageWidgetData.SwitchTalent(talentId);
	}
}
