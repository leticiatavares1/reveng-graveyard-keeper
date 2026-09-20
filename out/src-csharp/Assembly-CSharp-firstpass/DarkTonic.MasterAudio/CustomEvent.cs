using System;

namespace DarkTonic.MasterAudio;

[Serializable]
public class CustomEvent
{
	public string EventName;

	public string ProspectiveName;

	public bool IsEditing;

	public bool eventExpanded = true;

	public MasterAudio.CustomEventReceiveMode eventReceiveMode;

	public float distanceThreshold = 1f;

	public MasterAudio.EventReceiveFilter eventRcvFilterMode;

	public int filterModeQty = 1;

	public bool isTemporary;

	public int frameLastFired = -1;

	public string categoryName = "[Uncategorized]";

	public CustomEvent(string eventName)
	{
		EventName = eventName;
		ProspectiveName = eventName;
	}
}
