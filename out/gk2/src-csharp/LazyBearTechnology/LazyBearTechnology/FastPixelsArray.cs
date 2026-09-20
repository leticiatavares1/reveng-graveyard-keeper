using UnityEngine;

namespace LazyBearTechnology;

public class FastPixelsArray
{
	private readonly Color[] pixels;

	private Texture2D tex;

	private TextureFormat textureFormat;

	public int width { get; }

	public int height { get; }

	public Color this[int x, int y]
	{
		get
		{
			return pixels[InBoundsX(x) + InBoundsY(y) * width];
		}
		set
		{
			pixels[InBoundsX(x) + InBoundsY(y) * width] = value;
		}
	}

	public FastPixelsArray(Texture2D texture)
	{
		pixels = texture.GetPixels();
		width = texture.width;
		height = texture.height;
		textureFormat = texture.format;
	}

	public FastPixelsArray(int width, int height)
		: this(width, height, new Color(0f, 0f, 0f, 0f))
	{
	}

	public FastPixelsArray(int width, int height, Color fillColor, TextureFormat textureFormat = TextureFormat.RGBA32)
	{
		pixels = new Color[width * height];
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = fillColor;
		}
		this.width = width;
		this.height = height;
		this.textureFormat = textureFormat;
	}

	public bool IsTransparent(int x, int y)
	{
		return this[x, y].a > 0.5f;
	}

	public bool IsTransparent(IntVector2 pos)
	{
		return IsTransparent(pos.x, pos.y);
	}

	public Texture2D GetTexture()
	{
		if (tex == null)
		{
			tex = new Texture2D(width, height, textureFormat, mipChain: false);
		}
		tex.SetPixels(pixels);
		tex.Apply();
		return tex;
	}

	private int InBoundsX(int x)
	{
		return Mathf.Max(0, Mathf.Min(width - 1, x));
	}

	private int InBoundsY(int y)
	{
		return Mathf.Max(0, Mathf.Min(height - 1, y));
	}

	public void DrawFilledTriangle(IntVector2 p1, IntVector2 p2, IntVector2 p3, Color color)
	{
		int a = p1.x;
		int a2 = p1.y;
		int a3 = p2.x;
		int a4 = p2.y;
		int b = p3.x;
		int b2 = p3.y;
		if (a4 > b2)
		{
			SwapInts(ref a3, ref b);
			SwapInts(ref a4, ref b2);
		}
		if (a2 > a4)
		{
			SwapInts(ref a, ref a3);
			SwapInts(ref a2, ref a4);
		}
		if (a4 > b2)
		{
			SwapInts(ref a3, ref b);
			SwapInts(ref a4, ref b2);
		}
		float num = (float)(b - a) / (float)(b2 - a2 + 1);
		float num2 = (float)(a3 - a) / (float)(a4 - a2 + 1);
		float num3 = (float)(b - a3) / (float)(b2 - a4 + 1);
		float num4 = a;
		float num5 = (float)a + num2;
		for (int i = a2; i <= Mathf.Min(b2, height - 1); i++)
		{
			if (i >= 0)
			{
				for (int j = Mathf.Max(0, (int)num4); (float)j <= Mathf.Min(width - 1, num5); j++)
				{
					this[j, i] = color;
				}
				int num6 = Mathf.Min((int)num4, width - 1);
				while ((float)num6 >= Mathf.Max(0f, num5))
				{
					this[num6, i] = color;
					num6--;
				}
			}
			num4 += num;
			num5 = ((i >= a4) ? (num5 + num3) : (num5 + num2));
		}
	}

	private static void SwapInts(ref int a, ref int b)
	{
		int num = a;
		a = b;
		b = num;
	}

	public void DrawFilledCircle(IntVector2 center, int radius, Color color)
	{
		int num = radius * radius;
		for (int i = -radius; i < radius; i++)
		{
			int num2 = i + center.x;
			if (num2 < 0 || num2 >= width)
			{
				continue;
			}
			for (int j = -radius; j < radius; j++)
			{
				int num3 = j + center.y;
				if (num3 >= 0 && num3 < height && i * i + j * j <= num)
				{
					this[num2, num3] = color;
				}
			}
		}
	}
}
