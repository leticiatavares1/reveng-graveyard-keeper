using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsSelectHandler : EventSoundsUGUIHandler, ISelectHandler, IEventSystemHandler
{
	public void OnSelect(BaseEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnSelect(data);
		}
	}
}
