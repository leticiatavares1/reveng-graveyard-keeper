using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace LazyBearTechnology;

public class PlaylistController : MonoBehaviour
{
	private class FadeData
	{
		public AudioSource audioSource;

		public float maxVolume;
	}

	[SerializeField]
	private string id;

	[Space(20f)]
	[SerializeField]
	private List<Track> playedTracks = new List<Track>();

	[SerializeField]
	private List<Track> notPlayedTracks = new List<Track>();

	[SerializeField]
	private Track lastTrack;

	[Space(20f)]
	[SerializeField]
	private bool isPaused;

	[SerializeField]
	private bool isPlaying;

	private Playlist playlist;

	private AudioSource audioSource1;

	private AudioSource audioSource2;

	private AudioSource activeAudioSource;

	private bool isCrossfade;

	private float currentFadeTime;

	private float activeCrossfadeDuration;

	private float pendingFadeDuration = -1f;

	private bool isPauseFading;

	private float pauseFadeTime;

	private float pauseFadeDuration;

	private float pauseFadeStartVolume;

	private float pausedVolume;

	private bool pausedWithFade;

	private bool isUnpauseFading;

	private float unpauseFadeTime;

	private float unpauseFadeDuration;

	private float unpauseFadeTargetVolume;

	private Tween pauseFadeTween;

	private Tween unpauseFadeTween;

	private FadeData fadeOutData = new FadeData();

	private FadeData fadeInData = new FadeData();

	private bool isInitialized;

	public Track LastTrack => lastTrack;

	public Playlist Playlist => playlist;

	public bool IsActive
	{
		get
		{
			if (!isPlaying)
			{
				return isPaused;
			}
			return true;
		}
	}

	public bool IsPaused => isPaused;

	public string Id => id;

	public void Initialize(Playlist playlist)
	{
		if (!isInitialized)
		{
			id = playlist.id;
			this.playlist = playlist;
			base.name = "Playlist: " + id;
			GameObject gameObject = new GameObject();
			gameObject.transform.SetParent(base.transform);
			audioSource1 = gameObject.AddComponent<AudioSource>();
			gameObject = new GameObject();
			audioSource1.outputAudioMixerGroup = playlist.group;
			gameObject.transform.SetParent(base.transform);
			audioSource2 = gameObject.AddComponent<AudioSource>();
			audioSource2.outputAudioMixerGroup = playlist.group;
			PrepareTracks();
			isInitialized = true;
		}
	}

	public void Play()
	{
		if (isPaused)
		{
			float num = pendingFadeDuration;
			pendingFadeDuration = -1f;
			if (num >= 0f)
			{
				UnPause(num);
			}
			else
			{
				UnPause();
			}
		}
		else
		{
			CancelVolumeFades();
			if (isPlaying)
			{
				pendingFadeDuration = -1f;
				return;
			}
			isPlaying = true;
			NextTrack();
		}
	}

	public void Play(float fadeDuration)
	{
		pendingFadeDuration = fadeDuration;
		Play();
	}

	public void UnPause()
	{
		CancelVolumeFades();
		if (isPlaying && isPaused)
		{
			isPaused = false;
			if (pausedWithFade)
			{
				activeAudioSource.volume = pausedVolume;
				pausedWithFade = false;
			}
			activeAudioSource.UnPause();
		}
	}

	public void UnPause(float duration)
	{
		if (isPlaying && isPaused)
		{
			if (duration <= 0f)
			{
				UnPause();
				return;
			}
			CancelVolumeFades();
			BeginUnpauseFade(duration);
		}
	}

	public void UnPause(float duration, Ease ease)
	{
		if (isPlaying && isPaused)
		{
			if (duration <= 0f)
			{
				UnPause();
				return;
			}
			CancelVolumeFades();
			BeginUnpauseFade(duration, ease);
		}
	}

	public void Pause()
	{
		CancelVolumeFades();
		if (isPlaying && !isPaused)
		{
			activeAudioSource.Pause();
			isPaused = true;
		}
	}

	public void Pause(float duration)
	{
		if (isPlaying && !isPaused)
		{
			if (duration <= 0f)
			{
				Pause();
				return;
			}
			CancelVolumeFades();
			BeginPauseFade(duration);
		}
	}

	public void Pause(float duration, Ease ease)
	{
		if (isPlaying && !isPaused)
		{
			if (duration <= 0f)
			{
				Pause();
				return;
			}
			CancelVolumeFades();
			BeginPauseFade(duration, ease);
		}
	}

	public void Stop()
	{
		if (isPlaying)
		{
			CancelVolumeFades();
			isPlaying = false;
			isPaused = false;
			pausedWithFade = false;
			FadeOutActiveAudioSource();
		}
	}

