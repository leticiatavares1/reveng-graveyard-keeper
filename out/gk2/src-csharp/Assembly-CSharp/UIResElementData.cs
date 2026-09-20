using System;
using LazyBearTechnology;
using UnityEngine;

public class UIResElementData : LazyWidgetDataBase
{
	public Action<UIResElementData> OnShowTimeComplete { get; private set; }

	public string Id { get; private set; }

	public float Value { get; private set; }

	public string IconId { get; private set; }

	public bool HasTarget { get; private set; }

	public bool IsUITarget { get; private set; }

	public UIGameResDisplayingType DisplayingType { get; private set; }

	public Transform Target { get; private set; }

	public Vector3 StartPosition { get; private set; }

	public UIResElementData(string id, UIGameResDisplayingType displayType, string iconId, float value, Transform target, bool isUITarget, Action<UIResElementData> onShowTimeComplete = null)
	{
		Id = id;
		DisplayingType = displayType;
		IconId = iconId;
		Value = value;
		OnShowTimeComplete = onShowTimeComplete;
		IsUITarget = isUITarget;
		Target = target;
		StartPosition = target.position;
		HasTarget = target != null;
	}

	public void AddValue(float value)
	{
		Value += value;
	}

	public int GetShowValue()
	{
		int num = Mathf.RoundToInt(Value);
		int num2 = (Value.EqualsTo(num) ? num : ((int)Mathf.Sign(Value) * Mathf.FloorToInt(Mathf.Abs(Value))));
		Value -= num2;
		return num2;
	}
}
