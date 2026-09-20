using LazyBearTechnology;
using UnityEngine;

public class DockPointHint : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer icon;

	[SerializeField]
	private float yOffset = 0.02f;

	private DockPoint targetDockPoint;

	public void Display(DockPoint dockPoint)
	{
		targetDockPoint = dockPoint;
		string text = targetDockPoint.Direction.ToString()[0].ToString();
		icon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("dockpoint_marker_" + text);
		UpdatePos();
		base.gameObject.SetActive(value: true);
	}

	public void Remove()
	{
		targetDockPoint = null;
		base.gameObject.SetActive(value: false);
	}

	private void LateUpdate()
	{
		if (!(targetDockPoint == null))
		{
			UpdatePos();
		}
	}

	private void UpdatePos()
	{
		Vector3 position = targetDockPoint.transform.position;
		position.y += yOffset;
		base.transform.position = position;
	}
}
