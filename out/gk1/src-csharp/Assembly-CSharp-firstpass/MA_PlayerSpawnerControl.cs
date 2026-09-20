using DarkTonic.MasterAudio;
using UnityEngine;

public class MA_PlayerSpawnerControl : MonoBehaviour
{
	public GameObject Player;

	private float nextSpawnTime;

	private bool PlayerActive => Player.activeInHierarchy;

	private void Awake()
	{
		base.useGUILayout = false;
		nextSpawnTime = -1f;
	}

	private void Update()
	{
		if (!PlayerActive)
		{
			if (nextSpawnTime < 0f)
			{
				nextSpawnTime = AudioUtil.Time + 1f;
			}
			if (Time.time >= nextSpawnTime)
			{
				Player.SetActive(value: true);
				nextSpawnTime = -1f;
			}
		}
	}
}
