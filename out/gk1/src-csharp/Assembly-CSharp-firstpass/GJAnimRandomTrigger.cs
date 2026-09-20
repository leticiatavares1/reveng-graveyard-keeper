using System.Collections.Generic;
using UnityEngine;

public class GJAnimRandomTrigger : MonoBehaviour
{
	private Animator _animator;

	public float time_period = 10f;

	public bool roll_on_awake = true;

	public List<float> weights = new List<float>();

	public List<string> triggers = new List<string>();

	private float _update_time_counter;

	private float _w_sum;

	private bool is_roll_enable = true;

	public void Start()
	{
		_animator = GetComponentInChildren<Animator>();
		if (_animator == null)
		{
			Debug.LogError("GJAnimRandomTrigger: Animator is null", this);
			return;
		}
		_update_time_counter = 0f;
		CalculateSum();
	}

	public float CalculateSum()
	{
		_w_sum = 0f;
		foreach (float weight in weights)
		{
			_w_sum += weight;
		}
		return _w_sum;
	}

	public void SetRollActive(bool is_active)
	{
		is_roll_enable = is_active;
	}

	public void Awake()
	{
		if (roll_on_awake)
		{
			DoRoll();
		}
	}

	public void Update()
	{
		_update_time_counter += Time.deltaTime;
		if (_update_time_counter >= time_period && is_roll_enable)
		{
			DoRoll();
		}
	}

	private void DoRoll()
	{
		if (_animator == null)
		{
			Start();
		}
		_update_time_counter = 0f;
		float num = Random.Range(0f, _w_sum);
		float num2 = 0f;
		int num3 = -1;
		foreach (float weight in weights)
		{
			num3++;
			num2 += weight;
			if (num <= num2)
			{
				if (triggers[num3] != "")
				{
					_animator.SetTrigger(triggers[num3]);
				}
				break;
			}
		}
	}
}
