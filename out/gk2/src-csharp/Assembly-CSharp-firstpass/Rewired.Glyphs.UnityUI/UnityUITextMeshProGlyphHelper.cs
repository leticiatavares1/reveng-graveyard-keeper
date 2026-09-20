using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

namespace Rewired.Glyphs.UnityUI;

[AddComponentMenu("Rewired/Glyphs/Unity UI/Unity UI Text Mesh Pro Glyph Helper")]
[RequireComponent(typeof(TextMeshProUGUI))]
public class UnityUITextMeshProGlyphHelper : MonoBehaviour
{
	private delegate bool ParseTagAttributesHandler(string text, int startIndex, int count, out string replacement);

	private abstract class Tag
	{
		public enum TagType
		{
			ControllerElement,
			Action,
			Player
		}

		public abstract class Pool
		{
			public abstract bool Return(Tag obj);
		}

		public sealed class Pool<T> : Pool where T : Tag, new()
		{
			private readonly List<T> _list;

			public Pool()
			{
				_list = new List<T>();
			}

			public T Get()
			{
				T val;
				if (_list.Count == 0)
				{
					val = new T();
					if (val != null)
					{
						val.pool = this;
					}
					return val;
				}
				int index = _list.Count - 1;
				val = _list[index];
				_list.RemoveAt(index);
				return val;
			}

			public override bool Return(Tag obj)
			{
				if (!(obj is T val) || val.pool != this)
				{
					return false;
				}
				val.Clear();
				if (_list.Contains(val))
				{
					return false;
				}
				_list.Add(val);
				return true;
			}
		}

		public readonly TagType tagType;

		private Pool _pool;

		protected Pool pool
		{
			get
			{
				return _pool;
			}
			set
			{
				_pool = value;
			}
		}

		protected Tag(TagType tagType)
		{
			this.tagType = tagType;
		}

		public void ReturnToPool()
		{
			if (_pool != null)
			{
				_pool.Return(this);
			}
		}

		protected abstract void Clear();

