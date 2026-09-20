using System;
using System.Collections.Generic;

namespace LazyBearTechnology;

[Serializable]
public class SoundData
{
	public string id;

	public float volume;

	public float panning;

	public List<SampleData> samples = new List<SampleData>();
}
