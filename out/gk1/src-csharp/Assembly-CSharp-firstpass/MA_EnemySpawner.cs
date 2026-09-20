using DarkTonic.MasterAudio;
using UnityEngine;

public class MA_EnemySpawner : MonoBehaviour
{
	public GameObject Enemy;

	public bool spawnerEnabled;

	private Transform trans;

	private float nextSpawnTime;

	private void Awake()
	{
		base.useGUILayout = false;
		trans = base.transform;
		nextSpawnTime = AudioUtil.Time + Random.Range(0.3f, 0.7f);
	}

	private void Update()
	{
		if (spawnerEnabled && Time.time >= nextSpawnTime)
		{
			Vector3 position = trans.position;
			int num = Random.Range(1, 3);
			for (int i = 0; i < num; i++)
			{
				position.x = Random.Range(position.x - 6f, position.x + 6f);
				Object.Instantiate(Enemy, position, Enemy.transform.rotation);
			}
			nextSpawnTime = AudioUtil.Time + Random.Range(0.3f, 0.7f);
		}
	}
}