		public static void Clear(List<Tag> list)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				if (list[i] != null)
				{
					list[i].ReturnToPool();
				}
			}
			list.Clear();
		}
	}

	private sealed class ControllerElementTag : Tag
	{
		public DisplayType type;

		public int playerId;

		public int actionId;

		public AxisRange actionRange;

		private readonly List<GlyphOrText> _glyphsOrText;

		public List<GlyphOrText> glyphsOrText => _glyphsOrText;

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(typeof(ControllerElementTag).Name);
			stringBuilder.Append(": ");
			stringBuilder.Append("type = ");
			stringBuilder.Append(type);
			stringBuilder.Append(", playerId = ");
			stringBuilder.Append(playerId);
			stringBuilder.Append(", actionId = ");
			stringBuilder.Append(actionId);
			stringBuilder.Append(", actionRange = ");
			stringBuilder.Append(actionRange);
			return stringBuilder.ToString();
		}

		public ControllerElementTag()
			: base(TagType.ControllerElement)
		{
			_glyphsOrText = new List<GlyphOrText>();
			Clear();
		}

		protected override void Clear()
		{
			type = DisplayType.GlyphOrText;
			playerId = -1;
			actionId = -1;
			actionRange = AxisRange.Full;
			_glyphsOrText.Clear();
		}

		public static bool TryParseString(string text, int startIndex, int count, StringBuilder sb1, StringBuilder sb2, Dictionary<string, string> workDictionary, Pool<ControllerElementTag> pool, out ControllerElementTag result)
		{
			result = null;
			if (string.IsNullOrEmpty(text) || startIndex < 0 || startIndex + count >= text.Length)
			{
				return false;
			}
			ParseAttributes(text, startIndex, count, sb1, sb2, workDictionary);
			if (workDictionary.Count == 0)
			{
				return false;
			}
			result = pool.Get();
			try
			{
				if (workDictionary.TryGetValue("type", out var value))
				{
					bool flag = false;
					for (int i = 0; i < s_displayTypeNames.Length; i++)
					{
						if (string.Equals(value, s_displayTypeNames[i], StringComparison.OrdinalIgnoreCase))
						{
							result.type = s_displayTypeValues[i];
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						throw new Exception("Invalid type: " + value);
					}
				}
				else
				{
					result.type = DisplayType.GlyphOrText;
				}
				if (workDictionary.TryGetValue("playerid", out value))
				{
					result.playerId = int.Parse(value);
					if (ReInput.players.GetPlayer(result.playerId) == null)
					{
						throw new Exception("Invalid Player Id: " + result.playerId);
					}
				}
				else
				{
					if (!workDictionary.TryGetValue("playername", out value))
					{
						throw new Exception("Player name/id missing.");
					}
					Player player = ReInput.players.GetPlayer(value);
					if (player == null)
					{
						throw new Exception("Invalid Player name: " + value);
					}
					result.playerId = player.id;
				}
				if (workDictionary.TryGetValue("actionid", out value))
				{
					result.actionId = int.Parse(value);
					if (ReInput.mapping.GetAction(result.actionId) == null)
					{
						throw new Exception("Invalid Action Id: " + result.actionId);
					}
				}
				else
				{
					if (!workDictionary.TryGetValue("actionname", out value))
					{
						throw new Exception("Action name/id missing.");
					}
					InputAction action = ReInput.mapping.GetAction(value);
					if (action == null)
					{
						throw new Exception("Invalid Action name: " + value);
					}
					result.actionId = action.id;
				}
				if (workDictionary.TryGetValue("actionrange", out value))
				{
					bool flag2 = false;
					for (int j = 0; j < s_axisRangeNames.Length; j++)
					{
						if (string.Equals(value, s_axisRangeNames[j], StringComparison.OrdinalIgnoreCase))
						{
							result.actionRange = s_axisRangeValues[j];
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						throw new Exception("Invalid Action Range: " + value);
					}
				}
				else
				{
					result.actionRange = AxisRange.Full;
				}
				return true;
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				result.ReturnToPool();
				return false;
			}
		}
	}

	private sealed class ActionTag : Tag
	{
		public int actionId;

		public AxisRange actionRange;

		private string _displayName;

		public string displayName
		{
			get
			{
				return _displayName;
			}
			set
			{
				_displayName = value;
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(typeof(ControllerElementTag).Name);
			stringBuilder.Append(": ");
			stringBuilder.Append("actionId = ");
			stringBuilder.Append(actionId);
			stringBuilder.Append(", actionRange = ");
			stringBuilder.Append(actionRange);
			return stringBuilder.ToString();
		}

		public ActionTag()
			: base(TagType.Action)
		{
			Clear();
		}

		protected override void Clear()
		{
			actionId = -1;
			actionRange = AxisRange.Full;
			_displayName = null;
		}

		public static bool TryParseString(string text, int startIndex, int count, StringBuilder sb1, StringBuilder sb2, Dictionary<string, string> workDictionary, Pool<ActionTag> pool, out ActionTag result)
		{
			result = null;
			if (string.IsNullOrEmpty(text) || startIndex < 0 || startIndex + count >= text.Length)
			{
				return false;
			}
			ParseAttributes(text, startIndex, count, sb1, sb2, workDictionary);
			if (workDictionary.Count == 0)
			{
				return false;
			}
			result = pool.Get();
			try
			{
				if (workDictionary.TryGetValue("id", out var value) || workDictionary.TryGetValue("actionid", out value))
				{
					result.actionId = int.Parse(value);
					if (ReInput.mapping.GetAction(result.actionId) == null)
					{
						throw new Exception("Invalid Action Id: " + result.actionId);
					}
				}
				else
				{
					if (!workDictionary.TryGetValue("name", out value) && !workDictionary.TryGetValue("actionname", out value))
					{
						throw new Exception("Action name/id missing.");
					}
					InputAction action = ReInput.mapping.GetAction(value);
					if (action == null)
					{
						throw new Exception("Invalid Action name: " + value);
					}
					result.actionId = action.id;
				}
				if (workDictionary.TryGetValue("range", out value) || workDictionary.TryGetValue("actionrange", out value))
				{
					bool flag = false;
					for (int i = 0; i < s_axisRangeNames.Length; i++)
					{
						if (string.Equals(value, s_axisRangeNames[i], StringComparison.OrdinalIgnoreCase))
						{
							result.actionRange = s_axisRangeValues[i];
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						throw new Exception("Invalid Action Range: " + value);
					}
				}
				else
				{
					result.actionRange = AxisRange.Full;
				}
				return true;
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				result.ReturnToPool();
				return false;
			}
		}
	}

	private sealed class PlayerTag : Tag
	{
		public int playerId;

		private string _displayName;

		public string displayName
		{
			get
			{
				return _displayName;
			}
			set
			{
				_displayName = value;
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(typeof(ControllerElementTag).Name);
			stringBuilder.Append(": ");
			stringBuilder.Append("playerId = ");
			stringBuilder.Append(playerId);
			return stringBuilder.ToString();
		}

		public PlayerTag()
			: base(TagType.Player)
		{
			Clear();
		}

		protected override void Clear()
		{
			playerId = -1;
			_displayName = null;
		}

		public static bool TryParseString(string text, int startIndex, int count, StringBuilder sb1, StringBuilder sb2, Dictionary<string, string> workDictionary, Pool<PlayerTag> pool, out PlayerTag result)
		{
			result = null;
			if (string.IsNullOrEmpty(text) || startIndex < 0 || startIndex + count >= text.Length)
			{
				return false;
			}
			ParseAttributes(text, startIndex, count, sb1, sb2, workDictionary);
			if (workDictionary.Count == 0)
			{
				return false;
			}
			result = pool.Get();
			try
			{
				if (workDictionary.TryGetValue("id", out var value) || workDictionary.TryGetValue("playerid", out value))
				{
					result.playerId = int.Parse(value);
					if (ReInput.players.GetPlayer(result.playerId) == null)
					{
						throw new Exception("Invalid Player Id: " + result.playerId);
					}
				}
				else
				{
					if (!workDictionary.TryGetValue("name", out value) && !workDictionary.TryGetValue("playername", out value))
					{
						throw new Exception("Player name/id missing.");
					}
					Player player = ReInput.players.GetPlayer(value);
					if (player == null)
					{
						throw new Exception("Invalid Player name: " + value);
					}
					result.playerId = player.id;
				}
				return true;
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				result.ReturnToPool();
				return false;
			}
		}
	}

	private struct GlyphOrText : IEquatable<GlyphOrText>
	{
		public string glyphKey;

		public Sprite sprite;

		public string name;

		public override bool Equals(object obj)
		{
			if (!(obj is GlyphOrText glyphOrText))
			{
				return false;
			}
			if (string.Equals(glyphOrText.glyphKey, glyphKey, StringComparison.Ordinal) && glyphOrText.sprite == sprite)
			{
				return string.Equals(glyphOrText.name, name, StringComparison.Ordinal);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((17 * 29 + glyphKey.GetHashCode()) * 29 + sprite.GetHashCode()) * 29 + name.GetHashCode();
		}

		public bool Equals(GlyphOrText other)
		{
			if (string.Equals(other.glyphKey, glyphKey, StringComparison.Ordinal) && other.sprite == sprite)
			{
				return string.Equals(other.name, name, StringComparison.Ordinal);
			}
			return false;
		}

		public static bool operator ==(GlyphOrText a, GlyphOrText b)
		{
			if (string.Equals(a.glyphKey, b.glyphKey, StringComparison.Ordinal) && a.sprite == b.sprite)
			{
				return string.Equals(a.name, b.name, StringComparison.Ordinal);
			}
			return false;
		}

		public static bool operator !=(GlyphOrText a, GlyphOrText b)
		{
			return !(a == b);
		}
	}

	private class Asset
	{
		public readonly uint id;

		private ITMProSpriteAsset _spriteAsset;

		private Material _material;

		private static uint s_idCounter;

		private static Shader __tmProShader;

		public ITMProSpriteAsset spriteAsset => _spriteAsset;

		public Material material => _material;

		private static Shader tmProShader
		{
			get
			{
				if (__tmProShader == null)
				{
					ShaderUtilities.GetShaderPropertyIDs();
					__tmProShader = Shader.Find("TextMeshPro/Sprite");
				}
				return __tmProShader;
			}
		}

		public Asset(Material baseMaterial)
		{
			id = s_idCounter++;
			_spriteAsset = TMProAssetVersionHelper.CreateSpriteAsset();
			TMP_SpriteAsset tMP_SpriteAsset = _spriteAsset.GetSpriteAsset();
			tMP_SpriteAsset.name = typeof(UnityUITextMeshProGlyphHelper).Name + " SpriteAsset " + id;
			tMP_SpriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(tMP_SpriteAsset.name);
			_material = CreateMaterial(baseMaterial, id);
			if (_spriteAsset != null)
			{
				tMP_SpriteAsset.material = material;
				tMP_SpriteAsset.materialHashCode = TMP_TextUtilities.GetSimpleHashCode(material.name);
			}
		}

		public static Material CreateMaterial(Material baseMaterial, uint id)
		{
			Material obj = ((baseMaterial != null) ? new Material(baseMaterial) : new Material(tmProShader));
			obj.name = typeof(UnityUITextMeshProGlyphHelper).Name + " Material " + id;
			obj.hideFlags = HideFlags.HideInHierarchy;
			return obj;
		}

		public void Destroy()
		{
			if (_spriteAsset != null)
			{
				_spriteAsset.Destroy();
				_spriteAsset = null;
			}
			if (_material != null)
			{
				UnityEngine.Object.Destroy(_material);
				_material = null;
			}
		}
	}

	[Serializable]
	public struct TMProSpriteOptions : IEquatable<TMProSpriteOptions>
	{
		[Tooltip("Scale.")]
		[SerializeField]
		private float _scale;

		[Tooltip("This value will be multiplied by the Sprite width and height and applied to offset.")]
		[SerializeField]
		private Vector2 _offsetSizeMultiplier;

		[Tooltip("An extra offset that is cumulative with Offset Size Multiplier.")]
		[SerializeField]
		private Vector2 _extraOffset;

		[Tooltip("This value will be multiplied by the Sprite width applied to X Advance.")]
		[SerializeField]
		private float _xAdvanceWidthMultiplier;

		[Tooltip("An extra offset that is cumulative with X Advance Width Multiplier.")]
		[SerializeField]
		private float _extraXAdvance;

		public float scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
			}
		}

		public Vector2 offsetSizeMultiplier
		{
			get
			{
				return _offsetSizeMultiplier;
			}
			set
			{
				_offsetSizeMultiplier = value;
			}
		}

		public Vector2 extraOffset
		{
			get
			{
				return _extraOffset;
			}
			set
			{
				_extraOffset = value;
			}
		}

		public float xAdvanceWidthMultiplier
		{
			get
			{
				return _xAdvanceWidthMultiplier;
			}
			set
			{
				_xAdvanceWidthMultiplier = value;
			}
		}

		public float extraXAdvance
		{
			get
			{
				return _extraXAdvance;
			}
			set
			{
				_extraXAdvance = value;
			}
		}

		public static TMProSpriteOptions Default
		{
			get
			{
				TMProSpriteOptions result = default(TMProSpriteOptions);
				result.scale = 1.5f;
				result.extraOffset = default(Vector2);
				result.offsetSizeMultiplier = new Vector2(0f, 0.75f);
				result.xAdvanceWidthMultiplier = 1f;
				return result;
			}
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TMProSpriteOptions tMProSpriteOptions))
			{
				return false;
			}
			if (tMProSpriteOptions._scale == _scale && tMProSpriteOptions._offsetSizeMultiplier == _offsetSizeMultiplier && tMProSpriteOptions._extraOffset == _extraOffset && tMProSpriteOptions._xAdvanceWidthMultiplier == _xAdvanceWidthMultiplier)
			{
				return tMProSpriteOptions._extraXAdvance == _extraXAdvance;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((((17 * 29 + _scale.GetHashCode()) * 29 + _offsetSizeMultiplier.GetHashCode()) * 29 + _extraOffset.GetHashCode()) * 29 + _xAdvanceWidthMultiplier.GetHashCode()) * 29 + _extraXAdvance.GetHashCode();
		}

		public bool Equals(TMProSpriteOptions other)
		{
			if (other._scale == _scale && other._offsetSizeMultiplier == _offsetSizeMultiplier && other._extraOffset == _extraOffset && other._xAdvanceWidthMultiplier == _xAdvanceWidthMultiplier)
			{
				return other._extraXAdvance == _extraXAdvance;
			}
			return false;
		}

		public static bool operator ==(TMProSpriteOptions a, TMProSpriteOptions b)
		{
			if (a._scale == b._scale && a._offsetSizeMultiplier == b._offsetSizeMultiplier && a._extraOffset == b._extraOffset && a._xAdvanceWidthMultiplier == b._xAdvanceWidthMultiplier)
			{
				return a._extraXAdvance == b._extraXAdvance;
			}
			return false;
		}

		public static bool operator !=(TMProSpriteOptions a, TMProSpriteOptions b)
		{
			return !(a == b);
		}
	}

	[Serializable]
	public struct SpriteMaterialProperties
	{
		[Tooltip("Sprite material color.")]
		[SerializeField]
		private Color _color;

		public Color color
		{
			get
			{
				return _color;
			}
			set
			{
				_color = value;
			}
		}

		public static SpriteMaterialProperties Default
		{
			get
			{
				SpriteMaterialProperties result = default(SpriteMaterialProperties);
				result._color = Color.white;
				return result;
			}
		}
	}

	private interface ITMProSprite
	{
		uint id { get; set; }

		float width { get; set; }

		float height { get; set; }

		float xOffset { get; set; }

		float yOffset { get; set; }

		float xAdvance { get; set; }

		Vector2 position { get; set; }

		Vector2 pivot { get; set; }

		float scale { get; set; }

		string name { get; set; }

		uint unicode { get; set; }

		int hashCode { get; set; }

		Sprite sprite { get; set; }
	}

	private interface ITMProSpriteAsset
	{
		int spriteCount { get; }

		Texture spriteSheet { get; set; }

		TMP_SpriteAsset GetSpriteAsset();

		ITMProSprite GetSprite(int index);

		void AddSprite(ITMProSprite sprite);

		bool Contains(string spriteName);

		void Clear();

		void UpdateLookupTables();

		void Destroy();
	}

	private static class TMProAssetVersionHelper
	{
		private static bool _isVersionSupportedChecked;

		private static bool CheckVersionSupported()
		{
			bool result = TMProSprite_AssetV1_1_0.CheckVersionSupported();
			if (_isVersionSupportedChecked)
			{
				return result;
			}
			_isVersionSupportedChecked = true;
			return result;
		}

		public static ITMProSprite CreateSprite()
		{
			if (!CheckVersionSupported())
			{
				return new TMProSprite_AssetV1_0_0();
			}
			return new TMProSprite_AssetV1_1_0();
		}

		public static ITMProSpriteAsset CreateSpriteAsset()
		{
			if (!CheckVersionSupported())
			{
				return new TMProSprite_AssetV1_0_0.TMPro_SpriteAsset();
			}
			return new TMProSprite_AssetV1_1_0.TMPro_SpriteAsset();
		}
	}

	private class TMProSprite_AssetV1_0_0 : ITMProSprite
	{
		public class TMPro_SpriteAsset : ITMProSpriteAsset
		{
			private TMP_SpriteAsset _spriteAsset;

			private readonly List<TMProSprite_AssetV1_0_0> _sprites;

			public int spriteCount => _sprites.Count;

			public Texture spriteSheet
			{
				get
				{
					return _spriteAsset.spriteSheet;
				}
				set
				{
					_spriteAsset.spriteSheet = value;
				}
			}

			public TMPro_SpriteAsset()
			{
				_spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
				_spriteAsset.hideFlags = HideFlags.DontSave;
				if (_spriteAsset.spriteInfoList == null)
				{
					_spriteAsset.spriteInfoList = new List<TMP_Sprite>();
				}
				_sprites = new List<TMProSprite_AssetV1_0_0>();
			}

			public TMP_SpriteAsset GetSpriteAsset()
			{
				return _spriteAsset;
			}

			public ITMProSprite GetSprite(int index)
			{
				if ((uint)index >= (uint)_sprites.Count)
				{
					return null;
				}
				return _sprites[index];
			}

			public void AddSprite(ITMProSprite sprite)
			{
				TMProSprite_AssetV1_0_0 tMProSprite_AssetV1_0_ = sprite as TMProSprite_AssetV1_0_0;
				if (sprite == null)
				{
					throw new ArgumentException();
				}
				tMProSprite_AssetV1_0_.spriteInfo.id = _spriteAsset.spriteInfoList.Count;
				_spriteAsset.spriteInfoList.Add(tMProSprite_AssetV1_0_.spriteInfo);
				_sprites.Add(tMProSprite_AssetV1_0_);
			}

			public void Clear()
			{
				_spriteAsset.spriteInfoList.Clear();
				_sprites.Clear();
			}

			public bool Contains(string spriteName)
			{
				int count = _sprites.Count;
				for (int i = 0; i < count; i++)
				{
					if (string.Equals(_sprites[i].name, spriteName, StringComparison.Ordinal))
					{
						return true;
					}
				}
				return false;
			}

			public void UpdateLookupTables()
			{
				_spriteAsset.UpdateLookupTables();
			}

			public void Destroy()
			{
				if (!(_spriteAsset == null))
				{
					UnityEngine.Object.Destroy(_spriteAsset);
					_spriteAsset = null;
				}
			}
		}

		public TMP_Sprite spriteInfo;

		public uint id
		{
			get
			{
				return (uint)spriteInfo.id;
			}
			set
			{
				spriteInfo.id = (int)value;
			}
		}

		public float width
		{
			get
			{
				return spriteInfo.width;
			}
			set
			{
				spriteInfo.width = value;
			}
		}

		public float height
		{
			get
			{
				return spriteInfo.height;
			}
			set
			{
				spriteInfo.height = value;
			}
		}

		public float xOffset
		{
			get
			{
				return spriteInfo.xOffset;
			}
			set
			{
				spriteInfo.xOffset = value;
			}
		}

		public float yOffset
		{
			get
			{
				return spriteInfo.yOffset;
			}
			set
			{
				spriteInfo.yOffset = value;
			}
		}

		public float xAdvance
		{
			get
			{
				return spriteInfo.xAdvance;
			}
			set
			{
				spriteInfo.xAdvance = value;
			}
		}

		public Vector2 position
		{
			get
			{
				return new Vector2(spriteInfo.x, spriteInfo.y);
			}
			set
			{
				spriteInfo.x = value.x;
				spriteInfo.y = value.y;
			}
		}

		public Vector2 pivot
		{
			get
			{
				return spriteInfo.pivot;
			}
			set
			{
				spriteInfo.pivot = value;
			}
		}

		public float scale
		{
			get
			{
				return spriteInfo.scale;
			}
			set
			{
				spriteInfo.scale = value;
			}
		}

		public string name
		{
			get
			{
				return spriteInfo.name;
			}
			set
			{
				spriteInfo.name = value;
			}
		}

		public uint unicode
		{
			get
			{
				return (uint)spriteInfo.unicode;
			}
			set
			{
				spriteInfo.unicode = (int)value;
			}
		}

		public int hashCode
		{
			get
			{
				return spriteInfo.hashCode;
			}
			set
			{
				spriteInfo.hashCode = value;
			}
		}

		public Sprite sprite
		{
			get
			{
				return spriteInfo.sprite;
			}
			set
			{
				spriteInfo.sprite = value;
			}
		}

		public TMProSprite_AssetV1_0_0()
		{
			spriteInfo = new TMP_Sprite();
		}
	}

	private class TMProSprite_AssetV1_1_0 : ITMProSprite
	{
		public class TMPro_SpriteCharacter
		{
			private readonly TMP_SpriteCharacter _source;

			public TMP_SpriteCharacter source => _source;

			public Glyph glyph
			{
				get
				{
					return _source.glyph;
				}
				set
				{
					_source.glyph = value;
				}
			}

			public uint unicode
			{
				get
				{
					return _source.unicode;
				}
				set
				{
					if (value == 0)
					{
						value = 65534u;
					}
					_source.unicode = value;
				}
			}

			public string name
			{
				get
				{
					return _source.name;
				}
				set
				{
					_source.name = value;
				}
			}

			public float scale
			{
				get
				{
					return _source.scale;
				}
				set
				{
					_source.scale = value;
				}
			}

			public uint glyphIndex
			{
				get
				{
					return _source.glyphIndex;
				}
				set
				{
					_source.glyphIndex = value;
				}
			}

			public TMPro_SpriteCharacter()
			{
				_source = new TMP_SpriteCharacter();
			}
		}

		public class TMPro_SpriteGlyph
		{
			private readonly TMP_SpriteGlyph _source;

			public TMP_SpriteGlyph source => _source;

			public Sprite sprite
			{
				get
				{
					return _source.sprite;
				}
				set
				{
					_source.sprite = value;
				}
			}

			public TMPro_SpriteGlyph()
			{
				_source = new TMP_SpriteGlyph();
				Initialize(_source);
			}

			private static void Initialize(Glyph glyph)
			{
				glyph.scale = 1f;
				glyph.atlasIndex = 0;
			}
		}

		public class TMPro_SpriteAsset : ITMProSpriteAsset
		{
			private readonly PropertyInfo _spriteCharacterTable;

			private readonly PropertyInfo _spriteGlyphTable;

			private readonly IList _spriteCharacterTableList;

			private readonly IList _spriteGlyphTableList;

			private readonly List<TMProSprite_AssetV1_1_0> _sprites;

			private TMP_SpriteAsset _spriteAsset;

			public int spriteCount => _sprites.Count;

			public Texture spriteSheet
			{
				get
				{
					return _spriteAsset.spriteSheet;
				}
				set
				{
					_spriteAsset.spriteSheet = value;
				}
			}

			public TMPro_SpriteAsset()
			{
				_spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
				_spriteAsset.hideFlags = HideFlags.DontSave;
				Type typeFromHandle = typeof(TMP_SpriteAsset);
				if (typeFromHandle == null)
				{
					throw new ArgumentNullException("type");
				}
				PropertyInfo property = typeFromHandle.GetProperty("version", BindingFlags.Instance | BindingFlags.Public);
				if (property == null)
				{
					throw new ArgumentNullException("version");
				}
				property.SetValue(_spriteAsset, "1.1.0");
				_spriteCharacterTable = typeFromHandle.GetProperty("spriteCharacterTable", BindingFlags.Instance | BindingFlags.Public);
				if (_spriteCharacterTable == null)
				{
					throw new ArgumentNullException("spriteCharacterTable");
				}
				_spriteCharacterTableList = (IList)_spriteCharacterTable.GetValue(_spriteAsset);
				if (_spriteCharacterTableList == null)
				{
					throw new ArgumentNullException("spriteCharacterTableList");
				}
				_spriteGlyphTable = typeFromHandle.GetProperty("spriteGlyphTable", BindingFlags.Instance | BindingFlags.Public);
				if (_spriteGlyphTable == null)
				{
					throw new ArgumentNullException("spriteGlyphTable");
				}
				_spriteGlyphTableList = (IList)_spriteGlyphTable.GetValue(_spriteAsset);
				if (_spriteGlyphTableList == null)
				{
					throw new ArgumentNullException("spriteGlyphTableList");
				}
				_sprites = new List<TMProSprite_AssetV1_1_0>();
			}

			public TMP_SpriteAsset GetSpriteAsset()
			{
				return _spriteAsset;
			}

			public ITMProSprite GetSprite(int index)
			{
				if ((uint)index >= (uint)_sprites.Count)
				{
					return null;
				}
				return _sprites[index];
			}

			public void AddSprite(ITMProSprite sprite)
			{
				if (!(sprite is TMProSprite_AssetV1_1_0 tMProSprite_AssetV1_1_))
				{
					throw new ArgumentException();
				}
				tMProSprite_AssetV1_1_.id = (uint)_spriteCharacterTableList.Count;
				_spriteCharacterTableList.Add(tMProSprite_AssetV1_1_.spriteCharacter.source);
				_spriteGlyphTableList.Add(tMProSprite_AssetV1_1_.spriteGlyph.source);
				_sprites.Add(tMProSprite_AssetV1_1_);
			}

			public void Clear()
			{
				_spriteCharacterTableList.Clear();
				_spriteGlyphTableList.Clear();
				_sprites.Clear();
			}

			public bool Contains(string spriteName)
			{
				int count = _sprites.Count;
				for (int i = 0; i < count; i++)
				{
					if (string.Equals(_sprites[i].name, spriteName, StringComparison.Ordinal))
					{
						return true;
					}
				}
				return false;
			}

			public void UpdateLookupTables()
			{
				_spriteAsset.UpdateLookupTables();
			}

			public void Destroy()
			{
				if (!(_spriteAsset == null))
				{
					UnityEngine.Object.Destroy(_spriteAsset);
					_spriteAsset = null;
				}
			}
		}

		private readonly TMPro_SpriteGlyph _spriteGlyph;

		private readonly TMPro_SpriteCharacter _spriteCharacter;

		private static bool? s_isVersionSupported;

		public TMPro_SpriteGlyph spriteGlyph => _spriteGlyph;

		public TMPro_SpriteCharacter spriteCharacter => _spriteCharacter;

		public uint id
		{
			get
			{
				return _spriteGlyph.source.index;
			}
			set
			{
				_spriteGlyph.source.index = value;
				_spriteCharacter.glyphIndex = value;
			}
		}

		public float width
		{
			get
			{
				return _spriteGlyph.source.metrics.width;
			}
			set
			{
				GlyphMetrics metrics = _spriteGlyph.source.metrics;
				metrics.width = value;
				_spriteGlyph.source.metrics = metrics;
				GlyphRect glyphRect = _spriteGlyph.source.glyphRect;
				glyphRect.width = (int)value;
				_spriteGlyph.source.glyphRect = glyphRect;
			}
		}

		public float height
		{
			get
			{
				return _spriteGlyph.source.metrics.height;
			}
			set
			{
				GlyphMetrics metrics = _spriteGlyph.source.metrics;
				metrics.height = value;
				_spriteGlyph.source.metrics = metrics;
				GlyphRect glyphRect = _spriteGlyph.source.glyphRect;
				glyphRect.height = (int)value;
				_spriteGlyph.source.glyphRect = glyphRect;
			}
		}

		public float xOffset
		{
			get
			{
				return _spriteGlyph.source.metrics.horizontalBearingX;
			}
			set
			{
				GlyphMetrics metrics = _spriteGlyph.source.metrics;
				metrics.horizontalBearingX = value;
				_spriteGlyph.source.metrics = metrics;
			}
		}

		public float yOffset
		{
			get
			{
				return _spriteGlyph.source.metrics.horizontalBearingY;
			}
			set
			{
				GlyphMetrics metrics = _spriteGlyph.source.metrics;
				metrics.horizontalBearingY = value;
				_spriteGlyph.source.metrics = metrics;
			}
		}

		public float xAdvance
		{
			get
			{
				return _spriteGlyph.source.metrics.horizontalAdvance;
			}
			set
			{
				GlyphMetrics metrics = _spriteGlyph.source.metrics;
				metrics.horizontalAdvance = value;
				_spriteGlyph.source.metrics = metrics;
			}
		}

		public Vector2 position
		{
			get
			{
				GlyphRect glyphRect = _spriteGlyph.source.glyphRect;
				return new Vector2(glyphRect.x, glyphRect.y);
			}
			set
			{
				GlyphRect glyphRect = _spriteGlyph.source.glyphRect;
				glyphRect.x = (int)value.x;
				glyphRect.y = (int)value.y;
				_spriteGlyph.source.glyphRect = glyphRect;
			}
		}

		public Vector2 pivot
		{
			get
			{
				return default(Vector2);
			}
			set
			{
			}
		}

		public float scale
		{
			get
			{
				return _spriteCharacter.scale;
			}
			set
			{
				_spriteCharacter.scale = value;
			}
		}

		public string name
		{
			get
			{
				return _spriteCharacter.name;
			}
			set
			{
				_spriteCharacter.name = value;
			}
		}

		public uint unicode
		{
			get
			{
				return _spriteCharacter.unicode;
			}
			set
			{
				_spriteCharacter.unicode = value;
			}
		}

		public int hashCode
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public Sprite sprite
		{
			get
			{
				return _spriteGlyph.sprite;
			}
			set
			{
				_spriteGlyph.sprite = value;
			}
		}

		public TMProSprite_AssetV1_1_0()
		{
			_spriteGlyph = new TMPro_SpriteGlyph();
			_spriteCharacter = new TMPro_SpriteCharacter();
			_spriteCharacter.glyph = _spriteGlyph.source;
		}

		public static bool CheckVersionSupported()
		{
			if (s_isVersionSupported.HasValue)
			{
				return s_isVersionSupported.Value;
			}
			try
			{
				new TMPro_SpriteAsset();
				s_isVersionSupported = true;
			}
			catch
			{
				s_isVersionSupported = false;
			}
			return s_isVersionSupported.Value;
		}
	}

	private enum DisplayType
	{
		Glyph,
		Text,
		GlyphOrText
	}

	[Tooltip("Enter text into this field and not in the TMPro Text field directly. Text will be parsed for special tags, and the final result will be passed on to the Text Mesh Pro Text component. See the documentation for special tag format.")]
	[SerializeField]
	[TextArea(3, 10)]
	private string _text;

	[Tooltip("Optional reference to an object that defines options. If blank, the global default options will be used.")]
	[SerializeField]
	private ControllerElementGlyphSelectorOptionsSOBase _options;

	[Tooltip("Options that control how Text Mesh Pro displays Sprites.")]
	[SerializeField]
	private TMProSpriteOptions _spriteOptions = TMProSpriteOptions.Default;

	[Tooltip("Optional material for Sprites. If blank, the default material will be used.\nMaterial is instantiated for each Sprite Asset, so making changes to values in the base material later will not affect Sprites. Changing the base material at runtime will copy only certain properties from the new material to Sprite materials.")]
	[SerializeField]
	private Material _baseSpriteMaterial;

	[Tooltip("If enabled, local values such as Sprite color will be used instead of the value on the base material.")]
	[SerializeField]
	private bool _overrideSpriteMaterialProperties = true;

	[Tooltip("These properties will override the properties on the Sprite material if Override Sprite Material Properties is enabled.")]
	[SerializeField]
	private SpriteMaterialProperties _spriteMaterialProperties = SpriteMaterialProperties.Default;

	[NonSerialized]
	private TextMeshProUGUI _tmProText;

	[NonSerialized]
	private string _textPrev;

	[NonSerialized]
	private readonly StringBuilder _processTagSb = new StringBuilder();

	[NonSerialized]
	private readonly StringBuilder _tempSb = new StringBuilder();

	[NonSerialized]
	private readonly StringBuilder _tempSb2 = new StringBuilder();

	[NonSerialized]
	private Asset _primaryAsset;

	[NonSerialized]
	private readonly List<Asset> _assignedAssets = new List<Asset>();

	[NonSerialized]
	private readonly List<Asset> _assetsPool = new List<Asset>();

	[NonSerialized]
	private readonly List<ActionElementMap> _tempAems = new List<ActionElementMap>();

	[NonSerialized]
	private readonly List<Sprite> _tempGlyphs = new List<Sprite>();

	[NonSerialized]
	private readonly List<Asset> _dirtyAssets = new List<Asset>();

	[NonSerialized]
	private readonly List<string> _tempKeys = new List<string>();

	[NonSerialized]
	private readonly List<GlyphOrText> _glyphsOrTextTemp = new List<GlyphOrText>();

	[NonSerialized]
	private readonly List<Asset> _currentlyUsedAssets = new List<Asset>();

	[NonSerialized]
	private readonly List<Tag> _currentTags = new List<Tag>();

	[NonSerialized]
	private Dictionary<string, string> _tempStringDictionary = new Dictionary<string, string>();

	[NonSerialized]
	private bool _initialized;

	[NonSerialized]
	private bool _rebuildRequired;

	[NonSerialized]
	private Texture2D _stubTexture;

	private Tag.Pool<ControllerElementTag> __controllerElementTagPool;

	private Tag.Pool<ActionTag> __actionTagPool;

	private Tag.Pool<PlayerTag> __playerTagPool;

	[NonSerialized]
	private Dictionary<string, ParseTagAttributesHandler> __tagHandlers;

	private static string[] __s_displayTypeNames;

	private static DisplayType[] __s_displayTypeValues;

	private static string[] __s_axisRangeNames;

	private static AxisRange[] __s_axisRangeValues;

	private Tag.Pool<ControllerElementTag> controllerElementTagPool
	{
		get
		{
			if (__controllerElementTagPool == null)
			{
				return __controllerElementTagPool = new Tag.Pool<ControllerElementTag>();
			}
			return __controllerElementTagPool;
		}
	}

	private Tag.Pool<ActionTag> actionTagPool
	{
		get
		{
			if (__actionTagPool == null)
			{
				return __actionTagPool = new Tag.Pool<ActionTag>();
			}
			return __actionTagPool;
		}
	}

	private Tag.Pool<PlayerTag> playerTagPool
	{
		get
		{
			if (__playerTagPool == null)
			{
				return __playerTagPool = new Tag.Pool<PlayerTag>();
			}
			return __playerTagPool;
		}
	}

	private Dictionary<string, ParseTagAttributesHandler> tagHandlers
	{
		get
		{
			if (__tagHandlers == null)
			{
				Dictionary<string, ParseTagAttributesHandler> obj = new Dictionary<string, ParseTagAttributesHandler>
				{
					{ "rewiredelement", ProcessTag_ControllerElement },
					{ "rewiredaction", ProcessTag_Action },
					{ "rewiredplayer", ProcessTag_Player }
				};
				Dictionary<string, ParseTagAttributesHandler> result = obj;
				__tagHandlers = obj;
				return result;
			}
			return __tagHandlers;
		}
	}

	public virtual string text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
			RequireRebuild();
		}
	}

	public virtual ControllerElementGlyphSelectorOptionsSOBase options
	{
		get
		{
			return _options;
		}
		set
		{
			_options = value;
			RequireRebuild();
		}
	}

	public virtual TMProSpriteOptions spriteOptions
	{
		get
		{
			return _spriteOptions;
		}
		set
		{
			_spriteOptions = value;
			int count = _assignedAssets.Count;
			for (int i = 0; i < count; i++)
			{
				int spriteCount = _assignedAssets[i].spriteAsset.spriteCount;
				for (int j = 0; j < spriteCount; j++)
				{
					ITMProSprite sprite = _assignedAssets[i].spriteAsset.GetSprite(j);
					if (sprite != null && !(sprite.sprite == null))
					{
						Rect rect = sprite.sprite.rect;
						sprite.xOffset = rect.width * _spriteOptions.offsetSizeMultiplier.x + _spriteOptions.extraOffset.x;
						sprite.yOffset = rect.height * _spriteOptions.offsetSizeMultiplier.y + _spriteOptions.extraOffset.y;
						sprite.xAdvance = rect.width * _spriteOptions.xAdvanceWidthMultiplier + _spriteOptions.extraXAdvance;
						sprite.scale = _spriteOptions.scale;
					}
				}
				TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(isChanged: true, _assignedAssets[i].spriteAsset.GetSpriteAsset());
			}
		}
	}

	public virtual Material baseSpriteMaterial
	{
		get
		{
			return _baseSpriteMaterial;
		}
		set
		{
			_baseSpriteMaterial = value;
			Material sourceMaterial = ((_baseSpriteMaterial != null) ? _baseSpriteMaterial : _primaryAsset.material);
			ForEachAsset(delegate(Asset asset)
			{
				CopyMaterialProperties(sourceMaterial, asset.material);
				if (_overrideSpriteMaterialProperties)
				{
					CopySpriteMaterialPropertiesToMaterial(_spriteMaterialProperties, asset.material);
				}
				TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(isChanged: true, asset.material);
			});
		}
	}

	public virtual bool overrideSpriteMaterialProperties
	{
		get
		{
			return _overrideSpriteMaterialProperties;
		}
		set
		{
			_overrideSpriteMaterialProperties = value;
			if (value)
			{
				ForEachAsset(delegate(Asset asset)
				{
					CopySpriteMaterialPropertiesToMaterial(_spriteMaterialProperties, asset.material);
					TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(isChanged: true, asset.material);
				});
				return;
			}
			Material sourceMaterial = ((_baseSpriteMaterial != null) ? _baseSpriteMaterial : _primaryAsset.material);
			ForEachAsset(delegate(Asset asset)
			{
				CopyMaterialProperties(sourceMaterial, asset.material);
				TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(isChanged: true, asset.material);
			});
		}
	}

	public virtual SpriteMaterialProperties spriteMaterialProperties
	{
		get
		{
			return _spriteMaterialProperties;
		}
		set
		{
			_spriteMaterialProperties = value;
			if (_overrideSpriteMaterialProperties)
			{
				ForEachAsset(delegate(Asset asset)
				{
					CopySpriteMaterialPropertiesToMaterial(_spriteMaterialProperties, asset.material);
					TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(isChanged: true, asset.material);
				});
			}
		}
	}

	private static int shaderPropertyId_color => Shader.PropertyToID("_Color");

	private static string[] s_displayTypeNames
	{
		get
		{
			if (__s_displayTypeNames == null)
			{
				return __s_displayTypeNames = Enum.GetNames(typeof(DisplayType));
			}
			return __s_displayTypeNames;
		}
	}

	private static DisplayType[] s_displayTypeValues
	{
		get
		{
			if (__s_displayTypeValues == null)
			{
				return __s_displayTypeValues = (DisplayType[])Enum.GetValues(typeof(DisplayType));
			}
			return __s_displayTypeValues;
		}
	}

	private static string[] s_axisRangeNames
	{
		get
		{
			if (__s_axisRangeNames == null)
			{
				return __s_axisRangeNames = Enum.GetNames(typeof(AxisRange));
			}
			return __s_axisRangeNames;
		}
	}

	private static AxisRange[] s_axisRangeValues
	{
		get
		{
			if (__s_axisRangeValues == null)
			{
				return __s_axisRangeValues = (AxisRange[])Enum.GetValues(typeof(AxisRange));
			}
			return __s_axisRangeValues;
		}
	}

	protected virtual void OnEnable()
	{
		Initialize();
	}

	protected virtual void Start()
	{
		MainUpdate();
	}

	protected virtual void Update()
	{
		if (ReInput.isReady)
		{
			MainUpdate();
		}
	}

	protected virtual void OnDestroy()
	{
		if (_primaryAsset != null)
		{
			if (_tmProText != null && _tmProText.spriteAsset == _primaryAsset.spriteAsset.GetSpriteAsset())
			{
				_tmProText.spriteAsset = null;
			}
			_primaryAsset.Destroy();
			_primaryAsset = null;
		}
		for (int i = 0; i < _assignedAssets.Count; i++)
		{
			if (_assignedAssets[i] != null)
			{
				_assignedAssets[i].Destroy();
			}
		}
		_assignedAssets.Clear();
		for (int j = 0; j < _assetsPool.Count; j++)
		{
			if (_assetsPool[j] != null)
			{
				_assetsPool[j].Destroy();
			}
		}
		_assetsPool.Clear();
		if (_stubTexture != null)
		{
			UnityEngine.Object.Destroy(_stubTexture);
			_stubTexture = null;
		}
		for (int k = 0; k < _currentTags.Count; k++)
		{
			_currentTags[k].ReturnToPool();
		}
	}

	public virtual void ForceUpdate()
	{
		if (ReInput.isReady)
		{
			_rebuildRequired = true;
			Update();
		}
	}

	protected virtual ControllerElementGlyphSelectorOptions GetOptionsOrDefault()
	{
		if (_options != null && _options.options == null)
		{
			Debug.LogError("Rewired: Options missing on " + typeof(ControllerElementGlyphSelectorOptions).Name + ". Global default options will be used instead.");
			return ControllerElementGlyphSelectorOptions.defaultOptions;
		}
		if (!(_options != null))
		{
			return ControllerElementGlyphSelectorOptions.defaultOptions;
		}
		return _options.options;
	}

	private bool Initialize()
	{
		if (_initialized)
		{
			return true;
		}
		_tmProText = GetComponent<TextMeshProUGUI>();
		_stubTexture = new Texture2D(1, 1);
		CreatePrimaryAsset();
		_initialized = true;
		return true;
	}

	private void MainUpdate()
	{
		bool flag = false;
		int count = _currentTags.Count;
		for (int i = 0; i < count; i++)
		{
			Tag tag = _currentTags[i];
			switch (tag.tagType)
			{
			case Tag.TagType.ControllerElement:
			{
				ControllerElementTag controllerElementTag = (ControllerElementTag)tag;
				_glyphsOrTextTemp.Clear();
				TryGetControllerElementGlyphsOrText((ControllerElementTag)tag, _glyphsOrTextTemp);
				if (!IsEqual(_glyphsOrTextTemp, controllerElementTag.glyphsOrText))
				{
					flag = true;
				}
				break;
			}
			case Tag.TagType.Action:
			{
				ActionTag actionTag = (ActionTag)tag;
				TryGetActionDisplayName(actionTag, out var result2);
				if (!string.Equals(actionTag.displayName, result2, StringComparison.Ordinal))
				{
					flag = true;
				}
				break;
			}
			case Tag.TagType.Player:
			{
				PlayerTag playerTag = (PlayerTag)tag;
				TryGetPlayerDisplayName(playerTag, out var result);
				if (!string.Equals(playerTag.displayName, result, StringComparison.Ordinal))
				{
					flag = true;
				}
				break;
			}
			default:
				throw new NotImplementedException();
			}
		}
		if (!string.Equals(_text, _textPrev, StringComparison.Ordinal))
		{
			_textPrev = _text;
			flag = true;
		}
		if (flag || _rebuildRequired)
		{
			if (ParseText(_textPrev, out var newText))
			{
				_tmProText.text = newText;
			}
			else
			{
				_tmProText.text = _text;
			}
		}
		int count2 = _dirtyAssets.Count;
		if (count2 > 0)
		{
			for (int j = 0; j < count2; j++)
			{
				_dirtyAssets[j].spriteAsset.UpdateLookupTables();
			}
			_dirtyAssets.Clear();
		}
	}

	private bool ParseText(string text, out string newText)
	{
		newText = null;
		Tag.Clear(_currentTags);
		_currentlyUsedAssets.Clear();
		bool result = false;
		while (ProcessNextTag(ref text, _processTagSb))
		{
			result = true;
			newText = text;
		}
		RemoveUnusedAssets();
		if (_rebuildRequired)
		{
			_rebuildRequired = false;
		}
		return result;
	}

	private bool ProcessNextTag(ref string text, StringBuilder sb)
	{
		int num = 0;
		ParseTagAttributesHandler value = null;
		int num2 = -1;
		try
		{
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				switch (num)
				{
				case 0:
					if (c == '<')
					{
						num2 = i;
						sb.Length = 0;
						num = 1;
					}
					break;
				case 1:
					if (IsValidTagNameChar(c))
					{
						sb.Append(char.ToLowerInvariant(c));
					}
					else if (char.IsWhiteSpace(c))
					{
						if (sb.Length > 0)
						{
							if (tagHandlers.TryGetValue(sb.ToString(), out value))
							{
								sb.Length = 0;
								num = 2;
							}
							else
							{
								num = 0;
								i--;
							}
						}
					}
					else
					{
						num = 0;
						i--;
					}
					break;
				case 2:
				{
					int num3 = text.IndexOf('>', i);
					if (num3 < 0)
					{
						throw new Exception("Malformed tag.");
					}
					if (value(text, i, num3 - i, out var replacement))
					{
						sb.Length = 0;
						if (num2 > 0)
						{
							sb.Append(text, 0, num2);
						}
						sb.Append(replacement);
						int num4 = num3 + 1;
						if (num4 < text.Length)
						{
							sb.Append(text, num4, text.Length - num4);
						}
						text = sb.ToString();
						return true;
					}
					throw new Exception("Error parsing attributes.");
				}
				}
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		return false;
	}

	private bool ProcessTag_ControllerElement(string text, int startIndex, int count, out string replacement)
	{
		if (!ControllerElementTag.TryParseString(text, startIndex, count, _tempSb, _tempSb2, _tempStringDictionary, controllerElementTagPool, out var result))
		{
			replacement = null;
			return false;
		}
		_currentTags.Add(result);
		result.glyphsOrText.Clear();
		if (!TryGetControllerElementGlyphsOrText(result, result.glyphsOrText))
		{
			replacement = null;
			return true;
		}
		TryCreateTMProString(result.glyphsOrText, out replacement);
		return true;
	}

	private bool ProcessTag_Action(string text, int startIndex, int count, out string replacement)
	{
		if (!ActionTag.TryParseString(text, startIndex, count, _tempSb, _tempSb2, _tempStringDictionary, actionTagPool, out var result))
		{
			replacement = null;
			return false;
		}
		_currentTags.Add(result);
		TryGetActionDisplayName(result, out replacement);
		return true;
	}

	private bool ProcessTag_Player(string text, int startIndex, int count, out string replacement)
	{
		if (!PlayerTag.TryParseString(text, startIndex, count, _tempSb, _tempSb2, _tempStringDictionary, playerTagPool, out var result))
		{
			replacement = null;
			return false;
		}
		_currentTags.Add(result);
		TryGetPlayerDisplayName(result, out replacement);
		return true;
	}

	private bool TryCreateTMProString(List<GlyphOrText> glyphs, out string result)
	{
		StringBuilder tempSb = _tempSb;
		tempSb.Length = 0;
		int count = glyphs.Count;
		for (int i = 0; i < count; i++)
		{
			string glyphKey = glyphs[i].glyphKey;
			if (glyphs[i].sprite != null && !string.IsNullOrEmpty(glyphKey) && TryAssignSprite(glyphs[i].sprite, glyphKey))
			{
				WriteSpriteKey(tempSb, glyphKey);
			}
			else
			{
				tempSb.Append(glyphs[i].name);
			}
			if (i < count - 1)
			{
				tempSb.Append(" ");
			}
		}
		result = tempSb.ToString();
		return !string.IsNullOrEmpty(result);
	}

	private bool TryGetControllerElementGlyphsOrText(ControllerElementTag tag, List<GlyphOrText> results)
	{
		if (tag == null)
		{
			return false;
		}
		_tempAems.Clear();
		if (!GlyphTools.TryGetActionElementMaps(tag.playerId, tag.actionId, tag.actionRange, GetOptionsOrDefault(), _tempAems, out var aemResult, out var aemResult2))
		{
			return false;
		}
		if (aemResult != null && aemResult2 != null)
		{
			GlyphOrText item = default(GlyphOrText);
			_tempAems.Clear();
			_tempAems.Add(aemResult);
			_tempAems.Add(aemResult2);
			if (IsGlyphAllowed(tag.type) && ActionElementMap.TryGetCombinedElementIdentifierGlyph(_tempAems, out var result) && ActionElementMap.TryGetCombinedElementIdentifierFinalGlyphKey(_tempAems, out var result2))
			{
				item.glyphKey = result2;
				item.sprite = result as Sprite;
				results.Add(item);
				return true;
			}
			if (IsTextAllowed(tag.type) && ActionElementMap.TryGetCombinedElementIdentifierName(_tempAems, out var result3))
			{
				item.name = result3;
				results.Add(item);
				return true;
			}
		}
		_tempGlyphs.Clear();
		_tempKeys.Clear();
		int num = 0 | (TryGetGlyphsOrText(aemResult, tag.type, _tempGlyphs, _tempKeys, results) ? 1 : 0);
		_tempGlyphs.Clear();
		_tempKeys.Clear();
		return (byte)((uint)num | (TryGetGlyphsOrText(aemResult2, tag.type, _tempGlyphs, _tempKeys, results) ? 1u : 0u)) != 0;
	}

	private bool TryGetActionDisplayName(ActionTag tag, out string result)
	{
		if (tag == null)
		{
			result = null;
			return false;
		}
		InputAction action = ReInput.mapping.GetAction(tag.actionId);
		if (action == null)
		{
			result = null;
			return false;
		}
		result = action.GetDisplayName(tag.actionRange);
		tag.displayName = result;
		return true;
	}

	private bool TryGetPlayerDisplayName(PlayerTag tag, out string result)
	{
		if (tag == null)
		{
			result = null;
			return false;
		}
		Player player = ReInput.players.GetPlayer(tag.playerId);
		if (player == null)
		{
			result = null;
			return false;
		}
		result = player.descriptiveName;
		tag.displayName = result;
		return true;
	}

	private bool TryAssignSprite(Sprite sprite, string key)
	{
		Asset orCreateAsset = GetOrCreateAsset(sprite);
		if (orCreateAsset == null)
		{
			return false;
		}
		ITMProSpriteAsset spriteAsset = orCreateAsset.spriteAsset;
		if (!spriteAsset.Contains(key))
		{
			Rect rect = sprite.rect;
			ITMProSprite iTMProSprite = TMProAssetVersionHelper.CreateSprite();
			iTMProSprite.width = rect.width;
			iTMProSprite.height = rect.height;
			iTMProSprite.position = new Vector2(rect.x, rect.y);
			iTMProSprite.xOffset = rect.width * _spriteOptions.offsetSizeMultiplier.x + _spriteOptions.extraOffset.x;
			iTMProSprite.yOffset = rect.height * _spriteOptions.offsetSizeMultiplier.y + _spriteOptions.extraOffset.y;
			iTMProSprite.xAdvance = rect.width * _spriteOptions.xAdvanceWidthMultiplier + _spriteOptions.extraXAdvance;
			iTMProSprite.scale = _spriteOptions.scale;
			iTMProSprite.pivot = new Vector2(rect.width * -0.5f, rect.height * 0.5f);
			iTMProSprite.name = key;
			iTMProSprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(key);
			iTMProSprite.sprite = sprite;
			spriteAsset.AddSprite(iTMProSprite);
			SetDirty(orCreateAsset);
		}
		if (!_currentlyUsedAssets.Contains(orCreateAsset))
		{
			_currentlyUsedAssets.Add(orCreateAsset);
		}
		return true;
	}

	private void RequireRebuild()
	{
		_rebuildRequired = true;
	}

	private void CreatePrimaryAsset()
	{
		if (_primaryAsset == null)
		{
			_primaryAsset = new Asset(null);
			_tmProText.spriteAsset = _primaryAsset.spriteAsset.GetSpriteAsset();
		}
	}

	private Asset GetOrCreateAsset(Sprite sprite)
	{
		if (sprite == null || sprite.texture == null)
		{
			return null;
		}
		int count = _assignedAssets.Count;
		for (int i = 0; i < count; i++)
		{
			if (_assignedAssets[i] != null && _assignedAssets[i].spriteAsset.spriteSheet == sprite.texture)
			{
				return _assignedAssets[i];
			}
		}
		Asset asset = null;
		int count2 = _assetsPool.Count;
		for (int j = 0; j < count2; j++)
		{
			if (_assetsPool[j] != null)
			{
				asset = _assetsPool[j];
				_assetsPool.RemoveAt(j);
				break;
			}
		}
		if (asset == null)
		{
			asset = CreateAsset();
		}
		asset.spriteAsset.spriteSheet = sprite.texture;
		asset.material.SetTexture(ShaderUtilities.ID_MainTex, sprite.texture);
		List<TMP_SpriteAsset> list = _primaryAsset.spriteAsset.GetSpriteAsset().fallbackSpriteAssets;
		if (list == null)
		{
			list = new List<TMP_SpriteAsset>();
			_primaryAsset.spriteAsset.GetSpriteAsset().fallbackSpriteAssets = list;
		}
		list.Add(asset.spriteAsset.GetSpriteAsset());
		_assignedAssets.Add(asset);
		return asset;
	}

	private Asset CreateAsset()
	{
		Asset asset = new Asset(_baseSpriteMaterial);
		if (_overrideSpriteMaterialProperties)
		{
			CopySpriteMaterialPropertiesToMaterial(_spriteMaterialProperties, asset.material);
		}
		return asset;
	}

	private void RemoveUnusedAssets()
	{
		int num = 0;
		for (int num2 = _assignedAssets.Count - 1; num2 >= 0; num2--)
		{
			Asset asset = _assignedAssets[num2];
			if (asset != null && !_currentlyUsedAssets.Contains(asset))
			{
				if (num >= 2)
				{
					_primaryAsset.spriteAsset.GetSpriteAsset().fallbackSpriteAssets.Remove(asset.spriteAsset.GetSpriteAsset());
					asset.spriteAsset.spriteSheet = null;
					asset.spriteAsset.Clear();
					asset.material.SetTexture(ShaderUtilities.ID_MainTex, _stubTexture);
					_assetsPool.Add(asset);
					_assignedAssets.RemoveAt(num2);
				}
				else
				{
					num++;
				}
			}
		}
	}

	private void SetDirty(Asset asset)
	{
		if (!_dirtyAssets.Contains(asset))
		{
			_dirtyAssets.Add(asset);
		}
	}

	private void ForEachAsset(Action<Asset> callback)
	{
		if (callback == null)
		{
			return;
		}
		int count = _assignedAssets.Count;
		for (int i = 0; i < count; i++)
		{
			if (_assignedAssets[i] != null)
			{
				callback(_assignedAssets[i]);
			}
		}
		count = _assetsPool.Count;
		for (int j = 0; j < count; j++)
		{
			if (_assetsPool[j] != null)
			{
				callback(_assetsPool[j]);
			}
		}
	}

	private static void ParseAttributes(string text, int startIndex, int count, StringBuilder sbKey, StringBuilder sbValue, Dictionary<string, string> results)
	{
		if (string.IsNullOrEmpty(text) || startIndex < 0 || startIndex >= text.Length)
		{
			return;
		}
		results.Clear();
		sbKey.Length = 0;
		sbValue.Length = 0;
		bool flag = true;
		int num = startIndex + count - 1;
		int num2 = 0;
		try
		{
			for (int i = startIndex; i < startIndex + count; i++)
			{
				char c = text[i];
				switch (num2)
				{
				case 0:
					if (IsValidKeyChar(c))
					{
						num2 = 1;
						i--;
						sbKey.Length = 0;
					}
					break;
				case 1:
					if (c == '=')
					{
						if (sbKey.Length == 0)
						{
							throw new Exception("Key was blank.");
						}
						num2 = 2;
					}
					else if (IsValidKeyChar(c))
					{
						sbKey.Append(char.ToLowerInvariant(c));
					}
					else if (!char.IsWhiteSpace(c))
					{
						throw new Exception("Error parsing key.");
					}
					break;
				case 2:
					if ((flag = c == '"') || IsValidNonQuotedValueChar(c))
					{
						if (!flag)
						{
							i--;
						}
						sbValue.Length = 0;
						num2 = 3;
					}
					break;
				case 3:
					if ((flag && c == '"') || (!flag && (i == num || char.IsWhiteSpace(c))))
					{
						if (!flag && i == num)
						{
							sbValue.Append(c);
						}
						if (sbValue.Length == 0)
						{
							throw new Exception("Value was blank.");
						}
						if (results == null)
						{
							results = new Dictionary<string, string>();
						}
						results.Add(sbKey.ToString(), sbValue.ToString());
						num2 = 0;
					}
					else
					{
						sbValue.Append(c);
					}
					break;
				}
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	private static bool IsValidKeyChar(char c)
	{
		if (!char.IsLetterOrDigit(c))
		{
			return c == '_';
		}
		return true;
	}

	private static bool IsValidTagNameChar(char c)
	{
		if (!char.IsLetterOrDigit(c))
		{
			return c == '_';
		}
		return true;
	}

	private static bool IsValidNonQuotedValueChar(char c)
	{
		return char.IsDigit(c);
	}

	private static bool IsEqual(List<GlyphOrText> a, List<GlyphOrText> b)
	{
		if (a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	private static void WriteSpriteKey(StringBuilder sb, string key)
	{
		sb.Append("<sprite name=\"");
		sb.Append(key);
		sb.Append("\">");
	}

	private static bool TryGetGlyphsOrText(ActionElementMap aem, DisplayType displayType, List<Sprite> glyphs, List<string> keys, List<GlyphOrText> results)
	{
		if (aem == null || glyphs == null || results == null)
		{
			return false;
		}
		if (IsGlyphAllowed(displayType) && aem.GetElementIdentifierGlyphs(glyphs) > 0)
		{
			aem.GetElementIdentifierFinalGlyphKeys(keys);
			if (keys.Count != glyphs.Count)
			{
				Debug.LogError("Rewired: Glyph key count does not match glyph count.");
			}
			else
			{
				int count = glyphs.Count;
				for (int i = 0; i < count; i++)
				{
					GlyphOrText item = default(GlyphOrText);
					item.glyphKey = keys[i];
					item.sprite = glyphs[i];
					results.Add(item);
				}
				if (count > 0)
				{
					return true;
				}
			}
		}
		if (IsTextAllowed(displayType))
		{
			GlyphOrText item = default(GlyphOrText);
			item.name = aem.elementIdentifierName;
			results.Add(item);
			return true;
		}
		return false;
	}

	private static bool IsGlyphAllowed(DisplayType displayType)
	{
		if (displayType != 0)
		{
			return displayType == DisplayType.GlyphOrText;
		}
		return true;
	}

	private static bool IsTextAllowed(DisplayType displayType)
	{
		if (displayType != DisplayType.Text)
		{
			return displayType == DisplayType.GlyphOrText;
		}
		return true;
	}

	private static void CopyMaterialProperties(Material source, Material destination)
	{
		if (!(source == null) && !(destination == null))
		{
			destination.shader = source.shader;
			if (source.shaderKeywords != null)
			{
				string[] array = new string[source.shaderKeywords.Length];
				Array.Copy(source.shaderKeywords, array, source.shaderKeywords.Length);
				destination.shaderKeywords = array;
			}
			else
			{
				destination.shaderKeywords = null;
			}
			if (source.HasProperty(shaderPropertyId_color) && destination.HasProperty(shaderPropertyId_color))
			{
				destination.color = source.color;
			}
			destination.renderQueue = source.renderQueue;
			destination.globalIlluminationFlags = source.globalIlluminationFlags;
		}
	}

	private static void CopySpriteMaterialPropertiesToMaterial(SpriteMaterialProperties properties, Material material)
	{
		if (!(material == null) && material.HasProperty(shaderPropertyId_color))
		{
			material.color = properties.color;
		}
	}
}
