using UnityEngine;

public class HUDTechTrashCan : AnimatedGUIPanel
{
	public int hidden_y = 30;

	public int shown_y = -1;

	public GameObject pos;

	public void Init()
	{
		Init(hidden_y, shown_y);
	}
}
