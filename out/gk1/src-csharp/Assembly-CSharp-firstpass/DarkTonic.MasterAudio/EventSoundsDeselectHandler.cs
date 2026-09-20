using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsDeselectHandler : EventSoundsUGUIHandler, IDeselectHandler, IEventSystemHandler
{
	public void OnDeselect(BaseEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnDeselect(data);
		}
	}
}
