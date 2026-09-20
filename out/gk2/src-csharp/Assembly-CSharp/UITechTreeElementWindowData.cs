using System.Collections.Generic;
using LazyBearTechnology;

public class UITechTreeElementWindowData : LazyWidgetDataBase
{
	public string HeaderText { get; private set; }

	public string TopText { get; private set; }

	public string BotText { get; private set; }

	public TechDef TechDef { get; private set; }

	public LinkedEntityWidgetData[] LinkedEntityWidgetDatas { get; private set; }

	public List<UIDialogWindowData.ButtonData> ButtonsData { get; set; }

	public UITechTreeElementWindowData(TechDef techDef, List<UIDialogWindowData.ButtonData> buttonsData, string botLabelLocale = null, string topLabelLocale = null)
	{
		TechDef = techDef;
		HeaderText = LLBase.L(techDef.id);
		ButtonsData = buttonsData;
		if (!string.IsNullOrEmpty(topLabelLocale))
		{
			TopText = LLBase.L(topLabelLocale);
		}
		if (!string.IsNullOrEmpty(botLabelLocale))
		{
			BotText = LLBase.L(botLabelLocale);
		}
	}
}
