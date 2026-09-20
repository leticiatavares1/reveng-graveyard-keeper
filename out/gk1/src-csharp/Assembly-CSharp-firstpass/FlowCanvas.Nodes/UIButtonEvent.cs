using ParadoxNotion.Design;
using UnityEngine.UI;

namespace FlowCanvas.Nodes;

[Name("UI Button", 0)]
[Description("Called when the target UI Button is clicked")]
[Category("Events/Object/UI")]
public class UIButtonEvent : EventNode<Button>
{
	private FlowOutput o;

	public override void OnGraphStarted()
	{
		ResolveSelf();
		if (!target.isNull)
		{
			target.value.onClick.AddListener(OnClick);
		}
	}

	public override void OnGraphStoped()
	{
		if (!target.isNull)
		{
			target.value.onClick.RemoveListener(OnClick);
		}
	}

	protected override void RegisterPorts()
	{
		o = AddFlowOutput("Clicked");
		AddValueOutput("This", () => target.value);
	}

	private void OnClick()
	{
		o.Call(default(Flow));
	}
}
