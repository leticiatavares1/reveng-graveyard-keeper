using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIInteractionHintWidgetData : LazyWidgetDataBase
{
	public UIInteractionHintRowWidgetData RowData1 { get; private set; }

	public UIInteractionHintRowWidgetData RowData2 { get; private set; }

	public UIInteractionHintWidgetData(UIInteractionHintRowWidgetData rowData)
	{
		RowData1 = rowData;
		RowData2 = null;
	}

	public UIInteractionHintWidgetData(List<UIInteractionHintRowWidgetData> rowWidgetsData)
	{
		RowData1 = ((rowWidgetsData.Count > 0) ? rowWidgetsData[0] : null);
		RowData2 = ((rowWidgetsData.Count > 1) ? rowWidgetsData[1] : null);
		if (rowWidgetsData.Count > 2)
		{
			Debug.LogWarning("UIInteractionHintWidgetData: Too many row widgets data. Allowed only 2.");
		}
	}
}
