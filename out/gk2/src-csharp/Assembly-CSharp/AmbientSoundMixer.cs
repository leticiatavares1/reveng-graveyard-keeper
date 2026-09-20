using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class AmbientSoundMixer
{
	private struct FadingOutEntry
	{
		public string id;

		public SoundHandler handler;

		public float volume;
	}

	private readonly Dictionary<string, SoundHandler> active = new Dictionary<string, SoundHandler>();

	private readonly Dictionary<string, float> playTimeCache = new Dictionary<string, float>();

	private readonly List<FadingOutEntry> fadingOut = new List<FadingOutEntry>();

	private string pairId1;

	private string pairId2;

	private float pairLerp;

	private string overrideId;

	private float overrideBlend;

	private float switchDuration = 5f;

	private float overrideTarget;

	public float SwitchDuration
	{
		get
		{
			return switchDuration;
		}
		set
		{
			switchDuration = Mathf.Max(0.01f, value);
		}
	}

	public void SetAmbientPair(string id1, string id2, float lerp, bool crossfade = true)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (pairId1 != id1 || pairId2 != id2)
		{
			if (crossfade)
			{
				FadeOutUnusedPairSounds(id1, id2);
			}
			else
			{
				HardStopUnusedPairSounds(id1, id2, pairId1, pairId2);
			}
		}
		pairId1 = id1;
		pairId2 = id2;
		pairLerp = Mathf.Clamp01(lerp);
		EnsurePlaying(pairId1);
		if (!string.IsNullOrEmpty(pairId2) && pairId2 != pairId1)
		{
			EnsurePlaying(pairId2);
		}
		ApplyVolumes();
	}

	public void SetOverride(string id, bool crossfade = true)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		string text = (string.IsNullOrEmpty(id) ? null : id);
		if (text == overrideId)
		{
			overrideTarget = ((text != null) ? 1f : 0f);
			if (!crossfade)
			{
				overrideBlend = overrideTarget;
				ApplyVolumes();
			}
			return;
		}
		if (!string.IsNullOrEmpty(overrideId) && active.TryGetValue(overrideId, out var value) && overrideId != pairId1 && overrideId != pairId2)
		{
			if (crossfade)
			{
				fadingOut.Add(new FadingOutEntry
				{
					id = overrideId,
					handler = value,
					volume = overrideBlend
				});
			}
			else
			{
				CacheAndStop(overrideId, value);
			}
			active.Remove(overrideId);
		}
		overrideId = text;
		overrideTarget = ((text != null) ? 1f : 0f);
		if (text != null)
		{
			EnsurePlaying(text);
		}
		if (!crossfade)
		{
			overrideBlend = overrideTarget;
		}
		ApplyVolumes();
	}

	public void Update(float deltaTime)
	{
		bool flag = false;
		if (!Mathf.Approximately(overrideBlend, overrideTarget))
		{
			float num = deltaTime / switchDuration;
			if (overrideBlend < overrideTarget)
			{
				overrideBlend = Mathf.Min(overrideBlend + num, overrideTarget);
			}
			else
			{
				overrideBlend = Mathf.Max(overrideBlend - num, overrideTarget);
			}
			flag = true;
		}
		for (int num2 = fadingOut.Count - 1; num2 >= 0; num2--)
		{
			FadingOutEntry value = fadingOut[num2];
			value.volume = Mathf.Max(0f, value.volume - deltaTime / switchDuration);
			if (value.handler != null && value.handler.IsActive)
			{
				value.handler.SetVolume(value.volume);
			}
			if (value.volume <= 0f)
			{
				CacheAndStop(value.id, value.handler);
				fadingOut.RemoveAt(num2);
			}
			else
			{
				fadingOut[num2] = value;
			}
		}
		if (flag)
		{
			ApplyVolumes();
		}
	}

	public void StopAll()
	{
		foreach (KeyValuePair<string, SoundHandler> item in active)
		{
			CacheAndStop(item.Key, item.Value);
		}
		active.Clear();
		for (int i = 0; i < fadingOut.Count; i++)
		{
			CacheAndStop(fadingOut[i].id, fadingOut[i].handler);
		}
		fadingOut.Clear();
		pairId1 = null;
		pairId2 = null;
		overrideId = null;
		overrideBlend = 0f;
		overrideTarget = 0f;
	}

	private void EnsurePlaying(string id)
	{
		if (string.IsNullOrEmpty(id) || (active.TryGetValue(id, out var value) && value != null && value.IsActive))
		{
			return;
		}
		for (int num = fadingOut.Count - 1; num >= 0; num--)
		{
			if (fadingOut[num].id == id)
			{
				SoundHandler handler = fadingOut[num].handler;
				fadingOut.RemoveAt(num);
				if (handler == null || !handler.IsActive)
				{
					break;
				}
				active[id] = handler;
				return;
			}
		}
		SoundHandler soundHandler = LazyAudio.Play(id, checkDelay: false);
		if (soundHandler == null)
		{
			Debug.LogWarning("AmbientSoundMixer: failed to play sound [" + id + "]");
			return;
		}
		if (playTimeCache.TryGetValue(id, out var value2))
		{
			soundHandler.SetTime(value2);
		}
		active[id] = soundHandler;
	}

	private void ApplyVolumes()
	{
		float num = 1f - overrideBlend;
		if (!string.IsNullOrEmpty(pairId1) && active.TryGetValue(pairId1, out var value) && value != null && value.IsActive)
		{
			float volume = ((pairId1 == pairId2 || string.IsNullOrEmpty(pairId2)) ? num : ((1f - pairLerp) * num));
			value.SetVolume(volume);
		}
		if (!string.IsNullOrEmpty(pairId2) && pairId2 != pairId1 && active.TryGetValue(pairId2, out var value2) && value2 != null && value2.IsActive)
		{
			value2.SetVolume(pairLerp * num);
		}
		if (!string.IsNullOrEmpty(overrideId) && active.TryGetValue(overrideId, out var value3) && value3 != null && value3.IsActive)
		{
			if (overrideId != pairId1 && overrideId != pairId2)
			{
				value3.SetVolume(overrideBlend);
			}
			else
			{
				value3.SetVolume(Mathf.Max(GetPairVolume(overrideId) * num, overrideBlend));
			}
		}
	}

	private float GetPairVolume(string id)
	{
		if (id == pairId1 && (pairId1 == pairId2 || string.IsNullOrEmpty(pairId2)))
		{
			return 1f;
		}
		if (id == pairId1)
		{
			return 1f - pairLerp;
		}
		if (id == pairId2)
		{
			return pairLerp;
		}
		return 0f;
	}

	private void FadeOutUnusedPairSounds(string newId1, string newId2)
	{
		float num = 1f - overrideBlend;
		List<string> list = null;
		foreach (KeyValuePair<string, SoundHandler> item in active)
		{
			string key = item.Key;
			if (key == newId1 || key == newId2 || key == overrideId)
			{
				continue;
			}
			bool flag = false;
			for (int i = 0; i < fadingOut.Count; i++)
			{
				if (fadingOut[i].id == key)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				float num2 = GetPairVolume(key) * num;
				if (num2 < 0.001f)
				{
					CacheAndStop(key, item.Value);
				}
				else
				{
					fadingOut.Add(new FadingOutEntry
					{
						id = key,
						handler = item.Value,
						volume = num2
					});
				}
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(key);
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				active.Remove(list[j]);
			}
		}
	}

	private void HardStopUnusedPairSounds(string newId1, string newId2, string oldId1, string oldId2)
	{
		List<string> list = null;
		foreach (KeyValuePair<string, SoundHandler> item in active)
		{
			string key = item.Key;
			if (!(key == newId1) && !(key == newId2) && !(key == overrideId))
			{
				CacheAndStop(key, item.Value);
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				active.Remove(list[i]);
			}
		}
		for (int num = fadingOut.Count - 1; num >= 0; num--)
		{
			string id = fadingOut[num].id;
			if ((!(id != oldId1) || !(id != oldId2)) && !(id == newId1) && !(id == newId2) && !(id == overrideId))
			{
				CacheAndStop(id, fadingOut[num].handler);
				fadingOut.RemoveAt(num);
			}
		}
	}

	private void CacheAndStop(string id, SoundHandler handler)
	{
		if (handler != null && handler.IsActive)
		{
			playTimeCache[id] = handler.GetTime();
			handler.Stop();
		}
	}
}
