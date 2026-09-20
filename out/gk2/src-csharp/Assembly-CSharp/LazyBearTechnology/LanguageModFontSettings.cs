using System;

namespace LazyBearTechnology;

[Serializable]
public class LanguageModFontSettings
{
	public int faceIndex;

	public int pointSize;

	public int atlasWidth;

	public int atlasHeight;

	public bool overrideFaceInfo;

	public float lineHeight;

	public float ascentLine;

	public float capLine;

	public float meanLine;

	public float baseline;

	public float descentLine;
}
