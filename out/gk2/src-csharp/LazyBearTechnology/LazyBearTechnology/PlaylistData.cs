using System;
using System.Collections.Generic;

namespace LazyBearTechnology;

[Serializable]
public class PlaylistData
{
	public string id;

	public float volume;

	public List<TrackData> tracks = new List<TrackData>();
}
