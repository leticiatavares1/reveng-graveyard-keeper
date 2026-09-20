using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class Sample
{
	public bool isEnabled = true;

	public AudioClip clip;

	[Range(0f, 1f)]
	public float volume = 1f;

	[Range(-3f, 3f)]
	public float pitch = 1f;

	[Range(-1f, 1f)]
	public float panning;

	public float pitchVariation;

	public string filename = "";

	public float weight = 1f;

	public float Pitch
	{
		get
		{
			if (pitchVariation != 0f)
			{
				return UnityEngine.Random.Range(pitch - pitchVariation, pitch + pitchVariation);
			}
			return pitch;
		}
	}
}
