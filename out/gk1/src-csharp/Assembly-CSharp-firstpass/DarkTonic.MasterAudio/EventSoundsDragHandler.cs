using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsDragHandler : EventSoundsUGUIHandler, IDragHandler, IEventSystemHandler
{
	public void OnDrag(PointerEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnDrag(data);
		}
	}
}
