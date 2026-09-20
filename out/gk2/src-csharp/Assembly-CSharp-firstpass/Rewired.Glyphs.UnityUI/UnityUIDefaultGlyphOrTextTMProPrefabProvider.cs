using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.Glyphs.UnityUI;

internal class UnityUIDefaultGlyphOrTextTMProPrefabProvider
{
	[RuntimeInitializeOnLoadMethod]
	private static void Initialize()
	{
		UnityUIControllerElementGlyphBase.defaultGlyphOrTextPrefabProvider = CreateDefaultGlyphOrTextPrefab;
	}

	private static GameObject CreateDefaultGlyphOrTextPrefab()
	{
		GameObject gameObject = new GameObject("Glyph or text prefab");
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		Object.DontDestroyOnLoad(gameObject);
		UnityUIGlyphOrTextTMPro unityUIGlyphOrTextTMPro = gameObject.AddComponent<UnityUIGlyphOrTextTMPro>();
		VerticalLayoutGroup verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
		verticalLayoutGroup.childControlHeight = true;
		verticalLayoutGroup.childControlWidth = true;
		verticalLayoutGroup.childForceExpandHeight = true;
		verticalLayoutGroup.childForceExpandWidth = true;
		GameObject gameObject2 = new GameObject("Glyph");
		gameObject2.hideFlags = HideFlags.HideAndDontSave;
		gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
		Image image = gameObject2.AddComponent<Image>();
		image.preserveAspect = true;
		unityUIGlyphOrTextTMPro.glyphComponent = image;
		GameObject gameObject3 = new GameObject("Text");
		gameObject3.hideFlags = HideFlags.HideAndDontSave;
		gameObject3.transform.SetParent(gameObject.transform, worldPositionStays: false);
		TextMeshProUGUI textMeshProUGUI = gameObject3.AddComponent<TextMeshProUGUI>();
		textMeshProUGUI.alignment = TextAlignmentOptions.Center;
		textMeshProUGUI.fontSize = 32f;
		textMeshProUGUI.enableAutoSizing = true;
		textMeshProUGUI.fontSizeMin = 10f;
		textMeshProUGUI.fontSizeMax = 32f;
		textMeshProUGUI.raycastTarget = false;
		unityUIGlyphOrTextTMPro.textComponent = textMeshProUGUI;
		return gameObject;
	}
}
