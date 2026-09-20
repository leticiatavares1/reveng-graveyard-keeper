using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
	public List<EventDelegate> on_event = new List<EventDelegate>();

	public List<EventDelegate> on_event2 = new List<EventDelegate>();

	public List<EventDelegate> on_event3 = new List<EventDelegate>();

	public List<EventDelegate> on_event4 = new List<EventDelegate>();

	public List<EventDelegate> on_event5 = new List<EventDelegate>();

	public void OnEvent()
	{
		EventDelegate.Execute(on_event);
	}

	public void OnEvent2()
	{
		EventDelegate.Execute(on_event2);
	}

	public void OnEvent3()
	{
		EventDelegate.Execute(on_event3);
	}

	public void OnEvent4()
	{
		EventDelegate.Execute(on_event4);
	}

	public void OnEvent5()
	{
		EventDelegate.Execute(on_event5);
	}
}
