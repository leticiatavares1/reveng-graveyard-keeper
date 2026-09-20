using System.Collections.Generic;
using UnityEngine;

public class BaseInputController
{
	public List<GameKey> holded_keys = new List<GameKey>();

	public List<GameKey> pressed_keys = new List<GameKey>();

	public List<GameKey> released_keys = new List<GameKey>();

	protected Vector2 dir = Vector2.zero;

	protected Vector2 dir2 = Vector2.zero;

	public Vector2 direction => dir;

	public Vector2 direction2 => dir2;

	public virtual void Update()
	{
		holded_keys.Clear();
		pressed_keys.Clear();
	}

	public virtual bool IsActive()
	{
		if (holded_keys.Count <= 0 && !(dir.magnitude > 0f))
		{
			return dir2.magnitude > 0f;
		}
		return true;
	}
}
