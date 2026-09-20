using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TechPointsSpawner : MonoBehaviour
{
	public enum Type
	{
		R,
		G,
		B,
		H
	}

	private static Pool pool;

	[SerializeField]
	private float period = 0.1f;

	[SerializeField]
	private Vector2 force;

	[Range(0f, 359f)]
	[SerializeField]
	private float deltaAngle;

	[SerializeField]
	private bool releaseAfterSpawn = true;

	private int testR = 1;

	private int testG = 1;

	private int testB = 1;

	private int testH = 1;

	private int[] needSpawn = new int[4];

	private bool spawning;

	private float dt;

	private string worldId;

	private static readonly List<TechPointsSpawner> activeSpawners = new List<TechPointsSpawner>();

	public static void CreateSpawner(Vector3 pos, int r, int g, int b, int h)
	{
		if (pool == null)
		{
			pool = LazyPooler.CreatePool(Addressables.LoadAssetAsync<GameObject>("Assets/AddressableAssets/Drops/TechPointsSpawner.prefab").WaitForCompletion().GetComponent<TechPointsSpawner>(), 0, Pool.PoolType.ImmediateActivation, parentAllObjectsInPool: true);
		}
		TechPointsSpawner orCreateObject = pool.GetOrCreateObject<TechPointsSpawner>();
		orCreateObject.transform.position = pos;
		orCreateObject.Spawn(MainGame.PlayerController.CurrentGameScene.Id, r, g, b, h);
	}

	public static void FlushPendingAsWorldDrops()
	{
		for (int num = activeSpawners.Count - 1; num >= 0; num--)
		{
			TechPointsSpawner techPointsSpawner = activeSpawners[num];
			if (techPointsSpawner != null)
			{
				techPointsSpawner.FlushRemainingAsWorldDrops();
			}
		}
	}

	private void Spawn(string worldId, int r, int g, int b, int h)
	{
		base.transform.SetParent(MainGame.PlayerController.CurrentGameScene.transform);
		this.worldId = worldId;
		needSpawn[0] = r;
		needSpawn[1] = g;
		needSpawn[2] = b;
		needSpawn[3] = h;
		dt = 0f;
		spawning = true;
		if (!activeSpawners.Contains(this))
		{
			activeSpawners.Add(this);
		}
	}

	private void Update()
	{
		if (!spawning || MainGame.IsGamePaused)
		{
			return;
		}
		dt += Time.deltaTime;
		if (!(dt > period))
		{
			return;
		}
		dt -= period;
		int num = UnityEngine.Random.Range(0, 3);
		if (needSpawn[num] == 0)
		{
			if (++num == 4)
			{
				num = 0;
			}
			if (needSpawn[num] == 0 && ++num == 4)
			{
				num = 0;
			}
		}
		if (needSpawn[0] + needSpawn[1] + needSpawn[2] + needSpawn[3] == 0)
		{
			OnDoneSpawning();
		}
		else if (needSpawn[num] != 0)
		{
			needSpawn[num]--;
			SpawnTechPoint((Type)num, applyImpulse: true);
		}
	}

	private void SpawnTechPoint(Type type, bool applyImpulse)
	{
		TechPointDropData techPointDropData = new TechPointDropData(base.transform.position, type, worldId);
		TechPointDrop techPointDrop = TechPointDrop.Spawn(techPointDropData, base.transform.parent);
		MainGame.Instance.GameSave.WorldData.GetGameSceneDataById(techPointDropData.worldId).AddTechPointDrop(techPointDropData);
		if (applyImpulse)
		{
			Vector3 vector = MainGame.PlayerController.MovablePosition - base.transform.position;
			float num = Mathf.Atan2(vector.z, vector.x) * 57.29578f;
			float num2 = UnityEngine.Random.Range(force.x, force.y);
			float num3 = UnityEngine.Random.Range(num - deltaAngle, num + deltaAngle);
			Vector3 vector2 = new Vector3(Mathf.Cos(num3 * (MathF.PI / 180f)), 0f, Mathf.Sin(num3 * (MathF.PI / 180f))) * num2;
			techPointDrop.RigidBody.AddForce(vector2, ForceMode.Impulse);
		}
	}

	private void FlushRemainingAsWorldDrops()
	{
		if (!spawning)
		{
			return;
		}
		for (int i = 0; i < needSpawn.Length; i++)
		{
			int num = needSpawn[i];
			needSpawn[i] = 0;
			for (int j = 0; j < num; j++)
			{
				SpawnTechPoint((Type)i, applyImpulse: false);
			}
		}
		OnDoneSpawning();
	}

	private void OnDoneSpawning()
	{
		spawning = false;
		activeSpawners.Remove(this);
		for (int i = 0; i < needSpawn.Length; i++)
		{
			needSpawn[i] = 0;
		}
		dt = 0f;
		if (releaseAfterSpawn)
		{
			pool.ReleaseObject(this);
		}
	}

	private void TestSpawn()
	{
		Spawn(MainGame.PlayerData.currentGameSceneId, testR, testG, testB, testH);
	}
}
