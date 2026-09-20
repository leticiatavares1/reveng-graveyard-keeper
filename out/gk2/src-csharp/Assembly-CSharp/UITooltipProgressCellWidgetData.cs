using LazyBearTechnology;

public class UITooltipProgressCellWidgetData : LazyWidgetDataBase
{
	public PerkDef PerkDefBonus { get; private set; }

	public ItemDef ItemDefBonus { get; private set; }

	public UITooltipProgressCellWidgetData(PerkDef perkDefBonus, ItemDef itemDefBonus)
	{
		PerkDefBonus = perkDefBonus;
		ItemDefBonus = itemDefBonus;
	}
}
