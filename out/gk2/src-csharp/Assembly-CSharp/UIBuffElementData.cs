using LazyBearTechnology;

public class UIBuffElementData : LazyWidgetDataBase
{
	public PerkData PerkData { get; private set; }

	public float Duration { get; private set; }

	public bool HasHiddenTimer { get; private set; }

	public bool IsInfinite { get; private set; }

	public UIBuffElementData(PerkData perkData, float duration, bool hasHiddenTimer, bool isInfinite)
	{
		PerkData = perkData;
		Duration = duration;
		HasHiddenTimer = hasHiddenTimer;
		IsInfinite = isInfinite;
	}

	public void UpdateData(float duration)
	{
		Duration = duration;
	}
}
