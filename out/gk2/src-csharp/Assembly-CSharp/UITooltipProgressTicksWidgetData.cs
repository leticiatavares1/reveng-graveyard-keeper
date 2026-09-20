using LazyBearTechnology;

public class UITooltipProgressTicksWidgetData : LazyWidgetDataBase
{
	public int MasteryValue { get; private set; }

	public int MasteryLock { get; private set; }

	public bool IsStarCraft { get; private set; }

	public TalentDef TalentDef { get; private set; }

	public UITooltipProgressTicksWidgetData(int masteryValue, int mastery, bool starCraft, TalentDef talentDef)
	{
		MasteryValue = masteryValue;
		MasteryLock = mastery;
		IsStarCraft = starCraft;
		TalentDef = talentDef;
	}
}
