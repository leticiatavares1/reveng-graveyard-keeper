using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsCancelHandler : EventSoundsUGUIHandler, ICancelHandler, IEventSystemHandler
{
	public void OnCancel(BaseEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnCancel(data);
		}
	}
}
