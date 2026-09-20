using System.Collections.Generic;
using System.Text;
using LinqTools;
using UnityEngine;

public class SkinChanger
{
	private List<SpriteRenderer> _sprites;

	private WorldGameObject _wgo;

	private SkinPreset _skin;

	private static Dictionary<int, string> _skin_ids_cache = new Dictionary<int, string>();

	private static StringBuilder _sb = new StringBuilder();

	private static Dictionary<int, Sprite> _sprite_hash = new Dictionary<int, Sprite>();

	private Dictionary<int, Sprite> _skinned_sprite_top_level_hash = new Dictionary<int, Sprite>();

	private static Dictionary<int, bool> _valid_sprite_hash = new Dictionary<int, bool>();

	private static char[] _chars = new char[100];

	public SkinChanger(WorldGameObject wgo)
	{
		_wgo = wgo;
		OnWGOChanged();
	}

	public void ApplySkin(SkinPreset skin)
	{
		_skin = skin;
		_skinned_sprite_top_level_hash.Clear();
	}

	public void OnWGOChanged()
	{
		_sprites = _wgo.GetComponentsInChildren<SpriteRenderer>(includeInactive: true).ToList();
	}

	private string ReplaceSkinId(string old_skin_id, int skin_id)
	{
		switch (skin_id)
		{
		case -1:
			return old_skin_id;
		case 0:
			return "";
		default:
		{
			if (_skin_ids_cache.TryGetValue(skin_id, out var value))
			{
				return value;
			}
			value = skin_id.ToString("0##");
			_skin_ids_cache.Add(skin_id, value);
			return value;
		}
		}
	}

	public void CustomLateUpdate()
	{
		if (_skin == null)
		{
			return;
		}
		for (int i = 0; i < _sprites.Count; i++)
		{
			SpriteRenderer spriteRenderer = _sprites[i];
			bool value = false;
			int key = 0;
			if (spriteRenderer.sprite != null)
			{
				key = spriteRenderer.sprite.GetInstanceID();
				if (!_valid_sprite_hash.TryGetValue(key, out value))
				{
					value = IsValidSprite(spriteRenderer);
					_valid_sprite_hash.Add(key, value);
				}
			}
			if (!value)
			{
				continue;
			}
			if (!_skinned_sprite_top_level_hash.TryGetValue(key, out var value2))
			{
				string s = spriteRenderer.sprite.name;
				char c = s[4];
				char c2 = s[5];
				char c3 = s[6];
				int num = -1;
				if (c == 'b' && c2 == 'd' && c3 == 'y')
				{
					num = _skin.body;
				}
				else if (c == 'h' && c2 == 'e' && c3 == 'd')
				{
					num = _skin.head;
				}
				else if (c == 'b' && c2 == 'o' && c3 == 't')
				{
					num = _skin.bot;
				}
				else if (c == 'm' && c2 == 'i' && c3 == 'd')
				{
					num = _skin.mid;
				}
				else if (c == 'b' && c2 == 'k' && c3 == 'p')
				{
					num = _skin.backpack;
				}
				if (num == 0 || string.IsNullOrEmpty(s))
				{
					spriteRenderer.enabled = false;
					spriteRenderer.sprite = null;
				}
				else
				{
					spriteRenderer.enabled = true;
					if (num != -1)
					{
						GarbagelessStrings.StringToChars(ref s, ref _chars);
						GarbagelessStrings.IntToCharsWithLeadingZeros(num, ref _chars, 3, 0);
						int hashCode = GarbagelessStrings.GetHashCode(ref _chars);
						if (!_sprite_hash.TryGetValue(hashCode, out value2))
						{
							value2 = EasySpritesCollection.GetSprite(GarbagelessStrings.CharsToString(ref _chars));
							_sprite_hash.Add(hashCode, value2);
						}
						spriteRenderer.sprite = value2;
					}
				}
				_skinned_sprite_top_level_hash.Add(key, (num == -1) ? spriteRenderer.sprite : value2);
			}
			else
			{
				spriteRenderer.sprite = value2;
				spriteRenderer.enabled = value2 != null;
			}
		}
	}

	private bool IsValidSprite(SpriteRenderer spr)
	{
		if (spr.sprite == null)
		{
			return false;
		}
		string name = spr.sprite.name;
		if (name.Length <= 7)
		{
			return false;
		}
		for (int i = 0; i < 3; i++)
		{
			char c = name[i];
			if (c < '0' || c > '9')
			{
				return false;
			}
		}
		return true;
	}
}
