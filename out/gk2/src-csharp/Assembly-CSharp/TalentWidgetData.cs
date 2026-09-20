using LazyBearTechnology;

public class TalentWidgetData : LazyWidgetDataBase
{
	public string TalentId { get; private set; }

	public int MasteryValue { get; private set; }

	public TalentWidgetData(string talentId, int masteryValue)
	{
		TalentId = talentId;
		MasteryValue = masteryValue;
	}
}
