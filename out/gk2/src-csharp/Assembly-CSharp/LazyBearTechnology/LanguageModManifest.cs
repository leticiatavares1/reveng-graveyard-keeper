using System;

namespace LazyBearTechnology;

[Serializable]
public class LanguageModManifest
{
	public string name;

	public string font;

	public LanguageModFontSettings fontSettings;

	public bool rtl;

	public string[] preprocessors;

	public string fallback;

	public bool requireResize;
}
