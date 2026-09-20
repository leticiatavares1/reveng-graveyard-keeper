using System.Collections.Generic;
using TMPro;

namespace LazyBearTechnology;

public static class LanguageModHooks
{
	public delegate bool TryGetLanguageHandler(string langId, out LL language);

	public delegate bool TryGetFontAssetHandler(string lang, bool staticFont, out TMP_FontAsset fontAsset);

	public delegate bool UsesOwnMaterialHandler(string lang);

	public delegate bool RequiresResizeHandler(string lang);

	public delegate void ApplyDirectionHandler(TMP_Text label, string lang, bool staticFont);

	public delegate void AppendLanguagesHandler(Dictionary<string, LLBase.LanguageInfo> languages);

	public static TryGetLanguageHandler TryGetLanguage;

	public static TryGetFontAssetHandler TryGetFontAsset;

	public static UsesOwnMaterialHandler UsesOwnMaterial;

	public static RequiresResizeHandler RequiresResize;

	public static ApplyDirectionHandler ApplyDirection;

	public static AppendLanguagesHandler AppendLanguages;
}
