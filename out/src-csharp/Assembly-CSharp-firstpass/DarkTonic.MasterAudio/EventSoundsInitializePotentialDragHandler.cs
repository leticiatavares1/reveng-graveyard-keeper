using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsInitializePotentialDragHandler : EventSoundsUGUIHandler, IInitializePotentialDragHandler, IEventSystemHandler
{
	public void OnInitializePotentialDrag(PointerEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnInitializePotentialDrag(data);
		}
	}
}
