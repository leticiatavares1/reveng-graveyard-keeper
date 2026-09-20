using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace LazyBearTechnology.Preloader;

[Serializable]
public class LazyLogoData
{
	public enum ShowLengthMode
	{
		TimeLimited,
		SyncedToVideo
	}

	public enum ObjectMode
	{
		GameObject,
		Video
	}

	[Range(0f, 5f)]
	public float fadeOutTime = 1f;

	[Range(0f, 5f)]
	public float fadeInTime = 1f;

	public ShowLengthMode showLengthMode;

	[Range(0f, 10f)]
	public float showingTime;

	public ObjectMode objectMode;

	public GameObject logo;

	public VideoClip videoClip;

	private VideoPlayer videoPlayer;

	public UnityEvent onBeginShowing;

	public UnityEvent onEndShowing;

	public void Initialize(VideoPlayer videoPlayer = null)
	{
		this.videoPlayer = videoPlayer;
		Hide();
	}

	public void Show()
	{
		switch (objectMode)
		{
		case ObjectMode.GameObject:
			logo.SetActive(value: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ObjectMode.Video:
			break;
		}
		onBeginShowing?.Invoke();
	}

	public void Hide()
	{
		switch (objectMode)
		{
		case ObjectMode.GameObject:
			logo.SetActive(value: false);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ObjectMode.Video:
			break;
		}
		onEndShowing?.Invoke();
	}

	public IEnumerator DoProcess()
	{
		bool videoIsPlaying;
		switch (showLengthMode)
		{
		case ShowLengthMode.TimeLimited:
			yield return new WaitForSeconds(showingTime);
			break;
		case ShowLengthMode.SyncedToVideo:
			if (videoPlayer == null)
			{
				Debug.LogError("VideoPlayer is null during LazyPreloader. Please call Initialize(videoPlayer).");
				break;
			}
			videoPlayer.enabled = true;
			videoPlayer.clip = videoClip;
			videoPlayer.time = 0.0;
			videoPlayer.isLooping = false;
			videoPlayer.Play();
			yield return null;
			LazyPreloader.OnAnimationStarted();
			videoIsPlaying = true;
			videoPlayer.loopPointReached += OnLoopPointReached;
			while (videoIsPlaying)
			{
				yield return null;
			}
			videoPlayer.loopPointReached -= OnLoopPointReached;
			videoPlayer.enabled = false;
			videoPlayer.clip = null;
			LazyPreloader.OnAnimationStopped();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		void OnLoopPointReached(VideoPlayer vp)
		{
			videoIsPlaying = false;
		}
	}
}
