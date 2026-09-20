using System;
using System.Collections.Generic;

namespace Microsoft.Mixer;

[Serializable]
public class InteractiveGroup
{
	internal string _etag;

	public string GroupID { get; internal set; }

	public List<InteractiveParticipant> Participants
	{
		get
		{
			List<InteractiveParticipant> list = new List<InteractiveParticipant>();
			foreach (InteractiveParticipant item in InteractivityManager.SingletonInstance.Participants as List<InteractiveParticipant>)
			{
				if (item._groupID == GroupID)
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	public string SceneID { get; internal set; }

	public void SetScene(string sceneID)
	{
		SceneID = sceneID;
		InteractivityManager.SingletonInstance._SetCurrentSceneInternal(this, sceneID);
	}

	public InteractiveGroup(string groupID)
	{
		if (InteractivityManager.SingletonInstance.InteractivityState != InteractivityState.InteractivityEnabled && InteractivityManager.SingletonInstance.InteractivityState != InteractivityState.Initialized)
		{
			throw new Exception("Error: The InteractivityManager must be initialized and connected to the service to create new groups.");
		}
		GroupID = groupID;
		InteractivityManager.SingletonInstance._SendCreateGroupsMessage(GroupID, "default");
	}

	public InteractiveGroup(string groupID, string sceneID)
	{
		if (InteractivityManager.SingletonInstance.InteractivityState != InteractivityState.InteractivityEnabled && InteractivityManager.SingletonInstance.InteractivityState != InteractivityState.Initialized)
		{
			throw new Exception("Error: The InteractivityManager must be initialized and connected to the service to create new groups.");
		}
		GroupID = groupID;
		SceneID = sceneID;
		InteractivityManager.SingletonInstance._SendCreateGroupsMessage(GroupID, SceneID);
	}

	internal InteractiveGroup(string newEtag, string sceneID, string groupID)
	{
		_etag = newEtag;
		SceneID = sceneID;
		GroupID = groupID;
	}
}
