using System;
using UnityEngine;

[Serializable]
public abstract class WsoComponentDataBase
{
	[SerializeField]
	protected string componentType;

	public string ComponentType => componentType;

	protected WsoComponentDataBase()
	{
		componentType = GetType().Name;
	}

	public virtual void PrepareForGame()
	{
	}

	public virtual void Cleanup()
	{
	}
}
