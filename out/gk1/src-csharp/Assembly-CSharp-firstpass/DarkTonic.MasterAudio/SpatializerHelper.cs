using UnityEngine;

namespace DarkTonic.MasterAudio;

public static class SpatializerHelper
{
	public static bool SpatializerOptionExists => true;

	public static void TurnOnSpatializerIfEnabled(AudioSource source)
	{
		if (SpatializerOptionExists && !(MasterAudio.SafeInstance == null) && MasterAudio.Instance.useSpatializer)
		{
			source.spatialize = true;
		}
	}
}