	public void Stop(float fadeDuration)
	{
		if (isPlaying)
		{
			if (fadeDuration <= 0f)
			{
				StopImmediately();
				return;
			}
			CancelVolumeFades();
			isPlaying = false;
			isPaused = false;
			pausedWithFade = false;
			FadeOutActiveAudioSource(fadeDuration);
		}
	}

	public void StopImmediately()
	{
		CancelVolumeFades();
		audioSource1?.Stop();
		audioSource2?.Stop();
		if (isPlaying)
		{
			isPlaying = false;
			isPaused = false;
			pausedWithFade = false;
			activeAudioSource?.Stop();
		}
	}

	public float GetActiveSourcePlaybackPosition()
	{
		if (!(activeAudioSource == null))
		{
			return activeAudioSource.time;
		}
		return 0f;
	}

	public void NextTrack()
	{
		if (isPlaying)
		{
			isPaused = false;
			PlayTrack(GetNextTrack());
		}
	}

	public void PlayTrack(string trackId)
	{
		PlayTrackInternal(trackId);
	}

	public void PlayTrack(string trackId, float fadeDuration)
	{
		pendingFadeDuration = fadeDuration;
		PlayTrackInternal(trackId);
	}

	private void PlayTrackInternal(string trackId)
	{
		Track track = GetTrackById(trackId);
		if (track == null)
		{
			pendingFadeDuration = -1f;
			Debug.LogError("Audio System error: cannot find track " + trackId + " in playlist " + playlist.id);
			return;
		}
		if (playlist.weightRandomized)
		{
			if (isPlaying && track == lastTrack)
			{
				if (isPaused)
				{
					Play();
				}
				else
				{
					pendingFadeDuration = -1f;
				}
			}
			else
			{
				PlayTrack(track);
				lastTrack = track;
			}
			return;
		}
		if (isPlaying)
		{
			if (track == lastTrack)
			{
				if (isPaused)
				{
					Play();
				}
				else
				{
					pendingFadeDuration = -1f;
				}
				return;
			}
			int num = playedTracks.FindIndex((Track x) => x == track);
			if (num != -1)
			{
				playedTracks.RemoveAt(num);
				notPlayedTracks.Insert(0, track);
			}
			else
			{
				num = notPlayedTracks.FindIndex((Track x) => x == track);
				notPlayedTracks.RemoveAt(num);
				notPlayedTracks.Insert(0, track);
			}
			isPlaying = true;
			NextTrack();
			return;
		}
		if (track == lastTrack)
		{
			notPlayedTracks.Insert(0, lastTrack);
			lastTrack = null;
		}
		else
		{
			int num2 = playedTracks.FindIndex((Track x) => x == track);
			if (num2 != -1)
			{
				playedTracks.RemoveAt(num2);
				notPlayedTracks.Insert(0, track);
			}
			else
			{
				num2 = notPlayedTracks.FindIndex((Track x) => x == track);
				notPlayedTracks.RemoveAt(num2);
				notPlayedTracks.Insert(0, track);
			}
		}
		isPlaying = true;
		NextTrack();
	}

	public void SetWeightTrack(string trackId, float weight)
	{
		Track trackById = GetTrackById(trackId);
		if (trackById != null)
		{
			trackById.weight = weight;
			if (trackById.weight < 0f)
			{
				trackById.weight = 0f;
			}
		}
		else
		{
			Debug.LogError("Cannot change weight for trackId [" + trackId + "]. Track not found.");
		}
	}

	public void AddWeightTrack(string trackId, float weight)
	{
		Track trackById = GetTrackById(trackId);
		if (trackById != null)
		{
			SetWeightTrack(trackId, trackById.weight + weight);
		}
		else
		{
			Debug.LogError("Cannot change weight for trackId [" + trackId + "]. Track not found.");
		}
	}

	private void PrepareTracks()
	{
		notPlayedTracks.Clear();
		playedTracks.Clear();
		lastTrack = null;
		if (playlist.tracks.Count < 1)
		{
			Debug.LogError("Audio System Playlist error: not enough tracks in playlist " + playlist.id);
			return;
		}
		if (playlist.tracks.Count == 1)
		{
			Debug.LogWarning("Audio System Playlist playlist " + playlist.id + " contains only one track");
		}
		playedTracks.AddRange(playlist.tracks);
		if (playlist.shuffled)
		{
			while (playedTracks.Count != 0)
			{
				notPlayedTracks.Add(playedTracks.PopRandom());
			}
		}
		else
		{
			notPlayedTracks.AddRange(playedTracks);
		}
		playedTracks.Clear();
	}

