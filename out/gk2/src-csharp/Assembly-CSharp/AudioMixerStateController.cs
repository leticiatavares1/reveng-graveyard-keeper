using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerStateController
{
	private const float SnapshotTransitionSeconds = 0.01f;

	private readonly Dictionary<AudioMixerSnapshotLayer, AudioMixerSnapshot> snapshots = new Dictionary<AudioMixerSnapshotLayer, AudioMixerSnapshot>();

	private readonly HashSet<AudioMixerSnapshotLayer> activeLayers = new HashSet<AudioMixerSnapshotLayer> { AudioMixerSnapshotLayer.Default };

	private readonly bool isAvailable;

	public static AudioMixerStateController Unavailable { get; } = new AudioMixerStateController();


	public AudioMixerSnapshotLayer ActiveLayer => ResolveActiveLayer();

	private AudioMixerStateController()
	{
	}

	public AudioMixerStateController(AudioMixer audioMixer)
	{
		if (audioMixer == null)
		{
			Debug.LogError("AudioMixerStateController: audioMixer is null");
			return;
		}
		isAvailable = true;
		Register(audioMixer, AudioMixerSnapshotLayer.Default, "Default");
		Register(audioMixer, AudioMixerSnapshotLayer.Indoor, "Indoor");
		Register(audioMixer, AudioMixerSnapshotLayer.Cinematics, "Cinematics");
		Register(audioMixer, AudioMixerSnapshotLayer.MainMenu, "MainMenu");
		Register(audioMixer, AudioMixerSnapshotLayer.NotDirectlyInGame, "NotDirectlyInGame");
		ApplyActive();
	}

	public void Push(AudioMixerSnapshotLayer layer)
	{
		if (isAvailable && layer != 0 && activeLayers.Add(layer))
		{
			ApplyActive();
		}
	}

	public void Pop(AudioMixerSnapshotLayer layer)
	{
		if (isAvailable && layer != 0 && activeLayers.Remove(layer))
		{
			ApplyActive();
		}
	}

	public void SetLayerActive(AudioMixerSnapshotLayer layer, bool active)
	{
		if (isAvailable)
		{
			if (active)
			{
				Push(layer);
			}
			else
			{
				Pop(layer);
			}
		}
	}

	public void ResetToDefault()
	{
		if (isAvailable)
		{
			activeLayers.Clear();
			activeLayers.Add(AudioMixerSnapshotLayer.Default);
			ApplyActive();
		}
	}

	private void ApplyActive()
	{
		AudioMixerSnapshotLayer audioMixerSnapshotLayer = ResolveActiveLayer();
		if (!snapshots.TryGetValue(audioMixerSnapshotLayer, out var value) || value == null)
		{
			Debug.LogError($"AudioMixerStateController: snapshot for [{audioMixerSnapshotLayer}] is missing");
			return;
		}
		value.TransitionTo(0.01f);
		InWorldSfxFilterController.Reapply();
	}

	private AudioMixerSnapshotLayer ResolveActiveLayer()
	{
		AudioMixerSnapshotLayer audioMixerSnapshotLayer = AudioMixerSnapshotLayer.Default;
		foreach (AudioMixerSnapshotLayer activeLayer in activeLayers)
		{
			if (activeLayer > audioMixerSnapshotLayer)
			{
				audioMixerSnapshotLayer = activeLayer;
			}
		}
		return audioMixerSnapshotLayer;
	}

	private void Register(AudioMixer audioMixer, AudioMixerSnapshotLayer layer, string snapshotName)
	{
		if (audioMixer == null)
		{
			Debug.LogError("AudioMixerStateController: cannot register snapshot [" + snapshotName + "], audioMixer is null");
			snapshots[layer] = null;
			return;
		}
		AudioMixerSnapshot audioMixerSnapshot = audioMixer.FindSnapshot(snapshotName);
		if (audioMixerSnapshot == null)
		{
			Debug.LogError("AudioMixerStateController: snapshot [" + snapshotName + "] not found on [" + audioMixer.name + "]");
		}
		snapshots[layer] = audioMixerSnapshot;
	}
}
