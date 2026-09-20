using UnityEngine;

public class GroundLayoutObject : MonoBehaviour
{
	protected enum GrowType
	{
		Center,
		RightUp,
		LeftUp,
		LeftDown,
		RightDown
	}

	[SerializeField]
	protected SpriteRenderer right;

	[SerializeField]
	protected SpriteRenderer top;

	[SerializeField]
	protected SpriteRenderer left;

	[SerializeField]
	protected SpriteRenderer bot;

	[Space]
	[SerializeField]
	protected GrowType growType;

	[SerializeField]
	protected int xSizeInTiles = 64;

	[SerializeField]
	protected int zSizeInTiles = 64;

	protected virtual void Redraw()
	{
		float num = 50f;
		float num2 = 0.96f * (float)xSizeInTiles;
		float num3 = 0.96f * (float)zSizeInTiles;
		float num4 = num2 / 2f;
		float num5 = num3 / 2f;
		Sprite sprite = right.sprite;
		Sprite sprite2 = top.sprite;
		Sprite sprite3 = left.sprite;
		Sprite sprite4 = bot.sprite;
		Vector2 size = new Vector2(sprite.texture.width, sprite.texture.height) / num;
		Vector2 size2 = new Vector2(sprite2.texture.width, sprite2.texture.height) / num;
		Vector2 size3 = new Vector2(sprite3.texture.width, sprite3.texture.height) / num;
		Vector2 size4 = new Vector2(sprite4.texture.width, sprite4.texture.height) / num;
		size *= new Vector2(1f, num3 / size.y);
		size2 *= new Vector2(num2 / size2.x, 1f);
		size3 *= new Vector2(1f, num3 / size3.y);
		size4 *= new Vector2(num2 / size4.x, 1f);
		Vector3 a = default(Vector3);
		Vector3 a2 = default(Vector3);
		Vector3 a3 = default(Vector3);
		Vector3 a4 = default(Vector3);
		switch (growType)
		{
		case GrowType.Center:
			a = new Vector3(num4, 0f, 0f);
			a2 = new Vector3(0f, 0f, num5);
			a3 = new Vector3(0f - num4, 0f, 0f);
			a4 = new Vector3(0f, 0f, 0f - num5);
			break;
		case GrowType.RightUp:
			a = new Vector3(0f, 0f, 0f - num5);
			a2 = new Vector3(0f - num4, 0f, 0f);
			a3 = new Vector3(0f - num2, 0f, 0f - num5);
			a4 = new Vector3(0f - num4, 0f, 0f - num3);
			break;
		case GrowType.LeftUp:
			a = new Vector3(num2, 0f, 0f - num5);
			a2 = new Vector3(num4, 0f, 0f);
			a3 = new Vector3(0f, 0f, 0f - num5);
			a4 = new Vector3(num4, 0f, 0f - num3);
			break;
		case GrowType.LeftDown:
			a = new Vector3(num2, 0f, num5);
			a2 = new Vector3(num4, 0f, num3);
			a3 = new Vector3(0f, 0f, num5);
			a4 = new Vector3(num4, 0f, 0f);
			break;
		case GrowType.RightDown:
			a = new Vector3(0f, 0f, num5);
			a2 = new Vector3(0f - num4, 0f, num3);
			a3 = new Vector3(0f - num2, 0f, num5);
			a4 = new Vector3(0f - num4, 0f, 0f);
			break;
		}
		Vector3 b = new Vector3(1f, 1f, 1.25f);
		right.transform.localPosition = Vector3.Scale(a, b);
		top.transform.localPosition = Vector3.Scale(a2, b);
		left.transform.localPosition = Vector3.Scale(a3, b);
		bot.transform.localPosition = Vector3.Scale(a4, b);
		right.size = size;
		top.size = size2;
		left.size = size3;
		bot.size = size4;
	}

	private void DecSizeX()
	{
		xSizeInTiles--;
		Redraw();
	}

	private void IncSizeX()
	{
		xSizeInTiles++;
		Redraw();
	}

	private void DecSizeY()
	{
		zSizeInTiles--;
		Redraw();
	}

	private void IncSizeY()
	{
		zSizeInTiles++;
		Redraw();
	}
}
