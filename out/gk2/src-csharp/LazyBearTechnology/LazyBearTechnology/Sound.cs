using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace LazyBearTechnology;

[Serializable]
public class Sound
{
	public string id;

	public AudioSettings3DType audioSettings3DType;

	[Range(0f, 1f)]
	public float volume = 1f;

	[Range(-1f, 1f)]
	public float panning;

	public bool loop;

	public AudioMixerGroup group;

	public List<Sample> samples = new List<Sample>();

	public bool randomWithoutRepeat = true;

	private Sample previousSample;

	private float averagePlayingTime = -1f;

	public float AveragePlayingTime
	{
		get
		{
			if (averagePlayingTime <= -1f)
			{
				float num = 0f;
				int num2 = 0;
				foreach (Sample sample in samples)
				{
					if (sample.isEnabled)
					{
						num2++;
						num += sample.clip.length;
					}
				}
				averagePlayingTime = num / (float)num2;
			}
			return averagePlayingTime;
		}
	}

	public Sample RandomSample
	{
		get
		{
			int num = 0;
			while (num++ < 100)
			{
				Sample randomSample = GetRandomSample();
				if (randomSample.isEnabled)
				{
					return randomSample;
				}
			}
			Debug.LogError("Error: Sound.RandomSample() couldn't find any enabled sample. Returning null.");
			return null;
		}
	}

	private Sample GetRandomSample()
	{
		if (!randomWithoutRepeat)
		{
			return samples.GetRandom();
		}
		if (samples.Count <= 2)
		{
			return samples.GetRandom();
		}
		Sample random;
		do
		{
			random = samples.GetRandom();
		}
		while (random == previousSample);
		previousSample = random;
		return random;
	}
}
