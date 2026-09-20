using LazyBearTechnology;

public class TalentExpProgressWidgetData : LazyWidgetDataBase
{
	public int CurExp { get; private set; }

	public int TotalExp { get; private set; }

	public int TalentExpPoints { get; private set; }

	public string TalentId { get; private set; }

	public TalentExpProgressWidgetData(TalentData talentData)
	{
		CurExp = talentData.curExp;
		TotalExp = talentData.talentExpLevelBalanceData.GetExpForLevel(talentData.curTalentLevel);
		TalentExpPoints = talentData.talentExpPoints;
		TalentId = talentData.id;
	}
}
