using System;
using System.Collections.Generic;

namespace Microsoft.Mixer;

[Serializable]
public class InteractiveParticipant
{
	internal string _etag;

	internal string _groupID;

	internal string _sessionID;

	private List<string> channelGroups;

	public uint Level { get; internal set; }

	public uint UserID { get; internal set; }

	public string UserName { get; internal set; }

	public DateTime ConnectedAt { get; internal set; }

	public InteractiveGroup Group
	{
		get
		{
			foreach (InteractiveGroup item in InteractivityManager.SingletonInstance.Groups as List<InteractiveGroup>)
			{
				if (item.GroupID == _groupID)
				{
					return item;
				}
			}
			return new InteractiveGroup("default");
		}
		set
		{
			if (value == null)
			{
				InteractivityManager.SingletonInstance._LogError("Error: You cannot assign 'null' as the group value.");
				return;
			}
			_groupID = value.GroupID;
			InteractivityManager.SingletonInstance._SendUpdateParticipantsMessage(this);
		}
	}

	public DateTime LastInputAt { get; internal set; }

	public bool IsBroadcaster
	{
		get
		{
			if (channelGroups.Contains("Owner"))
			{
				return true;
			}
			return false;
		}
	}

	public bool InputDisabled { get; internal set; }

	public IList<InteractiveButtonControl> Buttons
	{
		get
		{
			List<InteractiveButtonControl> list = new List<InteractiveButtonControl>();
			if (InteractivityManager._buttonStatesByParticipant.TryGetValue(UserID, out var value))
			{
				foreach (string key in value.Keys)
				{
					foreach (InteractiveButtonControl button in InteractivityManager.SingletonInstance.Buttons)
					{
						if (key == button.ControlID)
						{
							list.Add(button);
							break;
						}
					}
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
			if (InteractivityManager._joystickStatesByParticipant.TryGetValue(UserID, out var value))
			{
				foreach (string key in value.Keys)
				{
					foreach (InteractiveJoystickControl joystick in InteractivityManager.SingletonInstance.Joysticks)
					{
						if (key == joystick.ControlID)
						{
							list.Add(joystick);
							break;
						}
					}
				}
			}
			return list;
		}
	}

	public InteractiveParticipantState State { get; internal set; }

	internal InteractiveParticipant(string newSessionID, string newEtag, uint userID, string newGroupID, string userName, List<string> newChannelGroups, uint level, DateTime lastInputAt, DateTime connectedAt, bool inputDisabled, InteractiveParticipantState state)
	{
		_sessionID = newSessionID;
		UserID = userID;
		UserName = userName;
		channelGroups = newChannelGroups;
		Level = level;
		LastInputAt = lastInputAt;
		ConnectedAt = connectedAt;
		InputDisabled = inputDisabled;
		State = state;
		_groupID = newGroupID;
		_etag = newEtag;
	}
}
