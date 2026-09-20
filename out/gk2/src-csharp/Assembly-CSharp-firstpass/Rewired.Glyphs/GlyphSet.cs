using System;
using UnityEngine;

namespace Rewired.Glyphs;

[Serializable]
public abstract class GlyphSet : ScriptableObject
{
	[Serializable]
	public abstract class EntryBase
	{
		[SerializeField]
		private string _key;

		public string key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		public abstract object GetValue();
	}

	[Serializable]
	public abstract class EntryBase<TValue> : EntryBase
	{
		[SerializeField]
		private TValue _value;

		public TValue value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public override object GetValue()
		{
			return _value;
		}
	}

	[Tooltip("A list of base keys. Final keys will be composed of base key + glyph key. Setting multiple base keys allows one glyph set to apply to multiple controllers, for example.")]
	[SerializeField]
	private string[] _baseKeys;

	public string[] baseKeys
	{
		get
		{
			return _baseKeys;
		}
		set
		{
			_baseKeys = value;
		}
	}

	public abstract int glyphCount { get; }

	public abstract EntryBase GetEntry(int index);
}
