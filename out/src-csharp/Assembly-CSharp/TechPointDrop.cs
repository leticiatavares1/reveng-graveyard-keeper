using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TechPointDrop : MonoBehaviour
{
	public Rigidbody2D rigid_body;

	public RoundAndSortComponent round_and_sort;

	public Animator animator;

	private float _collect_delay = 1f;

	private float _cur_time;

	private Transform _tf;

	private Transform _player;

	public float collect_radius = 10f;

	private string _type;

	private bool _inited;

	public float magnet_force = 5f;

	private Collider2D _collider;

	private const bool FLY_TECHPOINTS = true;

	private static List<TechPointDrop> _all = new List<TechPointDrop>();

	public string type => _type;

	public static List<TechPointDrop> all => _all;

	public static TechPointDrop Spawn(TechPointDrop prefab, TechPointsSpawner.Type type)
	{
		TechPointDrop techPointDrop = prefab.Copy(MainGame.me.world_root);
		techPointDrop.Init(TechDefinition.TECH_POINTS[(int)type]);
		techPointDrop.animator.SetInteger("color", (int)type);
		_all.Add(techPointDrop);
		return techPointDrop;
	}

	private void Init(string type)
	{
		round_and_sort.enabled = true;
		_collider = GetComponent<Collider2D>();
		_collect_delay = 0.7f;
		_tf = base.transform;
		_player = MainGame.me.player.tf;
		_type = type;
		_inited = true;
	}

	public void Update()
	{
		if (!_inited)
		{
			return;
		}
		_cur_time += Time.deltaTime;
		if (_cur_time < _collect_delay)
		{
			return;
		}
		Vector2 vector;
		try
		{
			vector = _player.position - _tf.position;
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			_inited = false;
			return;
		}
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude > 40000f)
		{
			_collider.isTrigger = false;
			return;
		}
		_collider.isTrigger = true;
		if (sqrMagnitude < collect_radius * collect_radius)
		{
			Collect();
		}
		else
		{
			rigid_body.AddForce(vector * magnet_force * 1.2f);
		}
	}

	private void Collect()
	{
		_inited = false;
		PlayerComponent p = MainGame.me.player_component;
		Color endValue = Color.black;
		base.gameObject.GetComponent<CircleCollider2D>().enabled = false;
		switch (_type)
		{
		case "r":
			endValue = Color.red;
			break;
		case "g":
			endValue = Color.green;
			break;
		case "b":
			endValue = Color.blue;
			break;
		}
		GUIElements.me.hud.tech_points_bar.Show();
		DOTween.To(() => p.player_additional_color, delegate(Color x)
		{
			p.player_additional_color = x;
		}, endValue, 0.03f).OnComplete(delegate
		{
			DOTween.To(() => p.player_additional_color, delegate(Color x)
			{
				p.player_additional_color = x;
			}, Color.black, 0.05f);
		});
		DebugDraw.DrawCross(TechPointsDrop.Drop(MainGame.me.player_pos, _type).transform.position, 1f, Color.yellow, 2f);
		DestroyMe();
		Sounds.PlaySound("tech_pickup");
	}

	public void DestroyMe()
	{
		NGUITools.Destroy(base.gameObject);
		if (_all.Contains(this))
		{
			_all.Remove(this);
		}
	}

	public static void DestroyAll()
	{
		foreach (TechPointDrop item in _all)
		{
			NGUITools.Destroy(item.gameObject);
		}
		_all.Clear();
	}
}
