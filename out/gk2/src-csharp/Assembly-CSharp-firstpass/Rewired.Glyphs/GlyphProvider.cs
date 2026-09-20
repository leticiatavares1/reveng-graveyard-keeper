using System;
using System.Collections.Generic;
using System.Text;
using Rewired.Interfaces;
using Rewired.Utils;
using UnityEngine;

namespace Rewired.Glyphs;

public class GlyphProvider : MonoBehaviour, IGlyphProvider
{
	[SerializeField]
	[Tooltip("Determines if glyphs should be fetched immediately in bulk when available. If false, glyphs will be fetched when queried.")]
	private bool _prefetch;

	[SerializeField]
	[Tooltip("A list of glyph set collections. At least one collection must be assigned.")]
	private List<GlyphSetCollection> _glyphSetCollections;

	[NonSerialized]
	private readonly Dictionary<string, object> _glyphs = new Dictionary<string, object>();

	[NonSerialized]
	private bool _initialized;

	public bool prefetch
	{
		get
		{
			return _prefetch;
		}
		set
		{
			_prefetch = value;
			if (base.isActiveAndEnabled && ReInput.isReady && ReInput.glyphs.glyphProvider == this)
			{
				ReInput.glyphs.prefetch = value;
			}
		}
	}

	public List<GlyphSetCollection> glyphSetCollections
	{
		get
		{
			return _glyphSetCollections;
		}
		set
		{
			_glyphSetCollections = value;
			Reload();
		}
	}

	protected Dictionary<string, object> glyphs => _glyphs;

	protected virtual void OnEnable()
	{
		if (!_initialized)
		{
			Initialize();
		}
		TrySetGlyphProvider();
	}

	protected virtual void OnDisable()
	{
		if (ReInput.isReady && ReInput.glyphs.glyphProvider == this)
		{
			ReInput.glyphs.glyphProvider = null;
		}
		ReInput.InitializedEvent -= TrySetGlyphProvider;
	}

	protected virtual void Update()
	{
	}

	protected virtual void TrySetGlyphProvider()
	{
		ReInput.InitializedEvent -= TrySetGlyphProvider;
		ReInput.InitializedEvent += TrySetGlyphProvider;
		if (ReInput.isReady)
		{
			if (!UnityTools.IsNullOrDestroyed(ReInput.glyphs.glyphProvider))
			{
				Debug.LogWarning("Rewired: A glyph provider is already set. Only one glyph provider can exist at a time.");
				return;
			}
			ReInput.glyphs.glyphProvider = this;
			ReInput.glyphs.prefetch = _prefetch;
		}
	}

	protected virtual bool Initialize()
	{
		_initialized = false;
		if (_glyphSetCollections == null)
		{
			return false;
		}
		_glyphs.Clear();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < _glyphSetCollections.Count; i++)
		{
			GlyphSetCollection glyphSetCollection = _glyphSetCollections[i];
			if (glyphSetCollection == null)
			{
				continue;
			}
			foreach (GlyphSet item in glyphSetCollection.IterateSetsRecursively())
			{
				if (item == null || item.baseKeys == null)
				{
					continue;
				}
				int num = item.baseKeys.Length;
				for (int j = 0; j < num; j++)
				{
					if (string.IsNullOrEmpty(item.baseKeys[j]))
					{
						continue;
					}
					int glyphCount = item.glyphCount;
					for (int k = 0; k < glyphCount; k++)
					{
						GlyphSet.EntryBase entry = item.GetEntry(k);
						if (entry != null && !string.IsNullOrEmpty(entry.key) && entry.GetValue() != null)
						{
							stringBuilder.Append(item.baseKeys[j]);
							stringBuilder.Append('/');
							stringBuilder.Append(entry.key);
							string text = stringBuilder.ToString();
							stringBuilder.Length = 0;
							if (_glyphs.ContainsKey(text))
							{
								Debug.LogError("Rewired: Duplicate glyph key found: " + text);
							}
							else
							{
								_glyphs.Add(text, entry.GetValue());
							}
						}
					}
				}
			}
		}
		_initialized = true;
		return true;
	}

	public void Reload()
	{
		Initialize();
		if (base.isActiveAndEnabled && ReInput.isReady && ReInput.glyphs.glyphProvider == this)
		{
			ReInput.glyphs.Reload();
		}
	}

	bool IGlyphProvider.TryGetGlyph(string key, out object result)
	{
		if (!_initialized)
		{
			result = null;
			return false;
		}
		return _glyphs.TryGetValue(key, out result);
	}
}
