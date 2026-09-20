using System.Collections.Generic;
using LazyBearTechnology;

public class TalentLevelUpsWidgetData : LazyWidgetDataBase
{
	public TalentSystemData.DelTalentLevelPurchased onTalentLevelPurchased;

	public TalentData TalentData { get; private set; }

	public List<TalentLevelUpDef> LevelUps { get; private set; }

	public ZombieWgoData ZombieWgoData { get; private set; }

	public TalentLevelUpsWidgetData(TalentData talentData)
	{
		TalentData = talentData;
		LevelUps = new List<TalentLevelUpDef>();
		for (int i = 0; i < GameBalance.Me.talentLevelUpDefs.Count; i++)
		{
			TalentLevelUpDef talentLevelUpDef = GameBalance.Me.talentLevelUpDefs[i];
			if (!(talentData.id != talentLevelUpDef.talentId) && !MainGame.Instance.GameSave.knowledgeSystem.hiddenTalentLevelUps.Contains(talentLevelUpDef.id) && !talentLevelUpDef.isZombiePerk)
			{
				LevelUps.Add(talentLevelUpDef);
			}
		}
	}

	public TalentLevelUpsWidgetData(ZombieWgoData zombieWgoData, ZombieTalentData zombieTalentData)
	{
		ZombieWgoData = zombieWgoData;
		LevelUps = new List<TalentLevelUpDef>();
		for (int i = 0; i < GameBalance.Me.talentLevelUpDefs.Count; i++)
		{
			TalentLevelUpDef talentLevelUpDef = GameBalance.Me.talentLevelUpDefs[i];
			if (!(zombieTalentData.id != talentLevelUpDef.talentId) && !MainGame.Instance.GameSave.knowledgeSystem.hiddenTalentLevelUps.Contains(talentLevelUpDef.id) && talentLevelUpDef.isZombiePerk)
			{
				LevelUps.Add(talentLevelUpDef);
			}
		}
	}
}
