using System;

public class TechTreeElementBaseWidgetData : TreeElementBaseWidgetData
{
	public Action<TechTreeElementBaseWidgetData> onTechClicked;

	public TechState VisualTechState
	{
		get
		{
			if (!techDef.isAvailableInDemo)
			{
				return TechState.Hidden;
			}
			return techDef.TechState;
		}
	}
}
