using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UIFade : UIBasicFade
{
	private Canvas canvas;

	public override void Init()
	{
		base.Init();
		canvas = GetComponent<Canvas>();
		canvas.overrideSorting = true;
		canvas.sortingOrder = 800;
	}
}
