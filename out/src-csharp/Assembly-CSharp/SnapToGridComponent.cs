using System;
using System.Collections.Generic;
using UnityEngine;

public class SnapToGridComponent : MonoBehaviour
{
	[Range(1f, 48f)]
	public int grid_divider = 1;

	[Range(-50f, 50f)]
	public int fine_tune_z;

	private static List<int> _grid_mdfrs;

	public virtual void Update()
	{
		if (!Application.isPlaying)
		{
			DoRound();
		}
	}

	public void DoRound()
	{
		Transform transform = base.transform;
		float num = GetPixelSize() / (float)grid_divider;
		Vector3 localPosition = transform.localPosition;
		Vector3 vector = new Vector3(Mathf.Round(localPosition.x / num) * num, Mathf.Round(localPosition.y / num) * num, localPosition.z);
		if (!Application.isPlaying || !((double)(vector - transform.localPosition).magnitude < 0.001))
		{
			transform.localPosition = vector;
		}
	}

	protected virtual float GetPixelSize()
	{
		return 1f;
	}

	private static List<int> GetGridModifiers()
	{
		if (_grid_mdfrs != null)
		{
			return _grid_mdfrs;
		}
		_grid_mdfrs = new List<int>();
		for (int i = 1; i < 96; i++)
		{
			if (96 % i == 0)
			{
				_grid_mdfrs.Add(i);
			}
		}
		return _grid_mdfrs;
	}

	public void OnValidate()
	{
		int index = -1;
		int num = 99999;
		List<int> gridModifiers = GetGridModifiers();
		for (int i = 0; i < gridModifiers.Count; i++)
		{
			if (grid_divider == gridModifiers[i])
			{
				return;
			}
			int num2 = Math.Abs(gridModifiers[i] - grid_divider);
			if (num2 < num)
			{
				num = num2;
				index = i;
			}
		}
		grid_divider = gridModifiers[index];
	}
}
