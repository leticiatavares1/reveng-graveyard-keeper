using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsScrollHandler : EventSoundsUGUIHandler, IScrollHandler, IEventSystemHandler
{
	public void OnScroll(PointerEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnScroll(data);
		}
	}
}
