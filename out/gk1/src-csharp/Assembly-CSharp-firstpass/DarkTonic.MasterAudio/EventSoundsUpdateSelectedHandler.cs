using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsUpdateSelectedHandler : EventSoundsUGUIHandler, IUpdateSelectedHandler, IEventSystemHandler
{
	public void OnUpdateSelected(BaseEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnUpdateSelected(data);
		}
	}
}
