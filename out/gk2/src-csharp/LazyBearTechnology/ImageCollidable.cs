using UnityEngine;
using UnityEngine.UI;

public class ImageCollidable : Image
{
	[SerializeField]
	[Space]
	public Collider2D collider2D;

	public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		bool flag = !(collider2D != null) || collider2D.OverlapPoint(screenPoint);
		return base.IsRaycastLocationValid(screenPoint, eventCamera) && flag;
	}
}
