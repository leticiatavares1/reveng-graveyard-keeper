using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Events/Input")]
[Name("Lazy Input", 0)]
public class Flow_LazyInput : EventNode, IUpdatable
{
	public BBParameter<GameKey> keyCode = GameKey.Down;

	private FlowOutput down;

	private FlowOutput pressed;

	public override string name => $"{base.name} [{keyCode}]";

	protected override void RegisterPorts()
	{
		down = AddFlowOutput("Down");
		pressed = AddFlowOutput("Pressed");
	}

	public void Update()
	{
		GameKey value = keyCode.value;
		if (LazyInput.GetKeyDown(value))
		{
			down.Call(default(Flow));
		}
		if (LazyInput.GetKey(value))
		{
			pressed.Call(default(Flow));
		}
	}
}
