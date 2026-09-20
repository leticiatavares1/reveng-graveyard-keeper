using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

public class MA_SampleICustomEventReceiver : MonoBehaviour, ICustomEventReceiver
{
	private readonly List<string> _eventsSubscribedTo = new List<string> { "PlayerMoved", "PlayerStoppedMoving" };

	private void Awake()
	{
	}

	private void Start()
	{
		CheckForIllegalCustomEvents();
	}

	private void OnEnable()
	{
		RegisterReceiver();
	}

	private void OnDisable()
	{
		if (!(MasterAudio.SafeInstance == null) && !MasterAudio.AppIsShuttingDown)
		{
			UnregisterReceiver();
		}
	}

	public void CheckForIllegalCustomEvents()
	{
		for (int i = 0; i < _eventsSubscribedTo.Count; i++)
		{
			if (!MasterAudio.CustomEventExists(_eventsSubscribedTo[i]))
			{
				Debug.LogError("Custom Event, listened to by '" + base.name + "', could not be found in MasterAudio.");
			}
		}
	}

	public void ReceiveEvent(string customEventName, Vector3 originPoint)
	{
		switch (customEventName)
		{
		case "PlayerMoved":
			Debug.Log("PlayerMoved event recieved by '" + base.name + "'.");
			break;
		case "PlayerStoppedMoving":
			Debug.Log("PlayerStoppedMoving event recieved by '" + base.name + "'.");
			break;
		}
	}

	public bool SubscribesToEvent(string customEventName)
	{
		if (string.IsNullOrEmpty(customEventName))
		{
			return false;
		}
		return _eventsSubscribedTo.Contains(customEventName);
	}

	public void RegisterReceiver()
	{
		MasterAudio.AddCustomEventReceiver(this, base.transform);
	}

	public void UnregisterReceiver()
	{
		MasterAudio.RemoveCustomEventReceiver(this);
	}

	public IList<AudioEventGroup> GetAllEvents()
	{
		List<AudioEventGroup> list = new List<AudioEventGroup>();
		for (int i = 0; i < _eventsSubscribedTo.Count; i++)
		{
			list.Add(new AudioEventGroup
			{
				customEventName = _eventsSubscribedTo[i]
			});
		}
		return list;
	}
}
