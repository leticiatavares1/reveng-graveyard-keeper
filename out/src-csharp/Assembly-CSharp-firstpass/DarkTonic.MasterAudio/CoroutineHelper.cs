using System.Collections;

namespace DarkTonic.MasterAudio;

public static class CoroutineHelper
{
	public static IEnumerator WaitForActualSeconds(float time)
	{
		float start = AudioUtil.Time;
		while (AudioUtil.Time < start + time)
		{
			yield return MasterAudio.EndOfFrameDelay;
		}
	}
}
