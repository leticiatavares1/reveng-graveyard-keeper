using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Glyphs;

public abstract class ControllerElementGlyphBase : MonoBehaviour
{
	protected class GlyphOrTextObject
	{
		private GlyphOrTextBase _glyphOrText;

		private int _frame;

		private bool _isVisible;

		public virtual bool isVisible
		{
			get
			{
				return _isVisible;
			}
			protected set
			{
				_isVisible = value;
			}
		}

		public GlyphOrTextBase glyphOrText
		{
			get
			{
				return _glyphOrText;
			}
			set
			{
				_glyphOrText = value;
			}
		}

		public GlyphOrTextObject(GlyphOrTextBase glyphOrText)
		{
			_glyphOrText = glyphOrText;
		}

		public virtual void ShowGlyph(object glyph)
		{
			if (!(_glyphOrText == null))
			{
				_glyphOrText.ShowGlyph(glyph);
				_frame = Time.frameCount;
				_isVisible = true;
			}
		}

		public virtual void ShowText(string text)
		{
			if (!(_glyphOrText == null))
			{
				_glyphOrText.ShowText(text);
				_frame = Time.frameCount;
				_isVisible = true;
			}
		}

		public virtual void Hide()
		{
			if (!(_glyphOrText == null) && _isVisible)
			{
				_glyphOrText.Hide();
				_isVisible = false;
			}
		}

		public virtual void HideIfIdle()
		{
			if (_frame != Time.frameCount)
			{
				Hide();
			}
		}

		public virtual void Destroy()
		{
			if (!(_glyphOrText == null))
			{
				UnityEngine.Object.Destroy(_glyphOrText.gameObject);
				_glyphOrText = null;
				_isVisible = false;
			}
		}
	}

	public enum AllowedTypes
	{
		All,
		Glyphs,
		Text
	}

	[Tooltip("If set, when glyph/text objects are created, they will be instantiated from this prefab. If left blank, the global default prefab will be used.")]
	[SerializeField]
	private GameObject _glyphOrTextPrefab;

	[Tooltip("Determines what types of objects are allowed.")]
	[SerializeField]
	private AllowedTypes _allowedTypes;

	[NonSerialized]
	private readonly List<GlyphOrTextObject> _entries = new List<GlyphOrTextObject>();

	[NonSerialized]
	private List<object> _tempGlyphs = new List<object>();

	[NonSerialized]
	private GameObject _lastGlyphOrTextPrefab;

	public virtual GameObject glyphOrTextPrefab
	{
		get
		{
			return _glyphOrTextPrefab;
		}
		set
		{
			_glyphOrTextPrefab = value;
			RequireRebuild();
		}
	}

	public virtual AllowedTypes allowedTypes
	{
		get
		{
			return _allowedTypes;
		}
		set
		{
			_allowedTypes = value;
		}
	}

	protected List<GlyphOrTextObject> entries => _entries;

	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void Update()
	{
		if (ReInput.isReady && _lastGlyphOrTextPrefab != GetGlyphOrTextPrefabOrDefault())
		{
			_lastGlyphOrTextPrefab = GetGlyphOrTextPrefabOrDefault();
			RequireRebuild();
		}
	}

	public virtual void RequireRebuild()
	{
		ClearObjects();
	}

	protected virtual void ClearObjects()
	{
		for (int i = 0; i < _entries.Count; i++)
		{
			if (_entries[i] != null)
			{
				_entries[i].Destroy();
			}
		}
		_entries.Clear();
		Hide();
	}

	protected virtual void EvaluateObjectVisibility()
	{
		for (int i = 0; i < _entries.Count; i++)
		{
			if (_entries[i] != null)
			{
				_entries[i].HideIfIdle();
			}
		}
	}

	protected virtual void EvaluateObjectVisibility(Transform transform)
	{
		EvaluateObjectVisibility(transform, _entries);
	}

