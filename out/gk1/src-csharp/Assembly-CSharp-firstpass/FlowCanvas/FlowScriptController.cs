using NodeCanvas.Framework;

namespace FlowCanvas;

public class FlowScriptController : GraphOwner<FlowScript>
{
	public object CallFunction(string name, params object[] args)
	{
		return base.behaviour.CallFunction(name, args);
	}
}
