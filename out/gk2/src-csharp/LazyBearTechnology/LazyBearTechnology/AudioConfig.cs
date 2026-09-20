using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace LazyBearTechnology;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Lazy/AudioConfig", order = 1)]
public class AudioConfig : LazySingletonSO<AudioConfig>
{
	public AudioClip defaultClip;

	public AudioMixerGroup voiceOverGroup;

	public AudioMixerGroup mumbleGroup;

	public AudioMixer audioMixer;

	public List<Sound> sounds;

	[Space(20f)]
	public List<Playlist> playlists;

	public float dBQuietestValue = -36f;

	public const string JSON_CONFIG_PATH = "/AudioConfigData.json";

	private const string CONFIGS_FOLDER = "AudioConfigs";

	public Sound Get(string id)
	{
		return sounds.Find((Sound x) => x.id == id);
	}

	public AudioConfigData CreateConfigData()
	{
		AudioConfigData audioConfigData = new AudioConfigData();
		foreach (Sound sound in sounds)
		{
			SoundData soundData = new SoundData
			{
				id = sound.id,
				volume = sound.volume,
				panning = sound.panning
			};
			foreach (Sample sample in sound.samples)
			{
				soundData.samples.Add(new SampleData
				{
					clipName = ((sample.clip != null) ? sample.clip.name : ""),
					volume = sample.volume,
					pitch = sample.pitch,
					panning = sample.panning,
					pitchVariation = sample.pitchVariation
				});
			}
			audioConfigData.sounds.Add(soundData);
		}
		foreach (Playlist playlist in playlists)
		{
			PlaylistData playlistData = new PlaylistData
			{
				id = playlist.id,
				volume = playlist.volume
			};
			foreach (Track track in playlist.tracks)
			{
				playlistData.tracks.Add(new TrackData
				{
					clipName = ((track.clip != null) ? track.clip.name : ""),
					volume = track.volume,
					pitch = track.pitch,
					panning = track.panning
				});
			}
			audioConfigData.playlists.Add(playlistData);
		}
		return audioConfigData;
	}

	public void ApplyConfigData(AudioConfigData config)
	{
		if (config == null)
		{
			return;
		}
		foreach (SoundData soundData in config.sounds)
		{
			Sound sound = sounds.Find((Sound s) => s.id == soundData.id);
			if (sound == null)
			{
				continue;
			}
			sound.volume = soundData.volume;
			sound.panning = soundData.panning;
			foreach (SampleData sampleData in soundData.samples)
			{
				Sample sample = sound.samples.Find((Sample s) => s.clip != null && s.clip.name == sampleData.clipName);
				if (sample != null)
				{
					sample.volume = sampleData.volume;
					sample.pitch = sampleData.pitch;
					sample.panning = sampleData.panning;
					sample.pitchVariation = sampleData.pitchVariation;
				}
			}
		}
		foreach (PlaylistData playlistData in config.playlists)
		{
			Playlist playlist = playlists.Find((Playlist p) => p.id == playlistData.id);
			if (playlist == null)
			{
				continue;
			}
			playlist.volume = playlistData.volume;
			foreach (TrackData trackData in playlistData.tracks)
			{
				Track track = playlist.tracks.Find((Track t) => t.clip != null && t.clip.name == trackData.clipName);
				if (track != null)
				{
					track.volume = trackData.volume;
					track.pitch = trackData.pitch;
					track.panning = trackData.panning;
				}
			}
		}
	}
}
