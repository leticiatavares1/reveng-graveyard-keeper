using System;
using LazyBearTechnology;

public class CharInspirationPageWidgetData : LazyWidgetDataBase
{
	private const string DEFAULT_TALENT_ID_TO_DRAW = "talent_orange";

	public TalentSystemData.DelTalentExpChanged onTalentExpChanged;

	public TalentSystemData.DelTalentLevelPurchased onTalentLevelPurchased;

	public Action<string> onInspirationPurchased;

	public Action<string> onInspirationProgressChanged;

	private static string lastShownTalentId;

	private bool subscribedEvents;

	public TalentData TalentData { get; private set; }

	public TalentSystemData TalentSystemData { get; private set; }

	public CharInspirationPageWidgetData(TalentSystemData talentSystemData, string talentId = null)
	{
		TalentSystemData = talentSystemData;
		SwitchTalent((!string.IsNullOrEmpty(talentId)) ? talentId : ((!string.IsNullOrEmpty(lastShownTalentId)) ? lastShownTalentId : "talent_orange"));
	}

	public void FillFromTalentsData(TalentSystemData talentSystemData, string talentId = null)
	{
	}

	public void SwitchTalent(string talentId)
	{
		TalentData = TalentSystemData.GetTalentBranch(talentId);
		lastShownTalentId = TalentData.id;
	}

	public void SubscribeEvents()
	{
		if (!subscribedEvents)
		{
			TalentSystemData.OnTalentExpChanged += onTalentExpChanged;
			TalentSystemData.OnInspirationProgressChanged += onInspirationProgressChanged;
			TalentSystemData.OnTalentLevelPurchased += onTalentLevelPurchased;
			TalentSystemData.OnInspirationPurchased += onInspirationPurchased;
			subscribedEvents = true;
		}
	}

	public void UnsubscribeEvents()
	{
		if (subscribedEvents)
		{
			TalentSystemData.OnTalentExpChanged -= onTalentExpChanged;
			TalentSystemData.OnInspirationProgressChanged -= onInspirationProgressChanged;
			TalentSystemData.OnTalentLevelPurchased -= onTalentLevelPurchased;
			TalentSystemData.OnInspirationPurchased -= onInspirationPurchased;
			subscribedEvents = false;
		}
	}
}
