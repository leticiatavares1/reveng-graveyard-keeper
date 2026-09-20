using System;

namespace LazyBearTechnology;

[Serializable]
public class HoldedGroup
{
	public HoldedGroupType groupType;

	public float timeBeforeRepeat;

	public float timeRepeatPeriod;
}
