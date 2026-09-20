using System;

[Serializable]
public class DelayedEvent
{
	public string eventName;

	public float delayTime;

	public DelayedEvent(string eventName, float delayTime)
	{
		this.eventName = eventName;
		this.delayTime = delayTime;
	}
}
