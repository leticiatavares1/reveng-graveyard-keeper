using System;
using UnityEngine;

namespace NodeCanvas.Framework;

[Serializable]
public class TaskAgent : BBParameter<UnityEngine.Object>
{
	public new UnityEngine.Object value
	{
		get
		{
			if (base.useBlackboard)
			{
				UnityEngine.Object @object = base.value;
				if (@object == null)
				{
					return null;
				}
				if (@object is GameObject)
				{
					return (@object as GameObject).transform;
				}
				if (@object is Component)
				{
					return (Component)@object;
				}
				return null;
			}
			return _value as Component;
		}
		set
		{
			_value = value;
		}
	}

	protected override object objectValue
	{
		get
		{
			return value;
		}
		set
		{
			this.value = (UnityEngine.Object)value;
		}
	}
}
