using System;
using System.Collections.Generic;
using System.Linq;
using FlowCanvas;
using NodeCanvas.Framework;
using NodeCanvas.StateMachines;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Weather State", 0)]
public class FSMWeatherState : FSMState
{
	[Serializable]
	public class WeatherStateExit
	{
		public int w;

		public WeatherStateFSMConnection connection;
	}

	public WeatherComponent weather;

	public FlowScript onEnterFlowScript;

	public FlowScript onExitFlowScript;

	public readonly List<WeatherStateExit> exits = new List<WeatherStateExit>();

	public override string name
	{
		get
		{
			if (!(weather != null))
			{
				return "NULL Weather";
			}
			return weather.name;
		}
	}

	private WeatherStateExit GetRandomExit()
	{
		if (exits.Count == 0)
		{
			return null;
		}
		if (exits.Count == 1)
		{
			return exits[0];
		}
		int num = exits.Sum((WeatherStateExit e) => e.w);
		int num2 = UnityEngine.Random.Range(0, num - 1);
		num = 0;
		foreach (WeatherStateExit exit in exits)
		{
			if (exit.w != 0)
			{
				num += exit.w;
				if (num2 <= num)
				{
					return exit;
				}
			}
		}
		return null;
	}

	protected override bool CanConnectFromSource(Node sourceNode)
	{
		return true;
	}

	protected override bool CanConnectToTarget(Node targetNode)
	{
		return true;
	}

	protected override void OnEnter()
	{
		if (onEnterFlowScript != null)
		{
			GlobalScriptsManager.RunFlowScript(onEnterFlowScript, null);
		}
		if (weather == null)
		{
			ProcessTransition();
		}
		else
		{
			weather.FadeIn();
		}
	}

	protected override void OnExit()
	{
		if (onExitFlowScript != null)
		{
			GlobalScriptsManager.RunFlowScript(onExitFlowScript, null);
		}
		weather?.FadeOut();
	}

	private void ProcessTransition()
	{
		weather?.FadeOut();
		WeatherStateFSMConnection weatherStateFSMConnection = GetRandomExit()?.connection;
		if (weatherStateFSMConnection != null)
		{
			base.FSM.EnterState((FSMState)weatherStateFSMConnection.targetNode, weatherStateFSMConnection.transitionCallMode);
			weatherStateFSMConnection.status = Status.Success;
		}
		else
		{
			base.FSM.EnterState(this, FSM.TransitionCallMode.Normal);
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		base.transitionEvaluation = TransitionEvaluationMode.CheckManually;
	}

	public void FinishWeatherState()
	{
		Finish();
		ProcessTransition();
	}
}
