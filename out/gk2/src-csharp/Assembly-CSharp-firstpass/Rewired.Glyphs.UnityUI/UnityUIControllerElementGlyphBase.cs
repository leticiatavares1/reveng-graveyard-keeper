using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.Glyphs.UnityUI;

public abstract class UnityUIControllerElementGlyphBase : ControllerElementGlyphBase
{
	private static GameObject s_defaultGlyphOrTextPrefab;

	private static Func<GameObject> s_defaultGlyphOrTextPrefabProvider;

	public static GameObject defaultGlyphOrTextPrefab
	{
		get
		{
			if (!(s_defaultGlyphOrTextPrefab != null))
			{
				return s_defaultGlyphOrTextPrefab = CreateDefaultGlyphOrTextPrefab();
			}
			return s_defaultGlyphOrTextPrefab;
		}
		set
		{
			s_defaultGlyphOrTextPrefab = value;
		}
	}

	public static Func<GameObject> defaultGlyphOrTextPrefabProvider
	{
		get
		{
			return s_defaultGlyphOrTextPrefabProvider;
		}
		set
		{
			s_defaultGlyphOrTextPrefabProvider = value;
		}
	}

	protected override GameObject GetDefaultGlyphOrTextPrefab()
	{
		return defaultGlyphOrTextPrefab;
	}

	private static GameObject CreateDefaultGlyphOrTextPrefab()
	{
		if (s_defaultGlyphOrTextPrefabProvider != null)
		{
			return s_defaultGlyphOrTextPrefabProvider();
		}
		GameObject gameObject = new GameObject("Glyph or text prefab");
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		UnityUIGlyphOrText unityUIGlyphOrText = gameObject.AddComponent<UnityUIGlyphOrText>();
		VerticalLayoutGroup verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
		verticalLayoutGroup.childControlHeight = true;
		verticalLayoutGroup.childControlWidth = true;
		verticalLayoutGroup.childForceExpandHeight = true;
		verticalLayoutGroup.childForceExpandWidth = true;
		GameObject obj = new GameObject("Glyph");
		obj.hideFlags = HideFlags.HideAndDontSave;
		obj.transform.SetParent(gameObject.transform, worldPositionStays: false);
		Image image = obj.AddComponent<Image>();
		image.preserveAspect = true;
		unityUIGlyphOrText.glyphComponent = image;
		GameObject obj2 = new GameObject("Text");
		obj2.hideFlags = HideFlags.HideAndDontSave;
		obj2.transform.SetParent(gameObject.transform, worldPositionStays: false);
		Text text = obj2.AddComponent<Text>();
		text.alignment = TextAnchor.MiddleCenter;
		text.fontSize = 32;
		text.resizeTextForBestFit = true;
		text.resizeTextMinSize = 10;
		text.resizeTextMaxSize = 32;
		text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		text.raycastTarget = false;
		unityUIGlyphOrText.textComponent = text;
		return gameObject;
	}
}
