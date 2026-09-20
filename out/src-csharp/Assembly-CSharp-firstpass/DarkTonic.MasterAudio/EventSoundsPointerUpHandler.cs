using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsPointerUpHandler : EventSoundsUGUIHandler, IPointerUpHandler, IEventSystemHandler
{
	public void OnPointerUp(PointerEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnPointerUp(data);
		}
	}
}
