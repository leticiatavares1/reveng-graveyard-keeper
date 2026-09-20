using System;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Services;

namespace FlowCanvas.Nodes;

[Name("Custom Event", 100)]
[Description("Called when a custom event is received on target.\n- To send an event from a graph use the SendEvent node.\n- To send an event from code use:'FlowScriptController.SendEvent(string)'")]
[Category("Events/Custom")]
[Color("ffffe6")]
public class CustomEvent : MessageEventNode<GraphOwner>
{
	[RequiredField]
	public BBParameter<string> eventName = "EventName";

	private FlowOutput onReceived;

	private GraphOwner receiver;

	public override string name => base.name + $" [ <color=#1a1a00>{eventName}</color> ]";

	protected override string[] GetTargetMessageEvents()
	{
		return new string[1] { "OnCustomEvent" };
	}

	protected override void RegisterPorts()
	{
		onReceived = AddFlowOutput("Received");
		AddValueOutput("Receiver", () => receiver);
	}

	public void OnCustomEvent(MessageRouter.MessageData<EventData> msg)
	{
		if (msg.value.name == eventName.value)
		{
			receiver = ResolveReceiver(msg.receiver);
			onReceived.Call(default(Flow));
		}
	}
}
[ContextDefinedOutputs(new Type[] { typeof(Wild) })]
[Description("Called when a custom event is received on target.\n- To send an event from a graph use the SendEvent node.\n- To send an event from code use:'FlowScriptController.SendEvent(string)'")]
[Category("Events/Custom")]
[Name("Custom Event", 100)]
[Color("ffffe6")]
public class CustomEvent<T> : MessageEventNode<GraphOwner>
{
	[RequiredField]
	public BBParameter<string> eventName = "EventName";

	private FlowOutput onReceived;

	private T receivedValue;

	private GraphOwner receiver;

	public override string name => base.name + $" [ <color=#1a1a00>{eventName}</color> ]";

	protected override string[] GetTargetMessageEvents()
	{
		return new string[1] { "OnCustomEvent" };
	}

	protected override void RegisterPorts()
	{
		onReceived = AddFlowOutput("Received");
		AddValueOutput("Receiver", () => receiver);
		AddValueOutput("Event Value", () => receivedValue);
	}

	public void OnCustomEvent(MessageRouter.MessageData<EventData> msg)
	{
		if (msg.value.name == eventName.value)
		{
			receiver = ResolveReceiver(msg.receiver);
			if (msg.value is EventData<T>)
			{
				receivedValue = (msg.value as EventData<T>).value;
			}
			onReceived.Call(default(Flow));
		}
	}
}
