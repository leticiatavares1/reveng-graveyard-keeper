public class SmartCondition_DayTime : SmartCondition
{
	public bool is_night = true;

	public override bool CheckCondition()
	{
		if (!is_night)
		{
			return !TimeOfDay.me.is_night;
		}
		return TimeOfDay.me.is_night;
	}

	public override string GetName()
	{
		if (!is_night)
		{
			return "Is Day";
		}
		return "Is Night";
	}
}
