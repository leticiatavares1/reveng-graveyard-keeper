using System;
using System.Collections.Generic;

namespace LazyBearTechnology;

[Serializable]
public class AudioConfigData
{
	public List<SoundData> sounds = new List<SoundData>();

	public List<PlaylistData> playlists = new List<PlaylistData>();
}
