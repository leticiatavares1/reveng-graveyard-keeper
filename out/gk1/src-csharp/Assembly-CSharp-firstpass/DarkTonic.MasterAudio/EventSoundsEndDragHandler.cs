using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsEndDragHandler : EventSoundsUGUIHandler, IEndDragHandler, IEventSystemHandler
{
	public void OnEndDrag(PointerEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnEndDrag(data);
		}
	}
}
