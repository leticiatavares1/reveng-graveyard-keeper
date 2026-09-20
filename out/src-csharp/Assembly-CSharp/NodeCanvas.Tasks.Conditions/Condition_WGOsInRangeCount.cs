using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions;

[Name("WGOs In Range Count", 0)]
[Category("Mob")]
public class Condition_WGOsInRangeCount : WGOBehaviourCondition
{
	public BBParameter<float> range = new BBParameter<float>(1f);

	public BBParameter<int> count = new BBParameter<int>(1);

	public BBParameter<string> custom_tag = new BBParameter<string>();

	protected override string info => "WGOs count in\nrange " + range.value + " less than " + count.value;

	protected override bool OnCheck()
	{
		if (string.IsNullOrEmpty(custom_tag.value))
		{
			return true;
		}
		if (count.value <= 0)
		{
			return false;
		}
		if (range.value < 0f)
		{
			return true;
		}
		List<WorldGameObject> worldGameObjectsByCustomTag = WorldMap.GetWorldGameObjectsByCustomTag(custom_tag.value);
		if (worldGameObjectsByCustomTag == null || worldGameObjectsByCustomTag.Count < count.value)
		{
			return true;
		}
		int num = 0;
		foreach (WorldGameObject item in worldGameObjectsByCustomTag)
		{
			if (!(item == null) && item.IsInRange(base.self_wgo, range.value))
			{
				num++;
			}
		}
		return num < count.value;
	}
}
