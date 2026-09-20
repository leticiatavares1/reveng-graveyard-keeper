using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsDropHandler : EventSoundsUGUIHandler, IDropHandler, IEventSystemHandler
{
	public void OnDrop(PointerEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnDrop(data);
		}
	}
}
