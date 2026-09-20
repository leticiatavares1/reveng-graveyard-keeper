using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion;
using ParadoxNotion.Design;

[Name("On Time Of Day", 8)]
[Description("Automatically calls when time reaches timeToCall")]
public class Flow_OnTimeOfDayEvent : EventNode
{
	private ValueInput<float> timeToCall;

	private FlowOutput onTimeOfDay;

	private bool called;

	protected override void RegisterPorts()
	{
		onTimeOfDay = AddFlowOutput("onTimeOfDay".CapitalizeFirst());
		timeToCall = AddValueInput<float>("timeToCall");
	}

	public override void OnGraphStarted()
	{
		EnvironmentEngine.OnTimeOfDayChangedEvent += CheckCall;
		called = EnvironmentEngine.Instance.timeOfDay >= timeToCall.value;
	}

	public override void OnGraphStoped()
	{
		EnvironmentEngine.OnTimeOfDayChangedEvent -= CheckCall;
	}

	private void CheckCall(float currentTime, bool isFake)
	{
		if (!isFake)
		{
			if (currentTime < timeToCall.value)
			{
				called = false;
			}
			if (!called && currentTime >= timeToCall.value)
			{
				onTimeOfDay.Call(default(Flow));
				called = true;
			}
		}
	}
}
