using System;

namespace LazyBearTechnology;

[Serializable]
public class PlaylistWeight
{
	public string playlistId;

	public string trackId;

	public float weight;

	public PlaylistWeight()
	{
	}

	public PlaylistWeight(string playlistId, string trackId, float weight)
	{
		this.playlistId = playlistId;
		this.trackId = trackId;
		this.weight = weight;
	}
}
