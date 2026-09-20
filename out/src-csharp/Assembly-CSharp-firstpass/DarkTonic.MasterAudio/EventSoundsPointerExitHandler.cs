using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsPointerExitHandler : EventSoundsUGUIHandler, IPointerExitHandler, IEventSystemHandler
{
	public void OnPointerExit(PointerEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnPointerExit(data);
		}
	}
}
