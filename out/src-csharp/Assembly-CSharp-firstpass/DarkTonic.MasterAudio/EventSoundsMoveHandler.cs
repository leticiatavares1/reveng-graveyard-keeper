using UnityEngine.EventSystems;

namespace DarkTonic.MasterAudio;

public class EventSoundsMoveHandler : EventSoundsUGUIHandler, IMoveHandler, IEventSystemHandler
{
	public void OnMove(AxisEventData data)
	{
		if (base.eventSounds != null)
		{
			base.eventSounds.OnMove(data);
		}
	}
}
