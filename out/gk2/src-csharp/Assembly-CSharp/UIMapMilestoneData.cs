using System;
using UnityEngine;

[Serializable]
public class UIMapMilestoneData
{
	public string wgoId;

	public bool hasCustomMapPos;

	public Vector2 customMapPos;

	public bool isNeedApplyPreset;

	public string presetNameToApply;

	public string lazyExpressionOnTeleporting;
}
