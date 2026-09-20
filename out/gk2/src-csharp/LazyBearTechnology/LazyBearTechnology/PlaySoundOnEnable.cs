using System;
using UnityEngine;

namespace LazyBearTechnology;

public class PlaySoundOnEnable : MonoBehaviour
{
	[SerializeField]
	public string soundId;

	[SerializeField]
	private bool stopOnDisable = true;

	[SerializeField]
	private bool stopOnDestroy;

	[SerializeField]
	private SpatialType spatial = SpatialType.sound1D;

	protected SoundHandler soundHandler;

	public string SoundId => soundId;

	public SpatialType Spatial => spatial;

	protected virtual void OnEnable()
	{
		soundHandler = LazyAudio.PlayAtGameObject(soundId, base.transform, spatial);
	}

	protected virtual void OnDisable()
	{
		if (stopOnDisable)
		{
			soundHandler?.Stop();
		}
	}

	protected virtual void OnDestroy()
	{
		if (stopOnDestroy)
		{
			soundHandler?.Stop();
		}
	}

	private void OnDidApplyAnimationProperties()
	{
		throw new NotImplementedException();
	}

	public void Play()
	{
		LazyAudio.Play(soundId);
	}

	public void PlayAtGameObject()
	{
		LazyAudio.PlayAtGameObject(soundId, base.transform, spatial);
	}
}
