using LazyBearTechnology;

public class UIProgressCellsInfoWidgetData : LazyWidgetDataBase
{
	public int MasteryValue { get; private set; }

	public int MasteryLock { get; private set; }

	public TalentDef TalentDef { get; private set; }

	public bool IsStarCraft { get; private set; }

	public string TooltipHeaderLngId { get; private set; }

	public int PerksMasteryBonus { get; private set; }

	public UIProgressCellsInfoWidgetData(int masteryValue, int masteryLock, TalentDef talentDef, bool isStarCraft, string tooltipHeaderLngId, int perksMasteryBonus = 0)
	{
		MasteryValue = masteryValue;
		MasteryLock = masteryLock;
		TalentDef = talentDef;
		IsStarCraft = isStarCraft;
		TooltipHeaderLngId = tooltipHeaderLngId;
		PerksMasteryBonus = perksMasteryBonus;
	}

	public string FormatPlayerMasteryValue()
	{
		if (PerksMasteryBonus == 0)
		{
			return MasteryValue.ToString();
		}
		int num = MasteryValue - PerksMasteryBonus;
		if (PerksMasteryBonus > 0)
		{
			return $"{num}+{PerksMasteryBonus}";
		}
		return $"{num}{PerksMasteryBonus}";
	}
}
