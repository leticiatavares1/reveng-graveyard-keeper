using UnityEngine;

public class LogoVolume : MonoBehaviour
{
	private void Awake()
	{
		AudioSource component = GetComponent<AudioSource>();
		if (component != null)
		{
			component.volume = (float)(GameSettings.me.volume_master * GameSettings.me.volume_sfx) / 100f;
		}
	}
}
