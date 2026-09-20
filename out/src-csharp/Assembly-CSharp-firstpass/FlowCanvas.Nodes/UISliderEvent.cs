using ParadoxNotion.Design;
using UnityEngine.UI;

namespace FlowCanvas.Nodes;

[Description("Called when the target UI Slider value changed.")]
[Name("UI Slider", 0)]
[Category("Events/Object/UI")]
public class UISliderEvent : EventNode<Slider>
{
	private FlowOutput o;

	private float value;

	public override void OnGraphStarted()
	{
		ResolveSelf();
		if (!target.isNull)
		{
			target.value.onValueChanged.AddListener(OnValueChanged);
		}
	}

	public override void OnGraphStoped()
	{
		if (!target.isNull)
		{
			target.value.onValueChanged.RemoveListener(OnValueChanged);
		}
	}

	protected override void RegisterPorts()
	{
		o = AddFlowOutput("Value Changed");
		AddValueOutput("This", () => target.value);
		AddValueOutput("Value", () => value);
	}

	private void OnValueChanged(float value)
	{
		this.value = value;
		o.Call(default(Flow));
	}
}
