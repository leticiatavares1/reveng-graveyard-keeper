using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Display Cinematics Scene", 0)]
[Category("Game/Cutscenes")]
[Color("8a8a8a")]
public class Flow_DisplayCinematicsScene : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onFinished;

	private ValueInput<string> id;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), DisplayCinematicsScene);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onFinished = AddFlowOutput("onFinished".CapitalizeFirst());
		id = AddValueInput<string>("id");
	}

	private void DisplayCinematicsScene(Flow flow)
	{
		if (string.IsNullOrEmpty(id.value))
		{
			Debug.LogError("Flow_DisplayCinematicsScene: id is null or empty");
			@out.Call(flow);
			onFinished.Call(flow);
		}
		else
		{
			LazySingleton<CinematicsSceneDisplayManager>.Instance.DisplayCinematicsScene(id.value, delegate
			{
				onFinished.Call(flow);
			});
			@out.Call(flow);
		}
	}
}
