using UnityEngine;

public class SoulExtractorInfoWidget : MonoBehaviour
{
	[SerializeField]
	private float min_corruption_chance;

	[SerializeField]
	private float max_corruption_chance;

	[SerializeField]
	private UILabel label;

	public void SetData(float min_corruption_chance = 0f, float max_corruption_chance = 0f)
	{
		this.min_corruption_chance = min_corruption_chance;
		this.max_corruption_chance = max_corruption_chance;
	}

	public void Redraw()
	{
		string empty = string.Empty;
		empty = ((!min_corruption_chance.EqualsTo(0f) || !max_corruption_chance.EqualsTo(0f)) ? $"{(int)(min_corruption_chance * 100f)}-{(int)(max_corruption_chance * 100f)}" : "0");
		label.text = GJL.L("possible_damage") + ": " + empty + "%";
	}
}
