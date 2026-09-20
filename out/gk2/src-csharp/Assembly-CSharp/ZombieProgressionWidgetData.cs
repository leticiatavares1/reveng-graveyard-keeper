using LazyBearTechnology;

public class ZombieProgressionWidgetData : LazyWidgetDataBase
{
	private const string DEFAULT_TALENT_ID_TO_DRAW = "talent_orange";

	private static string lastShownTalentId;

	public ZombieTalentData ZombieTalentData { get; private set; }

	public ZombieWgoData ZombieWgoData { get; private set; }

	public ZombieProgressionWidgetData(ZombieWgoData zombieWgoData, string talentId = null)
	{
		ZombieWgoData = zombieWgoData;
		SwitchTalent((!string.IsNullOrEmpty(talentId)) ? talentId : ((!string.IsNullOrEmpty(lastShownTalentId)) ? lastShownTalentId : "talent_orange"));
	}

	public void SwitchTalent(string talentId)
	{
		ZombieTalentData = ZombieWgoData.GetTalentBranch(talentId);
		lastShownTalentId = ZombieTalentData.id;
	}
}
