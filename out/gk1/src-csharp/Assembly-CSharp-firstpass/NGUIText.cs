using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class NGUIText
{
	public enum Alignment
	{
		Automatic,
		Left,
		Center,
		Right,
		Justified
	}

	public enum SymbolStyle
	{
		None,
		Normal,
		Colored,
		NoEffect
	}

	public class GlyphInfo
	{
		public Vector2 v0;

		public Vector2 v1;

		public Vector2 u0;

		public Vector2 u1;

		public Vector2 u2;

		public Vector2 u3;

		public float advance;

		public int channel;
	}

	public static UIFont bitmapFont;

	public static Font dynamicFont;

	public static GlyphInfo glyph = new GlyphInfo();

	public static int fontSize = 16;

	public static float fontScale = 1f;

	public static float pixelDensity = 1f;

	public static FontStyle fontStyle = FontStyle.Normal;

	public static Alignment alignment = Alignment.Left;

	public static Color tint = Color.white;

	public static int rectWidth = 1000000;

	public static int rectHeight = 1000000;

	public static int regionWidth = 1000000;

	public static int regionHeight = 1000000;

	public static int maxLines = 0;

	public static bool gradient = false;

	public static Color gradientBottom = Color.white;

	public static Color gradientTop = Color.white;

	public static bool encoding = false;

	public static float spacingX = 0f;

	public static float spacingY = 0f;

	public static bool premultiply = false;

	public static SymbolStyle symbolStyle;

	public static int finalSize = 0;

	public static float finalSpacingX = 0f;

	public static float finalLineHeight = 0f;

	public static float baseline = 0f;

	public static bool useSymbols = false;

	private static Color mInvisible = new Color(0f, 0f, 0f, 0f);

	private static BetterList<Color> mColors = new BetterList<Color>();

	private static float mAlpha = 1f;

	private static CharacterInfo mTempChar;

	private static BetterList<float> mSizes = new BetterList<float>();

	private static Color s_c0;

	private static Color s_c1;

	private static float[] mBoldOffset = new float[8] { -0.25f, 0f, 0.25f, 0f, 0f, -0.25f, 0f, 0.25f };

	public static void Update()
	{
		Update(request: true);
	}

	public static void Update(bool request)
	{
		finalSize = Mathf.RoundToInt((float)fontSize / pixelDensity);
		finalSpacingX = spacingX * fontScale;
		finalLineHeight = ((float)fontSize + spacingY) * fontScale;
		useSymbols = (dynamicFont != null || bitmapFont != null) && encoding && symbolStyle != SymbolStyle.None;
		Font font = dynamicFont;
		if (!(font != null && request))
		{
			return;
		}
		font.RequestCharactersInTexture(")_-", finalSize, fontStyle);
		if (!font.GetCharacterInfo(')', out mTempChar, finalSize, fontStyle) || (float)mTempChar.maxY == 0f)
		{
			font.RequestCharactersInTexture("A", finalSize, fontStyle);
			if (!font.GetCharacterInfo('A', out mTempChar, finalSize, fontStyle))
			{
				baseline = 0f;
				return;
			}
		}
		float num = mTempChar.maxY;
		float num2 = mTempChar.minY;
		baseline = Mathf.Round(num + ((float)finalSize - num + num2) * 0.5f);
	}

	public static void Prepare(string text)
	{
		if (dynamicFont != null)
		{
			dynamicFont.RequestCharactersInTexture(text, finalSize, fontStyle);
		}
	}

	public static BMSymbol GetSymbol(string text, int index, int textLength)
	{
		if (!(bitmapFont != null))
		{
			return null;
		}
		return bitmapFont.MatchSymbol(text, index, textLength);
	}

	public static float GetGlyphWidth(int ch, int prev)
	{
		if (bitmapFont != null)
		{
			bool flag = false;
			if (ch == 8201)
			{
				flag = true;
				ch = 32;
			}
			BMGlyph bMGlyph = bitmapFont.bmFont.GetGlyph(ch);
			if (bMGlyph != null)
			{
				int num = bMGlyph.advance;
				if (flag)
				{
					num >>= 1;
				}
				return fontScale * (float)((prev != 0) ? (num + bMGlyph.GetKerning(prev)) : bMGlyph.advance);
			}
		}
		else if (dynamicFont != null && dynamicFont.GetCharacterInfo((char)ch, out mTempChar, finalSize, fontStyle))
		{
			return (float)mTempChar.advance * fontScale * pixelDensity;
		}
		return 0f;
	}

	public static GlyphInfo GetGlyph(int ch, int prev)
	{
		if (bitmapFont != null)
		{
			bool flag = false;
			if (ch == 8201)
			{
				flag = true;
				ch = 32;
			}
			BMGlyph bMGlyph = bitmapFont.bmFont.GetGlyph(ch);
			if (bMGlyph != null)
			{
				int num = ((prev != 0) ? bMGlyph.GetKerning(prev) : 0);
				glyph.v0.x = ((prev != 0) ? (bMGlyph.offsetX + num) : bMGlyph.offsetX);
				glyph.v1.y = -bMGlyph.offsetY;
				glyph.v1.x = glyph.v0.x + (float)bMGlyph.width;
				glyph.v0.y = glyph.v1.y - (float)bMGlyph.height;
				glyph.u0.x = bMGlyph.x;
				glyph.u0.y = bMGlyph.y + bMGlyph.height;
				glyph.u2.x = bMGlyph.x + bMGlyph.width;
				glyph.u2.y = bMGlyph.y;
				glyph.u1.x = glyph.u0.x;
				glyph.u1.y = glyph.u2.y;
				glyph.u3.x = glyph.u2.x;
				glyph.u3.y = glyph.u0.y;
				int num2 = bMGlyph.advance;
				if (flag)
				{
					num2 >>= 1;
				}
				glyph.advance = num2 + num;
				glyph.channel = bMGlyph.channel;
				if (fontScale != 1f)
				{
					glyph.v0 *= fontScale;
					glyph.v1 *= fontScale;
					glyph.advance *= fontScale;
				}
				return glyph;
			}
		}
		else if (dynamicFont != null && dynamicFont.GetCharacterInfo((char)ch, out mTempChar, finalSize, fontStyle))
		{
			glyph.v0.x = mTempChar.minX;
			glyph.v1.x = mTempChar.maxX;
			glyph.v0.y = (float)mTempChar.maxY - baseline;
			glyph.v1.y = (float)mTempChar.minY - baseline;
			glyph.u0 = mTempChar.uvTopLeft;
			glyph.u1 = mTempChar.uvBottomLeft;
			glyph.u2 = mTempChar.uvBottomRight;
			glyph.u3 = mTempChar.uvTopRight;
			glyph.advance = mTempChar.advance;
			glyph.channel = 0;
			glyph.v0.x = Mathf.Round(glyph.v0.x);
			glyph.v0.y = Mathf.Round(glyph.v0.y);
			glyph.v1.x = Mathf.Round(glyph.v1.x);
			glyph.v1.y = Mathf.Round(glyph.v1.y);
			float num3 = fontScale * pixelDensity;
			if (num3 != 1f)
			{
				glyph.v0 *= num3;
				glyph.v1 *= num3;
				glyph.advance *= num3;
			}
			return glyph;
		}
		return null;
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static float ParseAlpha(string text, int index)
	{
		return Mathf.Clamp01((float)((NGUIMath.HexToDecimal(text[index + 1]) << 4) | NGUIMath.HexToDecimal(text[index + 2])) / 255f);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static Color ParseColor(string text, int offset = 0)
	{
		return ParseColor24(text, offset);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static Color ParseColor24(string text, int offset = 0)
	{
		int num = (NGUIMath.HexToDecimal(text[offset]) << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
		int num2 = (NGUIMath.HexToDecimal(text[offset + 2]) << 4) | NGUIMath.HexToDecimal(text[offset + 3]);
		int num3 = (NGUIMath.HexToDecimal(text[offset + 4]) << 4) | NGUIMath.HexToDecimal(text[offset + 5]);
		float num4 = 0.003921569f;
		return new Color(num4 * (float)num, num4 * (float)num2, num4 * (float)num3);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static Color ParseColor32(string text, int offset)
	{
		int num = (NGUIMath.HexToDecimal(text[offset]) << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
		int num2 = (NGUIMath.HexToDecimal(text[offset + 2]) << 4) | NGUIMath.HexToDecimal(text[offset + 3]);
		int num3 = (NGUIMath.HexToDecimal(text[offset + 4]) << 4) | NGUIMath.HexToDecimal(text[offset + 5]);
		int num4 = (NGUIMath.HexToDecimal(text[offset + 6]) << 4) | NGUIMath.HexToDecimal(text[offset + 7]);
		float num5 = 0.003921569f;
		return new Color(num5 * (float)num, num5 * (float)num2, num5 * (float)num3, num5 * (float)num4);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static string EncodeColor(Color c)
	{
		return EncodeColor24(c);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static string EncodeColor(string text, Color c)
	{
		return "[c][" + EncodeColor24(c) + "]" + text + "[-][/c]";
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static string EncodeAlpha(float a)
	{
		return NGUIMath.DecimalToHex8(Mathf.Clamp(Mathf.RoundToInt(a * 255f), 0, 255));
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static string EncodeColor24(Color c)
	{
		return NGUIMath.DecimalToHex24(0xFFFFFF & (NGUIMath.ColorToInt(c) >> 8));
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static string EncodeColor32(Color c)
	{
		return NGUIMath.DecimalToHex32(NGUIMath.ColorToInt(c));
	}

	public static bool ParseSymbol(string text, ref int index)
	{
		int sub = 1;
		bool bold = false;
		bool italic = false;
		bool underline = false;
		bool strike = false;
		bool ignoreColor = false;
		return ParseSymbol(text, ref index, null, premultiply: false, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static bool IsHex(char ch)
	{
		if ((ch < '0' || ch > '9') && (ch < 'a' || ch > 'f'))
		{
			if (ch >= 'A')
			{
				return ch <= 'F';
			}
			return false;
		}
		return true;
	}

	public static bool ParseSymbol(string text, ref int index, BetterList<Color> colors, bool premultiply, ref int sub, ref bool bold, ref bool italic, ref bool underline, ref bool strike, ref bool ignoreColor)
	{
		int length = text.Length;
		if (index + 3 > length || text[index] != '[')
		{
			return false;
		}
		if (text[index + 2] == ']')
		{
			if (text[index + 1] == '-')
			{
				if (colors != null && colors.size > 1)
				{
					colors.RemoveAt(colors.size - 1);
				}
				index += 3;
				return true;
			}
			switch (text.Substring(index, 3))
			{
			case "[b]":
				bold = true;
				index += 3;
				return true;
			case "[i]":
				italic = true;
				index += 3;
				return true;
			case "[u]":
				underline = true;
				index += 3;
				return true;
			case "[s]":
				strike = true;
				index += 3;
				return true;
			case "[c]":
				ignoreColor = true;
				index += 3;
				return true;
			}
		}
		if (index + 4 > length)
		{
			return false;
		}
		if (text[index + 3] == ']')
		{
			switch (text.Substring(index, 4))
			{
			case "[/b]":
				bold = false;
				index += 4;
				return true;
			case "[/i]":
				italic = false;
				index += 4;
				return true;
			case "[/u]":
				underline = false;
				index += 4;
				return true;
			case "[/s]":
				strike = false;
				index += 4;
				return true;
			case "[/c]":
				ignoreColor = false;
				index += 4;
				return true;
			}
			char ch = text[index + 1];
			char ch2 = text[index + 2];
			if (IsHex(ch) && IsHex(ch2))
			{
				mAlpha = (float)((NGUIMath.HexToDecimal(ch) << 4) | NGUIMath.HexToDecimal(ch2)) / 255f;
				index += 4;
				return true;
			}
		}
		if (index + 5 > length)
		{
			return false;
		}
		if (text[index + 4] == ']')
		{
			switch (text.Substring(index, 5))
			{
			case "[sub]":
				sub = 1;
				index += 5;
				return true;
			case "[sup]":
				sub = 2;
				index += 5;
				return true;
			}
		}
		if (index + 6 > length)
		{
			return false;
		}
		if (text[index + 5] == ']')
		{
			switch (text.Substring(index, 6))
			{
			case "[/sub]":
				sub = 0;
				index += 6;
				return true;
			case "[/sup]":
				sub = 0;
				index += 6;
				return true;
			case "[/url]":
				index += 6;
				return true;
			}
		}
		if (text[index + 1] == 'u' && text[index + 2] == 'r' && text[index + 3] == 'l' && text[index + 4] == '=')
		{
			int num = text.IndexOf(']', index + 4);
			if (num != -1)
			{
				index = num + 1;
				return true;
			}
			index = text.Length;
			return true;
		}
		if (index + 8 > length)
		{
			return false;
		}
		if (text[index + 7] == ']')
		{
			Color color = ParseColor24(text, index + 1);
			if (EncodeColor24(color) != text.Substring(index + 1, 6).ToUpper())
			{
				return false;
			}
			if (colors != null)
			{
				color.a = colors[colors.size - 1].a;
				if (premultiply && color.a != 1f)
				{
					color = Color.Lerp(mInvisible, color, color.a);
				}
				colors.Add(color);
			}
			index += 8;
			return true;
		}
		if (index + 10 > length)
		{
			return false;
		}
		if (text[index + 9] == ']')
		{
			Color color2 = ParseColor32(text, index + 1);
			if (EncodeColor32(color2) != text.Substring(index + 1, 8).ToUpper())
			{
				return false;
			}
			if (colors != null)
			{
				if (premultiply && color2.a != 1f)
				{
					color2 = Color.Lerp(mInvisible, color2, color2.a);
				}
				colors.Add(color2);
			}
			index += 10;
			return true;
		}
		return false;
	}

	public static string StripSymbols(string text)
	{
		if (text != null)
		{
			int num = 0;
			int length = text.Length;
			while (num < length)
			{
				if (text[num] == '[')
				{
					int sub = 0;
					bool bold = false;
					bool italic = false;
					bool underline = false;
					bool strike = false;
					bool ignoreColor = false;
					int index = num;
					if (ParseSymbol(text, ref index, null, premultiply: false, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor))
					{
						text = text.Remove(num, index - num);
						length = text.Length;
						continue;
					}
				}
				num++;
			}
		}
		return text;
	}

	public static void Align(List<Vector3> verts, int indexOffset, float printedWidth, int elements = 4)
	{
		switch (alignment)
		{
		case Alignment.Right:
		{
			float num12 = (float)rectWidth - printedWidth;
			if (!(num12 < 0f))
			{
				int j = indexOffset;
				for (int count3 = verts.Count; j < count3; j++)
				{
					Vector3 value3 = verts[j];
					value3.x += num12;
					verts[j] = value3;
				}
			}
			break;
		}
		case Alignment.Center:
		{
			float num9 = ((float)rectWidth - printedWidth) * 0.5f;
			if (!(num9 < 0f))
			{
				int num10 = Mathf.RoundToInt((float)rectWidth - printedWidth);
				int num11 = Mathf.RoundToInt(rectWidth);
				bool flag = (num10 & 1) == 1;
				bool flag2 = (num11 & 1) == 1;
				if ((flag && !flag2) || (!flag && flag2))
				{
					num9 += 0.5f * fontScale;
				}
				int i = indexOffset;
				for (int count2 = verts.Count; i < count2; i++)
				{
					Vector3 value2 = verts[i];
					value2.x += num9;
					verts[i] = value2;
				}
			}
			break;
		}
		case Alignment.Justified:
		{
			if (printedWidth < (float)rectWidth * 0.65f || ((float)rectWidth - printedWidth) * 0.5f < 1f)
			{
				break;
			}
			int num = (verts.Count - indexOffset) / elements;
			if (num < 1)
			{
				break;
			}
			float num2 = 1f / (float)(num - 1);
			float num3 = (float)rectWidth / printedWidth;
			int num4 = indexOffset + elements;
			int num5 = 1;
			int count = verts.Count;
			while (num4 < count)
			{
				float x = verts[num4].x;
				float x2 = verts[num4 + elements / 2].x;
				float num6 = x2 - x;
				float num7 = x * num3;
				float a = num7 + num6;
				float num8 = x2 * num3;
				float b = num8 - num6;
				float t = (float)num5 * num2;
				x2 = Mathf.Lerp(a, num8, t);
				x = Mathf.Lerp(num7, b, t);
				x = Mathf.Round(x);
				x2 = Mathf.Round(x2);
				switch (elements)
				{
				case 4:
				{
					Vector3 value = verts[num4];
					value.x = x;
					verts[num4++] = value;
					value = verts[num4];
					value.x = x;
					verts[num4++] = value;
					value = verts[num4];
					value.x = x2;
					verts[num4++] = value;
					value = verts[num4];
					value.x = x2;
					verts[num4++] = value;
					break;
				}
				case 2:
				{
					Vector3 value = verts[num4];
					value.x = x;
					verts[num4++] = value;
					value = verts[num4];
					value.x = x2;
					verts[num4++] = value;
					break;
				}
				case 1:
				{
					Vector3 value = verts[num4];
					value.x = x;
					verts[num4++] = value;
					break;
				}
				}
				num5++;
			}
			break;
		}
		}
	}

	public static int GetExactCharacterIndex(List<Vector3> verts, List<int> indices, Vector2 pos)
	{
		int i = 0;
		for (int count = indices.Count; i < count; i++)
		{
			int num = i << 1;
			int index = num + 1;
			float x = verts[num].x;
			if (pos.x < x)
			{
				continue;
			}
			float x2 = verts[index].x;
			if (pos.x > x2)
			{
				continue;
			}
			float y = verts[num].y;
			if (!(pos.y < y))
			{
				float y2 = verts[index].y;
				if (!(pos.y > y2))
				{
					return indices[i];
				}
			}
		}
		return 0;
	}

	public static int GetApproximateCharacterIndex(List<Vector3> verts, List<int> indices, Vector2 pos)
	{
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		int index = 0;
		int i = 0;
		for (int count = verts.Count; i < count; i++)
		{
			float num3 = Mathf.Abs(pos.y - verts[i].y);
			if (!(num3 > num2))
			{
				float num4 = Mathf.Abs(pos.x - verts[i].x);
				if (num3 < num2)
				{
					num2 = num3;
					num = num4;
					index = i;
				}
				else if (num4 < num)
				{
					num = num4;
					index = i;
				}
			}
		}
		return indices[index];
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	private static bool IsSpace(int ch)
	{
		if (ch != 32 && ch != 8202 && ch != 8203)
		{
			return ch == 8201;
		}
		return true;
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static void EndLine(ref StringBuilder s)
	{
		int num = s.Length - 1;
		if (num > 0 && IsSpace(s[num]))
		{
			s[num] = '\n';
		}
		else
		{
			s.Append('\n');
		}
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	private static void ReplaceSpaceWithNewline(ref StringBuilder s)
	{
		int num = s.Length - 1;
		if (num > 0 && IsSpace(s[num]))
		{
			s[num] = '\n';
		}
	}

	public static Vector2 CalculatePrintedSize(string text)
	{
		Vector2 zero = Vector2.zero;
		if (!string.IsNullOrEmpty(text))
		{
			if (encoding)
			{
				text = StripSymbols(text);
			}
			Prepare(text);
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			int length = text.Length;
			int num4 = 0;
			int prev = 0;
			for (int i = 0; i < length; i++)
			{
				num4 = text[i];
				if (num4 == 10)
				{
					if (num > num3)
					{
						num3 = num;
					}
					num = 0f;
					num2 += finalLineHeight;
				}
				else
				{
					if (num4 < 32)
					{
						continue;
					}
					BMSymbol bMSymbol = (useSymbols ? GetSymbol(text, i, length) : null);
					if (bMSymbol == null)
					{
						float glyphWidth = GetGlyphWidth(num4, prev);
						if (glyphWidth == 0f)
						{
							continue;
						}
						glyphWidth += finalSpacingX;
						if (Mathf.RoundToInt(num + glyphWidth) > regionWidth)
						{
							if (num > num3)
							{
								num3 = num - finalSpacingX;
							}
							num = glyphWidth;
							num2 += finalLineHeight;
						}
						else
						{
							num += glyphWidth;
						}
						prev = num4;
						continue;
					}
					float num5 = finalSpacingX + (float)bMSymbol.advance * fontScale;
					if (Mathf.RoundToInt(num + num5) > regionWidth)
					{
						if (num > num3)
						{
							num3 = num - finalSpacingX;
						}
						num = num5;
						num2 += finalLineHeight;
					}
					else
					{
						num += num5;
					}
					i += bMSymbol.sequence.Length - 1;
					prev = 0;
				}
			}
			zero.x = ((num > num3) ? (num - finalSpacingX) : num3);
			zero.y = num2 + finalLineHeight;
		}
		return zero;
	}

	public static int CalculateOffsetToFit(string text)
	{
		if (string.IsNullOrEmpty(text) || regionWidth < 1)
		{
			return 0;
		}
		Prepare(text);
		int length = text.Length;
		int prev = 0;
		int i = 0;
		for (int length2 = text.Length; i < length2; i++)
		{
			BMSymbol bMSymbol = (useSymbols ? GetSymbol(text, i, length) : null);
			if (bMSymbol == null)
			{
				char num = text[i];
				float glyphWidth = GetGlyphWidth(num, prev);
				if (glyphWidth != 0f)
				{
					mSizes.Add(finalSpacingX + glyphWidth);
				}
				prev = num;
				continue;
			}
			mSizes.Add(finalSpacingX + (float)bMSymbol.advance * fontScale);
			int j = 0;
			for (int num2 = bMSymbol.sequence.Length - 1; j < num2; j++)
			{
				mSizes.Add(0f);
			}
			i += bMSymbol.sequence.Length - 1;
			prev = 0;
		}
		float num3 = regionWidth;
		int num4 = mSizes.size;
		while (num4 > 0 && num3 > 0f)
		{
			num3 -= mSizes[--num4];
		}
		mSizes.Clear();
		if (num3 < 0f)
		{
			num4++;
		}
		return num4;
	}

	public static string GetEndOfLineThatFits(string text)
	{
		int length = text.Length;
		int num = CalculateOffsetToFit(text);
		return text.Substring(num, length - num);
	}

	public static bool WrapText(string text, out string finalText, bool wrapLineColors = false)
	{
		return WrapText(text, out finalText, keepCharCount: false, wrapLineColors);
	}

	public static bool WrapText(string text, out string finalText, bool keepCharCount, bool wrapLineColors, bool useEllipsis = false)
	{
		if (regionWidth < 1 || regionHeight < 1 || finalLineHeight < 1f)
		{
			finalText = "";
			return false;
		}
		float num = ((maxLines > 0) ? Mathf.Min(regionHeight, finalLineHeight * (float)maxLines) : ((float)regionHeight));
		int num2 = ((maxLines > 0) ? maxLines : 1000000);
		num2 = Mathf.FloorToInt(Mathf.Min(num2, num / finalLineHeight) + 0.01f);
		if (num2 == 0)
		{
			finalText = "";
			return false;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		StringBuilder s = new StringBuilder();
		int length = text.Length;
		float num3 = regionWidth;
		int num4 = 0;
		int i = 0;
		int num5 = 1;
		int prev = 0;
		bool flag = true;
		bool flag2 = true;
		bool flag3 = false;
		Color color = tint;
		int sub = 0;
		bool bold = false;
		bool italic = false;
		bool underline = false;
		bool strike = false;
		bool ignoreColor = false;
		if (!useSymbols)
		{
			wrapLineColors = false;
		}
		if (wrapLineColors)
		{
			mColors.Add(color);
			s.Append("[");
			s.Append(EncodeColor(color));
			s.Append("]");
		}
		for (; i < length; i++)
		{
			char c = text[i];
			if (c > '⿿')
			{
				flag3 = true;
			}
			if (c == '\n')
			{
				if (num5 == num2)
				{
					break;
				}
				num3 = regionWidth;
				if (num4 < i)
				{
					s.Append(text.Substring(num4, i - num4 + 1));
				}
				else
				{
					s.Append(c);
				}
				if (wrapLineColors)
				{
					for (int j = 0; j < mColors.size; j++)
					{
						s.Insert(s.Length - 1, "[-]");
					}
					for (int k = 0; k < mColors.size; k++)
					{
						s.Append("[");
						s.Append(EncodeColor(mColors[k]));
						s.Append("]");
					}
				}
				flag = true;
				num5++;
				num4 = i + 1;
				prev = 0;
				continue;
			}
			if (encoding)
			{
				if (!wrapLineColors)
				{
					if (ParseSymbol(text, ref i))
					{
						i--;
						continue;
					}
				}
				else if (ParseSymbol(text, ref i, mColors, premultiply, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor))
				{
					if (ignoreColor)
					{
						color = mColors[mColors.size - 1];
						color.a *= mAlpha * tint.a;
					}
					else
					{
						color = tint * mColors[mColors.size - 1];
						color.a *= mAlpha;
					}
					int l = 0;
					for (int num6 = mColors.size - 2; l < num6; l++)
					{
						color.a *= mColors[l].a;
					}
					i--;
					if (num4 < i)
					{
						s.Append(text.Substring(num4, i - num4 + 1));
					}
					else
					{
						s.Append(c);
					}
					num4 = i + 1;
					continue;
				}
			}
			BMSymbol bMSymbol = (useSymbols ? GetSymbol(text, i, length) : null);
			float num7;
			if (bMSymbol == null)
			{
				float glyphWidth = GetGlyphWidth(c, prev);
				if (glyphWidth == 0f && !IsSpace(c))
				{
					continue;
				}
				num7 = finalSpacingX + glyphWidth;
			}
			else
			{
				num7 = finalSpacingX + (float)bMSymbol.advance * fontScale;
			}
			num3 -= num7;
			if (IsSpace(c) && !flag3 && num4 < i)
			{
				int num8 = i - num4 + 1;
				if (num5 == num2 && num3 <= 0f && i < length)
				{
					char c2 = text[i];
					if (c2 < ' ' || IsSpace(c2))
					{
						num8--;
					}
				}
				s.Append(text.Substring(num4, num8));
				flag = false;
				num4 = i + 1;
				prev = c;
			}
			if (Mathf.RoundToInt(num3) < 0)
			{
				if (!flag && num5 != num2)
				{
					flag = true;
					num3 = regionWidth;
					i = num4 - 1;
					prev = 0;
					if (num5++ == num2)
					{
						break;
					}
					if (keepCharCount)
					{
						ReplaceSpaceWithNewline(ref s);
					}
					else
					{
						EndLine(ref s);
					}
					if (wrapLineColors)
					{
						for (int m = 0; m < mColors.size; m++)
						{
							s.Insert(s.Length - 1, "[-]");
						}
						for (int n = 0; n < mColors.size; n++)
						{
							s.Append("[");
							s.Append(EncodeColor(mColors[n]));
							s.Append("]");
						}
					}
					continue;
				}
				if (useEllipsis && num5 == num2 && i > 1)
				{
					float num9 = GetGlyphWidth(46, 46) * 3f;
					if (num9 < (float)regionWidth)
					{
						num3 += num7;
						int num10 = i;
						int num11 = 0;
						while (num10 > 1 && num3 < num9)
						{
							num10--;
							char prev2 = text[num10 - 1];
							char ch = text[num10];
							bool flag4 = num3 == 0f && IsSpace(ch);
							num3 += GetGlyphWidth(ch, prev2);
							if (num10 < num4 && !flag4)
							{
								num11++;
							}
						}
						if (num3 >= num9)
						{
							if (num11 > 0)
							{
								s.Length = Mathf.Max(0, s.Length - num11);
							}
							s.Append(text.Substring(num4, Mathf.Max(0, num10 - num4)));
							while (s.Length > 0 && IsSpace(s[s.Length - 1]))
							{
								StringBuilder stringBuilder = s;
								int length2 = stringBuilder.Length - 1;
								stringBuilder.Length = length2;
							}
							s.Append("...");
							num5++;
							num4 = (i = num10);
							break;
						}
					}
				}
				s.Append(text.Substring(num4, Mathf.Max(0, i - num4)));
				bool flag5 = IsSpace(c);
				if (!flag5 && !flag3)
				{
					flag2 = false;
				}
				if (wrapLineColors && mColors.size > 0)
				{
					s.Append("[-]");
				}
				if (num5++ == num2)
				{
					num4 = i;
					break;
				}
				if (keepCharCount)
				{
					ReplaceSpaceWithNewline(ref s);
				}
				else
				{
					EndLine(ref s);
				}
				if (wrapLineColors)
				{
					for (int num12 = 0; num12 < mColors.size; num12++)
					{
						s.Insert(s.Length - 1, "[-]");
					}
					for (int num13 = 0; num13 < mColors.size; num13++)
					{
						s.Append("[");
						s.Append(EncodeColor(mColors[num13]));
						s.Append("]");
					}
				}
				flag = true;
				if (flag5)
				{
					num4 = i + 1;
					num3 = regionWidth;
				}
				else
				{
					num4 = i;
					num3 = (float)regionWidth - num7;
				}
				prev = 0;
			}
			else
			{
				prev = c;
			}
			if (bMSymbol != null)
			{
				i += bMSymbol.length - 1;
				prev = 0;
			}
		}
		if (num4 < i)
		{
			s.Append(text.Substring(num4, i - num4));
		}
		if (wrapLineColors && mColors.size > 0)
		{
			s.Append("[-]");
		}
		finalText = s.ToString();
		mColors.Clear();
		if (flag2)
		{
			if (i != length)
			{
				return num5 <= Mathf.Min(maxLines, num2);
			}
			return true;
		}
		return false;
	}

	public static void Print(string text, List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		int count = verts.Count;
		Prepare(text);
		mColors.Add(Color.white);
		mAlpha = 1f;
		int num = 0;
		int prev = 0;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = finalSize;
		Color a = tint * gradientBottom;
		Color b = tint * gradientTop;
		Color color = tint;
		int length = text.Length;
		Rect rect = default(Rect);
		float num6 = 0f;
		float num7 = 0f;
		float num8 = num5 * pixelDensity;
		bool flag = false;
		int sub = 0;
		bool bold = false;
		bool italic = false;
		bool underline = false;
		bool strike = false;
		bool ignoreColor = false;
		float num9 = 0f;
		int num10 = 0;
		if (bitmapFont != null)
		{
			rect = bitmapFont.uvRect;
			num6 = rect.width / (float)bitmapFont.texWidth;
			num7 = rect.height / (float)bitmapFont.texHeight;
			num10 = bitmapFont.shift_y;
		}
		for (int i = 0; i < length; i++)
		{
			num = text[i];
			num9 = num2;
			if (num == 10)
			{
				if (num2 > num4)
				{
					num4 = num2;
				}
				if (alignment != Alignment.Left)
				{
					Align(verts, count, num2 - finalSpacingX);
					count = verts.Count;
				}
				num2 = 0f;
				num3 += finalLineHeight;
				prev = 0;
				continue;
			}
			if (num < 32)
			{
				prev = num;
				continue;
			}
			if (encoding && ParseSymbol(text, ref i, mColors, premultiply, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor))
			{
				if (ignoreColor)
				{
					color = mColors[mColors.size - 1];
					color.a *= mAlpha * tint.a;
				}
				else
				{
					color = tint * mColors[mColors.size - 1];
					color.a *= mAlpha;
				}
				int j = 0;
				for (int num11 = mColors.size - 2; j < num11; j++)
				{
					color.a *= mColors[j].a;
				}
				if (gradient)
				{
					a = gradientBottom * color;
					b = gradientTop * color;
				}
				i--;
				continue;
			}
			BMSymbol bMSymbol = (useSymbols ? GetSymbol(text, i, length) : null);
			float num12;
			float num13;
			float num15;
			float num14;
			if (bMSymbol != null)
			{
				num12 = num2 + (float)bMSymbol.offsetX * fontScale;
				num13 = num12 + (float)bMSymbol.width * fontScale;
				num14 = 0f - (num3 + (float)bMSymbol.offsetY * fontScale);
				num15 = num14 - (float)bMSymbol.height * fontScale;
				if (Mathf.RoundToInt(num2 + (float)bMSymbol.advance * fontScale) > regionWidth)
				{
					if (num2 == 0f)
					{
						return;
					}
					if (alignment != Alignment.Left && count < verts.Count)
					{
						Align(verts, count, num2 - finalSpacingX);
						count = verts.Count;
					}
					num12 -= num2;
					num13 -= num2;
					num15 -= finalLineHeight;
					num14 -= finalLineHeight;
					num2 = 0f;
					num3 += finalLineHeight;
					num9 = 0f;
				}
				verts.Add(new Vector3(num12, num15));
				verts.Add(new Vector3(num12, num14));
				verts.Add(new Vector3(num13, num14));
				verts.Add(new Vector3(num13, num15));
				num2 += finalSpacingX + (float)bMSymbol.advance * fontScale;
				i += bMSymbol.length - 1;
				prev = 0;
				if (uvs != null)
				{
					Rect uvRect = bMSymbol.uvRect;
					float xMin = uvRect.xMin;
					float yMin = uvRect.yMin;
					float xMax = uvRect.xMax;
					float yMax = uvRect.yMax;
					uvs.Add(new Vector2(xMin, yMin));
					uvs.Add(new Vector2(xMin, yMax));
					uvs.Add(new Vector2(xMax, yMax));
					uvs.Add(new Vector2(xMax, yMin));
				}
				if (cols == null)
				{
					continue;
				}
				if (symbolStyle == SymbolStyle.Colored)
				{
					for (int k = 0; k < 4; k++)
					{
						cols.Add(color);
					}
					continue;
				}
				Color item = Color.white;
				if (symbolStyle == SymbolStyle.NoEffect)
				{
					item = new Color(1f, 0f, 1f, 0f);
				}
				else
				{
					item.a = color.a;
				}
				for (int l = 0; l < 4; l++)
				{
					cols.Add(item);
				}
				continue;
			}
			GlyphInfo glyphInfo = GetGlyph(num, prev);
			if (glyphInfo == null)
			{
				continue;
			}
			prev = num;
			if (sub != 0)
			{
				glyphInfo.v0.x *= 0.75f;
				glyphInfo.v0.y *= 0.75f;
				glyphInfo.v1.x *= 0.75f;
				glyphInfo.v1.y *= 0.75f;
				if (sub == 1)
				{
					glyphInfo.v0.y -= fontScale * (float)fontSize * 0.4f;
					glyphInfo.v1.y -= fontScale * (float)fontSize * 0.4f;
				}
				else
				{
					glyphInfo.v0.y += fontScale * (float)fontSize * 0.05f;
					glyphInfo.v1.y += fontScale * (float)fontSize * 0.05f;
				}
			}
			num12 = glyphInfo.v0.x + num2;
			num15 = glyphInfo.v0.y - num3;
			num13 = glyphInfo.v1.x + num2;
			num14 = glyphInfo.v1.y - num3;
			num15 += (float)num10;
			num14 += (float)num10;
			float num16 = glyphInfo.advance;
			if (finalSpacingX < 0f)
			{
				num16 += finalSpacingX;
			}
			if (Mathf.RoundToInt(num2 + num16) > regionWidth)
			{
				if (num2 == 0f)
				{
					return;
				}
				if (alignment != Alignment.Left && count < verts.Count)
				{
					Align(verts, count, num2 - finalSpacingX);
					count = verts.Count;
				}
				num12 -= num2;
				num13 -= num2;
				num15 -= finalLineHeight;
				num14 -= finalLineHeight;
				num2 = 0f;
				num3 += finalLineHeight;
				num9 = 0f;
			}
			if (IsSpace(num))
			{
				if (underline)
				{
					num = 95;
				}
				else if (strike)
				{
					num = 45;
				}
			}
			num2 += ((sub == 0) ? (finalSpacingX + glyphInfo.advance) : ((finalSpacingX + glyphInfo.advance) * 0.75f));
			if (sub != 0)
			{
				num2 = Mathf.Round(num2);
			}
			if (IsSpace(num))
			{
				continue;
			}
			if (uvs != null)
			{
				if (bitmapFont != null)
				{
					glyphInfo.u0.x = rect.xMin + num6 * glyphInfo.u0.x;
					glyphInfo.u2.x = rect.xMin + num6 * glyphInfo.u2.x;
					glyphInfo.u0.y = rect.yMax - num7 * glyphInfo.u0.y;
					glyphInfo.u2.y = rect.yMax - num7 * glyphInfo.u2.y;
					glyphInfo.u1.x = glyphInfo.u0.x;
					glyphInfo.u1.y = glyphInfo.u2.y;
					glyphInfo.u3.x = glyphInfo.u2.x;
					glyphInfo.u3.y = glyphInfo.u0.y;
				}
				int m = 0;
				for (int num17 = ((!bold) ? 1 : 4); m < num17; m++)
				{
					uvs.Add(glyphInfo.u0);
					uvs.Add(glyphInfo.u1);
					uvs.Add(glyphInfo.u2);
					uvs.Add(glyphInfo.u3);
				}
			}
			if (cols != null)
			{
				if (glyphInfo.channel == 0 || glyphInfo.channel == 15)
				{
					if (gradient)
					{
						float num18 = num8 + glyphInfo.v0.y / fontScale;
						float num19 = num8 + glyphInfo.v1.y / fontScale;
						num18 /= num8;
						num19 /= num8;
						s_c0 = Color.Lerp(a, b, num18);
						s_c1 = Color.Lerp(a, b, num19);
						int n = 0;
						for (int num20 = ((!bold) ? 1 : 4); n < num20; n++)
						{
							cols.Add(s_c0);
							cols.Add(s_c1);
							cols.Add(s_c1);
							cols.Add(s_c0);
						}
					}
					else
					{
						int num21 = 0;
						for (int num22 = (bold ? 16 : 4); num21 < num22; num21++)
						{
							cols.Add(color);
						}
					}
				}
				else
				{
					Color item2 = color;
					item2 *= 0.49f;
					switch (glyphInfo.channel)
					{
					case 1:
						item2.b += 0.51f;
						break;
					case 2:
						item2.g += 0.51f;
						break;
					case 4:
						item2.r += 0.51f;
						break;
					case 8:
						item2.a += 0.51f;
						break;
					}
					int num23 = 0;
					for (int num24 = (bold ? 16 : 4); num23 < num24; num23++)
					{
						cols.Add(item2);
					}
				}
			}
			if (!bold)
			{
				if (!italic)
				{
					verts.Add(new Vector3(num12, num15));
					verts.Add(new Vector3(num12, num14));
					verts.Add(new Vector3(num13, num14));
					verts.Add(new Vector3(num13, num15));
				}
				else
				{
					float num25 = (float)fontSize * 0.1f * ((num14 - num15) / (float)fontSize);
					verts.Add(new Vector3(num12 - num25, num15));
					verts.Add(new Vector3(num12 + num25, num14));
					verts.Add(new Vector3(num13 + num25, num14));
					verts.Add(new Vector3(num13 - num25, num15));
				}
			}
			else
			{
				for (int num26 = 0; num26 < 4; num26++)
				{
					float num27 = mBoldOffset[num26 * 2];
					float num28 = mBoldOffset[num26 * 2 + 1];
					float num29 = (italic ? ((float)fontSize * 0.1f * ((num14 - num15) / (float)fontSize)) : 0f);
					verts.Add(new Vector3(num12 + num27 - num29, num15 + num28));
					verts.Add(new Vector3(num12 + num27 + num29, num14 + num28));
					verts.Add(new Vector3(num13 + num27 + num29, num14 + num28));
					verts.Add(new Vector3(num13 + num27 - num29, num15 + num28));
				}
			}
			if (!(underline || strike))
			{
				continue;
			}
			GlyphInfo glyphInfo2 = GetGlyph(strike ? 45 : 95, prev);
			if (glyphInfo2 == null)
			{
				continue;
			}
			if (uvs != null)
			{
				if (bitmapFont != null)
				{
					glyphInfo2.u0.x = rect.xMin + num6 * glyphInfo2.u0.x;
					glyphInfo2.u2.x = rect.xMin + num6 * glyphInfo2.u2.x;
					glyphInfo2.u0.y = rect.yMax - num7 * glyphInfo2.u0.y;
					glyphInfo2.u2.y = rect.yMax - num7 * glyphInfo2.u2.y;
				}
				float x = (glyphInfo2.u0.x + glyphInfo2.u2.x) * 0.5f;
				int num30 = 0;
				for (int num31 = ((!bold) ? 1 : 4); num30 < num31; num30++)
				{
					uvs.Add(new Vector2(x, glyphInfo2.u0.y));
					uvs.Add(new Vector2(x, glyphInfo2.u2.y));
					uvs.Add(new Vector2(x, glyphInfo2.u2.y));
					uvs.Add(new Vector2(x, glyphInfo2.u0.y));
				}
			}
			if (flag && strike)
			{
				num15 = (0f - num3 + glyphInfo2.v0.y) * 0.75f;
				num14 = (0f - num3 + glyphInfo2.v1.y) * 0.75f;
			}
			else
			{
				num15 = 0f - num3 + glyphInfo2.v0.y;
				num14 = 0f - num3 + glyphInfo2.v1.y;
			}
			if (bold)
			{
				for (int num32 = 0; num32 < 4; num32++)
				{
					float num33 = mBoldOffset[num32 * 2];
					float num34 = mBoldOffset[num32 * 2 + 1];
					verts.Add(new Vector3(num9 + num33, num15 + num34));
					verts.Add(new Vector3(num9 + num33, num14 + num34));
					verts.Add(new Vector3(num2 + num33, num14 + num34));
					verts.Add(new Vector3(num2 + num33, num15 + num34));
				}
			}
			else
			{
				verts.Add(new Vector3(num9, num15));
				verts.Add(new Vector3(num9, num14));
				verts.Add(new Vector3(num2, num14));
				verts.Add(new Vector3(num2, num15));
			}
			if (gradient)
			{
				float num35 = num8 + glyphInfo2.v0.y / fontScale;
				float num36 = num8 + glyphInfo2.v1.y / fontScale;
				num35 /= num8;
				num36 /= num8;
				s_c0 = Color.Lerp(a, b, num35);
				s_c1 = Color.Lerp(a, b, num36);
				int num37 = 0;
				for (int num38 = ((!bold) ? 1 : 4); num37 < num38; num37++)
				{
					cols.Add(s_c0);
					cols.Add(s_c1);
					cols.Add(s_c1);
					cols.Add(s_c0);
				}
			}
			else
			{
				int num39 = 0;
				for (int num40 = (bold ? 16 : 4); num39 < num40; num39++)
				{
					cols.Add(color);
				}
			}
		}
		if (alignment != Alignment.Left && count < verts.Count)
		{
			Align(verts, count, num2 - finalSpacingX);
			count = verts.Count;
		}
		mColors.Clear();
	}

	public static void PrintApproximateCharacterPositions(string text, List<Vector3> verts, List<int> indices)
	{
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = (float)fontSize * fontScale * 0.5f;
		int length = text.Length;
		int count = verts.Count;
		int num5 = 0;
		int prev = 0;
		for (int i = 0; i < length; i++)
		{
			num5 = text[i];
			verts.Add(new Vector3(num, 0f - num2 - num4));
			indices.Add(i);
			if (num5 == 10)
			{
				if (num > num3)
				{
					num3 = num;
				}
				if (alignment != Alignment.Left)
				{
					Align(verts, count, num - finalSpacingX, 1);
					count = verts.Count;
				}
				num = 0f;
				num2 += finalLineHeight;
				prev = 0;
				continue;
			}
			if (num5 < 32)
			{
				prev = 0;
				continue;
			}
			if (encoding && ParseSymbol(text, ref i))
			{
				i--;
				continue;
			}
			BMSymbol bMSymbol = (useSymbols ? GetSymbol(text, i, length) : null);
			if (bMSymbol == null)
			{
				float glyphWidth = GetGlyphWidth(num5, prev);
				if (glyphWidth == 0f)
				{
					continue;
				}
				glyphWidth += finalSpacingX;
				if (Mathf.RoundToInt(num + glyphWidth) > regionWidth)
				{
					if (num == 0f)
					{
						return;
					}
					if (alignment != Alignment.Left && count < verts.Count)
					{
						Align(verts, count, num - finalSpacingX, 1);
						count = verts.Count;
					}
					num = glyphWidth;
					num2 += finalLineHeight;
				}
				else
				{
					num += glyphWidth;
				}
				verts.Add(new Vector3(num, 0f - num2 - num4));
				indices.Add(i + 1);
				prev = num5;
				continue;
			}
			float num6 = (float)bMSymbol.advance * fontScale + finalSpacingX;
			if (Mathf.RoundToInt(num + num6) > regionWidth)
			{
				if (num == 0f)
				{
					return;
				}
				if (alignment != Alignment.Left && count < verts.Count)
				{
					Align(verts, count, num - finalSpacingX, 1);
					count = verts.Count;
				}
				num = num6;
				num2 += finalLineHeight;
			}
			else
			{
				num += num6;
			}
			verts.Add(new Vector3(num, 0f - num2 - num4));
			indices.Add(i + 1);
			i += bMSymbol.sequence.Length - 1;
			prev = 0;
		}
		if (alignment != Alignment.Left && count < verts.Count)
		{
			Align(verts, count, num - finalSpacingX, 1);
		}
	}

	public static void PrintExactCharacterPositions(string text, List<Vector3> verts, List<int> indices)
	{
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		float num = (float)fontSize * fontScale;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		int length = text.Length;
		int count = verts.Count;
		int num5 = 0;
		int prev = 0;
		for (int i = 0; i < length; i++)
		{
			num5 = text[i];
			if (num5 == 10)
			{
				if (num2 > num4)
				{
					num4 = num2;
				}
				if (alignment != Alignment.Left)
				{
					Align(verts, count, num2 - finalSpacingX, 2);
					count = verts.Count;
				}
				num2 = 0f;
				num3 += finalLineHeight;
				prev = 0;
				continue;
			}
			if (num5 < 32)
			{
				prev = 0;
				continue;
			}
			if (encoding && ParseSymbol(text, ref i))
			{
				i--;
				continue;
			}
			BMSymbol bMSymbol = (useSymbols ? GetSymbol(text, i, length) : null);
			if (bMSymbol == null)
			{
				float glyphWidth = GetGlyphWidth(num5, prev);
				if (glyphWidth == 0f)
				{
					continue;
				}
				float num6 = glyphWidth + finalSpacingX;
				if (Mathf.RoundToInt(num2 + num6) > regionWidth)
				{
					if (num2 == 0f)
					{
						return;
					}
					if (alignment != Alignment.Left && count < verts.Count)
					{
						Align(verts, count, num2 - finalSpacingX, 2);
						count = verts.Count;
					}
					num2 = 0f;
					num3 += finalLineHeight;
					prev = 0;
					i--;
				}
				else
				{
					indices.Add(i);
					verts.Add(new Vector3(num2, 0f - num3 - num));
					verts.Add(new Vector3(num2 + num6, 0f - num3));
					prev = num5;
					num2 += num6;
				}
				continue;
			}
			float num7 = (float)bMSymbol.advance * fontScale + finalSpacingX;
			if (Mathf.RoundToInt(num2 + num7) > regionWidth)
			{
				if (num2 == 0f)
				{
					return;
				}
				if (alignment != Alignment.Left && count < verts.Count)
				{
					Align(verts, count, num2 - finalSpacingX, 2);
					count = verts.Count;
				}
				num2 = 0f;
				num3 += finalLineHeight;
				prev = 0;
				i--;
			}
			else
			{
				indices.Add(i);
				verts.Add(new Vector3(num2, 0f - num3 - num));
				verts.Add(new Vector3(num2 + num7, 0f - num3));
				i += bMSymbol.sequence.Length - 1;
				num2 += num7;
				prev = 0;
			}
		}
		if (alignment != Alignment.Left && count < verts.Count)
		{
			Align(verts, count, num2 - finalSpacingX, 2);
		}
	}

	public static void PrintCaretAndSelection(string text, int start, int end, List<Vector3> caret, List<Vector3> highlight)
	{
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		int num = end;
		if (start > end)
		{
			end = start;
			start = num;
		}
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = (float)fontSize * fontScale;
		int indexOffset = caret?.Count ?? 0;
		int num6 = highlight?.Count ?? 0;
		int length = text.Length;
		int i = 0;
		int num7 = 0;
		int prev = 0;
		bool flag = false;
		bool flag2 = false;
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		for (; i < length; i++)
		{
			if (caret != null && !flag2 && num <= i)
			{
				flag2 = true;
				caret.Add(new Vector3(num2 - 1f, 0f - num3 - num5));
				caret.Add(new Vector3(num2 - 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3 - num5));
			}
			num7 = text[i];
			if (num7 == 10)
			{
				if (num2 > num4)
				{
					num4 = num2;
				}
				if (caret != null && flag2)
				{
					if (alignment != Alignment.Left)
					{
						Align(caret, indexOffset, num2 - finalSpacingX);
					}
					caret = null;
				}
				if (highlight != null)
				{
					if (flag)
					{
						flag = false;
						highlight.Add(vector2);
						highlight.Add(vector);
					}
					else if (start <= i && end > i)
					{
						highlight.Add(new Vector3(num2, 0f - num3 - num5));
						highlight.Add(new Vector3(num2, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3 - num5));
					}
					if (alignment != Alignment.Left && num6 < highlight.Count)
					{
						Align(highlight, num6, num2 - finalSpacingX);
						num6 = highlight.Count;
					}
				}
				num2 = 0f;
				num3 += finalLineHeight;
				prev = 0;
				continue;
			}
			if (num7 < 32)
			{
				prev = 0;
				continue;
			}
			if (encoding && ParseSymbol(text, ref i))
			{
				i--;
				continue;
			}
			BMSymbol bMSymbol = (useSymbols ? GetSymbol(text, i, length) : null);
			float num8 = ((bMSymbol != null) ? ((float)bMSymbol.advance * fontScale) : GetGlyphWidth(num7, prev));
			if (num8 == 0f)
			{
				continue;
			}
			float num9 = num2;
			float num10 = num2 + num8;
			float num11 = 0f - num3 - num5;
			float num12 = 0f - num3;
			if (Mathf.RoundToInt(num10 + finalSpacingX) > regionWidth)
			{
				if (num2 == 0f)
				{
					return;
				}
				if (num2 > num4)
				{
					num4 = num2;
				}
				if (caret != null && flag2)
				{
					if (alignment != Alignment.Left)
					{
						Align(caret, indexOffset, num2 - finalSpacingX);
					}
					caret = null;
				}
				if (highlight != null)
				{
					if (flag)
					{
						flag = false;
						highlight.Add(vector2);
						highlight.Add(vector);
					}
					else if (start <= i && end > i)
					{
						highlight.Add(new Vector3(num2, 0f - num3 - num5));
						highlight.Add(new Vector3(num2, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3 - num5));
					}
					if (alignment != Alignment.Left && num6 < highlight.Count)
					{
						Align(highlight, num6, num2 - finalSpacingX);
						num6 = highlight.Count;
					}
				}
				num9 -= num2;
				num10 -= num2;
				num11 -= finalLineHeight;
				num12 -= finalLineHeight;
				num2 = 0f;
				num3 += finalLineHeight;
			}
			num2 += num8 + finalSpacingX;
			if (highlight != null)
			{
				if (start > i || end <= i)
				{
					if (flag)
					{
						flag = false;
						highlight.Add(vector2);
						highlight.Add(vector);
					}
				}
				else if (!flag)
				{
					flag = true;
					highlight.Add(new Vector3(num9, num11));
					highlight.Add(new Vector3(num9, num12));
				}
			}
			vector = new Vector2(num10, num11);
			vector2 = new Vector2(num10, num12);
			prev = num7;
		}
		if (caret != null)
		{
			if (!flag2)
			{
				caret.Add(new Vector3(num2 - 1f, 0f - num3 - num5));
				caret.Add(new Vector3(num2 - 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3 - num5));
			}
			if (alignment != Alignment.Left)
			{
				Align(caret, indexOffset, num2 - finalSpacingX);
			}
		}
		if (highlight != null)
		{
			if (flag)
			{
				highlight.Add(vector2);
				highlight.Add(vector);
			}
			else if (start < i && end == i)
			{
				highlight.Add(new Vector3(num2, 0f - num3 - num5));
				highlight.Add(new Vector3(num2, 0f - num3));
				highlight.Add(new Vector3(num2 + 2f, 0f - num3));
				highlight.Add(new Vector3(num2 + 2f, 0f - num3 - num5));
			}
			if (alignment != Alignment.Left && num6 < highlight.Count)
			{
				Align(highlight, num6, num2 - finalSpacingX);
			}
		}
	}

	public static bool ReplaceLink(ref string text, ref int index, string prefix)
	{
		if (index == -1)
		{
			return false;
		}
		index = text.IndexOf(prefix, index);
		if (index == -1)
		{
			return false;
		}
		int num = index + prefix.Length;
		int num2 = text.IndexOf(' ', num);
		if (num2 == -1)
		{
			num2 = text.Length;
		}
		int num3 = text.IndexOfAny(new char[2] { '/', ' ' }, num);
		if (num3 == -1 || num3 == num)
		{
			index += 7;
			return true;
		}
		string text2 = text.Substring(0, index);
		string text3 = text.Substring(index, num2 - index);
		string text4 = text.Substring(num2);
		string text5 = text.Substring(num, num3 - num);
		text = text2 + "[url=" + text3 + "][u]" + text5 + "[/u][/url]";
		index = text.Length;
		text += text4;
		return true;
	}

	public static bool InsertHyperlink(ref string text, ref int index, string keyword, string link)
	{
		int num = text.IndexOf(keyword, index, StringComparison.CurrentCultureIgnoreCase);
		if (num == -1)
		{
			return false;
		}
		string text2 = text.Substring(0, num);
		string text3 = "[url=" + link + "][u]";
		string text4 = text.Substring(num, keyword.Length) + "[/u][/url]";
		string text5 = text.Substring(num + keyword.Length);
		text = text2 + text3 + text4;
		index = text.Length;
		text += text5;
		return true;
	}

	public static void ReplaceLinks(ref string text)
	{
		int index = 0;
		while (index < text.Length && ReplaceLink(ref text, ref index, "http://"))
		{
		}
		int index2 = 0;
		while (index2 < text.Length && ReplaceLink(ref text, ref index2, "https://"))
		{
		}
	}
}