	protected virtual void EvaluateObjectVisibility(Transform transform, List<GlyphOrTextObject> entries)
	{
		if (transform == base.transform)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < entries.Count; i++)
		{
			if (entries[i].isVisible)
			{
				flag = true;
			}
		}
		if (transform.gameObject.activeSelf != flag)
		{
			transform.gameObject.SetActive(flag);
		}
	}

	protected virtual int ShowGlyphsOrText(ActionElementMap actionElementMap, Transform parent, List<GlyphOrTextObject> entries)
	{
		_tempGlyphs.Clear();
		int num = 0;
		if (IsAllowed(AllowedTypes.Glyphs) && GetGlyphs(actionElementMap, _tempGlyphs) > 0)
		{
			if (!CreateObjectsAsNeeded(parent, entries, _tempGlyphs.Count))
			{
				return 0;
			}
			for (int i = 0; i < _tempGlyphs.Count; i++)
			{
				entries[i].ShowGlyph(_tempGlyphs[i]);
			}
			num += _tempGlyphs.Count;
		}
		else if (IsAllowed(AllowedTypes.Text) && actionElementMap != null)
		{
			if (!CreateObjectsAsNeeded(parent, entries, 1))
			{
				return 0;
			}
			entries[0].ShowText(actionElementMap.elementIdentifierName);
			num++;
		}
		return num;
	}

	protected virtual int ShowGlyphsOrText(ActionElementMap actionElementMap)
	{
		return ShowGlyphsOrText(actionElementMap, base.transform, _entries);
	}

	protected virtual int ShowGlyphsOrText(ControllerElementIdentifier elementIdentifier, AxisRange axisRange, Transform parent, List<GlyphOrTextObject> entries)
	{
		if (elementIdentifier == null)
		{
			return 0;
		}
		int num = 0;
		object glyph;
		if (IsAllowed(AllowedTypes.Glyphs) && (glyph = elementIdentifier.GetGlyph(axisRange)) != null)
		{
			if (!CreateObjectsAsNeeded(parent, entries, 1))
			{
				return 0;
			}
			entries[0].ShowGlyph(glyph);
			num++;
		}
		else if (IsAllowed(AllowedTypes.Text))
		{
			if (!CreateObjectsAsNeeded(parent, entries, 1))
			{
				return 0;
			}
			entries[0].ShowText(elementIdentifier.GetDisplayName(axisRange));
			num++;
		}
		return num;
	}

	protected virtual int ShowGlyphsOrText(ControllerElementIdentifier elementIdentifier, AxisRange axisRange)
	{
		return ShowGlyphsOrText(elementIdentifier, axisRange, base.transform, _entries);
	}

	protected virtual void Hide()
	{
		for (int i = 0; i < _entries.Count; i++)
		{
			if (_entries[i] != null)
			{
				_entries[i].Hide();
			}
		}
	}

	protected virtual GameObject GetGlyphOrTextPrefabOrDefault()
	{
		if (!(_glyphOrTextPrefab != null))
		{
			return GetDefaultGlyphOrTextPrefab();
		}
		return _glyphOrTextPrefab;
	}

	protected abstract GameObject GetDefaultGlyphOrTextPrefab();

	protected virtual bool CreateObjectsAsNeeded(Transform parent, List<GlyphOrTextObject> entries, int count)
	{
		if (count <= 0)
		{
			return false;
		}
		GameObject glyphOrTextPrefabOrDefault = GetGlyphOrTextPrefabOrDefault();
		if (glyphOrTextPrefabOrDefault == null)
		{
			Debug.LogError("Rewired: Default prefab is null.");
			return false;
		}
		if (entries == null)
		{
			return false;
		}
		for (int i = entries.Count; i < count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(glyphOrTextPrefabOrDefault);
			gameObject.name = "Object";
			gameObject.hideFlags = HideFlags.DontSave;
			gameObject.transform.SetParent(parent, worldPositionStays: false);
			GlyphOrTextBase component = gameObject.GetComponent<GlyphOrTextBase>();
			if (component == null)
			{
				Debug.LogError("Rewired: Prefab does not contain a " + typeof(GlyphOrTextBase)?.ToString() + " component.");
				UnityEngine.Object.Destroy(gameObject);
				continue;
			}
			GlyphOrTextObject item = new GlyphOrTextObject(component);
			entries.Add(item);
			if (entries != _entries)
			{
				_entries.Add(item);
			}
		}
		return true;
	}

	protected virtual bool IsAllowed(AllowedTypes allowedType)
	{
		if (_allowedTypes == AllowedTypes.All)
		{
			return true;
		}
		return allowedType == _allowedTypes;
	}

	protected static int GetGlyphs(ActionElementMap actionElementMap, List<object> results)
	{
		if (actionElementMap == null)
		{
			return 0;
		}
		int num = 1;
		if (actionElementMap.hasModifiers)
		{
			if (actionElementMap.modifierKey1 != 0)
			{
				num++;
			}
			if (actionElementMap.modifierKey2 != 0)
			{
				num++;
			}
			if (actionElementMap.modifierKey3 != 0)
			{
				num++;
			}
		}
		if (actionElementMap.elementIdentifierGlyphCount != num)
		{
			return 0;
		}
		actionElementMap.GetElementIdentifierGlyphs(results);
		return num;
	}
}
