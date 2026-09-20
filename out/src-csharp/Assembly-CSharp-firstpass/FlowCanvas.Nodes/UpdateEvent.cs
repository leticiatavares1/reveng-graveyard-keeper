using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("On Update", 6)]
[Description("Called per-frame. Update Interval optionally determines the period in seconds every which update is called.\nLeave at 0 to call update per-frame as normal.")]
[Category("Events/Graph")]
public class UpdateEvent : EventNode, IUpdatable
{
	public BBParameter<float> updateInterval = 0f;

	private FlowOutput update;

	private float lastUpdatedTime;

	protected override void RegisterPorts()
	{
		update = AddFlowOutput("Out");
	}

	public override void OnGraphStarted()
	{
		lastUpdatedTime = -1f;
	}

	public void Update()
	{
		if (updateInterval.value <= 0f)
		{
			update.Call(default(Flow));
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup > updateInterval.value + lastUpdatedTime)
		{
			update.Call(default(Flow));
			lastUpdatedTime = realtimeSinceStartup;
		}
	}
}
