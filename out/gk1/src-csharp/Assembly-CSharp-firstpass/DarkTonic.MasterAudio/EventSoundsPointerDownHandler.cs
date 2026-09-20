using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsPointerDownHandler : EventSoundsUGUIHandler, IPointerDownHandler, IEventSystemHandler
{
	public void OnPointerDown(PointerEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnPointerDown(data);
		}
	}
}
