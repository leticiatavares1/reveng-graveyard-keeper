using System.Collections.Generic;
using System.Linq;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Choose Fight Level", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_ChooseFightLevel : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FightingLevel curLevel;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ShowChooseLevelMultiAnswer);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void ShowChooseLevelMultiAnswer(Flow flow)
	{
		List<AnswerVisualData> list = new List<AnswerVisualData>();
		FightingLevel[] levels = Object.FindObjectsByType<FightingLevel>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
		for (int i = 0; i < levels.Length; i++)
		{
			if (levels[i].IsValid())
			{
				list.Add(new AnswerVisualData
				{
					answerData = new AnswerData(),
					hiddenByDefault = false,
					id = levels[i].name
				});
			}
		}
		AnswerVisualData item = new AnswerVisualData
		{
			answerData = new AnswerData(),
			hiddenByDefault = false,
			id = "Exit"
		};
		list.Add(item);
		Transform bubblePoint = MainGame.PlayerController.BubblePoint;
		Bubble.ShowMultiAnswer(list, bubblePoint, base.SelfWgoData, delegate(string chosen)
		{
			if (chosen == "Exit")
			{
				@out.Call(flow);
			}
			else if (base.PlayerData.HasMultipleOverheadItems)
			{
				Bubble.Talk(new PhraseData(isPlayer: true, null, LLBase.L("fight_start_overhead_reason"), null, null, SpeechBubbleType.Think));
				@out.Call(flow);
			}
			else if (!LazySingleton<FightingGameController>.Instance.CanStartFight())
			{
				Bubble.Talk(new PhraseData(isPlayer: true, null, LLBase.L("fight_start_equipment_reason"), null, null, SpeechBubbleType.Think));
				@out.Call(flow);
			}
			else
			{
				GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.MultiAnswerSay, chosen);
				curLevel = levels.FirstOrDefault((FightingLevel x) => x.id == chosen);
				LazyUI.GetWindow<UIPrefightWindow>().Open(new UIPrefightWindowData(curLevel.id, Start));
				@out.Call(flow);
			}
		}, delegate
		{
		}, isOverBlackout: true);
	}

	private void Start()
	{
		GDPointData gDPointDataById = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(curLevel.LevelGdPointId);
		if (base.PlayerData.HasOverheadItem)
		{
			base.PlayerData.DropOverheadItem();
		}
		PlayerController.Teleport(new GDPointTeleportData(gDPointDataById, curLevel.EnvironmentPreset));
		LazySingleton<FightingGameController>.Instance.StartPreFight(curLevel.id);
	}
}
