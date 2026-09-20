using TMPro;
using UnityEngine;

public class GroundLayoutObjectWithTMP : GroundLayoutObject
{
	[Space]
	[SerializeField]
	private TextMeshPro label;

	protected override void Redraw()
	{
		base.Redraw();
		Vector3 localPosition = top.transform.localPosition;
		label.rectTransform.localPosition = new Vector3(localPosition.x, label.rectTransform.localPosition.y, localPosition.z);
		label.rectTransform.localScale = new Vector3(1f, 1.666667f, 1f);
		label.rectTransform.sizeDelta = new Vector2(top.size.x, label.rectTransform.sizeDelta.y);
	}
}
