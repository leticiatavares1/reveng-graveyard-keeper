using System;
using UnityEngine;

public class TechPointsSpawner : MonoBehaviour
{
	public enum Type
	{
		R,
		G,
		B
	}

	public TechPointDrop prefab;

	public float period = 0.1f;

	public Vector2 force;

	[Range(0f, 359f)]
	public float delta_angle;

	private int _test_r = 1;

	private int _test_g = 1;

	private int _test_b = 1;

	private int[] _need_spawn = new int[3];

	private bool _spawning;

	private float _dt;

	public bool destroy_after_spawn = true;

	private void TestSpawn()
	{
		Spawn(_test_r, _test_g, _test_b);
	}

	public void Spawn(int r, int g, int b)
	{
		_need_spawn[0] = r;
		_need_spawn[1] = g;
		_need_spawn[2] = b;
		_dt = 0f;
		_spawning = true;
	}

	public void Update()
	{
		if (!_spawning)
		{
			return;
		}
		_dt += Time.deltaTime;
		if (!(_dt > period))
		{
			return;
		}
		_dt -= period;
		int num = NGUITools.RandomRange(0, 2);
		if (_need_spawn[num] == 0)
		{
			if (++num == 3)
			{
				num = 0;
			}
			if (_need_spawn[num] == 0 && ++num == 3)
			{
				num = 0;
			}
		}
		if (_need_spawn[0] + _need_spawn[1] + _need_spawn[2] == 0)
		{
			OnDoneSpawning();
			return;
		}
		if (_need_spawn[num] == 0)
		{
			Debug.LogWarning("Something is wrong");
			return;
		}
		_need_spawn[num]--;
		SpawnTechPoint((Type)num);
	}

	private void SpawnTechPoint(Type type)
	{
		TechPointDrop techPointDrop = TechPointDrop.Spawn(prefab, type);
		techPointDrop.transform.position = base.transform.position;
		float num = ((Vector2)MainGame.me.player.tf.position - (Vector2)base.transform.position).Atan2() * 57.29578f;
		float num2 = UnityEngine.Random.Range(force.x, force.y);
		float num3 = UnityEngine.Random.Range(num - delta_angle, num + delta_angle);
		techPointDrop.rigid_body.AddForce(new Vector2(Mathf.Cos(num3 * ((float)Math.PI / 180f)), Mathf.Sin(num3 * ((float)Math.PI / 180f))) * num2, ForceMode2D.Impulse);
	}

	private void OnDoneSpawning()
	{
		_spawning = false;
		if (destroy_after_spawn)
		{
			NGUITools.Destroy(base.gameObject);
		}
	}
}
