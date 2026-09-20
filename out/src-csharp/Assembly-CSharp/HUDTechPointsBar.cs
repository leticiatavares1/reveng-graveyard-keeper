using UnityEngine;

public class HUDTechPointsBar : AnimatedGUIPanel
{
	public int hidden_coord = 30;

	public int shown_coord = -1;

	public UILabel label;

	public GameObject pos_r;

	public GameObject pos_g;

	public GameObject pos_b;

	public void Init()
	{
		Init(hidden_coord, shown_coord, AnimationType.AnimateX);
	}

	public override void Redraw()
	{
		label.text = PlayerComponent.GetTechPointsString();
	}

	public Vector2 GetTechPointsCounterPosition(string tech_type)
	{
		return tech_type switch
		{
			"r" => pos_r.transform.position, 
			"g" => pos_g.transform.position, 
			"b" => pos_b.transform.position, 
			_ => Vector2.zero, 
		};
	}
}
