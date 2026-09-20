using System.Collections;
using LazyBearTechnology;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PlaySoundOnParticleCount : MonoBehaviour
{
	[SerializeField]
	[Min(1f)]
	private int particleThreshold = 1;

	[SerializeField]
	private string soundId;

	[SerializeField]
	[Min(0f)]
	private float delay;

	private ParticleSystem cachedParticleSystem;

	private int lastCount;

	private void Awake()
	{
		cachedParticleSystem = GetComponent<ParticleSystem>();
	}

	private void OnEnable()
	{
		lastCount = ((cachedParticleSystem != null) ? cachedParticleSystem.particleCount : 0);
	}

	private void LateUpdate()
	{
		if (!(cachedParticleSystem == null))
		{
			int particleCount = cachedParticleSystem.particleCount;
			if (lastCount < particleThreshold && particleCount >= particleThreshold)
			{
				PlaySound();
			}
			lastCount = particleCount;
		}
	}

	private void PlaySound()
	{
		if (!string.IsNullOrEmpty(soundId))
		{
			if (delay > 0f)
			{
				StartCoroutine(PlayAfterDelay());
			}
			else
			{
				PlayNow();
			}
		}
	}

	private IEnumerator PlayAfterDelay()
	{
		yield return new WaitForSeconds(delay);
		PlayNow();
	}

	private void PlayNow()
	{
		LazyAudio.Play(soundId, checkDelay: false);
	}
}
