using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public abstract class BaseInputController
{
	public List<GameKey> holdedKeys = new List<GameKey>();

	public List<GameKey> pressedKeys = new List<GameKey>();

	protected Vector2 direction = Vector2.zero;

	protected Vector2 direction2 = Vector2.zero;

	public Vector2 Direction => direction;

	public Vector2 Direction2 => direction2;

	public virtual void Update()
	{
		holdedKeys.Clear();
		pressedKeys.Clear();
	}

	public virtual bool IsActive()
	{
		if (holdedKeys.Count <= 0)
		{
			return direction.magnitude > 0f;
		}
		return true;
	}
}