	private Track GetNextTrack()
	{
		Track result = null;
		if (playlist.weightRandomized)
		{
			float maxInclusive = CalculateTotalWeight();
			float num = Random.Range(0f, maxInclusive);
			float num2 = 0f;
			for (int i = 0; i < playlist.tracks.Count; i++)
			{
				float weight = playlist.tracks[i].weight;
				if (((double)Mathf.Abs(num2 + weight - num) <= 0.001 || num2 + weight > num) && (double)Mathf.Abs(weight) >= 0.001)
				{
					result = playlist.tracks[i];
					break;
				}
				num2 += weight;
			}
		}
		else
		{
			if (notPlayedTracks.Count == 0)
			{
				if (playlist.tracks.Count == 1)
				{
					notPlayedTracks.Add(playlist.tracks[0]);
				}
				else if (playlist.shuffled)
				{
					while (playedTracks.Count != 0)
					{
						notPlayedTracks.Add(playedTracks.PopRandom());
					}
					if (lastTrack != null && notPlayedTracks.Count > 1)
					{
						int index = Random.Range(1, notPlayedTracks.Count);
						notPlayedTracks.Insert(index, lastTrack);
					}
				}
				else
				{
					lastTrack = null;
					notPlayedTracks.AddRange(playlist.tracks);
					playedTracks.Clear();
				}
			}
			else if (lastTrack != null)
			{
				playedTracks.Add(lastTrack);
			}
			result = notPlayedTracks[0];
			notPlayedTracks.RemoveAt(0);
		}
		lastTrack = result;
		return result;
	}

	private Track GetTrackById(string id)
	{
		return playlist.tracks.Find((Track x) => x.id == id);
	}

	private void PlayTrack(Track track)
	{
		if (track != null)
		{
			AudioSource freeAudioSource = GetFreeAudioSource();
			freeAudioSource.clip = track.clip;
			freeAudioSource.volume = track.volume * playlist.volume;
			freeAudioSource.panStereo = track.panning;
			freeAudioSource.pitch = track.pitch;
			freeAudioSource.outputAudioMixerGroup = playlist.group;
			freeAudioSource.loop = track.loop;
			float num = ((pendingFadeDuration >= 0f) ? pendingFadeDuration : playlist.fadeDuration);
			pendingFadeDuration = -1f;
			if (num <= 0f)
			{
				if (isCrossfade)
				{
					fadeOutData.audioSource?.Stop();
					fadeInData.audioSource?.Stop();
					isCrossfade = false;
				}
				else if (activeAudioSource != null && activeAudioSource != freeAudioSource)
				{
					activeAudioSource.Stop();
				}
				freeAudioSource.Play();
				activeAudioSource = freeAudioSource;
				currentFadeTime = 0f;
				return;
			}
			freeAudioSource.Play();
			activeCrossfadeDuration = num;
			if (isCrossfade)
			{
				if (fadeInData.audioSource != null)
				{
					fadeOutData.audioSource = fadeInData.audioSource;
					fadeOutData.maxVolume = fadeInData.audioSource.volume;
				}
				else
				{
					fadeOutData.audioSource?.Stop();
					fadeOutData.audioSource = null;
				}
				fadeInData.audioSource = freeAudioSource;
				fadeInData.maxVolume = freeAudioSource.volume;
				freeAudioSource.volume = 0f;
			}
			else
			{
				isCrossfade = true;
				fadeInData.audioSource = freeAudioSource;
				fadeInData.maxVolume = freeAudioSource.volume;
				freeAudioSource.volume = 0f;
				if (activeAudioSource != null)
				{
					fadeOutData.audioSource = activeAudioSource;
					fadeOutData.maxVolume = activeAudioSource.volume;
				}
				else
				{
					fadeOutData.audioSource = null;
				}
			}
			currentFadeTime = 0f;
			activeAudioSource = freeAudioSource;
		}
		else
		{
			Debug.LogError("Trying to play null track in " + playlist.id);
			StopImmediately();
		}
	}

	private AudioSource GetFreeAudioSource()
	{
		if (activeAudioSource != null)
		{
			if (!(activeAudioSource == audioSource1))
			{
				return audioSource1;
			}
			return audioSource2;
		}
		return audioSource1;
	}

	private void BeginPauseFade(float duration)
	{
		ResolveActiveCrossfade();
		isPauseFading = true;
		pauseFadeTime = 0f;
		pauseFadeDuration = duration;
		pauseFadeStartVolume = activeAudioSource.volume;
		pausedVolume = pauseFadeStartVolume;
	}

	private void BeginUnpauseFade(float duration)
	{
		PrepareUnpauseFade();
		isUnpauseFading = true;
		unpauseFadeTime = 0f;
		unpauseFadeDuration = duration;
	}

	private void BeginPauseFade(float duration, Ease ease)
	{
		ResolveActiveCrossfade();
		pausedVolume = activeAudioSource.volume;
		pauseFadeTween = activeAudioSource.DOFade(0f, duration).SetEase(ease).OnComplete(CompletePauseFade);
	}

