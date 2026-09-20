using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Set All Capture Points Team", 0)]
[Category("Game/Fighting")]
[Description("Sets the owner team for all capture points in the current fighting level (base point + all sector points).")]
[Color("313c8f")]
public class Flow_SetCapturePointTeam : GKCustomFlowNode
{
	private ValueInput<LazyConsts.Fighting.TeamType> teamType;

	private FlowOutput outFlow;

	protected override void RegisterPorts()
	{
		teamType = AddValueInput<LazyConsts.Fighting.TeamType>("Team Type");
		AddFlowInput("In", delegate(Flow f)
		{
			Execute();
			f.Call(outFlow);
		});
		outFlow = AddFlowOutput("Out");
	}

	private void Execute()
	{
		FightingLevel currentLevel = LazySingleton<FightingGameController>.Instance.CurrentLevel;
		if (currentLevel == null)
		{
			Debug.LogError("[Flow_SetCapturePointTeam]: No active fighting level found.");
			return;
		}
		foreach (FightingLine fightingLine in currentLevel.FightingLines)
		{
			if (fightingLine == null)
			{
				continue;
			}
			foreach (FightingSector sector in fightingLine.sectors)
			{
				if (sector != null && sector.point != null)
				{
					sector.point.SetOwnedByTeam(teamType.value);
				}
			}
		}
	}
}
