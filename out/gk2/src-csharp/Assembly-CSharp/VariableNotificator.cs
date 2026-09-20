using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VariableNotificator<T> : IEquatable<T>
{
	[SerializeField]
	protected T value;

	public T Value
	{
		get
		{
			return value;
		}
		set
		{
			bool num = this.value.Equals(value);
			this.value = value;
			if (!num)
			{
				this.ValueChanged?.Invoke(this.value);
			}
		}
	}

	public event Action<T> ValueChanged;

	public VariableNotificator()
	{
	}

	public VariableNotificator(T value)
	{
		this.value = value;
	}

	protected bool Equals(VariableNotificator<T> other)
	{
		return EqualityComparer<T>.Default.Equals(value, other.value);
	}

	public bool Equals(T obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == (object)obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals(obj as VariableNotificator<T>);
	}
}
