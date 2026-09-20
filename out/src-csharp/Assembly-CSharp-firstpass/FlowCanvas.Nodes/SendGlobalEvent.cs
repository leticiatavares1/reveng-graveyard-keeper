using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Send a Global Event to all graphs")]
[Category("Utility")]
public class SendGlobalEvent : CallableActionNode<string>
{
	public override void Invoke(string eventName)
	{
		Graph.SendGlobalEvent(new EventData(eventName));
	}
}
[ExposeAsDefinition]
[Description("Send a Global Event with 1 argument to all graphs")]
[Category("Utility")]
public class SendGlobalEvent<T> : CallableActionNode<string, T>
{
	public override void Invoke(string eventName, T eventValue)
	{
		Graph.SendGlobalEvent(new EventData<T>(eventName, eventValue));
	}
}
