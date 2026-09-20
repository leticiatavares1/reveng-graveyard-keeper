using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Time Controller", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_TimeController : GKCustomFlowNode
{
	public enum TimeControlType
	{
		Pause,
		Resume,
		Set,
		SetAndPause,
		SetAndResume,
		SetFake,
		ResumeFromData
	}

	protected FlowInput @in;

	protected FlowOutput @out;

	[GatherPortsCallback]
	public TimeControlType timeControlType;

	private ValueInput<float> time;

	private ValueInput<bool> isStartGameTime;

	public override string name
	{
		get
		{
			string text = timeControlType switch
			{
				TimeControlType.Pause => " \n<color=#fc1403>Pause</color>", 
				TimeControlType.Resume => " \n<color=#03fc41>Resume</color>", 
				TimeControlType.Set => " \n<color=#dbdbdb>Set</color>", 
				TimeControlType.SetAndPause => " \n<color=#fc1403>Set & Pause</color>", 
				TimeControlType.SetAndResume => " \n<color=#03fc41>Set & Resume</color>", 
				_ => base.name, 
			};
			return base.name + text;
		}
	}

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ControlTime);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (timeControlType != TimeControlType.Resume && timeControlType != 0 && timeControlType != TimeControlType.ResumeFromData)
		{
			isStartGameTime = AddValueInput<bool>("isStartGameTime".CapitalizeFirst());
			time = AddValueInput<float>("time".CapitalizeFirst() + " [0;1]");
		}
	}

	protected void ControlTime(Flow flow)
	{
		EnvironmentEngine environmentEngine = MainGame.Instance.GameSave.environmentData.EnvironmentEngine;
		switch (timeControlType)
		{
		case TimeControlType.Pause:
			environmentEngine.IsPaused = true;
			break;
		case TimeControlType.Resume:
			environmentEngine.IsPaused = false;
			break;
		case TimeControlType.Set:
			environmentEngine.SetTimeOfDay(Mathf.Clamp(time.value, 0f, 1f));
			if (isStartGameTime.value)
			{
				MainGame.Instance.GameSave.gameLogicSystemData.PrepareForNewGame();
			}
			break;
		case TimeControlType.SetAndPause:
			environmentEngine.SetTimeOfDay(Mathf.Clamp(time.value, 0f, 1f));
			environmentEngine.IsPaused = true;
			if (isStartGameTime.value)
			{
				MainGame.Instance.GameSave.gameLogicSystemData.PrepareForNewGame();
			}
			break;
		case TimeControlType.SetAndResume:
			environmentEngine.SetTimeOfDay(Mathf.Clamp(time.value, 0f, 1f));
			environmentEngine.IsPaused = false;
			if (isStartGameTime.value)
			{
				MainGame.Instance.GameSave.gameLogicSystemData.PrepareForNewGame();
			}
			break;
		case TimeControlType.SetFake:
			environmentEngine.SetTimeOfDayFake(Mathf.Clamp(time.value, 0f, 1f));
			break;
		case TimeControlType.ResumeFromData:
			environmentEngine.ResumeTimeOfDayFromData();
			break;
		}
		@out.Call(flow);
	}
}
