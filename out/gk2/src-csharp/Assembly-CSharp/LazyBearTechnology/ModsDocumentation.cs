using UnityEngine;

namespace LazyBearTechnology;

public static class ModsDocumentation
{
	public const string GlobalReadmeResource = "Mods/README";

	public const string LanguagesReadmeResource = "Mods/Languages/README";

	public const string ExampleLanguageJsonResource = "Mods/Languages/language";

	public static string Load(string resourcePath)
	{
		TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
		if (textAsset == null)
		{
			Debug.LogWarning("[Mods] Missing documentation resource '" + resourcePath + "'.");
			return string.Empty;
		}
		return textAsset.text;
	}
}
