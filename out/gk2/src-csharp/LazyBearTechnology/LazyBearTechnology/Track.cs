using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class Track
{
	public string id;

	public AudioClip clip;

	[Range(0f, 1f)]
	public float volume = 1f;

	public bool loop;

	[SerializeField]
	[Range(-3f, 3f)]
	public float pitch = 1f;

	[Range(-1f, 1f)]
	public float panning;

	public float weight = 1f;
}
