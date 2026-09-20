using System;

namespace DarkTonic.MasterAudio;

[Serializable]
public class DuckGroupInfo
{
	public string soundType = "[None]";

	public float riseVolStart = 0.5f;

	public float unduckTime = 1f;

	public float duckedVolumeCut = -6f;

	public bool isTemporary;
}
