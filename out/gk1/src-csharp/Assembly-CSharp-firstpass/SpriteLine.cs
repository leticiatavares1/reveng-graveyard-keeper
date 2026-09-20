using System;
using UnityEngine;

public class SpriteLine : MonoBehaviour
{
	protected Vector3 pos1;

	protected Vector3 pos2;

	protected UI2DSprite spr;

	public int width
	{
		set
		{
			spr.width = value;
		}
	}

	public void Start()
	{
		spr = GetComponent<UI2DSprite>();
	}

	public void Draw(Vector3 from, Vector3 to)
	{
		pos1 = from;
		pos2 = to;
		Redraw();
	}

	public void SetDepth(int d)
	{
		spr.depth = d;
	}

	private void Redraw()
	{
		float angle = Mathf.Atan2(pos1.y - pos2.y, pos1.x - pos2.x) / (float)Math.PI * 180f + 90f;
		base.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		if (spr == null)
		{
			Start();
		}
		spr.height = (int)(pos2 - pos1).magnitude;
		base.transform.localPosition = pos1 + (pos2 - pos1) / 2f;
	}

	public void SetColor(Color c)
	{
		spr.color = c;
	}
}
