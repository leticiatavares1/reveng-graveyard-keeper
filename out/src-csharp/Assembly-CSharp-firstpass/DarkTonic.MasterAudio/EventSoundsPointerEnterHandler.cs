using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsPointerEnterHandler : EventSoundsUGUIHandler, IPointerEnterHandler, IEventSystemHandler
{
	public void OnPointerEnter(PointerEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnPointerEnter(data);
		}
	}
}
