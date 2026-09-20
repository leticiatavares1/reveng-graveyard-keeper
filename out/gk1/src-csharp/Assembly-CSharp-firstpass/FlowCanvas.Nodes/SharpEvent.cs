using System.Reflection;
using ParadoxNotion;

namespace FlowCanvas.Nodes;

public abstract class SharpEvent
{
	public object instance;

	public EventInfo eventInfo;

	public static SharpEvent Create(EventInfo eventInfo)
	{
		if (eventInfo == null)
		{
			return null;
		}
		SharpEvent obj = (SharpEvent)typeof(SharpEvent<>).RTMakeGenericType(eventInfo.EventHandlerType).CreateObject();
		obj.eventInfo = eventInfo;
		return obj;
	}

	public void StartListening(ReflectedDelegateEvent reflectedEvent, ReflectedDelegateEvent.DelegateEventCallback callback)
	{
		if (reflectedEvent != null && callback != null)
		{
			reflectedEvent.Add(callback);
			eventInfo.AddEventHandler(instance, reflectedEvent.AsDelegate());
		}
	}

	public void StopListening(ReflectedDelegateEvent reflectedEvent, ReflectedDelegateEvent.DelegateEventCallback callback)
	{
		if (reflectedEvent != null && callback != null)
		{
			reflectedEvent.Remove(callback);
			eventInfo.RemoveEventHandler(instance, reflectedEvent.AsDelegate());
		}
	}
}
public class SharpEvent<T> : SharpEvent
{
}
