using UnityEngine;

public class WOPPrefabName
{
	[SerializeField]
	public string part_1 = "";

	[SerializeField]
	public int stage;

	[SerializeField]
	public string part_2 = "";

	[SerializeField]
	public bool need_herb;

	public string GetName()
	{
		if (string.IsNullOrEmpty(part_1))
		{
			return "";
		}
		if (stage <= 0)
		{
			return part_1;
		}
		if (string.IsNullOrEmpty(part_2))
		{
			return part_1 + stage;
		}
		return part_1 + stage + part_2;
	}

	public bool EqualsTo(WOPPrefabName other_obj)
	{
		if (other_obj != null && part_1 == other_obj.part_1 && stage == other_obj.stage && part_2 == other_obj.part_2)
		{
			return need_herb == other_obj.need_herb;
		}
		return false;
	}
}
