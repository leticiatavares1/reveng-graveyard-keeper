using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Has Prepared Resurrection Table", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_HasPreparedResurrectionTable : GKCustomFlowNode
{
	private ValueOutput<bool> hasPreparedResurrectionTable;

	protected override void RegisterPorts()
	{
		hasPreparedResurrectionTable = AddValueOutput("isSelectedDayToday", HasPreparedResurrectionTable);
	}

	private bool HasPreparedResurrectionTable()
	{
		List<WgoData> wgoDataList = MainGame.WorldData.GetWgoDataList("resurrection_table_1");
		if (wgoDataList.Count == 0)
		{
			return false;
		}
		foreach (WgoData item in wgoDataList)
		{
			if (item.GetGameResInt("resurrection_prepared") == 1)
			{
				return true;
			}
		}
		return false;
	}
}
