using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Start Fight By Id", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_StartFightById : GKCustomFlowNode
{
	public bool showPreFightWindow;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> fightId;

	private FightingLevel curLevel;

	protected override void RegisterPorts()
	{
		fightId = AddValueInput<string>("id");
		@in = AddFlowInput("in".CapitalizeFirst(), StartFight);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void StartFight(Flow flow)
	{
		curLevel = MainGame.GetFightingLevel(fightId.value);
		if (curLevel == null)
		{
			Debug.LogError("Can't find fighting level with id [" + fightId.value + "]!");
			@out.Call(flow);
			return;
		}
		if (!curLevel.IsValid())
		{
			Debug.LogError("Fighting level [" + curLevel.id + "] is not valid!");
			@out.Call(flow);
			return;
		}
		if (base.PlayerData.HasMultipleOverheadItems)
		{
			Bubble.Talk(new PhraseData(isPlayer: true, null, LLBase.L("fight_start_overhead_reason"), null, null, SpeechBubbleType.Think));
			@out.Call(flow);
			return;
		}
		if (!LazySingleton<FightingGameController>.Instance.CanStartFight())
		{
			Bubble.Talk(new PhraseData(isPlayer: true, null, LLBase.L("fight_start_equipment_reason"), null, null, SpeechBubbleType.Think));
			@out.Call(flow);
			return;
		}
		if (showPreFightWindow)
		{
			LazyUI.GetWindow<UIPrefightWindow>().Open(new UIPrefightWindowData(curLevel.id, Start));
		}
		else
		{
			Start();
		}
		@out.Call(flow);
	}

	private void Start()
	{
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.MultiAnswerSay, curLevel.id);
		GDPointData gDPointDataById = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(curLevel.LevelGdPointId);
		if (base.PlayerData.HasOverheadItem)
		{
			base.PlayerData.DropOverheadItem();
		}
		PlayerController.Teleport(new GDPointTeleportData(gDPointDataById, curLevel.EnvironmentPreset));
		LazySingleton<FightingGameController>.Instance.StartPreFight(curLevel.id);
	}
}
