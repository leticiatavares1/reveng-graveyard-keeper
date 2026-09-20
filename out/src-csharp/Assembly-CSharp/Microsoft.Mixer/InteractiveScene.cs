using System;
using System.Collections.Generic;

namespace Microsoft.Mixer;

[Serializable]
public class InteractiveScene
{
	internal string _etag;

	public string SceneID { get; internal set; }

	public IList<InteractiveButtonControl> Buttons
	{
		get
		{
			List<InteractiveButtonControl> list = new List<InteractiveButtonControl>();
			foreach (InteractiveButtonControl item in InteractivityManager.SingletonInstance.Buttons as List<InteractiveButtonControl>)
			{
				if (item._sceneID == SceneID)
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	public IList<InteractiveJoystickControl> Joysticks
	{
		get
		{
			List<InteractiveJoystickControl> list = new List<InteractiveJoystickControl>();
			foreach (InteractiveJoystickControl item in InteractivityManager.SingletonInstance.Joysticks as List<InteractiveJoystickControl>)
			{
				if (item._sceneID == SceneID)
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	public IList<InteractiveGroup> Groups
	{
		get
		{
			List<InteractiveGroup> list = new List<InteractiveGroup>();
			foreach (InteractiveGroup item in InteractivityManager.SingletonInstance.Groups as List<InteractiveGroup>)
			{
				if (item.SceneID == SceneID)
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	public InteractiveButtonControl GetButton(string controlID)
	{
		return InteractivityManager.SingletonInstance.GetButton(controlID);
	}

	public InteractiveJoystickControl GetJoystick(string controlID)
	{
		return InteractivityManager.SingletonInstance.GetJoystick(controlID);
	}

	internal InteractiveScene(string sceneID = "", string newEtag = "")
	{
		SceneID = sceneID;
		_etag = newEtag;
	}
}
