using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsBeginDragHandler : EventSoundsUGUIHandler, IBeginDragHandler, IEventSystemHandler
{
	public void OnBeginDrag(PointerEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnBeginDrag(data);
		}
	}
}
