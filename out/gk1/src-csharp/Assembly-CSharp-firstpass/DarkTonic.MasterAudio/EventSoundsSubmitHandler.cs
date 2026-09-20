using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsSubmitHandler : EventSoundsUGUIHandler, ISubmitHandler, IEventSystemHandler
{
	public void OnSubmit(BaseEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnSubmit(data);
		}
	}
}
