using System;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace DarkTonic.MasterAudio;

[Serializable]
public class GroupBus
{
	public string busName;

	public float volume = 1f;

	public bool isSoloed;

	public bool isMuted;

	public int voiceLimit = -1;

	public bool stopOldest;

	public bool isExisting;

	public bool isTemporary;

	public bool isUsingOcclusion;

	public AudioMixerGroup mixerChannel;

	public bool forceTo2D;

	private readonly List<int> _activeAudioSourcesIds = new List<int>(50);

	private float _originalVolume = 1f;

	public int ActiveVoices => _activeAudioSourcesIds.Count;

	public bool BusVoiceLimitReached
	{
		get
		{
			if (voiceLimit <= 0)
			{
				return false;
			}
			return _activeAudioSourcesIds.Count >= voiceLimit;
		}
	}

	public float OriginalVolume
	{
		get
		{
			return _originalVolume;
		}
		set
		{
			_originalVolume = value;
		}
	}

	public void AddActiveAudioSourceId(int id)
	{
		if (!_activeAudioSourcesIds.Contains(id))
		{
			_activeAudioSourcesIds.Add(id);
		}
	}

	public void RemoveActiveAudioSourceId(int id)
	{
		_activeAudioSourcesIds.Remove(id);
	}
}