	private void BeginUnpauseFade(float duration, Ease ease)
	{
		PrepareUnpauseFade();
		unpauseFadeTween = activeAudioSource.DOFade(unpauseFadeTargetVolume, duration).SetEase(ease).OnComplete(CompleteUnpauseFade);
	}

	private void PrepareUnpauseFade()
	{
		ResolveActiveCrossfade();
		unpauseFadeTargetVolume = (pausedWithFade ? pausedVolume : activeAudioSource.volume);
		isPaused = false;
		pausedWithFade = false;
		activeAudioSource.volume = 0f;
		activeAudioSource.UnPause();
	}

	private void ResolveActiveCrossfade()
	{
		if (isCrossfade)
		{
			fadeOutData.audioSource?.Stop();
			if (fadeInData.audioSource != null)
			{
				fadeInData.audioSource.volume = fadeInData.maxVolume;
			}
			isCrossfade = false;
		}
	}

	private void CompletePauseFade()
	{
		isPauseFading = false;
		pauseFadeTween = null;
		pausedWithFade = true;
		activeAudioSource.Pause();
		isPaused = true;
	}

	private void CompleteUnpauseFade()
	{
		isUnpauseFading = false;
		unpauseFadeTween = null;
		activeAudioSource.volume = unpauseFadeTargetVolume;
	}

	private void CancelVolumeFades()
	{
		isPauseFading = false;
		isUnpauseFading = false;
		pauseFadeTween?.Kill();
		pauseFadeTween = null;
		unpauseFadeTween?.Kill();
		unpauseFadeTween = null;
	}

	private void FadeOutActiveAudioSource(float? fadeDuration = null)
	{
		isCrossfade = true;
		fadeOutData.audioSource = activeAudioSource;
		fadeOutData.maxVolume = activeAudioSource.volume;
		fadeInData.audioSource = null;
		activeCrossfadeDuration = fadeDuration ?? playlist.fadeDuration;
		currentFadeTime = 0f;
	}

	private float CalculateTotalWeight()
	{
		float num = 0f;
		for (int i = 0; i < playlist.tracks.Count; i++)
		{
			num += playlist.tracks[i].weight;
		}
		return num;
	}

	private bool ShouldAdvanceToNextTrack()
	{
		if (playlist.weightRandomized)
		{
			return true;
		}
		if (playlist.tracks.Count == 1)
		{
			return true;
		}
		if (notPlayedTracks.Count > 0 || playlist.loop || playlist.shuffled)
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (!(activeAudioSource != null))
		{
			return;
		}
		if (isPlaying && !isPaused && !isCrossfade && !isPauseFading && !isUnpauseFading && !activeAudioSource.loop && activeAudioSource.clip != null && activeAudioSource.time >= activeAudioSource.clip.length - playlist.fadeDuration && ShouldAdvanceToNextTrack())
		{
			PlayTrack(GetNextTrack());
		}
		if (isPauseFading)
		{
			pauseFadeTime += Time.deltaTime;
			float t = Mathf.Clamp01(pauseFadeTime / pauseFadeDuration);
			activeAudioSource.volume = Mathf.Lerp(pauseFadeStartVolume, 0f, t);
			if (pauseFadeTime >= pauseFadeDuration)
			{
				CompletePauseFade();
			}
		}
		if (isUnpauseFading)
		{
			unpauseFadeTime += Time.deltaTime;
			float t2 = Mathf.Clamp01(unpauseFadeTime / unpauseFadeDuration);
			activeAudioSource.volume = Mathf.Lerp(0f, unpauseFadeTargetVolume, t2);
			if (unpauseFadeTime >= unpauseFadeDuration)
			{
				CompleteUnpauseFade();
			}
		}
		if (!isCrossfade || isPauseFading || isUnpauseFading)
		{
			return;
		}
		if (activeCrossfadeDuration <= 0f)
		{
			if (fadeInData.audioSource != null)
			{
				fadeInData.audioSource.volume = fadeInData.maxVolume;
			}
			isCrossfade = false;
			fadeOutData.audioSource?.Stop();
			return;
		}
		currentFadeTime += Time.deltaTime;
		float num = currentFadeTime / activeCrossfadeDuration;
		if (fadeInData.audioSource != null)
		{
			fadeInData.audioSource.volume = Mathf.Lerp(0f, fadeInData.maxVolume, num);
		}
		if (fadeOutData.audioSource != null)
		{
			fadeOutData.audioSource.volume = Mathf.Lerp(0f, fadeOutData.maxVolume, 1f - num);
		}
		if (currentFadeTime >= activeCrossfadeDuration)
		{
			isCrossfade = false;
			fadeOutData.audioSource?.Stop();
		}
	}
}
