using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace LazyBearTechnology;

[Serializable]
public class Playlist
{
	public string id;

	public bool shuffled;

	public bool weightRandomized;

	public bool loop;

	[Range(0f, 1f)]
	public float volume = 1f;

	public float fadeDuration;

	public AudioMixerGroup group;

	public List<Track> tracks = new List<Track>();
}
