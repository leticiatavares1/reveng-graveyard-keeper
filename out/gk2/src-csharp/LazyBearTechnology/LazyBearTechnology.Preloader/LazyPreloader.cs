using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace LazyBearTechnology.Preloader;

public class LazyPreloader : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer fadingSprite;

	[SerializeField]
	private VideoPlayer videoPlayer;

	[Space]
	[SerializeField]
	private List<LazyLogoData> logoList = new List<LazyLogoData>();

	private Action onFinishedCallback;

	private IEnumerator preloaderCoroutine;

	private bool waitUntilPreloaderCoroutineEnded;

	private bool isAnimationRunning;

	private static LazyPreloader instance;

	private void Awake()
	{
		instance = this;
		logoList.ForEach(delegate(LazyLogoData x)
		{
			x.Initialize(videoPlayer);
		});
	}

	public void Run(IEnumerator onFinishedCallback, IEnumerator preloaderCoroutine = null, bool waitUntilPreloaderCoroutineEnded = false)
	{
		this.onFinishedCallback = delegate
		{
			StartCoroutine(onFinishedCallback);
		};
		this.preloaderCoroutine = preloaderCoroutine;
		this.waitUntilPreloaderCoroutineEnded = waitUntilPreloaderCoroutineEnded;
		StartCoroutine(RunLogoCoroutine());
		StartCoroutine(RunPreloaderCoroutine());
	}

	private IEnumerator RunPreloaderCoroutine()
	{
		if (preloaderCoroutine == null)
		{
			yield break;
		}
		while (true)
		{
			if (isAnimationRunning)
			{
				yield return null;
				continue;
			}
			if (!preloaderCoroutine.MoveNext())
			{
				break;
			}
			yield return null;
		}
		preloaderCoroutine = null;
	}

	private IEnumerator RunLogoCoroutine()
	{
		for (int i = 0; i < logoList.Count; i++)
		{
			LazyLogoData currentLogo = logoList[i];
			currentLogo.Show();
			if (currentLogo.fadeInTime > 0f)
			{
				yield return DoFadeCoroutine(currentLogo.fadeInTime, fadeIn: true);
			}
			else
			{
				SetFadingSpriteAlpha(0f);
			}
			yield return currentLogo.DoProcess();
			if (currentLogo.fadeOutTime > 0f)
			{
				yield return DoFadeCoroutine(currentLogo.fadeOutTime, fadeIn: false);
			}
			else
			{
				SetFadingSpriteAlpha(1f);
			}
			currentLogo.Hide();
		}
		yield return new WaitUntil(() => !waitUntilPreloaderCoroutineEnded || preloaderCoroutine == null);
		onFinishedCallback?.Invoke();
	}

	private IEnumerator DoFadeCoroutine(float fadeOutTime, bool fadeIn)
	{
		float time = 0f;
		float startAlpha = (fadeIn ? 1 : 0);
		float endAlpha = 1f - startAlpha;
		while (time < fadeOutTime)
		{
			float fadingSpriteAlpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeOutTime);
			SetFadingSpriteAlpha(fadingSpriteAlpha);
			time += Time.deltaTime;
			yield return null;
		}
		SetFadingSpriteAlpha(endAlpha);
	}

	private void SetFadingSpriteAlpha(float alpha)
	{
		if (!(fadingSprite == null))
		{
			Color color = fadingSprite.color;
			color.a = alpha;
			fadingSprite.color = color;
		}
	}

	public static void OnAnimationStarted()
	{
		instance.isAnimationRunning = true;
	}

	public static void OnAnimationStopped()
	{
		instance.isAnimationRunning = false;
	}
}
